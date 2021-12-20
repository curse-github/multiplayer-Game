//https://www.google.com/search?q=c%23+websocket+client&oq=c%23+websocket+client&aqs=chrome..69i57j69i58.7267j0j1&sourceid=chrome&ie=UTF-8
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System;

public class ServerReceiver : MonoBehaviour
{
    private static ServerReceiver _instance;
    public static ServerReceiver Instance { get { return _instance; } }
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
        Application.runInBackground = true;
        Application.targetFrameRate = 120;
        QualitySettings.vSyncCount = 1;
    }

    public List<string> toProcess = new List<string>();
    public bool isProcessing = false;
    private List<GameObject> objs = new List<GameObject>();
    private List<string> objNames = new List<string>();

    public void Proccess(string message) {
        toProcess.Add(message);
        Proccess(true);
    }
    public void Proccess() {
        Proccess(false);
    }
    public void Proccess(bool overrideProccessing) {
        string notDecoded = "";
        MessageData decoded = new MessageData();
        try {
            if (toProcess.Count <= 0 || (isProcessing && !overrideProccessing)) { return; }
            isProcessing = true;
            notDecoded = toProcess[0];
            decoded = MessageData.decodeMessage(notDecoded);
            toProcess.RemoveAt(0);
            if (decoded == null) { print(notDecoded); return; }
            //print(notDecoded);
            if (decoded.MessageType == null) { return; }
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
                if (decoded.Pos != null && decoded.Pos != new Vector3(0.0114f,0,0))
                {
                    obj.transform.localPosition = decoded.Pos;
                } else { obj.transform.localPosition = new Vector3(0,0,0); }
                //change scale
                if (decoded.Scale != null && decoded.Scale != new Vector3(0.0114f,0,0))
                {
                    obj.transform.localScale = decoded.Scale;
                } else { obj.transform.localScale = new Vector3(1,1,1); }
                //change rotation
                if (decoded.Rot != null && decoded.Rot != new Vector3(0.0114f,0,0))
                {
                    obj.transform.localEulerAngles = decoded.Rot;
                } else { obj.transform.localEulerAngles = new Vector3(0,0,0); }
                //add scripts
                if (decoded.ObjScripts != null && decoded.ObjScripts.Length > 0)
                {
                    for(int i = 0; i < decoded.ObjScripts.Length; i++) {
                        Type type = GetType(decoded.ObjScripts[i]);
                        obj.AddComponent(type);
                    }
                }
                if (decoded.ModScriptVars != null && decoded.ModScriptVars.Length > 0)
                {
                    int index = 0;
                    while (decoded.ModScriptVars.Length > index && decoded.ModScriptVars[index] != null) {
                        string name = decoded.ModScriptVars[index];
                        int numVars = int.Parse(decoded.ModScriptVars[index+1]);
                        if (name == "UnityEngine.GameObject" || name == "GameObject") {
                            index += 2;
                            int l;
                            for(l = index; l < index+numVars*2-1; l++) {
                                if (decoded.ModScriptVars[l].EndsWith("]")) {
                                    //it is assigning a value in a list
                                    string[] split = decoded.ModScriptVars[l].Split("[");
                                    PropertyInfo prop = typeof(GameObject).GetProperty(split[0]);
                                    if (prop == null) {
                                        FieldInfo prop2 = typeof(GameObject).GetField(split[0]);
                                        object[] value = (object[])prop2.GetValue(obj);
                                        value[int.Parse(split[1].Split("]")[0])] = stringToObj(decoded.ModScriptVars[l+1],prop2.FieldType);
                                        prop2.SetValue(obj,value);
                                    } else {
                                        object[] value = (object[])prop.GetValue(obj);
                                        value[int.Parse(split[1].Split("]")[0])] = stringToObj(decoded.ModScriptVars[l+1],prop.PropertyType);
                                        prop.SetValue(obj,value);
                                    }
                                } else {
                                    PropertyInfo prop = typeof(GameObject).GetProperty(decoded.ModScriptVars[l]);
                                    if (prop == null) {
                                        FieldInfo prop2 = typeof(GameObject).GetField(decoded.ModScriptVars[l]);
                                        prop2.SetValue(obj,stringToObj(decoded.ModScriptVars[l+1],prop2.FieldType));
                                    } else {
                                        prop.SetValue(obj,stringToObj(decoded.ModScriptVars[l+1],prop.PropertyType));
                                    }
                                }
                                l++;
                            }
                            index = l;
                            continue;
                        }
                        Type type = GetType(name);
                        Component comp = obj.GetComponent(type);
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
                                    value[int.Parse(split[1].Split("]")[0])] = stringToObj(decoded.ModScriptVars[j+1],prop2.FieldType);
                                    prop2.SetValue(comp,value);
                                } else {
                                    object[] value = (object[])prop.GetValue(comp);
                                    value[int.Parse(split[1].Split("]")[0])] = stringToObj(decoded.ModScriptVars[j+1],prop.PropertyType);
                                    prop.SetValue(comp,value);
                                }
                            } else {
                                PropertyInfo prop = type.GetProperty(decoded.ModScriptVars[j]);
                                if (prop == null) {
                                    FieldInfo prop2 = type.GetField(decoded.ModScriptVars[j]);
                                    prop2.SetValue(comp,stringToObj(decoded.ModScriptVars[j+1],prop2.FieldType));
                                } else {
                                    prop.SetValue(comp,stringToObj(decoded.ModScriptVars[j+1],prop.PropertyType));
                                }
                            }
                            j++;
                        }
                        index = j;
                    }
                }
            } else if (decoded.MessageType == "Modify" && decoded.ObjFindName != null)
            {
                if (objNames.IndexOf(decoded.ObjFindName) == -1) {
                    //print("adding " + decoded.ObjFindName + " to array.");
                    objNames.Add(decoded.ObjFindName);
                    objs.Add(GameObject.Find(decoded.ObjFindName));
                }// else { print("had " + decoded.ObjFindName + " on array already."); }
                GameObject obj = objs[objNames.IndexOf(decoded.ObjFindName)];
                //change object name
                if (decoded.ObjName != null && decoded.ObjName != "" && decoded.ObjName != obj.name)
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
                NetworkObj thing = obj.GetComponent<NetworkObj>();
                //change position
                if (decoded.Pos != null && decoded.Pos != new Vector3(0.0114f,0,0))
                {
                    obj.transform.localPosition = decoded.Pos;
                    if (thing != null) {
                        thing.oldPos = decoded.Pos;
                    }
                }
                //change scale
                if (decoded.Scale != null && decoded.Scale != new Vector3(0.0114f,0,0))
                {
                    obj.transform.localScale = decoded.Scale;
                    MessageData data2 = new MessageData();
                    data2.MessageType = "1";
                    data2.ObjFindName = obj.name;
                    data2.Scale = decoded.Scale;
                    WebsocketHandler.Instance.send(data2);
                }
                //change rotation
                if (decoded.Rot != null && decoded.Rot != new Vector3(0.0114f,0,0))
                {
                    obj.transform.localEulerAngles = decoded.Rot;
                    if (thing != null) {
                        thing.oldRot = decoded.Rot;
                    }
                }
                //add scripts
                if (decoded.ObjScripts != null && decoded.ObjScripts.Length > 0)
                {
                    for(int i = 0; i < decoded.ObjScripts.Length; i++) {
                        Type type = GetType(decoded.ObjScripts[i]);
                        obj.AddComponent(type);
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
                                    value[int.Parse(split[1].Split("]")[0])] = stringToObj(decoded.ModScriptVars[j+1],prop2.FieldType);
                                    prop2.SetValue(comp,value);
                                } else {
                                    object[] value = (object[])prop.GetValue(comp);
                                    value[int.Parse(split[1].Split("]")[0])] = stringToObj(decoded.ModScriptVars[j+1],prop.PropertyType);
                                    prop.SetValue(comp,value);
                                }
                            } else {
                                PropertyInfo prop = type.GetProperty(decoded.ModScriptVars[j]);
                                if (prop == null) {
                                    FieldInfo prop2 = type.GetField(decoded.ModScriptVars[j]);
                                    prop2.SetValue(comp,stringToObj(decoded.ModScriptVars[j+1],prop2.FieldType));
                                } else {
                                    prop.SetValue(comp,stringToObj(decoded.ModScriptVars[j+1],prop.PropertyType));
                                }
                            }
                            j++;
                        }
                        index = j;
                    }
                }
            } else if ((decoded.MessageType == "Delete" || decoded.MessageType == "Remove" || decoded.MessageType == "Destroy") && decoded.ObjFindName != null) {
                if (objNames.IndexOf(decoded.ObjFindName) == -1) {
                    //print("gameobject " + decoded.ObjFindName + " was not in array.");
                    GameObject obj = GameObject.Find(decoded.ObjFindName);
                    GameObject.Destroy(obj);
                } else {
                    print("had gameobject " + decoded.ObjFindName + " in array.");
                    GameObject obj = objs[objNames.IndexOf(decoded.ObjFindName)];
                    GameObject.Destroy(obj);
                    objs.RemoveAt(objNames.IndexOf(decoded.ObjFindName));
                    objNames.RemoveAt(objNames.IndexOf(decoded.ObjFindName));
                }
            } else if (decoded.MessageType == "ping") {
                WebsocketHandler.Instance.sendnow("{\"MessageType\":\"pong\",\"ObjName\":\"" + decoded.ObjName + "\"}");
            }
            if (!overrideProccessing) { isProcessing = false; }
        } catch (Exception e) {
            print(decoded.MessageType);
            print(decoded.ObjName);
            print(decoded.ObjFindName);
            Debug.LogError(e);
            print(notDecoded);
        }
    }
    public void proccessList() {
        isProcessing = true;
        List<string> list = new List<string>();
        string thing = toProcess[0].Remove(0, "{\"list\":[{\"obj\":".Length);
        thing = thing.Remove(thing.Length-3, 3);
        string dupe = thing;
        toProcess.RemoveAt(0);

        int bracketCount = 0;
        string tempString = "";
        for (int i = 0; i < thing.Length; i++) {
            char c = thing[i];
            tempString += c;
            if (c == '{') {
                bracketCount++;
            } else if (c == '}') {
                bracketCount--;
                if (bracketCount == 0) {
                    toProcess.Add(tempString);
                    tempString = "";
                    i+=",{\"obj\":".Length+1;
                    continue;
                }
            }
        }
        isProcessing = false;
    }
    private void Update()
    {
        if (!isProcessing && toProcess.Count > 0) {
            //print(toProcess[0]);
            if (toProcess[0].StartsWith("{\"list\":[")) {
                proccessList();
            } else { Proccess(); }
        }
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
        foreach (Material mat in Resources.FindObjectsOfTypeAll(typeof(Material)) as Material[])
        {
            if (mat.name == name) { return mat; }
        }
        return null;
    }
    public object stringToObj(string one, Type propType) {
        if (one.StartsWith("Statics.")) {
            return Statics.getValue(one.Split("Statics.")[1]);
        } else if (one.StartsWith("Default.Mesh.")) {
            //Cube.fbx
            //Capsule.fbx
            return Resources.GetBuiltinResource<Mesh>(one.Split("Default.Mesh.")[1].Replace(",","."));
        } else if (one.StartsWith("Default.Mat.")) {
            //Sprites-Default.mat
            //Default-Diffuse.mat
            return GetMat(one.Split("Default.Mat.")[1]);
        } else if (one.StartsWith("Create.Mat.")) {
            //Sprites-Default.mat
            //Default-Diffuse.mat
            string color = one.Split("Create.Mat.")[1];
            Color color2 = Color.white;
            Material material = new Material(GetMat("Default-Diffuse").shader);
            if (color == "Color,black") {
                color2 = Color.black;
            } else if (color == "Color,blue") {
                color2 = Color.blue;
            } else if (color == "Color,cyan") {
                color2 = Color.cyan;
            } else if (color == "Color,gray") {
                color2 = Color.gray;
            } else if (color == "Color,grey") {
                color2 = Color.grey;
            } else if (color == "Color,green") {
                color2 = Color.green;
            } else if (color == "Color,magenta") {
                color2 = Color.magenta;
            } else if (color == "Color,red") {
                color2 = Color.red;
            } else if (color == "Color,white") {
                color2 = Color.white;
            } else if (color == "Color,yellow") {
                color2 = Color.yellow;
            }
            material.SetColor("_Color", color2);
            return material;
        } else if (propType.ToString() == "UnityEngine.Vector3") {
            string[] split2 = one.Split("(")[1].Split(")")[0].Replace(" ","").Split(",");
            return new Vector3(float.Parse(split2[0]),float.Parse(split2[1]),float.Parse(split2[2]));
        } else if (propType.ToString() == "UnityEngine.CollisionDetectionMode") {
            return Enum.Parse(typeof(CollisionDetectionMode), one);
        } else if (propType.ToString() == "System.String") {
            return one;
        } else if (propType.ToString() == "System.Int32") {
            return int.Parse(one);
        } else if (propType.ToString() == "System.Single") {
            return float.Parse(one);
        } else if (propType.ToString() == "System.Double") {
            return double.Parse(one);
        } else if (propType.ToString() == "System.Boolean") {
            return bool.Parse(one);
        } else {
            print("Unsupported type: " + propType.ToString());
            return null;
        }
    }
}
