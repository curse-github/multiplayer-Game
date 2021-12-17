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

var networkObjects = [];
var networkObjectNames = [];

const socketport = "53586";
const wss = new WebSocket.Server({ port: socketport })
wss.on('connection', websocket => {
    var playerid = null;
    for (var i = 0; i < wsS.length+1; i++) {
        if (wsS[i] == null) { wsS[i] = websocket; playerid = i; console.log("player" + (i+1) + " joined!"); break; }
    }
    startingPos.y += 4;

    var scene = 0;
    for (var i = 0; i < scenes[scene].statics.length; i++) { scenes[scene].statics[i].Send(websocket); }
    for (var i = 0; i < scenes[scene].dynamics.length; i++) {
        if (scenes[scene].dynamics[i] != null) {
            //console.log(scenes[scene].dynamics[i].obj.ObjName);
            scenes[scene].dynamics[i].Send(websocket);
        }
    }

    //Create Player Network Object
    var PlayerDyn = lib.MessageData.Capsule("Player" + playerid, startingPos);
    PlayerDyn.addModVars([
        "UnityEngine.CapsuleCollider", "2", "height", "4", "radius", "1"
    ]);
    scenes[scene].dynamics.push(PlayerDyn);
    var key = scenes[scene].dynamics[scenes[scene].dynamics.length-1].obj.ObjName;
    scenes[scene].dynamicNames[key] = scenes[scene].dynamicCount;

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
    Player.Send(websocket);
    
    Modify("Main Camera", null, "Player" + playerid, new lib.Vector3(0, 1, 0), null, null, null, [
        "UnityEngine.Camera", "1", "nearClipPlane", "1"
    ], websocket);
    Modify("Directional Light", null, null, null, null, null, null, ["UnityEngine.Light", "1", "intensity", "0.25"], websocket);

    var Bbl = lib.MessageData.Capsule("Bbl", new lib.Vector3(0, 0, 0.75), new lib.Vector3(0.5, 0.4, 0.5), new lib.Vector3(90, 90, 0));
    Bbl.obj.ObjParent = "Player" + playerid + "/Main Camera";
    Bbl.obj.ObjScripts.push("UnityEngine.CapsuleCollider");
    Bbl.Send(websocket);

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
						wsS[index] = null;
						console.log("player" + (index + 1) + " disconnected.")
                        //send disconnection to other players
                        if (scenes[0].dynamicNames["Player" + index] != null) {
                            for (const [key, value] of Object.entries(scenes[0].dynamicNames)) {
                                console.log(`${key}: ${value}`);
                            }
                            console.log("\n");

                            scenes[0].dynamics[scenes[0].dynamicNames["Player" + index]] = null;
                            scenes[0].dynamicNames["Player" + index] = null;
                            
                            scenes[0].dynamics[scenes[0].dynamicNames["Player" + index + "/" + "Main Camera"]] = null;
                            scenes[0].dynamicNames["Player" + index + "/" + "Main Camera"] = null;
                            
                            scenes[0].dynamics[scenes[0].dynamicNames["Player" + index + "/" + "Main Camera/Bbl"]] = null;
                            scenes[0].dynamicNames["Player" + index + "/" + "Main Camera/Bbl"] = null;
                        }
                        startingPos.y -= 4;
					} else { playerAlive[index] = false; }
				}
				Pinging = false;
			}, 250);
		}
	});
	websocket.on('message', message => {
		var msg = JSON.parse(message);
        if (msg.MessageType != null) {
            if (msg.MessageType == "pong") {
                playerAlive[Number(msg.ObjName)] = true;
                console.log("Player" + (Number(msg.ObjName)+1) + " is alive.")
            } else if (msg.MessageType == "Modify") {
                var name = msg.ObjFindName;
                var scene = 0
                if (scenes[scene].dynamicNames[name] != null && msg.modifyId != null) {
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
                        //console.log("Pos: \"" + JSON.stringify(msg.Pos) + "\"");
                        scenes[scene].dynamics[scenes[scene].dynamicNames[msg.ObjName]].Pos = new lib.Vector3(msg.Pos.x,msg.Pos.y,msg.Pos.z);
                    }
                    if (msg.Scale != null && msg.Scale.x != 0.01140000019222498) {
                        scenes[scene].dynamics[scenes[scene].dynamicNames[msg.ObjName]].Scale = new lib.Vector3(msg.Scale.x,msg.Scale.y,msg.Scale.z);
                    }
                    if (msg.Rot != null && msg.Rot.x != 0.01140000019222498) {
                        //console.log("Rot: \"" + JSON.stringify(msg.Rot) + "\"");
                        scenes[scene].dynamics[scenes[scene].dynamicNames[msg.ObjName]].Rot = new lib.Vector3(msg.Rot.x,msg.Rot.y,msg.Rot.z);
                    }
                    if (msg.ObjScripts != null && msg.ObjScripts.length > 0) {
                        for(var i = 0; i < msg.ObjScripts.length; i++) {
                            scenes[scene].dynamics[scenes[scene].dynamicNames[msg.ObjName]].ObjScripts.push(msg.ObjScripts[i]);
                        }
                    }
                    //console.log(message + "\n");
                    for(var i = 0; i < wsS.length; i++) {
                        if (wsS[i] == null || i == Number(msg.modifyId)){ continue; }
                        wsS[i].send(message);
                    }
                }
            }
        }
	});
});
console.log("server open on localhost:53586");

function Create(ObjName, ObjParent, Pos, Scale, Rot, ObjScripts, ModScriptVars, ws) {
    var msg = new lib.MessageData("Create", ObjName, ObjParent, Pos, Scale, Rot, null, ObjScripts, ModScriptVars);
    msg.Send(ws);
}
function Modify(ObjFindName, ObjName, ObjParent, Pos, Scale, Rot, ObjScripts, ModScriptVars, ws) {
    var msg = new lib.MessageData("Modify", ObjName, ObjParent, Pos, Scale, Rot, ObjFindName, ObjScripts, ModScriptVars);
    msg.Send(ws);
}
function Destroy(ObjFindName, ws) {
    var msg = new lib.MessageData("Destroy", null, null, null, null, null, ObjFindName);
    msg.Send(ws);
}