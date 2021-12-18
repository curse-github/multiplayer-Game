using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {
    public static PlayerController Instance;

    public float moveSpeed = 10f;
    public float sensitivityX = 10f;
    public float sensitivityY = 10f;
    public float jumpForce = 10f;

    public bool onGround = false;

    public Rigidbody rigid;
    private GameObject pauseOverlay;

    public Vector3 oldPos;
    Vector3 oldRot;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else { GameObject.Destroy(gameObject); }
        if (!Statics.Instance.isPaused) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        rigid = gameObject.GetComponent<Rigidbody>();
        oldPos = transform.position;
        pauseOverlay = GameObject.Find("Canvas").transform.Find("PauseOverlay").gameObject;
    }
    private void Update() {
        int layerMask = 1 << 8;
        layerMask = ~layerMask;
        RaycastHit hit;
        bool[] rays = {
            Physics.Raycast(transform.position-Vector3.up, Vector3.Normalize(new Vector3(0,-1,0)),   out hit, 1.1f, layerMask),
            Physics.Raycast(transform.position-Vector3.up, Vector3.Normalize(new Vector3(1,-1,0)),   out hit, 1.01f, layerMask),
            Physics.Raycast(transform.position-Vector3.up, Vector3.Normalize(new Vector3(-1,-1,0)),  out hit, 1.01f, layerMask),
            Physics.Raycast(transform.position-Vector3.up, Vector3.Normalize(new Vector3(0,-1,1)),   out hit, 1.01f, layerMask),
            Physics.Raycast(transform.position-Vector3.up, Vector3.Normalize(new Vector3(0,-1,-1)),  out hit, 1.01f, layerMask),
        };
        
        if (rays[0]||rays[1]||rays[2]||rays[3]||rays[4]) {
            onGround = true;
        } else { onGround = false; }

        bool value = Statics.Instance.isPaused;
        if (!value) {
            float moveHorizontal = Input.GetAxis ("Horizontal");
            float moveVertical = Input.GetAxis ("Vertical");
            if(moveHorizontal != 0) {
                rigid.position += transform.right * moveHorizontal * moveSpeed * Time.deltaTime;
            }
            if(moveVertical != 0) {
                rigid.position += transform.forward * moveVertical * moveSpeed * Time.deltaTime;
            }
            float lookHorizontal = Input.GetAxis ("Mouse X");
            float lookVertical = Input.GetAxis ("Mouse Y");
            transform.localEulerAngles = new Vector3(0,lookHorizontal*sensitivityX + transform.localEulerAngles.y,0);
            Camera.main.transform.localEulerAngles = new Vector3(lookVertical*-sensitivityY + Camera.main.transform.localEulerAngles.x,0,0);
            if (Input.GetKeyDown(KeyCode.Space) && onGround) {
                rigid.velocity += new Vector3(0,10,0);
            }
            if (Input.GetKeyDown(KeyCode.H)) {
                MessageData data = new MessageData();
                data.MessageType = "Create";
                data.ObjScripts = new string[] {
                    "UnityEngine.MeshFilter",
                    "UnityEngine.MeshRenderer",
                    "UnityEngine.BoxCollider",
                    "UnityEngine.Rigidbody",
                    "NetworkObj"
                };
                data.ModScriptVars = new string[] {
                    "UnityEngine.MeshFilter","1","mesh","Default.Mesh.Cube,fbx",
                    "UnityEngine.MeshRenderer","1","materials[0]","Default.Mat.Default-Diffuse"
                };
                data.Pos = transform.position + Camera.main.transform.forward*3;
                data.Scale = new Vector3(2,2,2);
                WebsocketHandler.Instance.send(data);
            }

            //rotation is put in here as it shouldnt change while paused anyways
            if (transform.childCount > 0 && (Mathf.Abs(oldRot.y - Camera.main.transform.eulerAngles.y) >= 0.1 || Mathf.Abs(oldRot.x - Camera.main.transform.eulerAngles.x) >= 0.1)) {
                oldRot = Camera.main.transform.eulerAngles;
                MessageData rotdata = new MessageData();
                rotdata.MessageType = "Modify";
                rotdata.ObjFindName = transform.name + "/Main Camera";
                rotdata.Rot = Camera.main.transform.eulerAngles;
                rotdata.modifyId = transform.name.Split("Player")[1];
                WebsocketHandler.Instance.send(rotdata);
            }
        }
        //code for sending data about the player to the server
        if ((oldPos - transform.position).magnitude >= 0.1) {
            oldPos = transform.position;
            MessageData posdata = new MessageData();
            posdata.MessageType = "Modify";
            posdata.ObjFindName = transform.name;
            posdata.Pos = transform.position;
            posdata.modifyId = transform.name.Split("Player")[1];
            WebsocketHandler.Instance.send(posdata);
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            Statics.Instance.isPaused = !value;
            value = !value;
            if (value) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                pauseOverlay.SetActive(true);
            } else {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                pauseOverlay.SetActive(false);
            }
        }
    }
}
