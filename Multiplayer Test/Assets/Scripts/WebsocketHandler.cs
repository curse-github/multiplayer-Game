using WebSocketSharp;
using UnityEngine;
using System;

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
    void Start()
    {
        ws = new WebSocket("ws://localhost:53586");
        ws.OnMessage += (sender,e) => {
            ServerReceiver.Instance.toProcess.Add(e.Data);
        };
        ws.Connect();
    }
    public void send(string message) {
        ws.Send(message);
    }
}
