using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {
	
	const string MENU = "MENU";
	string collected = "0";

	bool displayVictory;

	float timeDisplayingVictoryMessage;

	// Use this for initialization
	void Start () {
	
	}

	void OnGUI() {
		GUI.skin.button.fontSize = (int)(Screen.height * 0.04f);

		if (GUI.Button (new Rect (15, 15, Screen.width * 0.20f, Screen.height * 0.05f), MENU)) {
			Application.LoadLevel("menu");
		}

		GUI.TextArea(new Rect (15, Screen.height * 0.05f + 15, Screen.width * 0.20f, Screen.height * 0.05f), "Collected: " + collected);

	}


	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape)) { 
			Application.LoadLevel("menu");
		}
	}


	/// <summary>
	/// Raises the collected event.
	/// </summary>
	/// <param name="amount">Amount.</param>
	void OnCollected(string amount) {
		collected = amount.ToString();
		
	}

	void OnVictory() {
		displayVictory = true;

	}

	void goToNextLevel ()
	{
		if (Application.loadedLevelName == "level1") {

		}
	}
}
