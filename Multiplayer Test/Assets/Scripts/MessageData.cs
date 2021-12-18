using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageData {
    public string MessageType = null;
    public string ObjName = null;
    public string ObjParent = null;
    public Vector3 Pos = new Vector3(0.0114f,0,0);
    public Vector3 Scale = new Vector3(0.0114f,0,0);
    public Vector3 Rot = new Vector3(0.0114f,0,0);
    public string ObjFindName = null;
    public string[] ObjScripts = null;
    public string[] ModScriptVars = null;
    public string modifyId = null;
    public MessageData[] list = null;
    
    public static MessageData decodeMessage(string message) {
        return JsonUtility.FromJson<MessageData>(message);
    }
    public string encodeMessage() {
        string str = "{";

        if (MessageType != null) {
            str += "\"MessageType\":\"" + MessageType + "\",";
        } else if (list.Length <= 0) { return "{}"; }
        if (ObjName != null) {
            str += "\"ObjName\":\"" + ObjName + "\",";
        }
        if (ObjParent != null) {
            str += "\"ObjParent\":\"" + ObjParent + "\",";
        }
        if (Pos != null && Pos != new Vector3(0.0114f,0,0)) {
            str += "\"Pos\":{\"x\":" + Pos.x + ",\"y\":" + Pos.y + ",\"z\":" + Pos.z + "},";
        }
        if (Scale != null && Scale != new Vector3(0.0114f,0,0)) {
            str += "\"Scale\":{\"x\":" + Scale.x + ",\"y\":" + Scale.y + ",\"z\":" + Scale.z + "},";
        }
        if (Rot != null && Rot != new Vector3(0.0114f,0,0)) {
            str += "\"Rot\":{\"x\":" + Rot.x + ",\"y\":" + Rot.y + ",\"z\":" + Rot.z + "},";
        }
        if (ObjFindName != null) {
            str += "\"ObjFindName\":\"" + ObjFindName + "\",";
        }
        if (ObjScripts != null && ObjScripts.Length > 0) {
            str += "\"ObjScripts\":[";
            for (int i = 0; i < ObjScripts.Length; i++) {
                str += "\"" + ObjScripts[i] + "\"" + (i != ObjScripts.Length-1 ? "," : "");
            }
            str += "],";
        }
        if (ModScriptVars != null && ModScriptVars.Length > 0) {
            str += "\"ModScriptVars\":[";
            for (int i = 0; i < ModScriptVars.Length; i++) {
                str += "\"" + ModScriptVars[i] + "\"" + (i != ModScriptVars.Length-1 ? "," : "");
            }
            str += "],";
        }
        if (modifyId != null) {
            str += "\"modifyId\":\"" + modifyId + "\",";
        }
        if (list != null && list.Length > 0) {
            str += "\"list\":[";
            for (int i = 0; i < list.Length; i++) {
                string encode = list[i].encodeMessage();
                str += encode + (i != list.Length-1 ? "," : "");
            }
            str += "] ";
        }
        return str.Remove(str.Length - 1, 1) + "}";
    }
}
