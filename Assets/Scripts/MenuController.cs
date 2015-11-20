using UnityEngine;
using System.Collections;
using System.Text;
using Playscape.Ads;
using Soomla.Store;
using Playscape.Internal;

public class MenuController : MonoBehaviour {

	const string MENU_TITLE = "ROLL-A-BALL MULTIPLAYER";
	const string PLAY = "PLAY";
	const string PLAY_A_RANDOM_GAME = "PLAY A RANDOM GAME";
	const string PLAY_WITH_FRIENDS = "PLAY GAME WITH FRIENDS";
	const string STORE = "STORE";
	const string SHOW_INTESITIAL = "Show Interstitial";
	const string DISABLE_ADS = "Disable ads";
	const string ENABLE_ADS = "Enable ads";
	const string SET_PW_CUSTOM_TAG = "Set PW custom tags";
	const string SHOW_CATALOG = "Show Catalog";
	const string GET_A_TESTING_VALUES = "Get AB Testing Values";
	const string QUIT = "QUIT";



	bool mSocialRequestReceived;

	// Use this for initialization
	void Start () {
		SocialController.Instance.OnSocialRequestReceived += HandleOnSocialRequestReceived;
		Banners.Instance.Hide ();
	}

	void Destroy() {
		SocialController.Instance.OnSocialRequestReceived -= HandleOnSocialRequestReceived;
		StopCoroutine( "ChangeLoadChar" );
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
		float buttonHeight = boxHeight / 14;
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
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight + marginTop * 1.4f, buttonWidth, buttonHeight), PLAY_A_RANDOM_GAME)) {
			GameState.IsHost = true;
			GameState.CurrentGameType = GameState.GameType.MultiplayerPublicGame;
			Application.LoadLevel ("lobby");
		}
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 2 + marginTop * 1.4f, buttonWidth, buttonHeight), PLAY_WITH_FRIENDS)) {
			GameState.IsHost = false;
			GameState.CurrentGameType = GameState.GameType.MultiplayerPrivateGame;
			Application.LoadLevel ("invite_screen");
		}

		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 3 + marginTop * 1.4f, buttonWidth, buttonHeight), STORE)) {
			Application.LoadLevel ("store");
		}

		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 4 + marginTop * 1.4f, buttonWidth, buttonHeight), SHOW_INTESITIAL)) {
			Playscape.Ads.Interstitials.Instance.Display("main-menu");
		}

		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 5 + marginTop * 1.4f, buttonWidth, buttonHeight), DISABLE_ADS)) {
			AdManager.Instance.DisableAds();
		}

		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 6 + marginTop * 1.4f, buttonWidth, buttonHeight), ENABLE_ADS)) {
			AdManager.Instance.EnableAds();
		}

		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 8 + marginTop * 1.4f, buttonWidth, buttonHeight), GET_A_TESTING_VALUES)) {
			Application.LoadLevel("ab_testing");
		}

        if (GUI.Button(new Rect(Screen.width / 2 - buttonWidth / 2, buttonHeight * 9 + marginTop * 2.4f, buttonWidth, buttonHeight), SHOW_CATALOG))
        {
            Playscape.Catalog.Catalog.Instance.showCatalog();
        }

		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 10 + marginTop * 2.6f, buttonWidth, buttonHeight), QUIT)) {
			Application.Quit ();
		}
	}

	// Update is called once per frame
	public void Update() {
	   if (Input.GetKeyUp(KeyCode.Escape)) {
	       if (Interstitials.Instance.OnBackPressed()) {
	            return;
	        } else {
	       		Application.Quit();
	         }
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
