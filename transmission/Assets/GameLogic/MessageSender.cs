using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class MessageSender : NetworkBehaviour {
  GameManager gameManager;

  void Awake() {
    gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
  }

  public void SendResponse(float sentimentScore) {
    Debug.Log("Sending response");
    if (Network.isServer) {
      Debug.Log("IS SERVER");
      gameManager.UpdatePlayerOneResponse(sentimentScore);
      return;
    };
    gameManager.CmdUpdatePlayerTwoResponse(sentimentScore);
  }
}
