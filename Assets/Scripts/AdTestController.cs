using UnityEngine;
using Playscape.Ads;
using System.Collections;

public class AdTestController : MonoBehaviour {

	const string MENU_TITLE = "ROLL-A-BALL AD TEST SCENE";
	const string OPEN_BANNER_TEST = "OPEN BANNER TEST";
	const string OPEN_VIDEO_TEST = "OPEN VIDEO TEST";
	const string OPEN_INTERSTITIAL_TEST = "OPEN INTERSTITIAL TEST";
	const string DISABLE_AD	= "DISABLE AD";
	const string ENABLE_AD 	= "ENABLE AD";
	
	void OnGUI () {
		float boxWidth = Screen.width / 1.5f;
		float boxHeight = Screen.height * 0.95f;
		
		GUI.skin.button.fontSize = (int)(Screen.height * 0.044f);
		GUI.skin.box.fontSize  = (int)(Screen.height * 0.07);
		
		GUI.Box (new Rect (Screen.width / 2 - boxWidth / 2, 10, boxWidth, boxHeight), MENU_TITLE);
		
		float buttonWidth = boxWidth * 0.85f;
		float buttonHeight = boxHeight / 14;
		float marginTop = 180;
		
		DrawMenu (buttonWidth, buttonHeight, marginTop);
	}
	
	static void DrawMenu (float buttonWidth, float buttonHeight, float marginTop)
	{
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, marginTop, buttonWidth, buttonHeight), OPEN_BANNER_TEST)) {
			Application.LoadLevel("ad_banner_ad_test");
		}
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight + marginTop * 1.1f, buttonWidth, buttonHeight), OPEN_VIDEO_TEST)) {
			Application.LoadLevel("ad_video_ad_test");
		}
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 2 + marginTop * 1.2f, buttonWidth, buttonHeight), OPEN_INTERSTITIAL_TEST)) {
			Application.LoadLevel("ad_interstitial_test");
		}
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 3 + marginTop * 1.3f, buttonWidth, buttonHeight), DISABLE_AD)) {
			AdManager.Instance.DisableAds();
		}
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 4 + marginTop * 1.4f, buttonWidth, buttonHeight), ENABLE_AD)) {
			AdManager.Instance.EnableAds();
		}
	}
}
