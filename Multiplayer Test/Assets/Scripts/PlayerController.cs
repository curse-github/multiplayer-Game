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
    public bool mousedown = false;
    public Rigidbody grabbed = null;
    public Rigidbody rigid;
    private GameObject pauseOverlay;
    private MessageData cubeData = new MessageData();

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
        
        cubeData.MessageType = "Create";
        cubeData.ObjScripts = new string[] {
            "UnityEngine.MeshFilter",
            "UnityEngine.MeshRenderer",
            "UnityEngine.BoxCollider",
            "UnityEngine.Rigidbody",
            "NetworkObj"
        };
        cubeData.ModScriptVars = new string[] {
            "UnityEngine.MeshFilter","1","mesh","Default.Mesh.Cube,fbx",
            "UnityEngine.MeshRenderer","1","materials[0]","Default.Mat.Default-Diffuse",
            "UnityEngine.Rigidbody","1","velocity","(0,0,0)"
        };
        cubeData.Scale = new Vector3(2,2,2);
    }
    private void FixedUpdate() {
        if (transform.childCount == 0) { return; }
        rigid.velocity = new Vector3(0,rigid.velocity.y,0);
        //check on ground
        int layerMask = 1 << 2;
        layerMask = ~layerMask;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, 2.2f, layerMask)) {
            onGround = true;
        } else { onGround = false; }

        bool value = Statics.Instance.isPaused;
        if (!value) {
            float horiz = Input.GetAxisRaw("Horizontal");
            float verti = Input.GetAxisRaw("Vertical");
            if (horiz != 0 || verti != 0) { rigid.position += Vector3.Normalize(transform.right * horiz + transform.forward * verti) * moveSpeed * Time.deltaTime; }
            
            float lookHorizontal = Input.GetAxisRaw("Mouse X");
            float lookVertical = Input.GetAxisRaw("Mouse Y");
            if ( lookHorizontal != 0) { transform.localEulerAngles = new Vector3(0,lookHorizontal*sensitivityX + transform.localEulerAngles.y,0); }
            if ( lookVertical != 0) { Camera.main.transform.localEulerAngles = new Vector3(lookVertical*-sensitivityY + Camera.main.transform.localEulerAngles.x,0,0); }
            if (Input.GetKeyDown(KeyCode.Space) && onGround) {
                rigid.velocity += new Vector3(0,jumpForce,0);
            }
            if (Input.GetKeyDown(KeyCode.H)) {
                Vector3 frwdVc = Camera.main.transform.forward*jumpForce*2;
                cubeData.ModScriptVars[11] = "(" + frwdVc.x + "," + frwdVc.y + "," + frwdVc.z + ")";
                cubeData.Pos = transform.position + Vector3.Normalize(transform.GetChild(0).forward)*10;
                WebsocketHandler.Instance.send(cubeData);
            }
            bool down = Input.GetMouseButton(0);
            if (down && (!mousedown || grabbed == null)) {
                mousedown = true;
                if (Physics.Raycast(transform.GetChild(0).position, Vector3.Normalize(transform.GetChild(0).forward), out hit, 8, layerMask)) {
                    if (hit.rigidbody != null) { grabbed = hit.rigidbody; }
                }
            } else if (!down && mousedown) {
                mousedown = false;
                grabbed = null;
            }
            if (mousedown && grabbed != null) {
                Vector3 pos1 = transform.GetChild(0).position + Vector3.Normalize(transform.GetChild(0).forward)*6;
                if ((pos1 - grabbed.transform.position).magnitude > 0.1f) {
                    grabbed.velocity += pos1-grabbed.transform.position;
                }
            }
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
    public MessageData getPos() {
        if ((oldPos - transform.position).magnitude >= 0.1) {
            oldPos = transform.position;
            MessageData posdata = new MessageData();
            posdata.MessageType = "Modify";
            posdata.ObjFindName = transform.name;
            posdata.Pos = transform.position;
            /*
            Vector3 vel = rigid.velocity;
            posdata.ModScriptVars = new string[]{
                "UnityEngine.Rigidbody","1","velocity","(" + vel.x + "," + vel.y + "," + vel.z + ")"
            };
            */
            return posdata;
        }
        return null;
    }
    public MessageData getRot() {
        if (transform.childCount > 0 && (Mathf.Abs(oldRot.y - Camera.main.transform.eulerAngles.y) >= 0.1 || Mathf.Abs(oldRot.x - Camera.main.transform.eulerAngles.x) >= 0.1)) {
            oldRot = Camera.main.transform.eulerAngles;
            MessageData rotdata = new MessageData();
            rotdata.MessageType = "Modify";
            rotdata.ObjFindName = transform.name + "/Main Camera";
            rotdata.Rot = Camera.main.transform.eulerAngles;
            return rotdata;
        }
        return null;
    }
}
