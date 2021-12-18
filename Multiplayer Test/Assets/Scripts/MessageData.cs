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
    public string modifyId;
    public MessageData[] list;
    public string DummyList = "DummyListValue";
    
    public static MessageData decodeMessage(string message) {
        return JsonUtility.FromJson<MessageData>(message);
    }
    public string encodeMessage() {
        string thing = JsonUtility.ToJson(this);
        thing = thing.Replace("\"DummyList\":\"DummyListValue\"","\"list\":[]");
        if (list != null && list.Length > 0) {
            string str = "\"list\":[";
            for (int i = 0; i < list.Length; i++) {
                string encode = list[i].encodeMessage();
                str += encode + (i != list.Length-1 ? "," : "");
            }
            str += "]";
            thing = thing.Replace("\"list\":[]",str);
        }
        return thing;
    }
}
