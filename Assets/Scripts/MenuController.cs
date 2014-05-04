﻿using UnityEngine;
using System.Collections;
using System.Text;
using Chartboost;
using Playscape.Notifications;

public class MenuController : MonoBehaviour {

	const string MENU_TITLE = "ROLL-A-BALL MULTIPLAYER";
	const string PLAY = "PLAY";
	const string PLAY_A_RANDOM_GAME = "PLAY A RANDOM GAME";
	const string PLAY_WITH_FRIENDS = "PLAY GAME WITH FRIENDS";
	const string STORE = "STORE";
	const string QUIT = "QUIT";

	const string SHOW_INTERSTITIAL = "SHOW CHARTBOOST INTERSTITIAL";

	bool mSocialRequestReceived;

	// Use this for initialization
	void Start () {
		SocialController.Instance.OnSocialRequestReceived += HandleOnSocialRequestReceived;


	}

	void Destroy() {
		StopCoroutine( "ChangeLoadChar" );
		SocialController.Instance.OnSocialRequestReceived -= HandleOnSocialRequestReceived;
	}

	void HandleOnSocialRequestReceived ()
	{
		if (SocialController.Instance.IsLoggedIn) {
			mSocialRequestReceived = true;
			StartCoroutine( ChangeLoadChar());
		}
	}

	void OnGUI () {
		float boxWidth = Screen.width / 1.5f;
		float boxHeight = Screen.height * 0.95f;

		GUI.skin.button.fontSize = (int)(Screen.height * 0.044f);
		GUI.skin.box.fontSize  = (int)(Screen.height * 0.07);

		GUI.Box (new Rect (Screen.width / 2 - boxWidth / 2, 10, boxWidth, boxHeight), MENU_TITLE);

		float buttonWidth = boxWidth * 0.85f;
		float buttonHeight = boxHeight / 10;
		float marginTop = 80;


		if (!mSocialRequestReceived) {
			DrawMenu (buttonWidth, buttonHeight, marginTop);
		} else {
			DrawStartingGameWithFriendsBox();
		}
	}


	void DrawStartingGameWithFriendsBox ()
	{


		GUI.Box(new Rect(0, Screen.height/2 - Screen.height/8, Screen.width, Screen.height/4), "Starting Game " + loadChars[currLoadChar]);
	}

	static void DrawMenu (float buttonWidth, float buttonHeight, float marginTop)
	{
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, marginTop, buttonWidth, buttonHeight), PLAY)) {
			GameState.CurrentGameType = GameState.GameType.SinglePlayer;
			Application.LoadLevel ("level1");
		}
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight + marginTop * 1.2f, buttonWidth, buttonHeight), PLAY_A_RANDOM_GAME)) {
			GameState.IsHost = true;
			GameState.CurrentGameType = GameState.GameType.MultiplayerPublicGame;
			Application.LoadLevel ("lobby");
		}
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 2 + marginTop * 1.4f, buttonWidth, buttonHeight), PLAY_WITH_FRIENDS)) {
			GameState.IsHost = false;
			GameState.CurrentGameType = GameState.GameType.MultiplayerPrivateGame;
			Application.LoadLevel ("invite_screen");
		}

		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 3 + marginTop * 1.6f, buttonWidth, buttonHeight), STORE)) {
			Application.LoadLevel ("store");
		}

		
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 4 + marginTop * 1.8f, buttonWidth, buttonHeight), SHOW_INTERSTITIAL)) {
			#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
			CBBinding.showInterstitial(null);
			#endif
		}

		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 5 + marginTop * 2.0f, buttonWidth, buttonHeight), "Set PW custom tags")) {
			int randomValue = (int) Random.Range(0, 20000);

			PushWoosh.Instance.SetTag("customTag1", randomValue.ToString());
			PushWoosh.Instance.SetTag("customTag2", (randomValue + 1).ToString());
			PushWoosh.Instance.SetTag("customTag3Numeric", randomValue + 2);
			PushWoosh.Instance.SetTag("customTag4Numeric", randomValue + 4);
		}

		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 6 + marginTop * 2.2f, buttonWidth, buttonHeight), QUIT)) {
			Application.Quit ();
		}
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape)) { 
			Application.Quit(); 
		}

	}

	char[] loadChars = {'/', '-', '\\'};
	int currLoadChar = 0;
	private IEnumerator ChangeLoadChar() {
		for (;;) {
			currLoadChar = (currLoadChar + 1 ) %  (loadChars.Length);
			yield return new WaitForSeconds(.2f);
		}
	}
}
