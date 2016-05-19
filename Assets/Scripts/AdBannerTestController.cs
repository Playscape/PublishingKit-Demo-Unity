using UnityEngine;
using Playscape.Internal;
using Playscape.Ads;
using System.Collections;

public class AdBannerTestController : MonoBehaviour {

	const string MENU_TITLE = "AD VIDEO TEST SCENE";
	const string SHOW_BANNER_TOP_MIDDLE = "SHOW BANNER TOP MIDDLE";
	const string SHOW_BANNER_TOP_LEFT = "SHOW BANNER TOP LEFT";
	const string SHOW_BANNER_TOP_RIGHT = "SHOW BANNER TOP RIGHT";
	const string SHOW_BANNER_BOTTOM_MIDDLE = "SHOW BANNER BOTTOM MIDDLE";
	const string SHOW_BANNER_BOTTOM_LEFT = "SHOW BANNER BOTTOM LEFT";
	const string SHOW_BANNER_BOTTOM_RIGHT = "SHOW BANNER BOTTOM RIGHT";
	const string HIDE_BANNER = "HIDE BANNER";
	
	void OnGUI () {
		float boxWidth = Screen.width / 1.5f;
		float boxHeight = Screen.height * 0.95f;
		
		GUI.skin.button.fontSize = (int)(Screen.height * 0.044f);
		GUI.skin.box.fontSize  = (int)(Screen.height * 0.07);
		
		GUI.Box (new Rect (Screen.width / 2 - boxWidth / 2, 10, boxWidth, boxHeight), MENU_TITLE);
		
		float buttonWidth = boxWidth * 0.85f;
		float buttonHeight = boxHeight / 14;
		float marginTop = 80;
		
		DrawMenu (buttonWidth, buttonHeight, marginTop);
	}
	
	static void DrawMenu (float buttonWidth, float buttonHeight, float marginTop)
	{
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, marginTop, buttonWidth, buttonHeight), SHOW_BANNER_TOP_MIDDLE)) {
			Banners.Instance.Display (Banners.BannerAlignment.topMiddle, "top-middle");
		}
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight + marginTop * 1.1f, buttonWidth, buttonHeight), SHOW_BANNER_TOP_LEFT)) {
			Banners.Instance.Display (Banners.BannerAlignment.topLeft, "top-left");
		}
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 2 + marginTop * 1.2f, buttonWidth, buttonHeight), SHOW_BANNER_TOP_RIGHT)) {
			Banners.Instance.Display (Banners.BannerAlignment.topRight, "top-right");
		}
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 3 + marginTop * 1.3f, buttonWidth, buttonHeight), SHOW_BANNER_BOTTOM_MIDDLE)) {
			Banners.Instance.Display (Banners.BannerAlignment.bottomMiddle, "bottom-middle");
		}
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 4 + marginTop * 1.4f, buttonWidth, buttonHeight), SHOW_BANNER_BOTTOM_LEFT)) {
			Banners.Instance.Display (Banners.BannerAlignment.bottomLeft, "bottom-left");
		}
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 5 + marginTop * 1.5f, buttonWidth, buttonHeight), SHOW_BANNER_BOTTOM_RIGHT)) {
			Banners.Instance.Display (Banners.BannerAlignment.bottomRight, "bottom-right");
		}
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 6 + marginTop * 1.6f, buttonWidth, buttonHeight), HIDE_BANNER)) {
			Banners.Instance.Hide();
		}

	}
}
