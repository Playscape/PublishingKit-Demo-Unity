#if UNITY_EDITOR
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

namespace Playscape.Editor {
		
	class IOSPostProcessor : AbstractPostProcessor {

		private readonly string pathToProjectSources;
		
		public IOSPostProcessor(string pathToProjectSources) {
			this.pathToProjectSources = pathToProjectSources;
		}

		public override void CheckForWarnings(WarningAccumulator warnings)
		{
			base.CheckForWarnings (warnings);

			warnings.WarnIfStringIsEmpty(
				ConfigurationInEditor.Instance.PushWooshIosId,
				Warnings.PUSH_WOOSH_APP_ID_NOT_SET_IOS);
		}

		public override void Run()
		{
			var plistFragment = 
				pathToProjectSources + "/Libraries/playscape_config.plist_fragment";

			var infoAppender = new PlistAppender(pathToProjectSources + "/Info.plist");

			infoAppender.AddString(
				"Pushwoosh_APPID",
				ConfigurationInEditor.Instance.PushWooshIosId);

			infoAppender.AddString(
				"PlayscapeReporterId",
				ConfigurationInEditor.Instance.ReporterId);

			infoAppender.AddString(
				"PlayscapeRemoteLoggerUrl",
				UnityEngine.Debug.isDebugBuild ? Settings.DebugRemoteLoggerUrl
								   : Settings.ReleaseRemoteLoggerUrl);

			infoAppender.AppendFragment(plistFragment);
			infoAppender.Save();

			File.Delete(plistFragment);

//			AddRequiredFrameworks(pathToProjectSources);

			if (PlayerSettings.iOS.sdkVersion == iOSSdkVersion.SimulatorSDK) {
				FixRegisterMonoModules(pathToProjectSources);
			}

			UnityEditor.XCodeEditor.XCProject project = new UnityEditor.XCodeEditor.XCProject(pathToProjectSources);
			
			// Find and run through all projmods files to patch the project
			var files = System.IO.Directory.GetFiles(Application.dataPath, "*.projmods", System.IO.SearchOption.AllDirectories);
			foreach (var file in files)
			{
				project.ApplyMod(file);
			}

			RunPushNotificationBashScript();

			project.Save();
		}

		static string ConvertStringArrayToString(string[] array)
		{
			//
			// Concatenate all the elements into a StringBuilder.
			//
			StringBuilder builder = new StringBuilder();
			foreach (string value in array)
			{
				builder.Append(value);
			}
			return builder.ToString();
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

		void AddRequiredFrameworks (string targetPath)
		{

			var files = Directory.GetFiles(targetPath, "project.pbxproj", SearchOption.AllDirectories);

			if (files.Length > 0) {

				var filePath = files[0];

				var contents = File.ReadAllText(filePath);

				// Adds AdSupport.Framework, needed for PushWoosh
				contents = AppendAfterLastToken(contents, new string[] {"/* Begin PBXBuildFile section */"}, "\nBDCDD2C918A253EF0099F776 /* AdSupport.framework in Frameworks */ = {isa = PBXBuildFile; fileRef = BDCDD2C818A253EF0099F776 /* AdSupport.framework */; };");
				contents = AppendAfterLastToken(contents, new string[] {"/* Begin PBXFileReference section */"}, "\nBDCDD2C818A253EF0099F776 /* AdSupport.framework */ = {isa = PBXFileReference; lastKnownFileType = wrapper.framework; name = AdSupport.framework; path = System/Library/Frameworks/AdSupport.framework; sourceTree = SDKROOT; };");
				contents = AppendAfterLastToken(contents, new string[] {"/* Begin PBXFrameworksBuildPhase section */", "files = ("}, "\nBDCDD2C918A253EF0099F776 /* AdSupport.framework in Frameworks */,");
				contents = AppendAfterLastToken(contents, new string[] {"29B97323FDCFA39411CA2CEA /* Frameworks */ = {", "children = ("}, "\nBDCDD2C818A253EF0099F776 /* AdSupport.framework */,");

				// Adds Storekit.Framework, needed for Chartboost
				contents = AppendAfterLastToken(contents, new string[] {"/* Begin PBXBuildFile section */"}, "\nBD9F3E9119127BE9001103C2 /* StoreKit.framework in Frameworks */ = {isa = PBXBuildFile; fileRef = BD9F3E9019127BE9001103C2 /* StoreKit.framework */; };");
				contents = AppendAfterLastToken(contents, new string[] {"/* Begin PBXFileReference section */"}, "\nBD9F3E9019127BE9001103C2 /* StoreKit.framework */ = {isa = PBXFileReference; lastKnownFileType = wrapper.framework; name = StoreKit.framework; path = System/Library/Frameworks/StoreKit.framework; sourceTree = SDKROOT; };");
				contents = AppendAfterLastToken(contents, new string[] {"/* Begin PBXFrameworksBuildPhase section */", "files = ("}, "\nBD9F3E9119127BE9001103C2 /* StoreKit.framework in Frameworks */,");
				contents = AppendAfterLastToken(contents, new string[] {"29B97323FDCFA39411CA2CEA /* Frameworks */ = {", "children = ("}, "\nBD9F3E9019127BE9001103C2 /* StoreKit.framework */,");

				// Make CoreLocation.Framework optional
				contents = contents.Replace("/* CoreLocation.framework */; };", "/* CoreLocation.framework */; settings = {ATTRIBUTES = (Weak, ); }; };");



				// TODO add warning if failed
				File.WriteAllText(filePath, contents);
			}

		
		}

		/// <summary>
		/// Takes what and appends it in the position immediately after the last token in the tokensTrail
		/// </summary>
		/// <returns>Modified contents</returns>
		/// <param name="contents">Contents.</param>
		/// <param name="tokensTrail">Tokens trail.</param>
		/// <param name="what">What.</param>
		private string AppendAfterLastToken(string contents, string[] tokensTrail, string what) {
			int pos = 0;
			bool reachedTrailEnd = true;
			foreach (var token in tokensTrail) { 
				pos = contents.IndexOf(token, pos);
				if (pos < 0) {
					reachedTrailEnd = false;
					break;
				}
			}

			if (reachedTrailEnd) {
				var lastToken = tokensTrail[tokensTrail.Length - 1];
				contents = contents.Insert(pos + lastToken.Length, what);
			}

			return contents;
		}

		private void RunPushNotificationBashScript() {
			string scriptName = "Push_Notification_Script.sh";
			string[] files = Directory.GetFiles(Application.dataPath, scriptName, System.IO.SearchOption.AllDirectories);

			string scriptFile = "";

			if (files.Length > 0)
			{
				scriptFile = files[0];
			}
			
			if (!String.IsNullOrEmpty(scriptFile)) 
			{
				string destScriptFile = Path.Combine(pathToProjectSources, scriptName);

				File.Copy(scriptFile, destScriptFile, true);

				if (File.Exists(destScriptFile)) {
					Process process = new System.Diagnostics.Process();
					process.StartInfo.FileName = destScriptFile;

					string argumentsPath = "\"" + pathToProjectSources + "\"";
					process.StartInfo.Arguments = argumentsPath;
					process.StartInfo.UseShellExecute = false; 
					process.StartInfo.RedirectStandardError = true;
					process.StartInfo.RedirectStandardInput = true;
					process.StartInfo.RedirectStandardOutput = true;
					process.Start();
					var output = process.StandardOutput.ReadToEnd ();
					L.I ("stdout: {0}", output);
				}

				System.IO.File.Delete(destScriptFile);
			}
		}
	}
}
#endif