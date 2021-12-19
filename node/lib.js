class MessageData {
    constructor(MessageType, ObjName, ObjParent, Pos, Scale, Rot, ObjFindName, ObjScripts, ModScriptVars) {
        this.obj = {};
        this.obj.MessageType = MessageType;
        if (ObjName != null) { this.obj.ObjName = ObjName; }
        if (ObjParent != null) { this.obj.ObjParent = ObjParent; }
        if (Pos != null) {
            this.obj.Pos =      { "x":Pos.x, "y":Pos.y, "z":Pos.z };
            this.obj.startPos = { "x":Pos.x, "y":Pos.y, "z":Pos.z };
        } else {
            this.obj.Pos =      { "x":0, "y":0, "z":0 };
            this.obj.startPos = { "x":0, "y":0, "z":0 };
        }
        if (Scale != null) {
            this.obj.Scale = { "x":Scale.x, "y":Scale.y, "z":Scale.z };
        }
        if (Rot != null) {
            this.obj.Rot = { "x":Rot.x, "y":Rot.y, "z":Rot.z };
        }
        this.obj.ObjFindName = ObjFindName;
        this.obj.ObjScripts = ObjScripts;
        this.obj.ModScriptVars = ModScriptVars;
    }
    get startPos() {
        return new Vector3(this.obj.startPos.x,this.obj.startPos.y,this.obj.startPos.z);
    }
    set startPos(val) {
        this.obj.startPos = Vector3.fromObject(val);
    }
    set Pos(vec) {
        this.obj.Pos = {
            "x":vec.x,
            "y":vec.y,
            "z":vec.z
        };
    }
    set Scale(vec) {
        this.obj.Scale = {
            "x":vec.x,
            "y":vec.y,
            "z":vec.z
        };
    }
    set Rot(vec) {
        this.obj.Rot = {
            "x":vec.x,
            "y":vec.y,
            "z":vec.z
        };
    }
    addModVars(lst) {
        for(var i = 0; i < lst.length; i++) {
            this.obj.ModScriptVars.push(lst[i]);
        }
    }
    Send(ws) {
        try {
            if (ws != null) {
                ws.send(JSON.stringify(this.obj));
            }
        } catch (error) { console.log(error.data) }
    }
    static Cube(Name, Pos, Scale, Rot) {
        var thing = new MessageData("Create",Name, null, null, null, null, null, [
            "UnityEngine.MeshFilter",
            "UnityEngine.MeshRenderer",
            "UnityEngine.BoxCollider"
        ], [
            "UnityEngine.MeshFilter","1","mesh","Default.Mesh.Cube,fbx",
            "UnityEngine.MeshRenderer","1","materials[0]","Default.Mat.Default-Diffuse",
        ]);
        if (Pos != null) { thing.Pos = Pos; }
        if (Scale != null) { thing.Scale = Scale; }
        if (Rot != null) { thing.Rot = Rot; }
        return thing;
    }
    static RigidCube(Name, Pos, Scale, Rot) {
        var thing = new MessageData("Create",Name, null, null, null, null, null, [
            "UnityEngine.MeshFilter",
            "UnityEngine.MeshRenderer",
            "UnityEngine.BoxCollider",
            "UnityEngine.Rigidbody",
            "NetworkObj"
        ], [
            "UnityEngine.MeshFilter","1","mesh","Default.Mesh.Cube,fbx",
            "UnityEngine.MeshRenderer","1","materials[0]","Default.Mat.Default-Diffuse"
        ]);
        if (Pos != null) { thing.Pos = Pos; }
        if (Scale != null) {
            if (typeof(Scale) == "object") { thing.Scale = Scale; }
            else if (typeof(Scale) == "number") { thing.Scale = new Vector3(Scale,Scale,Scale); }
        }
        if (Rot != null) { thing.Rot = Rot; }
        return thing;
    }
    static Capsule(Name, Pos, Scale, Rot) {
        var thing = new MessageData("Create",(Name != null ? Name : ""), null, null, null, null, null, [
            "UnityEngine.MeshFilter",
            "UnityEngine.MeshRenderer",
            "UnityEngine.CapsuleCollider"
        ], [
            "UnityEngine.MeshFilter","1","mesh","Default.Mesh.Capsule,fbx",
            "UnityEngine.MeshRenderer","1","materials[0]","Default.Mat.Default-Diffuse",
        ]);
        if (Pos != null) { thing.Pos = Pos; }
        if (Scale != null) {
            if (typeof(Scale) == "object") { thing.Scale = Scale; }
            else if (typeof(Scale) == "number") { thing.Scale = new Vector3(Scale,Scale,Scale); }
        }
        if (Rot != null) { thing.Rot = Rot; }
        return thing;
    }
}
class Vector3 {
    constructor(x,y,z) {
        if(typeof(x) == "number") {
            this.x = x;
            this.y = y;
            this.z = z;
        } else if (typeof(x) == "string") {
            if (Number(x) == null) {
                var split = x.split("(")[1].split(")")[0].replaceall(" ","").split(",");
                this.x = split[0];
                this.y = split[1];
                this.z = split[2];
            } else {
                this.x = Number(x);
                this.y = Number(y);
                this.z = Number(z);
            }
        }
    }
    set x(val){ this.x2 = val }
    set y(val){ this.y2 = val }
    set z(val){ this.z2 = val }
    get x() { return this.x2; }
    get y() { return this.y2; }
    get z() { return this.z2; }

