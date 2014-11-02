using UnityEngine;
using System.Collections;
using Playscape.ABTesting;

public class ABTestingController : MonoBehaviour {

	private const string MENU_TITLE = "AB Testing";

	private string experimentName = "AB Testing Experiment Name Place Holder";
	private string experimentGroup = "AB Testing Experiment Group Place Holder";
	private string experimentVariableName1 = "AB Testing Experiment Variable Name 1 Place Holder";
	private string experimentVariableValue1 = "AB Testing Experiment Variable value 1 Place Holder";
	private string experimentVariableName2 = "AB Testing Experiment Variable Name 2 Place Holder";
	private string experimentVariableValue2 = "AB Testing Experiment Variable value 2 Place Holder";

	void OnGUI () {
		float boxWidth = Screen.width / 1.5f;
		float boxHeight = Screen.height * 0.95f;
		
		GUI.skin.button.fontSize = (int)(Screen.height * 0.044f);
		GUI.skin.box.fontSize  = (int)(Screen.height * 0.07);
		
		GUI.Box (new Rect (Screen.width / 2 - boxWidth / 2, 10, boxWidth, boxHeight), MENU_TITLE);
		
		float buttonWidth = boxWidth * 0.85f;
		float buttonHeight = boxHeight / 11;
		float marginTop = 80;
		
		DrawMenu (buttonWidth, buttonHeight, marginTop);

	}
	
	void DrawMenu (float buttonWidth, float buttonHeight, float marginTop)
	{
		experimentName  = GUI.TextField (new Rect (Screen.width / 2 - buttonWidth / 2, marginTop, buttonWidth, buttonHeight), experimentName);
		experimentGroup = GUI.TextField (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight + marginTop * 1.2f, buttonWidth, buttonHeight), experimentGroup);
		experimentVariableName1 = GUI.TextField (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 2 + marginTop * 1.4f, buttonWidth, buttonHeight), experimentVariableName1);
		experimentVariableValue1 = GUI.TextField (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 3 + marginTop * 1.6f, buttonWidth, buttonHeight), experimentVariableValue1);
		experimentVariableName2 = GUI.TextField (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 4 + marginTop * 1.8f, buttonWidth, buttonHeight), experimentVariableName2);
		experimentVariableValue2 = GUI.TextField (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 5 + marginTop * 2.0f, buttonWidth, buttonHeight),experimentVariableValue2 );


		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 6 + marginTop * 2.2f, buttonWidth, buttonHeight), "Load Experiment")) {
			ABTesing.Instance.getExperimentDataByExperimentNameAsync("Experiment 1",new ABTesing.OnExperimentDataArrivedDelegate(OnExperimentDataArrived));
		}
		
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 7 + marginTop * 2.4f, buttonWidth, buttonHeight), "Report Experiment")) {
		}
	}

	private void UpdateTextFields(ExperimentData data) 
	{
		if (data != null)
		{
			experimentName = data.getExperimentName();
			experimentGroup = data.getExperimentGroup();

			experimentVariableName1 = "Roll A Ball 1";
			experimentVariableName2 = "Roll A Ball 2";
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
		}
	}


	public void OnExperimentDataArrived(ExperimentData data) 
	{
		UpdateTextFields (data);
	}
}
