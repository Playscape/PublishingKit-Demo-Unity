using UnityEngine;
using System.Collections;
using Playscape.ABTesting;

public class ABTestingController : MonoBehaviour {

	private const string MENU_TITLE = "AB Testing";

	private string experiment1str = "";
	private string experiment2str = "";

	void OnGUI () {
		float boxWidth = Screen.width / 1.05f;
		float boxHeight = Screen.height * 0.95f;
		
		GUI.skin.button.fontSize = (int)(Screen.height * 0.044f);
		GUI.skin.box.fontSize  = (int)(Screen.height * 0.07);
		
		GUI.Box (new Rect (Screen.width / 2 - boxWidth / 2, 10, boxWidth, boxHeight), MENU_TITLE);
		
		float buttonWidth = boxWidth * 0.85f;
		float buttonHeight = boxHeight / 7;
		float marginTop = 80;
		
		DrawMenu (buttonWidth, buttonHeight, marginTop);

	}
	
	void DrawMenu (float buttonWidth, float buttonHeight, float marginTop)
	{

		experiment1str  = GUI.TextField (new Rect (Screen.width / 2 - buttonWidth / 2, marginTop, buttonWidth, buttonHeight), experiment1str);
		experiment2str = GUI.TextField (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight + marginTop * 1.2f, buttonWidth, buttonHeight), experiment2str);


		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 2 + marginTop * 1.8f, buttonWidth / 3, buttonHeight), "Load Exp 1")) {
			ABTesing.Instance.getExperimentDataByExperimentNameAsync("Experiment 1",new ABTesing.OnExperimentDataArrivedDelegate(OnExperimentDataArrived));
		}
		
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2 + buttonWidth / 3, buttonHeight * 2 + marginTop * 1.8f, buttonWidth / 3, buttonHeight), "Report View Exp 1")) {
			ABTesing.Instance.reportExperimentEvent("Experiment 1","RollABallViewEvent");
		}

		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2 + buttonWidth / 3 + buttonWidth / 3 , buttonHeight * 2+ marginTop * 1.8f, buttonWidth / 3, buttonHeight), "Report Goal Exp 1")) {
			ABTesing.Instance.reportExperimentEvent("Experiment 1","RollABallGoal");
		}

		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 3 + marginTop * 2.2f, buttonWidth / 3, buttonHeight), "Load Exp 2")) {
			ABTesing.Instance.getExperimentDataByExperimentNameAsync("Experiment 2",new ABTesing.OnExperimentDataArrivedDelegate(OnExperimentDataArrived));
		}
		
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2 + buttonWidth / 3, buttonHeight * 3 + marginTop * 2.2f, buttonWidth / 3, buttonHeight), "Report View Exp 2")) {
			ABTesing.Instance.reportExperimentEvent("Experiment 2","RollABallViewEvent2");
		}
		
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2 + buttonWidth / 3 + buttonWidth / 3 , buttonHeight * 3+ marginTop * 2.2f, buttonWidth / 3, buttonHeight), "Report Goal Exp 2")) {
			ABTesing.Instance.reportExperimentEvent("Experiment 2","RollABallGoal2");
		}

		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2  , buttonHeight * 4+ marginTop * 2.3f, buttonWidth , buttonHeight), "Menu")) {
			Application.LoadLevel("menu");
		}
	}

	private void UpdateTextFields(ExperimentData data) 
	{
		if (data != null)
		{
			string experimentName = data.getExperimentName();
			string experimentGroup = data.getExperimentGroup();

			string experimentVariableName1 = "Roll A Ball 1";
			string experimentVariableName2 = "Roll A Ball 2";
			string experimentVariableValue1;
			string experimentVariableValue2;
			if (data.getExperimentValues().ContainsKey(experimentVariableName1))
			{
				experimentVariableValue1 = data.getExperimentValues()[experimentVariableName1];
			}
			else
			{
				experimentVariableValue1 = "unable to read data from serer";
			}

			if (data.getExperimentValues().ContainsKey(experimentVariableName2))
			{
				experimentVariableValue2 = data.getExperimentValues()[experimentVariableName2];
			}
			else
			{
				experimentVariableValue2 = "unable to read data from serer";
			}
			string output = "Name: " + experimentName + " Group: " + experimentGroup  + "\r\n" + "{" + experimentVariableName1 + 
				": " + experimentVariableValue1 + "},{" + experimentVariableName2  + ":" + experimentVariableValue2 + "}";

			if (experimentName == "Experiment 1"){
				experiment1str = output;
			}
			else {
				experiment2str = output;
			}
		}
	}


	public void OnExperimentDataArrived(ExperimentData data) 
	{
		UpdateTextFields (data);
	}
}
