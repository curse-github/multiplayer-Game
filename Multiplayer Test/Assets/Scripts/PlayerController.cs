using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public float moveSpeed = 10f;
    public float sensitivityX = 10f;
    public float sensitivityY = 10f;
    public float jumpForce = 10f;
    private Rigidbody rigid;
    private void Awake() {
        if (!Statics.Instance.isPaused) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        rigid = gameObject.GetComponent<Rigidbody>();
    }
    private void Update() {
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
            Transform cam = transform.Find("Main Camera");
            cam.localEulerAngles = new Vector3(lookVertical*-sensitivityY + cam.localEulerAngles.x,0,0);
            if (Input.GetKeyDown(KeyCode.Space)) {
                rigid.velocity += new Vector3(0,10,0);
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Statics.Instance.isPaused = !value;
            value = !value;
            if (value) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            } else {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
