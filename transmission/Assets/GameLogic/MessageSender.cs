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
    Debug.Log("SERVER " + isServer + " " + isLocalPlayer);
    if (isServer) {
      gameManager.UpdatePlayerOneResponse(sentimentScore);
      return;
    };
    if (isLocalPlayer) {
      gameManager.CmdUpdatePlayerTwoResponse(sentimentScore);
    }
  }
}
