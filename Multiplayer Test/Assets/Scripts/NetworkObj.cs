using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObj : MonoBehaviour
{
    public string name = "";
    public int skip = 0;
    public Vector3 oldPos;
    public Vector3 oldRot;
    public Vector3 oldSca;

    bool positionChange = false;
    bool rotationChange = false;
    bool scaleChange = false;

    bool skipping = false;
    void Awake()
    {
        oldPos = transform.position;
        oldRot = transform.localEulerAngles;
        oldSca = transform.localScale;

        if (name == "") {
            name = transform.name;
        }
    }
    void Update()
    {
        if (skip > 0) { skip--; skipping = true; return; } else { skipping = false; }
        if (!skipping) {
            if (Mathf.Abs(oldRot.x - transform.localEulerAngles.x) >= 0.1 || Mathf.Abs(oldRot.y - transform.localEulerAngles.y) >= 0.1 || Mathf.Abs(oldRot.z - transform.localEulerAngles.z) >= 0.1) {
                rotationChange = true;
                oldRot = transform.localEulerAngles;
            }
            if ((oldPos - transform.position).magnitude >= 0.1) {
                positionChange = true;
                oldPos = transform.position;
            }
            if (oldSca != transform.localScale) {
                positionChange = true;
                oldSca = transform.localScale;
            }
            if (rotationChange || positionChange) {
                MessageData data = new MessageData();
                data.MessageType = "Modify";
                data.ObjFindName = transform.name;
                int len = 4 + ((rotationChange && positionChange) ? 2 : 0);
                if (positionChange) {
                    data.Pos = transform.position;
                }
                if (rotationChange) {
                    data.Rot = transform.localEulerAngles;
                }
                if (scaleChange) {
                    data.Scale = transform.localScale;
                }
                data.modifyId = Camera.main.transform.parent.name.Split("Player")[1];
                WebsocketHandler.Instance.send(data.encodeMessage());
                //print("Moved!");
            }
        }
        positionChange = false;
        rotationChange = false;
        scaleChange = false;
    }
}
