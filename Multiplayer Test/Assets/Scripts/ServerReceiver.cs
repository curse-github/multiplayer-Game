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
    }

    public List<string> toProcess = new List<string>();
    public bool isProcessing = false;

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
            } else if (decoded.MessageType == "Modify" && decoded.ObjFindName != null)
            {
                GameObject obj = GameObject.Find(decoded.ObjFindName);
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
                    if (thing != null) {
                        thing.oldSca = decoded.Scale;
                    }
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
                GameObject obj = GameObject.Find(decoded.ObjFindName);
                GameObject.Destroy(obj);
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
        } else if (propType.ToString() == "UnityEngine.Vector3") {
            string[] split2 = one.Split("(")[1].Split(")")[0].Replace(" ","").Split(",");
            return new Vector3(float.Parse(split2[0]),float.Parse(split2[1]),float.Parse(split2[2]));
        } else if (propType.ToString() == "UnityEngine.CollisionDetectionMode") {
            //doesnt work :(
            //print()//(typeof(Enum)).GetTypeInfo());
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