    static Zero() {
        return new Vector3(0,0,0);
    }
    static fromObject(obj) {
        try {
            if (obj.x != 0.01140000019222498) {
                return new Vector3(obj.x,obj.y,obj.z);
            }
            return null;
        } catch (error) { console.log("Error parsing Vector3."); console.log(JSON.stringify(obj)); return null; }
    }
    ToString() {
        return "(" + this.x + "," + this.y + "," + this.z + ")";
    }
}
class PlayerData {
    constructor(x,y,z,rx,ry,rz) {
        if (typeof(x) == "number" && typeof(y) == "number") {
            this.Pos = new Vector3(x,y,z);
            this.Rot = new Vector3(rx,ry,rz);
        } else if (typeof(x) == "object" && typeof(y) == "object") {
            this.Pos = x;
            this.Rot = y;
        }
    }
    set Pos(val) {
        if (typeof(val) == "object") { this.Pos2 = val; }
        else if (typeof(val) == "string") {
            var split = val.split("(")[1].split(")")[0].replaceall(" ","").split(",");
            this.Pos2 = new Vector3(split[0],split[1],split[2]);
        }
    }
    get Pos() { return this.Pos2 }
    set Rot(val) {
        if (typeof(val) == "object") { this.Rot2 = val; }
        else if (typeof(val) == "string") {
            var split = val.split("(")[1].split(")")[0].replaceall(" ","").split(",");
            this.Rot2 = new Vector3(split[0],split[1],split[2]);
        }
    }
    get Rot() { return this.Rot2 }
    set x(val){ this.Pos2.x = val }
    set y(val){ this.Pos2.y = val }
    set z(val){ this.Pos2.z = val }
    get x(){ return this.Pos2.x }
    get y(){ return this.Pos2.y }
    get z(){ return this.Pos2.z }
    set rx(val){ this.Rot2.x = val }
    set ry(val){ this.Rot2.y = val }
    set rz(val){ this.Rot2.z = val }
    get rx(){ return this.Rot2.x }
    get ry(){ return this.Rot2.y }
    get rz(){ return this.Rot2.z }
}
String.prototype.replaceall = function replaceall(two,three) {
    var temp = this;
    if (!three.includes(two)) {
        while(temp.includes(two)) {
            temp = temp.replace(two,three);
        }
    }
    return temp;
}
function distance(p1,p2) {
    var dx = p1.x-p2.x;
    var dy = p1.y-p2.y;
    var dz = p1.z-p2.z;
    return Math.sqrt((dx*dx)+(dy*dy)+(dz*dz));
}
module.exports = { MessageData, Vector3, PlayerData, distance }