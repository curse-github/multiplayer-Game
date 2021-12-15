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
    public string[] ObjScriptVars = null;
    public string[] ModScriptVars = null;

    public static MessageData decodeMessage(string message) {
        return JsonUtility.FromJson<MessageData>(message);
    }
    public string encodeMessage() {
        return JsonUtility.ToJson(this);
    }
}
