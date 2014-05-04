#if UNITY_EDITOR
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using Playscape.Internal;
using System.Xml;
using System.Text;
using System;
using System.Collections.Generic;

namespace Playscape.Editor {
		
	class IOSPostProcessor : IPostProcessor {

		private readonly string pathToProjectSources;
		
		public IOSPostProcessor(string pathToProjectSources) {
			this.pathToProjectSources = pathToProjectSources;
		}

		public void CheckForWarnings(WarningAccumulator warnings)
		{
			warnings.WarnIfStringIsEmpty(
				ConfigurationInEditor.Instance.ReporterId,
				Warnings.REPORTED_ID_NOT_SET);

			warnings.WarnIfStringIsEmpty(
				ConfigurationInEditor.Instance.PushWooshIosId,
				Warnings.PUSH_WOOSH_APP_ID_NOT_SET_IOS);
		}

		public void Run()
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
				Debug.isDebugBuild ? Settings.DebugRemoteLoggerUrl
								   : Settings.ReleaseRemoteLoggerUrl);

			infoAppender.AppendFragment(plistFragment);
			infoAppender.Save();

			File.Delete(plistFragment);

			AddPushWooshSupport(pathToProjectSources);
		}

		void AddPushWooshSupport (string targetPath)
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