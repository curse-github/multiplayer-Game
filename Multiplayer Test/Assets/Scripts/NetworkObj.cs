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

        if ((oldRot - transform.localEulerAngles).magnitude >= 0.1) {
            rotationChange = true;
            oldRot = transform.localEulerAngles;
            data.Rot = transform.localEulerAngles;
        }
        if ((oldPos - transform.position).magnitude >= 0.1) {
            positionChange = true;
            oldPos = transform.position;
            data.Pos = transform.position;
        }
        if (rotationChange || positionChange) {
            return data;
        }
        return null;
    }
}
