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
using System.Text.RegularExpressions;

namespace Playscape.Editor {
		
	class IOSPostProcessor : AbstractPostProcessor {

		private readonly string pathToProjectSources;
		
		public IOSPostProcessor(string pathToProjectSources) {
			this.pathToProjectSources = pathToProjectSources;
		}			


		public void build(bool async, BuildProcess.BuildCompleted completedCallback, BuildProcess.BuildProgressChanged progressCallback, BuildProcess.BuildFailed failedCallback) {
			BuildParams bp = new BuildParams {
				isDebug = UnityEngine.Debug.isDebugBuild
			};
			BuildProcess process = new IOSBuildProcess(bp, new UnityDebugLogger(), completedCallback, progressCallback, failedCallback);

			if (async) {
				process.BuildAsync(pathToProjectSources);
			} else
			{
				process.Build(pathToProjectSources);
			}
		}


		public override void Run()
		{
			EditorUtility.ClearProgressBar();
			EditorUtility.DisplayProgressBar("Publishing kit post-process", "Applying publishing kit logic", 0);

			try
			{
				build(false, onComplete, onProgress, OnFailed);
			}
			catch (Exception e)
			{
				EditorUtility.DisplayDialog("Publishing Kit Error", "An error occured while applying post-build logic: " + e.Message, "OK");
			}				
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

		public static void onProgress(object sender, string info, int percentage)
		{
			EditorUtility.DisplayProgressBar("Publishing kit post-process", info, percentage);
		}

		public static void onComplete(object sender)
		{
			EditorUtility.ClearProgressBar();
		}

		public static void OnFailed(object sender, string message) {
			EditorUtility.DisplayDialog (
				"Playscape Post Proccessor",
				message,
				"OK"
			);
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
	}
}
#endif