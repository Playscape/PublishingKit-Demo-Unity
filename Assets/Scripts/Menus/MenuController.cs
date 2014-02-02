using UnityEngine;
using System.Collections;

public class MenuController : MonoBehaviour {

	const string MENU_TITLE = "ROLL-A-BALL MULTIPLAYER";
	const string PLAY = "PLAY";
	const string PLAY_A_RANDOM_GAME = "PLAY A RANDOM GAME";
	const string PLAY_WITH_FRIENDS = "PLAY GAME WITH FRIENDS";
	const string QUIT = "QUIT";

	// Use this for initialization
	void Start () {
	
	}

	void OnGUI () {
		float boxWidth = Screen.width / 1.5f;
		float boxHeight = Screen.height * 0.95f;

		GUI.skin.button.fontSize = (int)(Screen.height * 0.06f);
		GUI.skin.box.fontSize  = (int)(Screen.height * 0.07);

		GUI.Box (new Rect (Screen.width / 2 - boxWidth / 2, 10, boxWidth, boxHeight), MENU_TITLE);

		float buttonWidth = boxWidth * 0.85f;
		float buttonHeight = boxHeight / 6;
		float marginTop = 80;

		if(GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, marginTop, buttonWidth, buttonHeight), PLAY)) {
			Application.LoadLevel(1);
		}

		if(GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight + marginTop * 1.2f, buttonWidth, buttonHeight), PLAY_A_RANDOM_GAME)) {
			Application.LoadLevel(1);
		}

		if(GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 2 + marginTop * 1.4f, buttonWidth, buttonHeight), PLAY_WITH_FRIENDS)) {
			Application.LoadLevel(1);
		}
		
		if(GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 3 + marginTop * 1.6f, buttonWidth, buttonHeight), QUIT)) {
			Application.Quit();
		}

	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape)) { 
			Application.Quit(); 
		}
	}
}
