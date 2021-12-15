using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

public class Statics : MonoBehaviour {
    private static Statics _instance;
    public static Statics Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
        
    }
    public static object getValue(string property) {
        switch (property) {
            case(""):
                return "";
        }
        return null;
        //foreach (var m in Statics.Instance.GetType().GetFields()) {
        //    Debug.Log(m.Name);
        //}
        //PropertyInfo prop = _instance.GetType().GetProperty(property);
        //print(prop.GetValue(_instance));
        //return prop.GetValue(_instance);
    }
}