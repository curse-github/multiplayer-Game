//download node.js
//download yarn
//cmd: yarn init
//cmd: yarn ws

const { listenerCount } = require('ws');
const WebSocket = require('ws')
const lib = require("./lib");

var startingPos = new lib.Vector3(0, 2, 0);

var scenes = [{"statics":[],"dynamics":[],"dynamicCount":0,"dynamicNames":[]}];
//static objects
var floor = lib.MessageData.Cube();
floor.obj.ObjName = "Floor";
floor.Pos = new lib.Vector3(0, -0.125, 0);
floor.Scale = new lib.Vector3(50, 0.25, 50);
floor.Rot = new lib.Vector3(0, 45, 0);
scenes[0].statics.push(floor);
var size = 8
var sizem1 = size-1;
scenes[0].statics.push(lib.MessageData.Cube("Ramp1P1", new lib.Vector3(0, sizem1/2-(Math.sqrt(2)/4), -10-sizem1/2-Math.sqrt(2)/4), new lib.Vector3(sizem1,1,Math.sqrt(sizem1*sizem1+sizem1*sizem1)), new lib.Vector3(45, 0, 0)));
scenes[0].statics.push(lib.MessageData.Cube("Ramp1P2", new lib.Vector3(0, sizem1/2, -10-sizem1-0.5), new lib.Vector3(sizem1,sizem1,1), new lib.Vector3(0,0,0)));
//dynamic objects
scenes[0].dynamics.push(lib.MessageData.RigidCube("Cube1", new lib.Vector3(15, 1, 15),    2, new lib.Vector3(0, 45, 0)));
scenes[0].dynamics.push(lib.MessageData.RigidCube("Cube2", new lib.Vector3(15, 1.5, -15), 3, new lib.Vector3(0, 45, 0)));
scenes[0].dynamics.push(lib.MessageData.RigidCube("Cube3", new lib.Vector3(-15, 1, -15),  2, new lib.Vector3(0, 45, 0)));
scenes[0].dynamics.push(lib.MessageData.RigidCube("Cube4", new lib.Vector3(-15, 1.5, 15), 3, new lib.Vector3(0, 45, 0)));
scenes[0].dynamics[scenes[0].dynamics.length-4].obj.ModScriptVars[7] == "Default.Mat.Sprites-Default";
scenes[0].dynamics[scenes[0].dynamics.length-3].obj.ModScriptVars[7] == "Default.Mat.Sprites-Default";
scenes[0].dynamics[scenes[0].dynamics.length-2].obj.ModScriptVars[7] == "Default.Mat.Sprites-Default";
scenes[0].dynamics[scenes[0].dynamics.length-1].obj.ModScriptVars[7] == "Default.Mat.Sprites-Default";
scenes[0].dynamicNames["Cube1"] = 0
scenes[0].dynamicNames["Cube2"] = 1
scenes[0].dynamicNames["Cube3"] = 2
scenes[0].dynamicNames["Cube4"] = 3
scenes[0].dynamicCount = 4

var Pinging = false;
var wsS = [];
var playerAlive = [];
var playerindexes = [];

var ObjectIndex = 0;

