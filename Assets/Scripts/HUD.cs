using UnityEngine;
using System.Collections;
using System;
using Playscape.Analytics;
using System.Collections.Generic;

public class HUD : MonoBehaviour {

	private readonly Rect menuButtonRect = new Rect (15, 15, Screen.width * 0.20f, Screen.height * 0.05f);
	private readonly Rect statusTextRect = new  Rect (15, Screen.height * 0.05f + 15, Screen.width /2, Screen.height * 0.05f);

	string mWinnerName;

	private const int VICTORY_MESSAGE_DELAY_SECONDS = 3;
	private const string MENU = "MENU";

	int mHisAmount = -1;
	int mMyAmount;

	private static HUD mInstance;
	public static HUD Instance { get {
			if (mInstance == null) {
				var go = GameObject.Find("HUD");
				if (go != null) {
					mInstance = go.GetComponent<HUD>();
				}
			}
			return mInstance;
		}
	}

	bool shouldDisplayVictory;
	float timeDisplayingVictoryMessageSeconds;

	public event Action OnClickedMenu;

	// Use this for initialization
	void Start () {
	
	}

	void Destroy() {
		mInstance = null;
	}

	void OnGUI() {
		GUI.skin.button.fontSize = (int)(Screen.height * 0.04f);

		if (GUI.Button (menuButtonRect, MENU)) {
			if (OnClickedMenu != null) {
				OnClickedMenu();
			}

			Application.LoadLevel("menu");
		}

		// Only levels displaying status
		if (Application.loadedLevelName.StartsWith("level")) {
			string statusText  = "You Collected: " + mMyAmount;
			if (mHisAmount != -1) {
				statusText += " your opponent collected: " + mHisAmount; 
			}
			if (shouldDisplayVictory) {
				if (mWinnerName != null) {
					statusText = mWinnerName + " Wins!";
				} else {
					statusText = "You Win!";
				}
			}
			GUI.TextArea(statusTextRect, statusText);
		}
	}


	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape)) { 
			if (GameState.CurrentGameType != GameState.GameType.SinglePlayer) {
				PhotonNetwork.Disconnect();
			}

			var dict = new Dictionary<string, double>();
			dict["QuitToMenu"] = 1.0;
			Report.Instance.ReportLevelFailed(Application.loadedLevelName, dict);
			Application.LoadLevel("menu");
		}

		if (shouldDisplayVictory) {
			timeDisplayingVictoryMessageSeconds += Time.deltaTime;
			
			if (timeDisplayingVictoryMessageSeconds > VICTORY_MESSAGE_DELAY_SECONDS) {
				GameController.Instnace.GoToNextLevel();
			}
		}
	}

	public void OnCollected(int myAmount, int hisAmount = -1) {
		mMyAmount = myAmount;
		mHisAmount = hisAmount;
	}

	/// <summary>
	/// Victory the specified winnerName.
	/// </summary>
	/// <param name="winnerName">Winner name, if left out it means you are the winner.</param>
	public void Victory (string winnerName = null)
	{
		mWinnerName = winnerName;
		shouldDisplayVictory = true;
	}
}
