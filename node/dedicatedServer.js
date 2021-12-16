//download node.js
//download yarn
//cmd: yarn init
//cmd: yarn ws

const WebSocket = require('ws')
const http = require("http");
const fs = require('fs');
const { networkInterfaces } = require('os');
const url = require('url');

Pinging = false;
wsS = [];
playerAlive = [];
const socketport = "53586";
wss = new WebSocket.Server({ port: socketport })
wss.on('connection', websocket => {
    for (var i = 0; i < wsS.length + 1; i++) {
        if (wsS[i] != null) { wsS[i] = websocket; }
    }
    Create("Floor", null, vec3(0,-0.125,0), vec3(50,0.25,50), vec3(0,45,0), [
        "UnityEngine.MeshFilter",
        "UnityEngine.MeshRenderer",
        "UnityEngine.BoxCollider"
    ], [
        "1","mesh","Default.Mesh.Cube,fbx",
        "1","materials[0]","Default.Mat.Default-Diffuse,mat"
    ], websocket);
    Create("Player", null, vec3(0,5,0), null, null, [
        "UnityEngine.MeshFilter",
        "UnityEngine.MeshRenderer",
        "UnityEngine.Rigidbody",
        "UnityEngine.BoxCollider",
        "PlayerController",
        "NetworkObj"
    ], [
        "1","mesh","Default.Mesh.Capsule,fbx",
        "1","materials[0]","Default.Mat.Default-Diffuse,mat",
        "2","collisionDetectionMode","ContinuousDynamic","freezeRotation","true",
        "0",
        "4","moveSpeed","10","sensitivityX","10","sensitivityY","10","jumpForce","10"
    ], websocket);
    Modify("Main Camera", null, "Player", vec3(0,0,0), null, null, null, null, null, websocket);
    Modify("Directional Light", null, null, null, null, null, null, null, ["UnityEngine.Light","1","intensity","0.25"], websocket);
	websocket.on('close', function (reasonCode, description) {
		if (reasonCode == 1006) {
			if (!Pinging) {
				Pinging = true;
				//how many times we should loop
				var playerIndexes = [];
				for (let index = 0; index < wsS.length; index++) { if (wsS[index] != null) {playerIndexes.push(index); } }
				for (let index = 0; index < playerIndexes.length; index++) {
					if (wsS[playerIndexes[index]] != null) {
						playerAlive[playerIndexes[index]] = false;
						wsS[playerIndexes[index]].send("{\"type\":\"ping\", \"id\":\"" + playerIndexes[index] + "\"}");
					}
				}
				setTimeout(function () {
					for (let index = 0; index < playerIndexes.length; index++) {
						if (playerAlive[playerIndexes[index]] == false) {
							wsS[playerIndexes[index]] = null;
							console.log("player" + (playerIndexes[index] + 1) + " disconnected.")
                            //send disconnection to other players
						} else { playerAlive[playerIndexes[index]] = false; }
					}
					Pinging = false;
				}, 250);
			}
		}
	});
	websocket.on('message', message => {
		var msg = JSON.parse(message);
        if (msg.type = "pong") {
			playerAlive[msg.id] = true;
			console.log("turtle" + (parseInt(msg.id)+1) + " is alive.")
		}
	});
});
console.log("server open on localhost:53586");

function Create(ObjName, ObjParent, Pos, Scale, Rot, ObjScripts, ObjScriptVars, ws) {
    try {
        var obj = { "MessageType":"Create" };
        if (ObjName != null) {
            obj["ObjName"] = ObjName;
        }
        if (ObjParent != null) {
            obj["ObjParent"] = ObjParent;
        }
        if (Pos != null) {
            obj["Pos"] = {
                "x":Pos[0],
                "y":Pos[1],
                "z":Pos[2]
            };
        }
        if (Scale != null) {
            obj["Scale"] = {
                "x":Scale[0],
                "y":Scale[1],
                "z":Scale[2]
            };
        }
        if (Rot != null) {
            obj["Rot"] = {
                "x":Rot[0],
                "y":Rot[1],
                "z":Rot[2]
            };
        }
        if (ObjScripts != null) {
            obj["ObjScripts"] = ObjScripts;
        }
        if (ObjScriptVars != null) {
            obj["ObjScriptVars"] = ObjScriptVars;
        }
        ws.send(JSON.stringify(obj));
    } catch (error) { console.error(error) }
}
function Modify(ObjFindName, ObjName, ObjParent, Pos, Scale, Rot, ObjScripts, ObjScriptVars, ModScriptVars, ws) {
    try {
        var obj = { "MessageType":"Modify" };
        if (ObjFindName != null) {
            obj["ObjFindName"] = ObjFindName;
        }
        if (ObjParent != null) {
            obj["ObjParent"] = ObjParent;
        }
        if (ObjName != null) {
            obj["ObjName"] = ObjName;
        }
        if (Pos != null) {
            obj["Pos"] = {
                "x":Pos[0],
                "y":Pos[1],
                "z":Pos[2]
            };
        }
        if (Scale != null) {
            obj["Scale"] = {
                "x":Scale[0],
                "y":Scale[1],
                "z":Scale[2]
            };
        }
        if (Rot != null) {
            obj["Rot"] = {
                "x":Rot[0],
                "y":Rot[1],
                "z":Rot[2]
            };
        }
        if (ObjScripts != null) {
            obj["ObjScripts"] = ObjScripts;
        }
        if (ObjScriptVars != null) {
            obj["ObjScriptVars"] = ObjScriptVars;
        }
        if (ModScriptVars != null) {
            obj["ModScriptVars"] = ModScriptVars;
        }
        ws.send(JSON.stringify(obj));
    } catch (error) { console.error(error) }
}
function Destroy(ObjFindName,ws) {
    try {
        var obj = { "MessageType":"Destroy" };
        if (ObjFindName != null) {
            obj["ObjFindName"] = ObjFindName;
            ws.send(JSON.stringify(obj));
        } else {
            console.log("ObjFindName is required to Destroy object.")
        }
    } catch (error) { console.error(error) }
}
function vec3(x,y,z) {
    return [x,y,z];
}