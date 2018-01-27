using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {
    private TimeController timeController;
    private NotifierController notifierController;

    [SyncVar]
    public float playerOneResponse;
    [SyncVar]
    public float playerTwoResponse;

    void Start () {
    // SetUpComponents();
    timeController.StartTime(30);
    // When other player connects...
    //StartCoroutine(GetReady());
    }

    [Command]
    public void CmdUpdatePlayerTwoResponse(float response) {
    playerTwoResponse = response;
    Debug.Log("UPDATED PLAYER TWO RESPONSE " + response);
    }

    public void UpdatePlayerOneResponse(float response) {
    playerOneResponse = response;
    Debug.Log("UPDATED PLAYER ONE RESPONSE " + response);
    }

    void Update() {
    Debug.Log("VARS " + playerTwoResponse + " " + playerOneResponse);
    }

    IEnumerator GetReady() {
    // SetUpRound(allRounds);
    // PlayMusic();
    // notifierController.DisplayTextOnTopOfScreen("Get Ready", 2);
    yield return new WaitForSecondsRealtime(2);
    // streakNotifier.DisplayTextOnTopOfScreen(codeBlock.Length + " Chars", 3);
    // yield return new WaitForSecondsRealtime(2);
    // announcer.PlayGetReadySound();
    // streakNotifier.DisplayTextOnTopOfScreen("Get Ready", 2);
    // yield return new WaitForSecondsRealtime(1);
    // streakNotifier.DisplayTextOnTopOfScreen("3", 1);
    // yield return new WaitForSecondsRealtime(1);
    // streakNotifier.DisplayTextOnTopOfScreen("2", 1);
    // yield return new WaitForSecondsRealtime(1);
    // streakNotifier.DisplayTextOnTopOfScreen("1", 1);
    // PlayReloadSound();
    // yield return new WaitForSecondsRealtime(1);
    // announcer.PlayBeginSound();
    // streakNotifier.DisplayTextOnTopOfScreen("BEGIN", 1);
    // StartRound();
    }

    void Awake() {
    timeController = transform.GetComponent<TimeController>();
    notifierController = GameObject.Find("UI/Notifier").GetComponent<NotifierController>();
    }

    private Winner calculateWinner() {
        return Mathf.Abs(playerOneResponse) > Mathf.Abs(playerOneResponse) ? Winner.Player1 : Winner.Player2;
    }

    public enum Winner
    {
        Player1,
        Player2
    }
}