const socketport = "53586";
const wss = new WebSocket.Server({ port: socketport })
wss.on('connection', websocket => {
    var playerid = null;
    for (var i = 0; i < wsS.length+1; i++) {
        if (wsS[i] == null) { wsS[i] = websocket; playerid = i; console.log("player" + (i+1) + " joined!"); break; }
    }
    startingPos.y += 4;

    var scene = 0;
    //for (var i = 0; i < scenes[scene].statics.length; i++) { scenes[scene].statics[i].Send(websocket); }
    var SendList = {"list":[]};
    for (var i = 0; i < scenes[scene].statics.length; i++) { SendList.list.push(scenes[scene].statics[i]); }
    for (var i = 0; i < scenes[scene].dynamics.length; i++) {
        if (scenes[scene].dynamics[i] != null) {
            SendList.list.push(scenes[scene].dynamics[i]);
        }
    }
    websocket.send(JSON.stringify(SendList));

    //Create Player Network Object
    var PlayerDyn = lib.MessageData.Capsule("Player" + playerid, startingPos);
    PlayerDyn.addModVars([
        "UnityEngine.CapsuleCollider", "2", "height", "4", "radius", "1"
    ]);
    scenes[scene].dynamics.push(PlayerDyn);
    var key = scenes[scene].dynamics[scenes[scene].dynamics.length-1].obj.ObjName;
    scenes[scene].dynamicNames[key] = scenes[scene].dynamicCount;
    playerindexes[playerid] = scenes[scene].dynamicCount;

    //Set Camera Network Object
    var Camera = new lib.MessageData("Create","Main Camera", "Player" + playerid, new lib.Vector3(0, 1, 0), null, null, null, null, null);
    scenes[scene].dynamics.push(Camera);
    key += "/" + scenes[scene].dynamics[scenes[scene].dynamics.length-1].obj.ObjName;
    scenes[scene].dynamicNames[key] = scenes[scene].dynamicCount+1;
    //Set Bbl Network Object
    var Bbl = lib.MessageData.Capsule("Bbl", new lib.Vector3(0, 0, 0.75), new lib.Vector3(0.5, 0.4, 0.5), new lib.Vector3(90, 90, 0));
    Bbl.obj.ObjParent = "Player" + playerid + "/Main Camera";
    scenes[scene].dynamics.push(Bbl);
    key += "/" + scenes[scene].dynamics[scenes[scene].dynamics.length-1].obj.ObjName;
    scenes[scene].dynamicNames[key] = scenes[scene].dynamicCount+2;
    scenes[scene].dynamicCount += 3;

    for (var i = 0; i < wsS.length; i++) {
        if (wsS[i] == null || i == playerid) { continue; }
        PlayerDyn.Send(wsS[i]);
        Camera.Send(wsS[i]);
        Bbl.Send(wsS[i]);
    }

    //instantiate same objects
    SendList = {"list":[]};

    var Player = lib.MessageData.Capsule();
    Player.obj.ObjName = "Player" + playerid;
    Player.Pos = startingPos;
    Player.obj.ObjScripts.push("UnityEngine.Rigidbody");
    Player.obj.ObjScripts.push("UnityEngine.CapsuleCollider");
    Player.obj.ObjScripts.push("PlayerController");
    Player.addModVars([
        "UnityEngine.Rigidbody", "2", "collisionDetectionMode", "ContinuousDynamic", "freezeRotation", "true", 
        "PlayerController", "4", "moveSpeed", "10", "sensitivityX", "7.5", "sensitivityY", "5", "jumpForce", "5", 
        "UnityEngine.CapsuleCollider", "2", "height", "4", "radius", "1"
    ]);
    Player.startPos = startingPos;
    SendList.list.push(Player);
    
    SendList.list.push(Modify("Main Camera", null, "Player" + playerid, new lib.Vector3(0, 1, 0), null, null, null, [
        "UnityEngine.Camera", "1", "nearClipPlane", "0.3"
    ]));
    SendList.list.push(Modify("Directional Light", null, null, null, null, null, null, ["UnityEngine.Light", "1", "intensity", "0.25"]));

    var Bbl = lib.MessageData.Capsule("Bbl", new lib.Vector3(0, 0, 0.75), new lib.Vector3(0.5, 0.4, 0.5), new lib.Vector3(90, 90, 0));
    Bbl.obj.ObjParent = "Player" + playerid + "/Main Camera";
    Bbl.obj.ObjScripts.push("UnityEngine.CapsuleCollider");
    SendList.list.push(Bbl);

    websocket.send(JSON.stringify(SendList));

	websocket.on('close', function (reasonCode, description) {
		if (!Pinging) {
			Pinging = true;
			//how many times we should loop
			for (var index = 0; index < wsS.length; index++) {
                if (wsS[index] == null) { continue; }
				playerAlive[index] = false;
				wsS[index].send("{\"MessageType\":\"ping\", \"ObjName\":\"" + index + "\"}");
			}
			setTimeout(function () {
				for (var index = 0; index < wsS.length; index++) {
                    if (wsS[index] == null) { continue; }
					if (playerAlive[index] == false) {
						console.log("player" + (index + 1) + " disconnected.")

                        deleteObject(0,"Player" + index);
                        deleteObject(0,"Player" + index + "/" + "Main Camera");
                        deleteObject(0,"Player" + index + "/" + "Main Camera/Bbl");
                        scenes[0].dynamics[scenes[0].dynamicNames["Player" + index]] = null;
                        scenes[0].dynamicNames["Player" + index] = null;
                        scenes[0].dynamics[scenes[0].dynamicNames["Player" + index + "/" + "Main Camera"]] = null;
                        scenes[0].dynamicNames["Player" + index + "/" + "Main Camera"] = null;
                        scenes[0].dynamics[scenes[0].dynamicNames["Player" + index + "/" + "Main Camera/Bbl"]] = null;
                        scenes[0].dynamicNames["Player" + index + "/" + "Main Camera/Bbl"] = null;

                        //send disconnection to other players
                        for(var i = 0; i < wsS.length; i++) {
                            if (wsS[i] == null || playerAlive[i] == false || i == index) { continue; }
                            Destroy("Player" + index + "/" + "Main Camera/Bbl", wsS[i]);
                            Destroy("Player" + index + "/" + "Main Camera", wsS[i]);
                            Destroy("Player" + index, wsS[i]);
                        }
						wsS[index] = null;
                        playerindexes[index] = null;
                        startingPos.y -= 4;
					}
				}
                for (var i = 0; i < playerAlive.length; i++) {
                    playerAlive[i] = false;
                }
				Pinging = false;
			}, 250);
		}
	});
	websocket.on('message', message => {
        try {
            var msg = JSON.parse(message);
            if (msg.list == null || msg.list.length <= 0) {
                if (receiveMes(msg)) {
                    if (msg.modifyId == "") { msg.modifyId = -1; }
                    for(var i = 0; i < wsS.length; i++) {
                        if (wsS[i] == null || i == Number(msg.modifyId)){ continue; }
                        if (msg.ObjName.includes("Cube") || msg.ObjName.includes("Object")) {
                            console.log("Player" + (Number(msg.modifyId)+1) + " sent " + msg.ObjFindName + " to " + "Player" + (i+1));
                        }
                        wsS[i].send(message);
                    }
                }
            } else {
                var newLst = []
                for(var i = 0; i < msg.list.length; i++) {
                    if (receiveMes(msg.list[i])) {
                        newLst.push(msg.list[i]);
                    }
                }
                if (newLst.length > 0) {
                    if (msg.modifyId == "") { msg.modifyId = -1; }
                    for(var i = 0; i < wsS.length; i++) {
                        if (wsS[i] == null || i == Number(msg.modifyId)){ continue; }
                        for (var j = 0; j < newLst.length; j++) {
                            wsS[i].send(JSON.stringify(newLst[j]));
                        }
                    }
                }
            }
        } catch (error) { console.log(error); }
	});
});
console.log("server open on localhost:53586");

