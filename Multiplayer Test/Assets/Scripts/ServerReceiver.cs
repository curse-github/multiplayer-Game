//https://www.google.com/search?q=c%23+websocket+client&oq=c%23+websocket+client&aqs=chrome..69i57j69i58.7267j0j1&sourceid=chrome&ie=UTF-8
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System;

public class ServerReceiver : MonoBehaviour
{
    public static Mesh defCube;
    List<string> toProcess = new List<string>();
    void Start()
    {
        GameObject obj1 = new GameObject();
        obj1.transform.position = new Vector3(101001010101010,10,1010100101);
        print(obj1.AddComponent<Rigidbody>().angularVelocity);
        MessageData data = new MessageData();
        data.MessageType = "Create";
        data.ObjName = "Floor";
        data.Pos = new Vector3(0,0.25f/-2,0);
        data.Scale = new Vector3(50,0.25f,50);
        data.Rot = new Vector3(0,45,0);
        data.ObjScripts = new string[] {
            "UnityEngine.MeshFilter",
            "UnityEngine.MeshRenderer",
            "UnityEngine.BoxCollider"
        };
        data.ObjScriptVars = new string[]{
            "1","mesh","Default.Mesh.Cube,fbx",
            "1","materials[0]","Default.Mat.Default-Diffuse,mat"
        };
        toProcess.Add(data.encodeMessage());

        data = new MessageData();
        data.MessageType = "Create";
        data.ObjName = "Cube2";
        data.Pos = new Vector3(0,5,0);
        data.Scale = new Vector3(1,1,1);
        data.Rot = new Vector3(30,0,30);
        data.ObjScripts = new string[] {
            "UnityEngine.MeshFilter",
            "UnityEngine.MeshRenderer",
            "UnityEngine.Rigidbody",
            "UnityEngine.BoxCollider",
            "NetworkObj"
        };
        data.ObjScriptVars = new string[]{
            "1","mesh","Default.Mesh.Cube,fbx",
            "1","materials[0]","Default.Mat.Default-Diffuse,mat",
            "1","collisionDetectionMode","ContinuousDynamic"
        };
        toProcess.Add(data.encodeMessage());
        
        Proccess();
        StartCoroutine(waiter());
    }
    IEnumerator waiter()
    {
        yield return new WaitForSeconds(2);
        string msg = "{\"MessageType\":\"Modify\",\"ObjFindName\":\"Cube2\",\"ModScriptVars\":[\"UnityEngine.Rigidbody\",\"2\",\"velocity\",\"(0,10,0)\",\"angularVelocity\",\"(0,10,0)\"]}";
        toProcess.Add(msg);
        Proccess();
    }
    public void Proccess() {
        if (toProcess.Count <= 0) { return; }
        //print(toProcess[0]);
        MessageData decoded = MessageData.decodeMessage(toProcess[0]);
        toProcess.RemoveAt(0);
        if (decoded.MessageType == "Create")
        {
            GameObject obj = new GameObject();
            //change object name
            if (decoded.ObjName != null && decoded.ObjName != "")
            {
                obj.name = decoded.ObjName;
            }
            //change object parent
            if (decoded.ObjParent != null && decoded.ObjParent != "") {
                GameObject temp = GameObject.Find(decoded.ObjParent);
                if (temp != null) {
                    obj.transform.parent = temp.transform;
                }
            }
            //change position
            if (decoded.Pos != null)
            {
                obj.transform.localPosition = decoded.Pos;
            } else { obj.transform.localPosition = new Vector3(0,0,0); }
            //change scale
            if (decoded.Scale != null)
            {
                obj.transform.localScale = decoded.Scale;
            } else { obj.transform.localScale = new Vector3(1,1,1); }
            //change rotation
            if (decoded.Rot != null)
            {
                obj.transform.localEulerAngles = decoded.Rot;
            } else { obj.transform.localEulerAngles = new Vector3(0,0,0); }
            //add scripts
            if (decoded.ObjScripts != null && decoded.ObjScripts.Length > 0)
            {
                int index = 1;
                for(int i = 0; i < decoded.ObjScripts.Length; i++) {
                    Type type = GetType(decoded.ObjScripts[i]);
                    Component comp = obj.AddComponent(type);
                    if (decoded.ObjScriptVars != null && decoded.ObjScriptVars.Length > index && decoded.ObjScriptVars[index] != null) {
                        if (int.Parse(decoded.ObjScriptVars[index-1]) > 0) {
                            int propAmount = int.Parse(decoded.ObjScriptVars[index-1]);
                            int j;
                            for (j = index; j < propAmount*2+index; j++) {
                                if (decoded.ObjScriptVars[j].EndsWith("]")) {
                                    //it is assigning a value in a list
                                    string[] split = decoded.ObjScriptVars[j].Split("[");
                                    PropertyInfo prop = type.GetProperty(split[0]);
                                    if (prop == null) {
                                        FieldInfo prop2 = type.GetField(split[0]);
                                        object[] value = (object[])prop2.GetValue(comp);
                                        if (decoded.ObjScriptVars[j+1].StartsWith("Statics.")) {
                                            value[int.Parse(split[1].Split("]")[0])] = Statics.getValue(decoded.ObjScriptVars[j+1].Split("Statics.")[1]);
                                        } else if (decoded.ObjScriptVars[j+1].StartsWith("Default.Mesh.")) {
                                            value[int.Parse(split[1].Split("]")[0])] = Resources.GetBuiltinResource<Mesh>(decoded.ObjScriptVars[j+1].Split("Default.Mesh.")[1].Replace(",","."));
                                        } else if (decoded.ObjScriptVars[j+1].StartsWith("Default.Mat.")) {
                                            value[int.Parse(split[1].Split("]")[0])] = GetMat(decoded.ObjScriptVars[j+1].Split("Default.Mat.")[1].Replace(",","."));
                                        } else if (prop2.FieldType.ToString() == "UnityEngine.Vector3") {
                                            string[] split2 = decoded.ModScriptVars[j+1].Split("(")[1].Split(")")[0].Replace(" ","").Split(",");
                                            Vector3 vec = new Vector3(float.Parse(split2[0]),float.Parse(split2[1]),float.Parse(split2[2]));
                                            value[int.Parse(split[1].Split("]")[0])] = vec;
                                        } else if (prop2.FieldType.ToString() == "UnityEngine.CollisionDetectionMode") {
                                            value[int.Parse(split[1].Split("]")[0])] = Enum.Parse(typeof(CollisionDetectionMode), decoded.ObjScriptVars[j+1]);
                                        } else { value[int.Parse(split[1].Split("]")[0])] = decoded.ObjScriptVars[j+1]; }
                                        prop2.SetValue(comp,value);
                                    } else {
                                        object[] value = (object[])prop.GetValue(comp);
                                        if (decoded.ObjScriptVars[j+1].StartsWith("Statics.")) {
                                            value[int.Parse(split[1].Split("]")[0])] = Statics.getValue(decoded.ObjScriptVars[j+1].Split("Statics.")[1]);
                                        } else if (decoded.ObjScriptVars[j+1].StartsWith("Default.Mesh.")) {
                                            value[int.Parse(split[1].Split("]")[0])] = Resources.GetBuiltinResource<Mesh>(decoded.ObjScriptVars[j+1].Split("Default.Mesh.")[1].Replace(",","."));
                                        } else if (decoded.ObjScriptVars[j+1].StartsWith("Default.Mat.")) {
                                            value[int.Parse(split[1].Split("]")[0])] = GetMat(decoded.ObjScriptVars[j+1].Split("Default.Mat.")[1].Replace(",","."));
                                        } else if (prop.PropertyType.ToString() == "UnityEngine.Vector3") {
                                            string[] split2 = decoded.ModScriptVars[j+1].Split("(")[1].Split(")")[0].Replace(" ","").Split(",");
                                            Vector3 vec = new Vector3(float.Parse(split2[0]),float.Parse(split2[1]),float.Parse(split2[2]));
                                            value[int.Parse(split[1].Split("]")[0])] = vec;
                                        } else if (prop.PropertyType.ToString() == "UnityEngine.CollisionDetectionMode") {
                                            value[int.Parse(split[1].Split("]")[0])] = Enum.Parse(typeof(CollisionDetectionMode), decoded.ObjScriptVars[j+1]);
                                        } else { value[int.Parse(split[1].Split("]")[0])] = decoded.ObjScriptVars[j+1]; }
                                        prop.SetValue(comp,value);
                                    }
                                } else {
                                    PropertyInfo prop = type.GetProperty(decoded.ObjScriptVars[j]);
                                    if (prop == null) {
                                        FieldInfo prop2 = type.GetField(decoded.ObjScriptVars[j]);
                                        if (decoded.ObjScriptVars[j+1].StartsWith("Statics.")) {
                                            prop2.SetValue(comp,Statics.getValue(decoded.ObjScriptVars[j+1].Split("Statics.")[1]));
                                        } else if (decoded.ObjScriptVars[j+1].StartsWith("Default.Mesh.")) {
                                            //Cube.fbx
                                            prop2.SetValue(comp,Resources.GetBuiltinResource<Mesh>(decoded.ObjScriptVars[j+1].Split("Default.Mesh.")[1].Replace(",",".")));
                                        } else if (decoded.ObjScriptVars[j+1].StartsWith("Default.Mat.")) {
                                            //Sprites-Default.mat
                                            //Default-Diffuse.mat
                                            prop2.SetValue(comp,GetMat(decoded.ObjScriptVars[j+1].Split("Default.Mat.")[1].Replace(",",".")));
                                        } else if (prop2.FieldType.ToString() == "UnityEngine.Vector3") {
                                            string[] split2 = decoded.ModScriptVars[j+1].Split("(")[1].Split(")")[0].Replace(" ","").Split(",");
                                            Vector3 vec = new Vector3(float.Parse(split2[0]),float.Parse(split2[1]),float.Parse(split2[2]));
                                            prop2.SetValue(comp,vec);
                                        } else if (prop2.FieldType.ToString() == "UnityEngine.CollisionDetectionMode") {
                                            prop2.SetValue(comp,Enum.Parse(typeof(CollisionDetectionMode), decoded.ObjScriptVars[j+1]));
                                        } else { prop2.SetValue(comp,decoded.ObjScriptVars[j+1]); }
                                    } else {
                                        if (decoded.ObjScriptVars[j+1].StartsWith("Statics.")) {
                                            prop.SetValue(comp,Statics.getValue(decoded.ObjScriptVars[j+1].Split("Statics.")[1]));
                                        } else if (decoded.ObjScriptVars[j+1].StartsWith("Default.Mesh.")) {
                                            //Cube.fbx
                                            prop.SetValue(comp,Resources.GetBuiltinResource<Mesh>(decoded.ObjScriptVars[j+1].Split("Default.Mesh.")[1].Replace(",",".")));
                                        } else if (decoded.ObjScriptVars[j+1].StartsWith("Default.Mat.")) {
                                            //Sprites-Default.mat
                                            prop.SetValue(comp,GetMat(decoded.ObjScriptVars[j+1].Split("Default.Mat.")[1].Replace(",",".")));
                                        } else if (prop.PropertyType.ToString() == "UnityEngine.Vector3") {
                                            string[] split2 = decoded.ModScriptVars[j+1].Split("(")[1].Split(")")[0].Replace(" ","").Split(",");
                                            Vector3 vec = new Vector3(float.Parse(split2[0]),float.Parse(split2[1]),float.Parse(split2[2]));
                                            prop.SetValue(comp,vec);
                                        } else if (prop.PropertyType.ToString() == "UnityEngine.CollisionDetectionMode") {
                                            prop.SetValue(comp,Enum.Parse(typeof(CollisionDetectionMode), decoded.ObjScriptVars[j+1]));
                                        } else { prop.SetValue(comp,decoded.ObjScriptVars[j+1]); }
                                    }
                                }

                                //end
                                if (j+1 < propAmount*2+index) { j++; }
                                else { break; }
                            }
                            index = j;
                        }
                        index++;
                    }
                }
            }
            
        } else if (decoded.MessageType == "Modify" && decoded.ObjFindName != null) {
            GameObject obj = GameObject.Find(decoded.ObjFindName);
            //change object name
            if (decoded.ObjName != null && decoded.ObjName != "")
            {
                obj.name = decoded.ObjName;
            }
            //change object parent
            if (decoded.ObjParent != null && decoded.ObjParent != "") {
                GameObject temp = GameObject.Find(decoded.ObjParent);
                if (temp != null) {
                    obj.transform.parent = temp.transform;
                }
            }
            //change position
            if (decoded.Pos != null && decoded.Pos != new Vector3(0.0114f,0,0))
            {
                obj.transform.localPosition = decoded.Pos;
            }
            //change scale
            if (decoded.Scale != null && decoded.Scale != new Vector3(0.0114f,0,0))
            {
                obj.transform.localScale = decoded.Scale;
            } else {}
            //change rotation
            if (decoded.Rot != null && decoded.Rot != new Vector3(0.0114f,0,0))
            {
                obj.transform.localEulerAngles = decoded.Rot;
            }
            //add scripts
            if (decoded.ObjScripts != null && decoded.ObjScripts.Length > 0)
            {
                int index = 1;
                for(int i = 0; i < decoded.ObjScripts.Length; i++) {
                    Type type = GetType(decoded.ObjScripts[i]);
                    Component comp = obj.AddComponent(type);
                    if (decoded.ObjScriptVars != null && decoded.ObjScriptVars.Length > index && decoded.ObjScriptVars[index] != null) {
                        if (int.Parse(decoded.ObjScriptVars[index-1]) > 0) {
                            int propAmount = int.Parse(decoded.ObjScriptVars[index-1]);
                            int j;
                            for (j = index; j < propAmount*2+index; j++) {
                                if (decoded.ObjScriptVars[j].EndsWith("]")) {
                                    //it is assigning a value in a list
                                    string[] split = decoded.ObjScriptVars[j].Split("[");
                                    PropertyInfo prop = type.GetProperty(split[0]);
                                    if (prop == null) {
                                        FieldInfo prop2 = type.GetField(split[0]);
                                        object[] value = (object[])prop2.GetValue(comp);
                                        if (decoded.ObjScriptVars[j+1].StartsWith("Statics.")) {
                                            value[int.Parse(split[1].Split("]")[0])] = Statics.getValue(decoded.ObjScriptVars[j+1].Split("Statics.")[1]);
                                        } else if (decoded.ObjScriptVars[j+1].StartsWith("Default.Mesh.")) {
                                            value[int.Parse(split[1].Split("]")[0])] = Resources.GetBuiltinResource<Mesh>(decoded.ObjScriptVars[j+1].Split("Default.Mesh.")[1].Replace(",","."));
                                        } else if (decoded.ObjScriptVars[j+1].StartsWith("Default.Mat.")) {
                                            value[int.Parse(split[1].Split("]")[0])] = GetMat(decoded.ObjScriptVars[j+1].Split("Default.Mat.")[1].Replace(",","."));
                                        } else if (prop.PropertyType.ToString() == "UnityEngine.Vector3") {
                                            string[] split2 = decoded.ModScriptVars[j+1].Split("(")[1].Split(")")[0].Replace(" ","").Split(",");
                                            Vector3 vec = new Vector3(float.Parse(split2[0]),float.Parse(split2[1]),float.Parse(split2[2]));
                                            value[int.Parse(split[1].Split("]")[0])] = vec;
                                        } else if (prop.PropertyType.ToString() == "UnityEngine.CollisionDetectionMode") {
                                            value[int.Parse(split[1].Split("]")[0])] = Enum.Parse(typeof(CollisionDetectionMode), decoded.ModScriptVars[j+1]);
                                        } else { print(prop2.FieldType.ToString()); value[int.Parse(split[1].Split("]")[0])] = decoded.ObjScriptVars[j+1]; }
                                        prop2.SetValue(comp,value);
                                    } else {
                                        object[] value = (object[])prop.GetValue(comp);
                                        if (decoded.ObjScriptVars[j+1].StartsWith("Statics.")) {
                                            value[int.Parse(split[1].Split("]")[0])] = Statics.getValue(decoded.ObjScriptVars[j+1].Split("Statics.")[1]);
                                        } else if (decoded.ObjScriptVars[j+1].StartsWith("Default.Mesh.")) {
                                            value[int.Parse(split[1].Split("]")[0])] = Resources.GetBuiltinResource<Mesh>(decoded.ObjScriptVars[j+1].Split("Default.Mesh.")[1].Replace(",","."));
                                        } else if (decoded.ObjScriptVars[j+1].StartsWith("Default.Mat.")) {
                                            value[int.Parse(split[1].Split("]")[0])] = GetMat(decoded.ObjScriptVars[j+1].Split("Default.Mat.")[1].Replace(",","."));
                                        } else if (prop.PropertyType.ToString() == "UnityEngine.Vector3") {
                                            string[] split2 = decoded.ModScriptVars[j+1].Split("(")[1].Split(")")[0].Replace(" ","").Split(",");
                                            Vector3 vec = new Vector3(float.Parse(split2[0]),float.Parse(split2[1]),float.Parse(split2[2]));
                                            value[int.Parse(split[1].Split("]")[0])] = vec;
                                        } else if (prop.PropertyType.ToString() == "UnityEngine.CollisionDetectionMode") {
                                            value[int.Parse(split[1].Split("]")[0])] = Enum.Parse(typeof(CollisionDetectionMode), decoded.ModScriptVars[j+1]);
                                        } else { print(prop.PropertyType.ToString()); value[int.Parse(split[1].Split("]")[0])] = decoded.ObjScriptVars[j+1]; }
                                        prop.SetValue(comp,value);
                                    }
                                } else {
                                    PropertyInfo prop = type.GetProperty(decoded.ObjScriptVars[j]);
                                    if (prop == null) {
                                        FieldInfo prop2 = type.GetField(decoded.ObjScriptVars[j]);
                                        if (decoded.ObjScriptVars[j+1].StartsWith("Statics.")) {
                                            prop2.SetValue(comp,Statics.getValue(decoded.ObjScriptVars[j+1].Split("Statics.")[1]));
                                        } else if (decoded.ObjScriptVars[j+1].StartsWith("Default.Mesh.")) {
                                            //Cube.fbx
                                            prop2.SetValue(comp,Resources.GetBuiltinResource<Mesh>(decoded.ObjScriptVars[j+1].Split("Default.Mesh.")[1].Replace(",",".")));
                                        } else if (decoded.ObjScriptVars[j+1].StartsWith("Default.Mat.")) {
                                            //Sprites-Default.mat
                                            //Default-Diffuse.mat
                                            prop2.SetValue(comp,GetMat(decoded.ObjScriptVars[j+1].Split("Default.Mat.")[1].Replace(",",".")));
                                        } else if (prop.PropertyType.ToString() == "UnityEngine.Vector3") {
                                            string[] split2 = decoded.ModScriptVars[j+1].Split("(")[1].Split(")")[0].Replace(" ","").Split(",");
                                            Vector3 vec = new Vector3(float.Parse(split2[0]),float.Parse(split2[1]),float.Parse(split2[2]));
                                            prop2.SetValue(comp,vec);
                                        } else if (prop.PropertyType.ToString() == "UnityEngine.CollisionDetectionMode") {
                                            prop2.SetValue(comp,Enum.Parse(typeof(CollisionDetectionMode), decoded.ModScriptVars[j+1]));
                                        } else { print(prop2.FieldType.ToString()); prop2.SetValue(comp,decoded.ObjScriptVars[j+1]); }
                                    } else {
                                        if (decoded.ObjScriptVars[j+1].StartsWith("Statics.")) {
                                            prop.SetValue(comp,Statics.getValue(decoded.ObjScriptVars[j+1].Split("Statics.")[1]));
                                        } else if (decoded.ObjScriptVars[j+1].StartsWith("Default.Mesh.")) {
                                            //Cube.fbx
                                            prop.SetValue(comp,Resources.GetBuiltinResource<Mesh>(decoded.ObjScriptVars[j+1].Split("Default.Mesh.")[1].Replace(",",".")));
                                        } else if (decoded.ObjScriptVars[j+1].StartsWith("Default.Mat.")) {
                                            //Sprites-Default.mat
                                            prop.SetValue(comp,GetMat(decoded.ObjScriptVars[j+1].Split("Default.Mat.")[1].Replace(",",".")));
                                        } else if (prop.PropertyType.ToString() == "UnityEngine.Vector3") {
                                            string[] split2 = decoded.ModScriptVars[j+1].Split("(")[1].Split(")")[0].Replace(" ","").Split(",");
                                            Vector3 vec = new Vector3(float.Parse(split2[0]),float.Parse(split2[1]),float.Parse(split2[2]));
                                            prop.SetValue(comp,vec);
                                        } else if (prop.PropertyType.ToString() == "UnityEngine.CollisionDetectionMode") {
                                            prop.SetValue(comp,Enum.Parse(typeof(CollisionDetectionMode), decoded.ModScriptVars[j+1]));
                                        } else { print(prop.PropertyType.ToString()); prop.SetValue(comp,decoded.ObjScriptVars[j+1]); }
                                    }
                                }

                                //end
                                if (j+1 < propAmount*2+index) { j++; }
                                else { break; }
                            }
                            index = j;
                        }
                        index++;
                    }
                }
            }
            if (decoded.ModScriptVars != null && decoded.ModScriptVars.Length > 0)
            {
                int index = 0;
                while (decoded.ModScriptVars.Length > index && decoded.ModScriptVars[index] != null) {
                    string name = decoded.ModScriptVars[index];
                    Type type = GetType(name);
                    Component comp = obj.GetComponent(type);
                    int numVars = int.Parse(decoded.ModScriptVars[index+1]);
                    //print(decoded.ModScriptVars[index]);
                    //print(decoded.ModScriptVars[index+1]);
                    index += 2;
                    int j;
                    for(j = index; j < index+numVars*2-1; j++) {
                        if (decoded.ModScriptVars[j].EndsWith("]")) {
                            //it is assigning a value in a list
                            string[] split = decoded.ModScriptVars[j].Split("[");
                            PropertyInfo prop = type.GetProperty(split[0]);
                            if (prop == null) {
                                FieldInfo prop2 = type.GetField(split[0]);
                                object[] value = (object[])prop2.GetValue(comp);
                                if (decoded.ModScriptVars[j+1].StartsWith("Statics.")) {
                                    value[int.Parse(split[1].Split("]")[0])] = Statics.getValue(decoded.ModScriptVars[j+1].Split("Statics.")[1]);
                                } else if (decoded.ModScriptVars[j+1].StartsWith("Default.Mesh.")) {
                                    value[int.Parse(split[1].Split("]")[0])] = Resources.GetBuiltinResource<Mesh>(decoded.ModScriptVars[j+1].Split("Default.Mesh.")[1].Replace(",","."));
                                } else if (decoded.ModScriptVars[j+1].StartsWith("Default.Mat.")) {
                                    value[int.Parse(split[1].Split("]")[0])] = GetMat(decoded.ModScriptVars[j+1].Split("Default.Mat.")[1].Replace(",","."));
                                } else if (prop2.FieldType.ToString() == "UnityEngine.Vector3") {
                                    string[] split2 = decoded.ModScriptVars[j+1].Split("(")[1].Split(")")[0].Replace(" ","").Split(",");
                                    Vector3 vec = new Vector3(float.Parse(split2[0]),float.Parse(split2[1]),float.Parse(split2[2]));
                                    value[int.Parse(split[1].Split("]")[0])] = vec;
                                } else if (prop2.FieldType.ToString() == "UnityEngine.CollisionDetectionMode") {
                                    value[int.Parse(split[1].Split("]")[0])] = Enum.Parse(typeof(CollisionDetectionMode), decoded.ModScriptVars[j+1]);
                                } else { print(prop2.FieldType.ToString()); value[int.Parse(split[1].Split("]")[0])] = decoded.ModScriptVars[j+1]; }
                                prop2.SetValue(comp,value);
                            } else {
                                object[] value = (object[])prop.GetValue(comp);
                                if (decoded.ModScriptVars[j+1].StartsWith("Statics.")) {
                                    value[int.Parse(split[1].Split("]")[0])] = Statics.getValue(decoded.ModScriptVars[j+1].Split("Statics.")[1]);
                                } else if (decoded.ModScriptVars[j+1].StartsWith("Default.Mesh.")) {
                                    value[int.Parse(split[1].Split("]")[0])] = Resources.GetBuiltinResource<Mesh>(decoded.ModScriptVars[j+1].Split("Default.Mesh.")[1].Replace(",","."));
                                } else if (decoded.ModScriptVars[j+1].StartsWith("Default.Mat.")) {
                                    value[int.Parse(split[1].Split("]")[0])] = GetMat(decoded.ModScriptVars[j+1].Split("Default.Mat.")[1].Replace(",","."));
                                } else if (prop.PropertyType.ToString() == "UnityEngine.Vector3") {
                                    string[] split2 = decoded.ModScriptVars[j+1].Split("(")[1].Split(")")[0].Replace(" ","").Split(",");
                                    Vector3 vec = new Vector3(float.Parse(split2[0]),float.Parse(split2[1]),float.Parse(split2[2]));
                                    value[int.Parse(split[1].Split("]")[0])] = vec;
                                } else if (prop.PropertyType.ToString() == "UnityEngine.CollisionDetectionMode") {
                                    value[int.Parse(split[1].Split("]")[0])] = Enum.Parse(typeof(CollisionDetectionMode), decoded.ModScriptVars[j+1]);
                                } else { print(prop.PropertyType.ToString()); value[int.Parse(split[1].Split("]")[0])] = decoded.ModScriptVars[j+1]; }
                                prop.SetValue(comp,value);
                            }
                        } else {
                            PropertyInfo prop = type.GetProperty(decoded.ModScriptVars[j]);
                            if (prop == null) {
                                FieldInfo prop2 = type.GetField(decoded.ModScriptVars[j]);
                                if (decoded.ModScriptVars[j+1].StartsWith("Statics.")) {
                                    prop2.SetValue(comp,Statics.getValue(decoded.ModScriptVars[j+1].Split("Statics.")[1]));
                                } else if (decoded.ModScriptVars[j+1].StartsWith("Default.Mesh.")) {
                                    //Cube.fbx
                                    prop2.SetValue(comp,Resources.GetBuiltinResource<Mesh>(decoded.ModScriptVars[j+1].Split("Default.Mesh.")[1].Replace(",",".")));
                                } else if (decoded.ModScriptVars[j+1].StartsWith("Default.Mat.")) {
                                    //Sprites-Default.mat
                                    //Default-Diffuse.mat
                                    prop2.SetValue(comp,GetMat(decoded.ModScriptVars[j+1].Split("Default.Mat.")[1].Replace(",",".")));
                                } else if (prop2.FieldType.ToString() == "UnityEngine.Vector3") {
                                    string[] split2 = decoded.ModScriptVars[j+1].Split("(")[1].Split(")")[0].Replace(" ","").Split(",");
                                    Vector3 vec = new Vector3(float.Parse(split2[0]),float.Parse(split2[1]),float.Parse(split2[2]));
                                    prop2.SetValue(comp,vec);
                                } else if (prop2.FieldType.ToString() == "UnityEngine.CollisionDetectionMode") {
                                    prop2.SetValue(comp,Enum.Parse(typeof(CollisionDetectionMode), decoded.ModScriptVars[j+1]));
                                } else { print(prop2.FieldType.ToString()); prop2.SetValue(comp,decoded.ModScriptVars[j+1]); }
                            } else {
                                if (decoded.ModScriptVars[j+1].StartsWith("Statics.")) {
                                    prop.SetValue(comp,Statics.getValue(decoded.ModScriptVars[j+1].Split("Statics.")[1]));
                                } else if (decoded.ModScriptVars[j+1].StartsWith("Default.Mesh.")) {
                                    //Cube.fbx
                                    prop.SetValue(comp,Resources.GetBuiltinResource<Mesh>(decoded.ModScriptVars[j+1].Split("Default.Mesh.")[1].Replace(",",".")));
                                } else if (decoded.ModScriptVars[j+1].StartsWith("Default.Mat.")) {
                                    //Sprites-Default.mat
                                    prop.SetValue(comp,GetMat(decoded.ModScriptVars[j+1].Split("Default.Mat.")[1].Replace(",",".")));
                                } else if (prop.PropertyType.ToString() == "UnityEngine.Vector3") {
                                    string[] split2 = decoded.ModScriptVars[j+1].Split("(")[1].Split(")")[0].Replace(" ","").Split(",");
                                    Vector3 vec = new Vector3(float.Parse(split2[0]),float.Parse(split2[1]),float.Parse(split2[2]));
                                    prop.SetValue(comp,vec);
                                } else if (prop.PropertyType.ToString() == "UnityEngine.CollisionDetectionMode") {
                                    prop.SetValue(comp,Enum.Parse(typeof(CollisionDetectionMode), decoded.ModScriptVars[j+1]));
                                } else { print(prop.PropertyType.ToString()); prop.SetValue(comp,decoded.ModScriptVars[j+1]); }
                            }
                        }
                        j++;
                    }
                    index = j+1;
                }
            }
        }
        Proccess();
    }
    public static Type GetType(string TypeName)
    {
        // Try Type.GetType() first. This will work with types defined
        // by the Mono runtime, in the same assembly as the caller, etc.
        Type type = Type.GetType(TypeName);
        // If it worked, then we're done here
        if (type != null) return type;
        // If the TypeName is a full name, then we can try loading the defining assembly directly
        if( TypeName.Contains( "." ) ) {
            // Get the name of the assembly (Assumption is that we are using 
            // fully-qualified type names)
            var assemblyName = TypeName.Substring( 0, TypeName.IndexOf( '.' ) );

            // Attempt to load the indicated Assembly
            Assembly assembly = Assembly.Load(assemblyName);
            if (assembly == null) return null;
            // Ask that assembly to return the proper Type
            type = assembly.GetType(TypeName);
            if (type != null) return type;
        }
        // If we still haven't found the proper type, we can enumerate all of the
        // loaded assemblies and see if any of them define the type
        Assembly currentAssembly = Assembly.GetExecutingAssembly();
        AssemblyName[] referencedAssemblies = currentAssembly.GetReferencedAssemblies();
        foreach (AssemblyName assemblyName in referencedAssemblies)
        {
            //print(assemblyName);
            // Load the referenced assembly
            Assembly assembly2 = Assembly.Load(assemblyName);
            if (assembly2 != null)
            {
                // See if that assembly defines the named type
                type = assembly2.GetType(TypeName);
                if (type != null) return type;
            }
        }
        // The type just couldn't be found...
        return null;
    }
    public static Material GetMat(string name)
    {
        #if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>(name);
        #else
            return Resources.GetBuiltinResource<Material>(name);
        #endif
    }
}
