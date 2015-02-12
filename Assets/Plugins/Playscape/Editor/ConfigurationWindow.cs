﻿#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;
using Playscape.Internal;

namespace Playscape.Editor {
	public class ConfigurationWindow : EditorWindow {
		private const string APP_ID = "App Id";
		private const string ANALYTICS_REPORTING = "Analytics Reporting";
		private const string REPORTER_ID = "Reporter Id";
		private const string PUSHWOOSH_CONFIG = "PushWoosh Configuration";
		private const string ANDROID = "Android";
		private const string IOS = "iOS";
		private const string WINDOW_TITLE = "Playscape";
		private const string CLOSE = "Close";
		private Vector2  scrollPos;

		private const string AB_TESTING_TITLE = "AB Testing Configuration";

		void OnGUI () {
			title = WINDOW_TITLE;

            GUI.changed = false;
            
			EditorGUILayout.BeginHorizontal();
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width (650), GUILayout.Height (350));
				OnReportingGUI ();
				OnPushWooshGUI ();
				OnAdsGUI ();
				OnABTestingGUI ();
			EditorGUILayout.EndScrollView ();
			EditorGUILayout.EndHorizontal ();

			GUILayout.Space (30);

			if (GUILayout.Button (CLOSE)) {
				Close ();
			}
            
            if (GUI.changed) {
				EditorUtility.SetDirty (ConfigurationInEditor.Instance);
			}
		}

		private void OnABTestingGUI ()
		{
			GUILayout.Label (AB_TESTING_TITLE, EditorStyles.boldLabel);

			bool enableABTestingSystem = EditorGUILayout.ToggleLeft ("Enable AB Testing System ", ConfigurationInEditor.Instance.MyABTesting.EnableABTestingSystem);
			ConfigurationInEditor.Instance.MyABTesting.EnableABTestingSystem = enableABTestingSystem;

			string result = EditorGUILayout.TextField ("Amazon Public Key", ConfigurationInEditor.Instance.MyABTesting.AmazonPublicKey);
			if (result != null) {
				ConfigurationInEditor.Instance.MyABTesting.AmazonPublicKey = result;
			}

			result = EditorGUILayout.TextField ("Amazon private Key", ConfigurationInEditor.Instance.MyABTesting.AmazonPrivateKey);
			if (result != null) {
				ConfigurationInEditor.Instance.MyABTesting.AmazonPrivateKey = result;
			}

			int NumberOfExperiments = EditorGUILayout.IntSlider ("Custom Experiments", ConfigurationInEditor.Instance.MyABTesting.NumberOfCustomExperiments, 0, Configuration.MAX_NUMBER_OF_EXPERIMENTS);
			ConfigurationInEditor.Instance.MyABTesting.NumberOfCustomExperiments = NumberOfExperiments;

			for (int i = 0; i < NumberOfExperiments; i++)
			{
				GUILayout.Label ("Experiment " + (i + 1).ToString(), EditorStyles.boldLabel);
				string res = EditorGUILayout.TextField ("Experiment Name", ConfigurationInEditor.Instance.MyABTesting.MyCustomExperimentConfig[i].ExperimentName);
				if (result != null) {
					ConfigurationInEditor.Instance.MyABTesting.MyCustomExperimentConfig[i].ExperimentName = res;
				}
				int NumberOfVars = EditorGUILayout.IntSlider ("Number Of Variables", ConfigurationInEditor.Instance.MyABTesting.MyCustomExperimentConfig[i].NumberOfVarsInExperiment, 1, Configuration.MAX_NUMBER_OF_VARS_INEXPERIMENT);
				ConfigurationInEditor.Instance.MyABTesting.MyCustomExperimentConfig[i].NumberOfVarsInExperiment = NumberOfVars;

				for (int j = 0; j <  NumberOfVars; j++)
				{
					string varName = EditorGUILayout.TextField ("Varaible " + (j + 1).ToString(), ConfigurationInEditor.Instance.MyABTesting.MyCustomExperimentConfig[i].ExperimentVars[j]);
					if (varName != null)
					{
						ConfigurationInEditor.Instance.MyABTesting.MyCustomExperimentConfig[i].ExperimentVars[j] = varName;
					}
				}
			}
		}

		private void OnReportingGUI ()
		{
			GUILayout.Label (ANALYTICS_REPORTING, EditorStyles.boldLabel);
			string result = EditorGUILayout.TextField (REPORTER_ID, ConfigurationInEditor.Instance.ReporterId);
			if (result != null) {
				ConfigurationInEditor.Instance.ReporterId = result;
			}
		}

		private void OnPushWooshGUI ()
		{
			GUILayout.Label (PUSHWOOSH_CONFIG, EditorStyles.boldLabel);
			string result = EditorGUILayout.TextField (IOS + " " + APP_ID, ConfigurationInEditor.Instance.PushWooshIosId);
			if (result != null) {
				ConfigurationInEditor.Instance.PushWooshIosId = result;
			}
		}

		void OnAdsGUI ()
		{
			var categories = new Dictionary<object, bool> ();
			ConfigurationInEditor.Instance.TraverseAdsConfig ((category, fieldInfo) =>
						{
							if (!categories.ContainsKey(category)) {
								GUILayout.Label (category.GetType().Name, EditorStyles.boldLabel);
								categories.Add(category, true);
							}
                            
                            if (fieldInfo.FieldType == typeof(string)) {
                                string result = EditorGUILayout.TextField (fieldInfo.Name, fieldInfo.GetValue(category) as string);
                                if (result != null) {
								fieldInfo.SetValue(category, result);
                                }   
                            } else if (fieldInfo.FieldType == typeof(bool)) {
                                bool currentValue =  (bool) fieldInfo.GetValue(category);
                                bool result  = EditorGUILayout.ToggleLeft (fieldInfo.Name,currentValue);
                                fieldInfo.SetValue(category, result);
                            }
						});
					
		}
	}
}
#endif