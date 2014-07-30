#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

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
		void OnGUI () {
			title = WINDOW_TITLE;

            GUI.changed = false;
            
			EditorGUILayout.BeginHorizontal();
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width (520), GUILayout.Height (350));
				OnReportingGUI ();
				OnPushWooshGUI ();
				OnAdsGUI ();
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
			string result = EditorGUILayout.TextField (ANDROID + " " + APP_ID, ConfigurationInEditor.Instance.PushWooshAndroidId);
			if (result != null) {
				ConfigurationInEditor.Instance.PushWooshAndroidId = result;
			}
			result = EditorGUILayout.TextField (IOS + " " + APP_ID, ConfigurationInEditor.Instance.PushWooshIosId);
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
			
							string result = EditorGUILayout.TextField (fieldInfo.Name, fieldInfo.GetValue(category) as string);
							if (result != null) {
								fieldInfo.SetValue(category, result);
								
							}
						});
					
		}
	}
}
#endif