using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObj : MonoBehaviour
{
    public new string name = "";
    public Vector3 oldPos;
    public Vector3 oldRot;
    Rigidbody rigid;
    MessageData data = new MessageData();
    void Awake()
    {
        oldPos = transform.position;
        oldRot = transform.localEulerAngles;
        if (name == "") {
            name = transform.name;
        }

        data.MessageType = "Modify";
        data.ObjFindName = transform.name;
    }
    private void Start() {
        rigid = gameObject.GetComponent<Rigidbody>();
    }
    public MessageData getChange() {
        if (Camera.main.transform.parent == null) { return null; }
        bool positionChange = false;
        bool rotationChange = false;
        if ((oldPos - transform.position).magnitude >= 0.1) {
            positionChange = true;
        }
        List<string> ModScriptVars = new List<string>();

        if ((oldRot - transform.localEulerAngles).magnitude >= 0.1) {
            rotationChange = true;
            oldRot = transform.localEulerAngles;
            data.Rot = transform.localEulerAngles;
            
            ModScriptVars.Add("UnityEngine.Rigdbody");
            ModScriptVars.Add((1 + (positionChange ? 1 : 0)).ToString());
            ModScriptVars.Add("angularVelocity");
            ModScriptVars.Add("(" + rigid.angularVelocity.x + "," + rigid.angularVelocity.y + "," + rigid.angularVelocity.z + ")");
        }
        if (positionChange) {
            oldPos = transform.position;
            data.Pos = transform.position;

            if (ModScriptVars.Count == 0) {
                ModScriptVars.Add("UnityEngine.Rigdbody");
                ModScriptVars.Add("1");
            }
            ModScriptVars.Add("velocity");
            ModScriptVars.Add("(" + rigid.velocity.x + "," + rigid.velocity.y + "," + rigid.velocity.z + ")");
        }
        if (ModScriptVars.Count > 0) {
            data.ModScriptVars = ModScriptVars.ToArray();
        } else {
            data.ModScriptVars = null;
        }
        if (positionChange || rotationChange) {
            return data;
        }
        return null;
    }
}
