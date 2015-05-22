#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;
using Playscape.Internal;
using System.Text.RegularExpressions;

namespace Playscape.Editor {
    public class ConfigurationWindow : ThreadingEditorWindow
    {
		private const string APP_ID = "App Id";
		private const string ANALYTICS_REPORTING = "Analytics Reporting";
		private const string REPORTER_ID = "Reporter Id";
		private const string PUSHWOOSH_CONFIG = "PushWoosh Configuration";
		private const string ADS_CONFIG = "Ads Configuration";
		private const string ANDROID = "Android";
		private const string IOS = "iOS";
		private const string WINDOW_TITLE = "Playscape";
		private const string CLOSE = "Apply Changes";
        private const string TEST_BUILD = "Test";
		private Vector2  scrollPos;

		private const string AB_TESTING_TITLE = "AB Testing Configuration";

		void OnGUI () {
			title = WINDOW_TITLE;

            GUI.changed = false;

			EditorGUILayout.BeginHorizontal();
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width (650), GUILayout.Height (350));				
				OnAdsGUI ();
				OnABTestingGUI ();
			EditorGUILayout.EndScrollView ();
			EditorGUILayout.EndHorizontal ();

			GUILayout.Space (30);

			if (GUILayout.Button (CLOSE)) {
				onClose();
			}


            if (GUILayout.Button(TEST_BUILD))
            {
                TestBuild();
            }

            if (GUI.changed) {
				EditorUtility.SetDirty (ConfigurationInEditor.Instance);
			}
		}

		private void onClose() {
            WarningAccumulator warningAccumulator = new WarningAccumulator();
            warningAccumulator.WarnIfStringIsEmpty(
                ConfigurationInEditor.Instance.MyAds.MyAdsConfig.ApiKey,
                Warnings.ADS_API_KEY_NOT_SET
            );
			L.E ("API_KEY = {0}", ConfigurationInEditor.Instance.MyAds.MyAdsConfig.ApiKey);
			L.E ("REPORTER_ID = {0}", ConfigurationInEditor.Instance.ReporterId);

            if (warningAccumulator.HasWarnings())
            {
                warningAccumulator.ShowIfNecessary();
            }
            else
            {
				ApplyChanges();
            }
		}

        private void TestBuild()
        {
			AndroidPostProcessor app = new AndroidPostProcessor(@"/Users/artem/Desktop/test/Untitled.apk");
			
			EditorUtility.ClearProgressBar();
            EditorUtility.DisplayProgressBar("Publishing kit post-process", "Build Started", 0);

            app.build(false, completed, onProgress);
        }

        private void onProgress(object sender, string info, int progress)
        {
            if (!isUIThread)
            {
                this.RunOnUIThread(new BuildProcess.BuildProgressChanged(onProgress), new object[] { sender, info, progress });
                return;
            }

            EditorUtility.DisplayProgressBar("Publishing kit post-process", info, (float)progress / 100);
        }

		private void completed (object sender) {
            if (!isUIThread)
            {
                this.RunOnUIThread(new BuildProcess.BuildCompleted(completed), new object[] { sender });
                return;
            }

            EditorUtility.ClearProgressBar();
        }

//		/**
//		 * Method which download game configuration for API KEY setted in Game Configuration Window
//		 *
//		 **/
//		private void FetchAndApplyGameConfiguration() {
//			if (string.IsNullOrEmpty (CommonConsts.GAME_CONFIGURATION_API_URL)) {
//				EditorUtility.DisplayDialog("Playscape Configuration Proccess",
//				                            "Error!!! Can't find 'GAME API' url",
//				                            "OK");
//
//				return;
//			}
//
//			string API_KEY = ConfigurationInEditor.Instance.MyAds.MyAdsConfig.ApiKey;
//			bool applyLastSavedConfiguration = false;
//
//			try {
//				// CR-ZR: Your both displaying the progress bar and downloading the configuration from the main thread
//				//          Shouldn't you use a background thread in order to keep the UI free?
//				//Show progress dialog to user
//				GUI.enabled = false;
//				
//				EditorUtility.ClearProgressBar ();
//				EditorUtility.DisplayProgressBar ("Playscape Configuration Proccess",
//				                                  String.Format("Fetching Game Configuration for 'ApiKey': '{0}'", API_KEY),
//				                                  0);
//				
//				Configuration.GameConfigurationResponse response = ConfigurationInEditor.Instance.FetchGameConfigurationForApiKey (Configuration.Instance.MyAds.MyAdsConfig.ApiKey);;
//				
//				//If response from servers is success save fetched configuration to AssetDatabse
//				if (response != null) {
//					if (response.Success) {
//						ConfigurationInEditor.Instance.MyGameConfiguration = response.GameConfiguration;
//						
//						//Saving new fetched game configuration to the file-system
//						AssetDatabase.SaveAssets();
//					} else {
//						EditorUtility.DisplayDialog("Playscape Configuration Proccess",
//						                            "Error!!! Could not retrieve configuration from the server. Message: " + response.ErrorDescription + "\n\n Last saved game configuration will be applied.",
//						                            "OK");
//						applyLastSavedConfiguration = true;
//					}
//				} else {
//					L.W("Warning!!! Could not download game configuration. Please check your internet connection");
//					
//					applyLastSavedConfiguration = true;
//				}
//
//			} finally {
//				GUI.enabled = true;
//				EditorUtility.ClearProgressBar();
//				
//				
//				//In any case we apply changes. If request to GAME API was failed, successfully last saved configuration will be applied.
//				ApplyChanges (applyLastSavedConfiguration);
//			}
//		}

		private void ApplyChanges() {
			string targetManifest = "Assets/Plugins/Android/PlayscapePublishingKit/AndroidManifest.xml";

			string manifestContents = File.ReadAllText(CommonConsts.PLAYSCAPE_MANIFEST_PATH);
			manifestContents = AndroidPostProcessor.ApplyCommonAndroidManifestParams(manifestContents);
			File.WriteAllText(targetManifest, manifestContents);

			targetManifest = "Assets/Plugins/Android/AndroidManifest.xml";
			AndroidManifestMerger.Merge (targetManifest, false);

			EditorUtility.DisplayDialog(
				"Configuration Ended",
				"The configuration process has ended successfully",
				"OK");

			Close ();
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

		void OnAdsGUI ()
		{
			var categories = new Dictionary<object, bool> ();
			ConfigurationInEditor.Instance.TraverseAdsUIConfig ((category, fieldInfo) =>
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
