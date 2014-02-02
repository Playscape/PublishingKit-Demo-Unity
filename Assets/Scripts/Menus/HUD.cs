using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {

	private readonly Rect menuButtonRect = new Rect (15, 15, Screen.width * 0.20f, Screen.height * 0.05f);
	private readonly Rect statusTextRect = new  Rect (15, Screen.height * 0.05f + 15, Screen.width * 0.20f, Screen.height * 0.05f);

	private const int VICTORY_MESSAGE_DELAY_SECONDS = 3;
	private const string MENU = "MENU";
	private string collected = "0";


	bool shouldDisplayVictory;
	float timeDisplayingVictoryMessageSeconds;

	// Use this for initialization
	void Start () {
	
	}

	void OnGUI() {
		GUI.skin.button.fontSize = (int)(Screen.height * 0.04f);

		if (GUI.Button (menuButtonRect, MENU)) {
			Application.LoadLevel("menu");
		}

		string statusText  = "Collected: " + collected;
		if (shouldDisplayVictory) {
			statusText = "VICTORY!";
		}

		GUI.TextArea(statusTextRect, statusText);
	}


	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape)) { 
			Application.LoadLevel("menu");
		}

		if (shouldDisplayVictory) {
			timeDisplayingVictoryMessageSeconds += Time.deltaTime;
			
			if (timeDisplayingVictoryMessageSeconds > VICTORY_MESSAGE_DELAY_SECONDS) {
				goToNextLevel();
			}
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
		shouldDisplayVictory = true;

	}

	void goToNextLevel ()
	{
		if (Application.loadedLevelName == "level1") {
			Application.LoadLevel("level2");
		} else {
			Application.LoadLevel("menu");
		}
	}
}
