using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {
  private TimeController timeController;
  // Positive Sentiment
  [SyncVar]
  public float playerOneResponse;
  // Negative Sentiment
  [SyncVar]
  public float playerTwoResponse;
  [SyncVar]
  private bool gameStarted = false;
  private NotifierController notifierController;
  private WeatherManager weatherManager;

  void Start () {
    // SetUpComponents();
    // Find reference to WeatherManager
    weatherManager = GameObject.Find("WeatherManager").GetComponent<WeatherManager>();
  }

  public void UpdatePlayerOneResponse(float response) {
    playerOneResponse += response;
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
    var players = GameObject.FindGameObjectsWithTag("Player");
    if (players.Length > 1) {
      StartCoroutine(GetReady());
      gameStarted = true;
    }
  }
}
