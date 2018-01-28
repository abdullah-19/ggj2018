using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TimeController : NetworkBehaviour {
  [SyncVar]
	public int timeRemaining;
  Text timerText;
  int ROUND_LENGTH = 30;
  GameManager gameManager;

	void Awake() {
    gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
		timeRemaining = ROUND_LENGTH;
    timerText = GameObject.Find("GameClock").GetComponent<Text>();
	}

	public void StartTime(int duration) {
		// Must use named coroutine so StopCoroutine() invokes on correct IEnumerator instance below!
		StartCoroutine("Countdown", duration);
	}

	public void ResetTimerAndStart() {
		ClearTime();
		timeRemaining = ROUND_LENGTH;
		StartTime(ROUND_LENGTH);
	}

	public void ClearTime () {
		StopTime();
		timerText.text = "0";
		timerText.color = Color.white;
	}

	public void StopTime() {
		timeRemaining = 1; // Prevents defeat from playing on a win!
		StopCoroutine("Countdown");
    gameManager.EndRound();
	}

	IEnumerator Countdown(int duration) {
		for (var i = duration; i >= 0; i--) {
			timerText.text = i.ToString();
			if (i <= 2) {
				timerText.color = Color.red;
			}
			timeRemaining = i;
			yield return new WaitForSecondsRealtime(1);
		}
	}

}
