using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObj : MonoBehaviour
{
    public new string name = "";
    public Vector3 oldPos;
    public Vector3 oldRot;
    public Vector3 oldSca;
    Rigidbody rigid;
    void Awake()
    {
        oldPos = transform.position;
        oldRot = transform.localEulerAngles;
        oldSca = transform.localScale;

        if (name == "") {
            name = transform.name;
        }
    }
    private void Start() {
        rigid = gameObject.GetComponent<Rigidbody>();
    }
    public MessageData getChange() {
        bool positionChange = false;
        bool rotationChange = false;
        bool scaleChange = false;
        if (Camera.main.transform.parent == null) { return null; }
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
            List<GameObject> otherPlayers = new List<GameObject>();
            GameObject[] goS = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
            string mainPlayerName = Camera.main.transform.parent.name;
            float closest = 9999999999999;
            for (int i = 0; i < goS.Length; i++) {
                if (goS[i].name.Contains("Player") && goS[i].name != mainPlayerName) {
                    otherPlayers.Add(goS[i]);
                    float dist = Vector3.Distance(goS[i].transform.position, transform.position);
                    if (dist < closest) {
                        dist = closest;
                    }
                }
            }
            if (Vector3.Distance(Camera.main.transform.parent.position, transform.position) - closest > 1) {
                positionChange = false;
                rotationChange = false;
                scaleChange = false;
                return null;
            }

            MessageData data = new MessageData();
            data.MessageType = "Modify";
            data.ObjFindName = transform.name;
            List<string> ModScriptVars = new List<string>();
            ModScriptVars.Add("UnityEngine.Rigidbody");
            ModScriptVars.Add((1+((rotationChange && positionChange) ? 1 : 0)).ToString());
            if (positionChange) {
                data.Pos = transform.position;
                ModScriptVars.Add("velocity");
                Vector3 vel = rigid.velocity;
                ModScriptVars.Add("(" + vel.x + "," + vel.y + "," + vel.z + ")");
            }
            if (rotationChange) {
                data.Rot = transform.localEulerAngles;
                ModScriptVars.Add("angularVelocity");
                Vector3 vel = rigid.angularVelocity;
                ModScriptVars.Add("(" + vel.x + "," + vel.y + "," + vel.z + ")");
            }
            data.ModScriptVars = ModScriptVars.ToArray();
            if (scaleChange) {
                data.Scale = transform.localScale;
            }
            data.modifyId = Camera.main.transform.parent.name.Split("Player")[1];
            return data;
        }
        return null;
    }
}
