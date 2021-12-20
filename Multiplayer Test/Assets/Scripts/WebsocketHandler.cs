using WebSocketSharp;
using System;
using System.Reflection;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WebsocketHandler : MonoBehaviour 
{
    private static WebsocketHandler _instance;
    public static WebsocketHandler Instance { get { return _instance; } }
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
        
    }
    public static WebSocket ws;
    private float lastTime = 0;
    private List<MessageData> toSend = new List<MessageData>();
    public string modifyId = "";
    public static string sending = "";
    public bool isalive = false;
    void Start() {
        Connect();
    }
    void OnApplicationQuit()  { 
        Disconnect();
    }
    void Connect() {
        ws = new WebSocket ("ws://mc.campbellsimpson.com:53586");
        ws.OnOpen += (sender, e) => {
            Debug.Log("WebSocket Open");
            isalive = true;
        };
        ws.EmitOnPing = true;
        ws.OnMessage += (sender, e) => {
            ServerReceiver.Instance.toProcess.Add(e.Data);
        };
        ws.OnError += (sender, e) => {
            Debug.Log("WebSocket Error Message: " + e.Message);
        };
        ws.OnClose += (sender, e) => {
            isalive = false;
            Debug.Log("WebSocket Close" + e.Reason + " --- " + e.Code);
                
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            StartCoroutine(ConnectCoroutine());
        };
        ConnectF();
    }
    private IEnumerator ConnectCoroutine() {
        yield return new WaitForSeconds(5);
        ConnectF();
    }
    private void ConnectF() {
        ws.ConnectAsync();
    }
    void Disconnect() {
        ws.Close();
        ws = null;
    }
    public void send(MessageData message) {
        toSend.Add(message);
    }
    public void sendnow(string data) {
        ws.Send(data);
    }
    private void Update() {
        if (isalive == true && modifyId != "") {
            //lastTime += Time.fixedUnscaledDeltaTime;
            lastTime += Time.unscaledDeltaTime;
            if (lastTime >= 250/1000) {
                if (PlayerController.Instance != null) {
                    MessageData PlayerPos = PlayerController.Instance.getPos();
                    if (PlayerPos != null) { toSend.Add(PlayerPos); }
                    MessageData CameraRot = PlayerController.Instance.getRot();
                    if (CameraRot != null) { toSend.Add(CameraRot); }
                }
                foreach(NetworkObj obj in Resources.FindObjectsOfTypeAll(typeof(NetworkObj)) as NetworkObj[]) {
                    if (obj == null) { continue; }
                    MessageData change = obj.getChange();
                    if (change != null) {
                        toSend.Add(change);
                    }
                }
                if (toSend.Count > 0) {
                    MessageData data = new MessageData();
                    data.list = toSend.ToArray();
                    data.modifyId = modifyId;
                    toSend = new List<MessageData>();
                    string encoded = data.encodeMessage();
                    sendnow(encoded);
                }
                lastTime = 0;
            } else { return; }
        } else if (isalive == true && modifyId == "") {
            if (Camera.main.transform.parent != null && Camera.main.transform.parent.name.Contains("Player")) { modifyId = Camera.main.transform.parent.name.Split("Player")[1]; }
        } else { return; }
    }
}