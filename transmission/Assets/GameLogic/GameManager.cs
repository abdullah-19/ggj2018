using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {
    private TimeController timeController;
    private NotifierController notifierController;
    private WeatherManager weatherManager;

    // Positive Sentiment
    [SyncVar]
    public float playerOneResponse;

    // Negative Sentiment
    [SyncVar]
    public float playerTwoResponse;

    void Start () {
        // SetUpComponents();
        timeController.StartTime(30);
        // When other player connects...
        //StartCoroutine(GetReady());

        // Find reference to WeatherManager
        weatherManager = GameObject.Find("WeatherManager").GetComponent<WeatherManager>();
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

    // Takes the two responses and changes the weather state based on who wins.
    public void calculateWinner() {
        if (Mathf.Abs(playerOneResponse) > Mathf.Abs(playerOneResponse)) {
            weatherManager.IncrementWeatherState();
        } else {
            weatherManager.DecrementWeatherState();
        }
    }
}
