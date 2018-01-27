using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class MessageSender : NetworkBehaviour {
    GameManager gameManager;
    private float totalSentimentScore { get; set; }

    void Awake() {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void SendResponse(float sentimentScore)
    {
        Debug.Log("Sending response");
        Debug.Log("SERVER " + isServer + " " + isLocalPlayer);
        if (isServer)
        {
            // Player One is positive
            totalSentimentScore += sentimentScore;
            gameManager.UpdatePlayerOneResponse(totalSentimentScore);
            return;
        };
        if (isLocalPlayer)
        {
            // Player Two is negative.
            totalSentimentScore -= sentimentScore;
            CmdUpdatePlayerTwoResponse(totalSentimentScore);
        }
    }

    [Command]
    public void CmdUpdatePlayerTwoResponse(float response) {
        gameManager.playerTwoResponse = response;
        Debug.Log("UPDATED PLAYER TWO RESPONSE " + response);
    }
}
