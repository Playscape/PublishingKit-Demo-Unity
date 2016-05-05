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
        private const string PLAYSCAPE_CONFIGURATION_TITLE = "Playscape Configuration";
		private const string API_KEY_TITLE = "API Key";
		private const string JAVA_HEAP_SIZE_TITLE = "Java Heap Size (MB)";
		private const string ENABLE_ADS_TITLE = "Enable Ads System";
        private const string INCLUDE_ARCHITECTURE_TITLE = "Include ARMEABI Architecture";
		private const string INCLUDE_PL_EXCHANGE_TITLE = "Include Playscape Exchange";
		private Vector2  scrollPos;

		private const string AB_TESTING_TITLE = "AB Testing Configuration";

        private const string TARGET_ARMEABI = "Assets/Plugins/Android/PlayscapePublishingKit/libs/armeabi";
        private const string TEMP_ARMEABI = "Assets/Temp/armeabi";

		void OnGUI () {
			title = WINDOW_TITLE;

            GUI.changed = false;

			EditorGUILayout.BeginHorizontal();
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width (650), GUILayout.Height (120));
				OnAdsGUI ();
			EditorGUILayout.EndScrollView ();
			EditorGUILayout.EndHorizontal ();

			GUILayout.Space (30);

			if (GUILayout.Button (CLOSE)) {
				onClose();
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

            app.build(false, completed, onProgress, OnFailed);
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

		public void OnFailed(object sender, string message)
		{
			if (!isUIThread)
			{
				this.RunOnUIThread(new BuildProcess.BuildProgressChanged(onProgress), new object[] { sender, message });
				return;
			}

			EditorUtility.DisplayDialog (
				"Playscape Build Post Proccessor",
				message,
				"OK"
				);
		}

		private void ApplyChanges() {
      string templateManifestPath = CommonConsts.PLAYSCAPE_MANIFEST_PATH;

      if (!ConfigurationInEditor.Instance.IncludePlayscapeExchange) {
          templateManifestPath = CommonConsts.PLAYSCAPE_WITHOUT_EXCHANGE_MANIFEST_PATH;
      }

      string targetManifest = "Assets/Plugins/Android/PlayscapePublishingKit/AndroidManifest.xml";

			string manifestContents = File.ReadAllText(templateManifestPath);
			manifestContents = AndroidPostProcessor.ApplyCommonAndroidManifestParams(manifestContents);
			File.WriteAllText(targetManifest, manifestContents);
      
      string minSdkVersion = Regex.Match (PlayerSettings.Android.minSdkVersion.ToString (), @"\d+").Value;
			AndroidManifestMerger.InsertUsesSDK ("Assets/Plugins/Android/AndroidManifest.xml", minSdkVersion, "22");

			targetManifest = "Assets/Plugins/Android/AndroidManifest.xml";
			AndroidManifestMerger.Merge (targetManifest, false);

			AndroidApkCreator.IncludeArchitecture (Configuration.Instance.IncludeArchitectures, TARGET_ARMEABI, TEMP_ARMEABI);
			AndroidApkCreator.IncludePlayscapeExchange (ConfigurationInEditor.Instance.IncludePlayscapeExchange);

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
			GUILayout.Label (PLAYSCAPE_CONFIGURATION_TITLE, EditorStyles.boldLabel);

			string apiKey = EditorGUILayout.TextField (API_KEY_TITLE, ConfigurationInEditor.Instance.MyAds.MyAdsConfig.ApiKey);
			ConfigurationInEditor.Instance.MyAds.MyAdsConfig.ApiKey = apiKey;

			int javaHeapSize = EditorGUILayout.IntField (JAVA_HEAP_SIZE_TITLE, ConfigurationInEditor.Instance.JavaHeapSize);
			ConfigurationInEditor.Instance.JavaHeapSize = javaHeapSize;

			bool enableAdsSystem = EditorGUILayout.ToggleLeft (ENABLE_ADS_TITLE, Configuration.Instance.MyAds.MyAdsConfig.EnableAdsSystem);
			Configuration.Instance.MyAds.MyAdsConfig.EnableAdsSystem = enableAdsSystem;

            bool includeArchitectures = EditorGUILayout.ToggleLeft (INCLUDE_ARCHITECTURE_TITLE, Configuration.Instance.IncludeArchitectures);
			Configuration.Instance.IncludeArchitectures = includeArchitectures;

			bool includePlayscapeExchange = EditorGUILayout.ToggleLeft (INCLUDE_PL_EXCHANGE_TITLE, ConfigurationInEditor.Instance.IncludePlayscapeExchange);
			ConfigurationInEditor.Instance.IncludePlayscapeExchange = includePlayscapeExchange;
		}
	}
}
#endif
