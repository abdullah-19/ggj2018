using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {
  private TimeController timeController;
  private NotifierController notifierController;
  [SyncVar]
  public float playerOneRoundScore;
  [SyncVar]
  public float playerTwoRoundScore;
  [SyncVar]
  private bool gameStarted = false;

  void Start () {
    // SetUpComponents();
  }

  public void UpdatePlayerOneResponse(float response) {
    playerOneRoundScore += response;
  }

  IEnumerator GetReady() {
    // PlayMusic();
    notifierController.DisplayTextOnTopOfScreen("Get Ready", 3);
    yield return new WaitForSecondsRealtime(3);
    notifierController.DisplayTextOnTopOfScreen("3", 1);
    yield return new WaitForSecondsRealtime(1);
    notifierController.DisplayTextOnTopOfScreen("2", 1);
    yield return new WaitForSecondsRealtime(1);
    notifierController.DisplayTextOnTopOfScreen("1", 1);
    yield return new WaitForSecondsRealtime(1);
    var emotionString = playerEmotion();
    notifierController.DisplayTextOnTopOfScreen(emotionString, 1);
    yield return new WaitForSecondsRealtime(1);
    StartRound();
  }

  void StartRound() {
    timeController.StartTime(5);
  }

  string playerEmotion() {
    if (isServer) return "SPEAK POSITIVELY";
    return "SPEAK NEGATIVELY";
  }

  void Awake() {
    timeController = transform.GetComponent<TimeController>();
    notifierController = GameObject.Find("UI/Notifier").GetComponent<NotifierController>();
  }

  void Update() {
    if (!isServer) return;
    if (gameStarted) return;
    checkIfTwoPlayers();
  }

  void checkIfTwoPlayers() {
    var players = GameObject.FindGameObjectsWithTag("player");
    if (players.Length > 1) {
      StartCoroutine(GetReady());
      gameStarted = true;
    }
  }

}
