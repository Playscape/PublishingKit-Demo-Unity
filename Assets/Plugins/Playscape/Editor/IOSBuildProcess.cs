#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.XCodeEditor;

using Playscape.Internal;
using System.Xml;
using System.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;


namespace Playscape.Editor
{
	class IOSBuildProcess : BuildProcess
	{
		public IOSBuildProcess(BuildParams buildParams, 
			ILogger logger,
			BuildCompleted buildCompleted, 
			BuildProgressChanged buildProgressChanged,
			BuildFailed buildFailed) : base (buildParams, logger, buildCompleted, buildProgressChanged, buildFailed)
		{

		}

		public override void Build(string path) {
			try {
				retrieveGameConfig();

				ApplyConfig (ConfigurationInEditor.Instance.MyGameConfiguration, path);
				FixRegisterMonoModulesIfNeed (path);
				PatchXcodeProjectFile (path);

			} finally {
				Cleanup ();
				OnProgress ("Done", 100);
				OnCompleted ();

			}				
		}	

		private void ApplyConfig(Configuration.GameConfiguration gameConfig, string pathToProjectSources) {
			var plistFragment = "Assets/Plugins/iOS/playscape_config.plist_fragment";
			var infoAppender = new PlistAppender(pathToProjectSources + "/Info.plist");

			gameConfig.EnumerateConfiguration((category, fieldInfo) =>
				{
					var settingName = new StringBuilder();
					settingName.Length = 0;
					settingName.Append("Playscape")
						.Append(category.GetType().Name)
						.Append(fieldInfo.Name);

					var name = settingName.ToString();
					object value = fieldInfo.GetValue(category);

					if (value.GetType() == typeof(string)) {
						infoAppender.AddString (name, (string)value);
					} else if (value.GetType() == typeof(int)) {						
						infoAppender.AddNumber (name, (int)value);
					} else if (value.GetType() == typeof(bool)) {
						infoAppender.AddBool (name, (bool)value);
					}						
				});

			infoAppender.AddString (
				"PlayscapeApiKey",
				ConfigurationInEditor.Instance.MyAds.MyAdsConfig.ApiKey);

			infoAppender.AddBool (
				"PlayscapeAdsConfigAdEnabled",
				ConfigurationInEditor.Instance.MyAds.MyAdsConfig.EnableAdsSystem);

			infoAppender.AddBool (
				"PlayscapeIsPublishedByPlayscape",
				ConfigurationInEditor.Instance.MyGameConfiguration.PublishedByPlayscape);

			infoAppender.AddString(
				"PlayscapeRemoteLoggerUrl",
				UnityEngine.Debug.isDebugBuild ? Settings.DebugRemoteLoggerUrl
				: Settings.ReleaseRemoteLoggerUrl);
				
			infoAppender.AddString(
				"PlayscapeAnalyticsReportReporterId",
				ConfigurationInEditor.Instance.ReporterId);

			infoAppender.AppendFragment (plistFragment);
			infoAppender.Save();
		}			

		private void FixRegisterMonoModulesIfNeed(string pathToProjectSources) {
			if (PlayerSettings.iOS.sdkVersion == iOSSdkVersion.SimulatorSDK) {
				FixRegisterMonoModules(pathToProjectSources);
			}
		}

		/// <summary>
		/// By default, external native plugin functions are not available to mono in iOS simulator - you will get EntryNotFoundException.
		/// This enable them by modifying RegisterMonoModules.cpp, for more info read this http://tech.enekochan.com/2012/05/28/using-the-xcode-simulator-with-a-unity-3-native-ios-plug-in/
		/// </summary>
		void FixRegisterMonoModules (string targetPath)
		{
			var files = Directory.GetFiles(targetPath, "RegisterMonoModules.cpp", SearchOption.AllDirectories);

			if (files.Length > 0) {

				var filePath = files[0];

				var lines = File.ReadAllLines(filePath);
				var newLines = new List<string>();

				int step = 0;
				for (int i = 0; i < lines.Length; ++i) {
					bool appendLine = true;

					if (step == 0) {

						if (lines[i].Contains("TARGET_IPHONE_SIMULATOR")) {
							newLines.Add(@"void mono_dl_register_symbol(const char* name, void *addr);");
							step = 1;
						}
					} else if (step == 1) {
						if (lines[i].Contains("RegisterMonoModules()")) {
							step = 2;
						}
					} else if (step == 2) {
						if (lines[i].Contains("mono_dl_register_symbol")) {
							newLines.Add("#endif // !(TARGET_IPHONE_SIMULATOR)");
							step = 3;
						}
					} else if (step == 3) {
						if (lines[i].Contains("#endif") && lines[i].Contains("TARGET_IPHONE_SIMULATOR")) {
							appendLine = false;
						}
					}

					if (appendLine) {
						newLines.Add(lines[i]);
					}
				}

				string contents = string.Join("\n", newLines.ToArray());
				File.WriteAllText(filePath, contents);
			}
		}

		private void PatchXcodeProjectFile(String pathToProjectSources) {
			UnityEditor.XCodeEditor.XCProject project = new UnityEditor.XCodeEditor.XCProject(pathToProjectSources);

			// Find and run through all projmods files to patch the project
			var files = System.IO.Directory.GetFiles(Application.dataPath, "*.projmods", System.IO.SearchOption.AllDirectories);
			foreach (var file in files)
			{
				project.ApplyMod(file);
			}

			project.Save();
		}

	}
}

#endif