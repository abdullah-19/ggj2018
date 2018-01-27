using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {
  private TimeController timeController;

  void Awake() {
    timeController = transform.GetComponent<TimeController>();
  }

	void Start () {
    timeController.StartTime(30);
	}

}
