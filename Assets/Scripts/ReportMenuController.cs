using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Playscape.Analytics;

public class ReportMenuController : MonoBehaviour {

	const string MENU_TITLE = "ROLL-A-BALL REPORT SCENE";
	const string REPORT_EVENT = "Report Event";
	const string REPORT_ATTR_EVENT = "Report Event With Attributes";
	
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
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, marginTop, buttonWidth, buttonHeight), REPORT_EVENT)) {
			Report.Instance.ReportEvent("custom_event");
			AppsFlyer.trackEvent ("custom_event", "custom_event_value");
		}
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight + marginTop * 1.4f, buttonWidth, buttonHeight), REPORT_ATTR_EVENT)) {
			Dictionary<string, string> eventAttrs = new Dictionary<string, string>()
			{
				{ "key_1", "value_1" },
				{ "key_2", "value_2" }
			};

			Report.Instance.ReportEvent("custom_event", eventAttrs);
			AppsFlyer.trackRichEvent ("custom_event", eventAttrs);
		}
	}
}
