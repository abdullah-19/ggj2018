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
    Debug.Log("Sending response");
    if (isServer) {
      Debug.Log("IS SERVER");
      gameManager.UpdatePlayerOneResponse(watsonResponse);
      return;
    };
    gameManager.CmdUpdatePlayerTwoResponse(watsonResponse);
  }
}
