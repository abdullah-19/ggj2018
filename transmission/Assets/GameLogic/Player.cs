using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {
  [SyncVar]
  bool isRecording;

	// Use this for initialization
	void Start () {

	}

  void Update() {

  }

  [Command]
  void CmdSendWatsonResponse() {
    if (!isLocalPlayer) return;

  }

  


}
