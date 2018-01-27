using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class MessageSender : NetworkBehaviour {
    [SyncVar]
    WatsonResponse player2Message; 
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    [Command]
    void CmdsendWatsonReponse(WatsonResponse response)
    {
        if(!isLocalPlayer) 
        {
            player2Message = response;   
        }
        
    } 
}
