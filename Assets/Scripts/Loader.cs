using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour {

	public GameObject gameManager;

	// On load, create the GameManager (if there is none).
	void Awake () {
		if (GameManager.instance == null) {
			Instantiate(gameManager);
		}
		
	}
}
