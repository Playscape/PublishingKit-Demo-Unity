using UnityEngine;
using Playscape.Internal;
using Playscape.Ads;
using System.Collections;

public class AdVideoTestController : MonoBehaviour {

	const string MENU_TITLE = "AD VIDEO TEST SCENE";
	const string SHOW = "SHOW NONINCENTIVISED VIDEO";
	
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
			Videos.Instance.OnDisplayEnded += (Videos.State state, Videos.Kind kind) => {
				L.I ("Unity. OnDisplayVideoEnded(state: {0}, kind: {1})", state, kind);
			};

			Videos.Instance.Display(Videos.Kind.Incentivised, "main-menu");
		}
	}
}
