using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class MessageSender : NetworkBehaviour {
  GameManager gameManager;

  void Awake() {
    gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
  }

  public void SendResponse(WatsonResponse watsonResponse) {
    gameManager.CmdUpdatePlayerTwoResponse(watsonResponse);
  }

}