function Create(ObjName, ObjParent, Pos, Scale, Rot, ObjScripts, ModScriptVars, ws) {
    var msg = new lib.MessageData("Create", ObjName, ObjParent, Pos, Scale, Rot, null, ObjScripts, ModScriptVars);
    msg.Send(ws);
}
function Modify(ObjFindName, ObjName, ObjParent, Pos, Scale, Rot, ObjScripts, ModScriptVars) {
    var msg = new lib.MessageData("Modify", ObjName, ObjParent, Pos, Scale, Rot, ObjFindName, ObjScripts, ModScriptVars);
    return msg;
}
function Destroy(ObjFindName, ws) {
    var msg = new lib.MessageData("Destroy", null, null, null, null, null, ObjFindName);
    msg.Send(ws);
}
function receiveMes(msg) {
    if (msg.MessageType != null) {
        if (msg.MessageType == "pong") {
            playerAlive[Number(msg.ObjName)] = true;
            console.log("Player" + (Number(msg.ObjName)+1) + " is alive.")
        } else if (msg.MessageType == "Modify") {
            var name = msg.ObjFindName;
            var scene = 0

            var closestDis = 99999999999999999;
            var closestInd = -2;
            for (var i = 0; i < wsS.length; i++) {
                if (wsS[i] == null || scenes[scene].dynamicNames["Player" + i] == null) { continue; }
                //console.log(i);
                var pos = scenes[scene].dynamics[scenes[scene].dynamicNames["Player" + i]].obj.Pos;
                var lst = name.split("/");
                var pos2 = lib.Vector3.Zero();
                for (var j = 0; j < lst.length; j++) {
                    var str = lst[0];
                    for (var l = 1; l <= j ; l++) {
                        str += "/" + lst[l];
                    }
                    if (scenes[scene].dynamics[scenes[scene].dynamicNames[str]] != null) {
                        var pos1 = scenes[scene].dynamics[scenes[scene].dynamicNames[str]].obj.Pos
                        pos2.x = pos2.x + pos1.x;
                        pos2.y = pos2.y + pos1.y;
                        pos2.z = pos2.z + pos1.z;
                    }
                }
                var dis = lib.distance(pos,pos2);
                //console.log((i+1) + ".dis = " + dis + ".");
                if (dis < closestDis || (dis == closestDis && i == msg.modifyId)) {
                    closestDis = dis; closestInd = i;
                }
            }
            //console.log((closestInd+1) + " is closer.");
            if (closestInd < 0 || closestInd != msg.modifyId) { return false; console.log("Player" + (Number(msg.modifyId)+1) + " tried to move object " + scenes[scene].dynamics[scenes[scene].dynamicNames[name]].obj.ObjName + " but player " + (closestInd+1) + " was closer.\n\n"); return false; }
            //console.log("Player" + (Number(msg.modifyId)+1) + " moved " + msg.ObjFindName);
            if (scenes[scene].dynamicNames[name] != null) {
                if (msg.ObjName != null && msg.ObjName != "" && msg.ObjName != name) {
                    scenes[scene].dynamicNames[msg.ObjName] = scenes[scene].dynamicNames[name];
                    scenes[scene].dynamicNames[name] = null;
                    scenes[scene].dynamics[scenes[scene].dynamicNames[msg.ObjName]].obj.ObjName = msg.ObjName;
                } else { msg.ObjName = name; }
                if (msg.ObjParent != null && msg.ObjParent != "") {
                    scenes[scene].dynamics[scenes[scene].dynamicNames[msg.ObjName]].obj.ObjParent = msg.ObjParent;

                    if (msg.ObjName.include("/")) {
                        scenes[scene].dynamicNames[msg.ObjParent + "/" + msg.ObjName.split("/")[msg.ObjName.split("/").length]] = scenes[scene].dynamicNames[msg.ObjName];
                    } else {
                        scenes[scene].dynamicNames[msg.ObjParent + "/" + msg.ObjName] = scenes[scene].dynamicNames[msg.ObjName];
                    }
                    scenes[scene].dynamicNames[msg.ObjName] = null;
                    msg.ObjName = msg.ObjParent + "/" + msg.ObjName.split("/")[msg.ObjName.split("/").length]
                }
                if (msg.Pos != null && msg.Pos.x != 0.01140000019222498) {
                    if (msg.Pos.y < -50) {
                        if (msg.ObjName.includes("Object")) {
                            //delete
                            deleteObject(scene,msg.ObjName);
                            for(var i = 0; i < wsS.length; i++) {
                                if (wsS[i] == null) { continue; }
                                Destroy(msg.ObjFindName, wsS[i]);
                            }
                            //console.log("object \"" + msg.ObjName + "\" deleted");
                            return false;
                        }
                        else {
                            //reset pos
                            for(var i = 0; i < wsS.length; i++) {
                                if (wsS[i] == null) { continue; }
                                Modify(msg.ObjName, null, null, scenes[scene].dynamics[scenes[scene].dynamicNames[msg.ObjName]].startPos, null, new lib.Vector3(0,0,0), null, [
                                    "UnityEngine.Rigidbody","1","velocity","(0,0,0)"
                                ]).Send(wsS[i]);
                                scenes[scene].dynamics[scenes[scene].dynamicNames[msg.ObjName]].Pos = scenes[scene].dynamics[scenes[scene].dynamicNames[msg.ObjName]].startPos;
                            }
                        }
                    } else {
                        scenes[scene].dynamics[scenes[scene].dynamicNames[msg.ObjName]].Pos = new lib.Vector3(msg.Pos.x,msg.Pos.y,msg.Pos.z);
                    }
                }
                if (msg.Scale != null && msg.Scale.x != 0.01140000019222498) {
                    scenes[scene].dynamics[scenes[scene].dynamicNames[msg.ObjName]].Scale = new lib.Vector3(msg.Scale.x,msg.Scale.y,msg.Scale.z);
                }
                if (msg.Rot != null && msg.Rot.x != 0.01140000019222498) {
                    scenes[scene].dynamics[scenes[scene].dynamicNames[msg.ObjName]].Rot = new lib.Vector3(msg.Rot.x,msg.Rot.y,msg.Rot.z);
                }
                if (msg.ObjScripts != null && msg.ObjScripts.length > 0) {
                    for(var i = 0; i < msg.ObjScripts.length; i++) {
                        if (!scenes[scene].dynamics[scenes[scene].dynamicNames[msg.ObjName]].ObjScripts.includes(msg.ObjScripts[i])) {
                            scenes[scene].dynamics[scenes[scene].dynamicNames[msg.ObjName]].ObjScripts.push(msg.ObjScripts[i]);
                        }
                    }
                }
                if (msg.ModScriptVars != null && msg.ModScriptVars.length > 0) {
                    var Velocity = lib.Vector3.Zero();
                    var AngularVelocity = lib.Vector3.Zero();
                    if (msg.ModScriptVars.length > 3 && msg.ModScriptVars[0] == "UnityEngine.Rigidbody" && scenes[scene].dynamics[scenes[scene].dynamicNames[msg.ObjName]].obj.ObjScripts.includes("UnityEngine.Rigidbody")) {
                        if (msg.ModScriptVars[1] == "2") {
                            var split1 = msg.ModScriptVars[3].split("(")[1].split(")")[0].replaceall(" ","").split(",");
                            var split2 = msg.ModScriptVars[5].split("(")[1].split(")")[0].replaceall(" ","").split(",");
                            Velocity = new lib.Vector3(split1[0],split1[1],split1[2]);
                            AngularVelocity = new lib.Vector3(split2[0],split2[1],split2[2]);

                        } else if (msg.ModScriptVars[1] == "1") {
                            if (msg.ModScriptVars[2] == "velocity") {
                                var split1 = msg.ModScriptVars[3].split("(")[1].split(")")[0].replaceall(" ","").split(",");
                                Velocity = new lib.Vector3(split1[0],split1[1],split1[2]);
                            } else if (msg.ModScriptVars[2] == "angularVelocity") {
                                var split1 = msg.ModScriptVars[3].split("(")[1].split(")")[0].replaceall(" ","").split(",");
                                AngularVelocity = new lib.Vector3(split1[0],split1[1],split1[2]);
                            }
                        }
                        var list = scenes[scene].dynamics[scenes[scene].dynamicNames[msg.ObjName]].obj.ModScriptVars
                        var foundVel = false;
                        var foundAng = false;
                        for (var i = 0; i < list.length ; i++) {
                            if (list[i] == "velocity") {
                                foundVel = true
                                list[i+1] = "(" + Velocity.x + "," + Velocity.y + "," + Velocity.z + ")";
                                i++;
                            } else if (list[i] == "angularVelocity") {
                                foundAng = true
                                list[i+1] = "(" + AngularVelocity.x + "," + AngularVelocity.y + "," + AngularVelocity.z + ")";
                                i++;
                            }
                        }
                        var foundRigidbody = false
                        if (!foundVel) {
                            for (var i = 0; i < list.length; i++) {
                                if (list[i] == "UnityEngine.Rigidbody") {
                                    foundRigidbody = true;
                                    list[i+1] = str(Number(list[i+1])+1);
                                    list.splice(i+2, 0, "velocity");
                                    list.splice(i+3, 0, "(" + Velocity.x + "," + Velocity.y + "," + Velocity.z + ")");
                                    break;
                                }
                            }
                        }
                        if (!foundRigidbody) {
                            list.push("UnityEngine.Rigidbody");
                            list.push("2");
                            list.push("velocity");
                            list.push("(" + Velocity.x + "," + Velocity.y + "," + Velocity.z + ")");
                            list.push("angularVelocity");
                            list.push("(" + AngularVelocity.x + "," + AngularVelocity.y + "," + AngularVelocity.z + ")");
                            foundAng = true;
                        }
                        if (!foundAng) {
                            for (var i = 0; i < list.length; i++) {
                                if (list[i] == "UnityEngine.Rigidbody") {
                                    list[i+1] = str(Number(list[i+1])+1);
                                    list.splice(i+2, 0, "angularVelocity");
                                    list.splice(i+3, 0, "(" + AngularVelocity.x + "," + AngularVelocity.y + "," + AngularVelocity.z + ")");
                                    break;
                                }
                            }
                        }
                        scenes[scene].dynamics[scenes[scene].dynamicNames[msg.ObjName]].obj.ModScriptVars = list;
                    }
                }
                return true;
            }
        } else if (msg.MessageType == "Create") {
            var data = new lib.MessageData(msg.MessageType,msg.ObjName,msg.ObjParent,msg.Pos != null ? lib.Vector3.fromObject(msg.Pos) : null,msg.Scale != null ? lib.Vector3.fromObject(msg.Scale) : null,msg.Rot != null ? lib.Vector3.fromObject(msg.Rot) : null,msg.ObjFindName,msg.ObjScripts,msg.ModScriptVars);
            if (msg.ObjName == null || msg.ObjName == "") {
                msg.ObjName = "Object" + ObjectIndex;
                data.obj.ObjName = "Object" + ObjectIndex;
                ObjectIndex++;
            }
            if (msg.modifyId == "") { msg.modifyId = -1; }
            for(var i = 0; i < wsS.length; i++) {
                if (wsS[i] == null || i == Number(msg.modifyId)){ continue; }
                data.Send(wsS[i]);
            }
            scenes[0].dynamics.push(data);
            scenes[0].dynamicNames[msg.ObjName] = scenes[0].dynamics.length-1;
            scenes[0].dynamicCount++;
            return false;
        }
    }
    return false;
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
function deleteObject(scene, name) {
    for(var i = 0; i < scenes[scene].dynamics.length; i++) {
        if (scenes[scene].dynamics[i] == null) { continue; }
        var split = name.split("/");
        if (scenes[scene].dynamics[i].obj.ObjName == name || scenes[scene].dynamics[i].obj.ObjName == split[split.length-1]) {
            //console.log("removing " + scenes[scene].dynamics[i].obj.ObjName + " from dynamics.");
            scenes[scene].dynamics[i] = null;
        }
        if (scenes[scene].dynamicNames[name] != null) {
            scenes[scene].dynamicNames[name] = null
            //console.log("removing " + name + " from dynamicNames.");
        }
    }
}