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

	class AndroidPostProcessor : IPostProcessor {

		private const string PLAYSCAPE_CONFIG_XML_PATH = CommonConsts.PUBLISHING_PATH_ANDROID_LIB_PATH + "/res/values/playscape_config.xml";
		private const string PUSH_WOOSH_GCM_SENDER_TOKEN = "%{GCM_SENDER_ID}";
		private const string PUSH_WOOSH_APP_ID_TOKEN = "%{PUSH_WOOSH_APP_ID}";
		private const string LIBS_ANDROID_SUPPORT_V4_PATH = "/libs/android-support-v4.jar";
		private const string LIBS_UNITY_CLASSES_PATH = "/libs/unity-classes.jar";

		private readonly string mPathToMainProjcetSources;
		private readonly string mTargetPath;
		private readonly string mPathToPublishingKitLibSources;

		public AndroidPostProcessor(string targetPath) {
			mPathToMainProjcetSources = targetPath + "/" + PlayerSettings.productName;
			mPathToPublishingKitLibSources = targetPath + "/PlayscapePublishingKit";
			mTargetPath = targetPath;
		}

		public void CheckForWarnings(WarningAccumulator warnings)
		{
			warnings.WarnIfStringIsEmpty(
				ConfigurationInEditor.Instance.ReporterId,
				Warnings.REPORTED_ID_NOT_SET);

			warnings.WarnIfStringIsEmpty(
				ConfigurationInEditor.Instance.PushWooshAndroidId,
				Warnings.PUSH_WOOSH_APP_ID_NOT_SET_ANDROID);
			
			warnings.WarnIf(
				mPathToMainProjcetSources.ToLower().EndsWith(".apk"),
				Warnings.PLAYSCAPE_ANDROID_APK_BUILD);
		}

		public void Run()
		{
			// If our manifests are merged then all manifest fragments will reside in the same file and therefore we point
			// the various sdks to the main manifest file
			string generatedPlayscapeManifestPath =  mPathToMainProjcetSources + "/AndroidManifest.xml";
			string generatedPushWooshManifestPath = mPathToMainProjcetSources + "/AndroidManifest.xml";
			
			if (ConfigurationInEditor.Instance.MergeAndroidManifests) {
				AndroidManifestMerger.Merge(mPathToMainProjcetSources + "/AndroidManifest.xml");
			} else {
				generatedPlayscapeManifestPath =  mPathToMainProjcetSources + "/" + new FileInfo(CommonConsts.PLAYSCAPE_MANIFEST_PATH).Name;
				generatedPushWooshManifestPath = mPathToMainProjcetSources + "/" + new FileInfo(CommonConsts.PUSHWOOSH_MANIFEST_PATH).Name;
				
				// Copy fragments
				File.Copy(CommonConsts.PLAYSCAPE_MANIFEST_PATH, generatedPlayscapeManifestPath);
				File.Copy(CommonConsts.PUSHWOOSH_MANIFEST_PATH, generatedPushWooshManifestPath);
			}
			
			ApplyPlayscapeAndroidConfiguration(generatedPlayscapeManifestPath);
			ApplyPushWooshAndroidConfiguration(generatedPushWooshManifestPath);

			CopyDepedencyJarsToLibs();
		}

		private void ApplyPushWooshAndroidConfiguration(string generatedPushWooshManifestPath)
		{
			string manifestContents = File.ReadAllText(generatedPushWooshManifestPath);
			
			manifestContents = manifestContents.Replace(PUSH_WOOSH_GCM_SENDER_TOKEN, "A" + ConfigurationInEditor.PLAYSCAPE_GCM_SENDER_ID) // the "A" is required according to PW docs
				.Replace(PUSH_WOOSH_APP_ID_TOKEN, ConfigurationInEditor.Instance.PushWooshAndroidId);
			
			string manifestBaseName = new FileInfo(generatedPushWooshManifestPath).Name;
			
			manifestContents = ApplyCommonAndroidManifestParams(manifestContents);
			File.WriteAllText(mPathToMainProjcetSources + "/" + manifestBaseName, manifestContents);
		}
		
		void ApplyPlayscapeAndroidConfiguration(string generatedPlayscapeManifestPath)
		{
			string manifestContents = File.ReadAllText(generatedPlayscapeManifestPath);
			string manifestBaseName = new FileInfo(generatedPlayscapeManifestPath).Name;
			
			manifestContents = ApplyCommonAndroidManifestParams(manifestContents);
			
			File.WriteAllText(mPathToMainProjcetSources + "/" + manifestBaseName, manifestContents);
			
			// Manipulate Config
			var configDoc = new XmlDocument();
			configDoc.LoadXml(File.ReadAllText (PLAYSCAPE_CONFIG_XML_PATH));

			configDoc.SelectSingleNode("resources/string[@name='playscape_reporter_id']").InnerText = 
				ConfigurationInEditor.Instance.ReporterId;

			configDoc.SelectSingleNode("resources/string[@name='playscape_remote_logger_url']").InnerText = 
				Debug.isDebugBuild ? Settings.DebugRemoteLoggerUrl
								   : Settings.ReleaseRemoteLoggerUrl;

			var configBaseName = new FileInfo(PLAYSCAPE_CONFIG_XML_PATH).Name;
			
			var writerSettings = new XmlWriterSettings();
			writerSettings.Indent = true;
			using (var writer = XmlWriter.Create(mPathToPublishingKitLibSources + "/res/values/" + configBaseName, writerSettings)) {
				configDoc.Save(writer);
			}
		}
		
		/// <summary>
		/// Replaces common placeholders according to unity configuration in an android manifest
		/// </summary>
		/// <returns>The common android manifest parameters.</returns>
		/// <param name="manifestContents">Manifest contents.</param>
		private string ApplyCommonAndroidManifestParams (string manifestContents)
		{
			return manifestContents.Replace("PACKAGE_NAME", PlayerSettings.bundleIdentifier);
		}

		private void CopyDepedencyJarsToLibs ()
		{
			foreach (var dirPath in Directory.GetDirectories(mTargetPath)) {
				if (new DirectoryInfo(dirPath).Name != PlayerSettings.productName) {
                    if (!Directory.Exists(dirPath + "/libs")) {
                        Directory.CreateDirectory(dirPath + "/libs");
                    }

					if (File.Exists(mPathToMainProjcetSources + LIBS_ANDROID_SUPPORT_V4_PATH )) {
						File.Copy(mPathToMainProjcetSources + LIBS_ANDROID_SUPPORT_V4_PATH, dirPath + LIBS_ANDROID_SUPPORT_V4_PATH, true);
					}
                    
                    if (File.Exists(mPathToMainProjcetSources + LIBS_UNITY_CLASSES_PATH )) {
                        File.Copy(mPathToMainProjcetSources + LIBS_UNITY_CLASSES_PATH, dirPath + LIBS_UNITY_CLASSES_PATH, true);
                    }
				}
			}
		}
	}

	/// <summary>
	/// Handles the OnPostProcessBuild callback, in which we apply configurations to the sdks in the various platforms
	/// </summary>
	
}
#endif