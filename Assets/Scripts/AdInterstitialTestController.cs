using UnityEngine;
using Playscape.Ads;
using Playscape.Internal;
using System.Collections;

public class AdInterstitialTestController : MonoBehaviour {

	const string MENU_TITLE = "AD INTERSTITIAL TEST SCENE";
	const string SHOW = "SHOW INTERSTITIAL OVERLAY/NONOVERLAY";
	
	void OnGUI () {
		float boxWidth = Screen.width / 1.5f;
		float boxHeight = Screen.height * 0.95f;
		
		GUI.skin.button.fontSize = (int)(Screen.height * 0.044f);
		GUI.skin.box.fontSize  = (int)(Screen.height * 0.07);
		
		GUI.Box (new Rect (Screen.width / 2 - boxWidth / 2, 10, boxWidth, boxHeight), MENU_TITLE);
		
		float buttonWidth = boxWidth * 0.85f;
		float buttonHeight = boxHeight / 14;
		float marginTop = 280;
		
		DrawMenu (buttonWidth, buttonHeight, marginTop);
	}
	
	static void DrawMenu (float buttonWidth, float buttonHeight, float marginTop)
	{
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, marginTop, buttonWidth, buttonHeight), SHOW)) {
			Interstitials.Instance.OnShown += (Interstitials.Kind kind) =>  {
				L.I ("Unity. onShow(kind: {0})", kind);
			};

			Interstitials.Instance.OnDisplayEnded += (Interstitials.State state, Interstitials.Kind kind) => {
				L.I ("Unity. OnDisplayEnded(state: {0}, kind: {1})", state, kind);
			};

			Interstitials.Instance.Display("main-menu");			 
		}
	}
}
