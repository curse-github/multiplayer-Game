using WebSocketSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System;
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
    WebSocket ws;
    private IEnumerator checkConnectionAlive;
    private float lastTime = 0;
    private List<MessageData> toSend = new List<MessageData>();
    void Start()
    {
        checkConnectionAlive = CheckConnectionAlive();
        Connect();
    }
    IEnumerator CheckConnectionAlive()
    {
        while (true) {
            yield return new WaitForSeconds(5);
            //Debug.Log("checking connection");
            if (ws.IsAlive == true) {
                //Debug.Log("connected");
            } else {
                Debug.Log("Disconnected From Server.");
                Disconnect();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
    void OnApplicationQuit()  { 
        Disconnect(); 
        StopCoroutine(checkConnectionAlive);
    }
    void Connect() {
        ws = new WebSocket ("ws://192.168.1.37:53586");
        ws.OnOpen += (sender, e) => {
            Debug.Log("WebSocket Open");
        };
        ws.EmitOnPing = true;
        ws.OnMessage += (sender, e) => {
            ServerReceiver.Instance.toProcess.Add(e.Data);
        };
        ws.OnError += (sender, e) => {
            Debug.Log("WebSocket Error Message: " + e.Message);
        };
        ws.OnClose += (sender, e) => {
            Debug.Log("WebSocket Close" + e.Reason + " --- " + e.Code);
        };
        ConnectF();
    }
    private void ConnectF() {
        ws.ConnectAsync();
        StartCoroutine(checkConnectionAlive);
    }
    void Disconnect() {
        ws.Close();
        ws = null;
    }
    public void send(MessageData message) {
        if (ws.IsAlive == true) {
            toSend.Add(message);
        }
    }
    public void sendnow(string message) {
        if (ws.IsAlive == true) {
            ws.Send(message);
        }
    }
    private void Update() {
        if (ws.IsAlive == true && toSend.Count > 0) {
            lastTime += Time.unscaledDeltaTime;
            if (lastTime >= 250/1000) {
                MessageData data = new MessageData();
                data.list = new MessageData[toSend.Count];
                for(int i = 0; i < toSend.Count; i++) {
                    data.list[i] = toSend[i];
                }
                data.modifyId = Camera.main.transform.parent.name.Split("Player")[1];
                toSend = new List<MessageData>();
                string encode = data.encodeMessage();
                sendnow(encode);
                lastTime = 0;
            }
        }
    }
}