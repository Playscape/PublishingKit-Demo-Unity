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
using System.Reflection;
using System.Collections.Generic;

namespace Playscape.Editor {
	
	class AndroidPostProcessor : AbstractPostProcessor {
		
		private const string PLAYSCAPE_CONFIG_XML_PATH = CommonConsts.PUBLISHING_PATH_ANDROID_LIB_PATH + "/res/values/playscape_config.xml";
		private const string PUSH_WOOSH_GCM_SENDER_TOKEN = "%{GCM_SENDER_ID}";
		private const string PUSH_WOOSH_APP_ID_TOKEN = "%{PUSH_WOOSH_APP_ID}";
		private const string LIBS_ANDROID_SUPPORT_V4_PATH = "/libs/android-support-v4.jar";
		private const string LIBS_ANDROID_SUPPORT_V4_PATH_THAT_COMES_WITH_PUBKIT  = "/libs/android-support-v4.jar_v19.1";
		private const string LIBS_UNITY_CLASSES_PATH = "/libs/unity-classes.jar";
		
		private readonly string mPathToMainProjcetSources;
		private readonly string mTargetPath;
		private readonly string mPathToPublishingKitLibSources;
		private readonly string mOutputPath;
		
		public AndroidPostProcessor(string targetPath) 
		{
			mTargetPath = targetPath;
			mPathToMainProjcetSources = mTargetPath + "/" + PlayerSettings.productName;
			if (isApkBuild ()) {
				mOutputPath = mTargetPath.Substring (0, mTargetPath.Length - 4) + "_out";
				mPathToPublishingKitLibSources = Environment.CurrentDirectory + "/Assets/Plugins/Android/PlayscapePublishingKit";
			} else {
				mOutputPath = mTargetPath;
				mPathToPublishingKitLibSources = targetPath + "/PlayscapePublishingKit";
			}
		}
		
		public override void CheckForWarnings(WarningAccumulator warnings)
		{
			base.CheckForWarnings (warnings);
			
			warnings.WarnIf(
				mPathToMainProjcetSources.ToLower().EndsWith(".apk"),
				Warnings.PLAYSCAPE_ANDROID_APK_BUILD);
		}

		private Boolean isApkBuild() {
			return mTargetPath.ToLower ().EndsWith (".apk");
		}
		
		public override void Run()
		{
			return;
		}
		
		private void ApplyPushWooshAndroidConfiguration(string generatedPushWooshManifestPath)
		{
			//			string manifestContents = File.ReadAllText(generatedPushWooshManifestPath);
			//
			//			manifestContents = manifestContents.Replace(PUSH_WOOSH_GCM_SENDER_TOKEN, "A" + ConfigurationInEditor.PLAYSCAPE_GCM_SENDER_ID) // the "A" is required according to PW docs
			//				.Replace(PUSH_WOOSH_APP_ID_TOKEN, ConfigurationInEditor.Instance.PushWooshAndroidId);
			//			
			//			string manifestBaseName = new FileInfo(generatedPushWooshManifestPath).Name;
			//			
			//			manifestContents = ApplyCommonAndroidManifestParams(manifestContents);
			//			File.WriteAllText(mPathToMainProjcetSources + "/" + manifestBaseName, manifestContents);
		}
		
		void ApplyPlayscapeAndroidConfiguration(string generatedPlayscapeManifestPath)
		{
			string manifestContents = File.ReadAllText(generatedPlayscapeManifestPath);
			string manifestBaseName = new FileInfo(generatedPlayscapeManifestPath).Name;
			
			manifestContents = ApplyCommonAndroidManifestParams(manifestContents);
			
			File.WriteAllText(mOutputPath + "/" + manifestBaseName, manifestContents);
			
			// Manipulate Config
			var configDoc = new XmlDocument();
			string stringsFile = mOutputPath + "/res/values/strings.xml";
			//			configDoc.LoadXml(File.ReadAllText (PLAYSCAPE_CONFIG_XML_PATH));
			configDoc.LoadXml(File.ReadAllText (stringsFile));
			
			configDoc.SelectSingleNode("resources/string[@name='playscape_reporter_id']").InnerText = 
				ConfigurationInEditor.Instance.ReporterId;
			
			configDoc.SelectSingleNode("resources/string[@name='playscape_remote_logger_url']").InnerText = 
				UnityEngine.Debug.isDebugBuild ? Settings.DebugRemoteLoggerUrl
					: Settings.ReleaseRemoteLoggerUrl;
			
			injectABTestingConfig (configDoc);
			injectAdConfigs (configDoc);
			
			var writerSettings = new XmlWriterSettings();
			writerSettings.Indent = true;
			using (var writer = XmlWriter.Create(stringsFile, writerSettings)) {
				configDoc.Save(writer);
			}
		}
		
		void injectAdConfigs (XmlDocument configDoc)
		{
			// Use relfection to enumerate all ad provider identifiers, and inject them into the 
			// configuration xml.
			//
			// Reflection assumes that Ads class contains either fields which are classes with fields of type String.
			
			Configuration.Instance.TraverseAdsConfig (
				(category, settingFieldInfo) =>
				{
				var settingName = new StringBuilder();
				settingName.Length = 0;
				settingName.Append("playscape_")
					.Append(CamelToSnake(category.GetType().Name));
				
				settingName.Append("_")
					.Append(CamelToSnake(settingFieldInfo.Name));
				object value = settingFieldInfo.GetValue(category);
				// Ads Config Url
				var xmlElement = configDoc.SelectSingleNode(string.Format("resources/string[@name='{0}']", settingName.ToString()));
				
				if (xmlElement == null) {
					throw new ApplicationException(string.Format("Unable to find xml element <string name='{0}'>, please " +
					                                             "verify playscape_config.xml or your ad provider fields naming conventions.", settingName));
				}
				
				xmlElement.InnerText = 
					string.Format("{0}", value);
				
			});
		}
		
		private void injectABTestingConfig(XmlDocument configDoc)
		{
			XmlNode rootResources = configDoc.SelectSingleNode("resources");
			XmlNode lastExperimentsElement = configDoc.SelectSingleNode("resources/string[@name='playscape_enable_ab_testing_system']");
			
			configDoc.SelectSingleNode("resources/string[@name='playscape_amazon_abtesing_public_key']").InnerText = 
				ConfigurationInEditor.Instance.MyABTesting.AmazonPublicKey;
			configDoc.SelectSingleNode("resources/string[@name='playscape_amazon_abtesing_private_key']").InnerText = 
				ConfigurationInEditor.Instance.MyABTesting.AmazonPrivateKey;
			configDoc.SelectSingleNode("resources/string[@name='playscape_enable_ab_testing_system']").InnerText = 
				ConfigurationInEditor.Instance.MyABTesting.EnableABTestingSystem.ToString().ToLower();
			
			for (int i = 0; i < ConfigurationInEditor.Instance.MyABTesting.NumberOfCustomExperiments; i++) 
			{
				XmlElement playscapeExperimentElement = configDoc.CreateElement("string-array");
				playscapeExperimentElement.SetAttribute("name","playscape_experiment_" + (i + 1).ToString());
				
				XmlElement experimentNameElement = configDoc.CreateElement("item");
				experimentNameElement.InnerText = ConfigurationInEditor.Instance.MyABTesting.MyCustomExperimentConfig[i].ExperimentName;
				playscapeExperimentElement.AppendChild(experimentNameElement);
				
				XmlElement experimentTypeElement = configDoc.CreateElement("item");
				experimentTypeElement.InnerText = ConfigurationInEditor.Instance.MyABTesting.MyCustomExperimentConfig[i].ExperimentType;
				playscapeExperimentElement.AppendChild(experimentTypeElement);
				
				for (int j =0; j <   ConfigurationInEditor.Instance.MyABTesting.MyCustomExperimentConfig[i].NumberOfVarsInExperiment; j++)
				{
					XmlElement experimentVariableElement = configDoc.CreateElement("item");
					experimentVariableElement.InnerText = ConfigurationInEditor.Instance.MyABTesting.MyCustomExperimentConfig[i].ExperimentVars[j];
					playscapeExperimentElement.AppendChild(experimentVariableElement);
				}
				
				rootResources.InsertAfter(playscapeExperimentElement, lastExperimentsElement);
				lastExperimentsElement = playscapeExperimentElement;
			}
		}
		
		/// <summary>
		/// Converts C# camel case to lower case snake.
		/// e.g: ThisIsACamel to this_is_a_camel
		/// </summary>
		/// <returns>Snake cased string.</returns>
		/// <param name="camelCase">Camel case string.</param>
		static string CamelToSnake(string camelCase)
		{
			var builder = new StringBuilder();
			
			for (int i = 0; i < camelCase.Length; ++i)
			{
				if (char.IsUpper(camelCase[i]) && i > 0)
				{
					builder.Append("_");
				}
				
				
				builder.Append(char.ToLower(camelCase[i]));
			}
			
			return builder.ToString();
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
			if (isApkBuild()) {
				copyDependencyToApk();
				return;
			}

			string path = mTargetPath;
//			if (isApkBuild ()) {
//				path = mOutputPath;
//			} else {
//				path = mTargetPath;
//			}

			string v4supportLibPath = string.Empty;
			L.W ("mTargetPath: {0}", mTargetPath);
			L.W ("mOutputPath: {0}", mOutputPath);
			foreach (var dirPath in Directory.GetDirectories(path)) {
				// Is there a v4 support lib in a project other than the pubkit?
				L.W ("dirPath + LIBS_ANDROID_SUPPORT_V4_PATH: {0}", dirPath + LIBS_ANDROID_SUPPORT_V4_PATH);
				L.W ("new DirectoryInfo(dirPath).FullName: {0}", new DirectoryInfo(dirPath).FullName);
				L.W ("new DirectoryInfo(mPathToPublishingKitLibSources).FullName: {0}", new DirectoryInfo(mPathToPublishingKitLibSources).FullName);
				if (File.Exists(dirPath + LIBS_ANDROID_SUPPORT_V4_PATH) && 
				    new DirectoryInfo(dirPath).FullName != new DirectoryInfo(mPathToPublishingKitLibSources).FullName) 
				{
					v4supportLibPath = dirPath + LIBS_ANDROID_SUPPORT_V4_PATH;
					L.W ("in if v4supportLibPath: {0}", v4supportLibPath);
					break;
				}
			}


			// If no v4 support lib, use the one that comes with the pubkit
			if (string.IsNullOrEmpty (v4supportLibPath)) 
			{
				L.W ("string.IsNullOrEmpty (v4supportLibPath) is null: {0}", v4supportLibPath);
				v4supportLibPath = mPathToPublishingKitLibSources + LIBS_ANDROID_SUPPORT_V4_PATH;
				File.Move(mPathToPublishingKitLibSources + LIBS_ANDROID_SUPPORT_V4_PATH_THAT_COMES_WITH_PUBKIT, v4supportLibPath);
			} else {
				L.W ("string.IsNullOrEmpty (v4supportLibPath) is not null: {0}", v4supportLibPath);
//				File.Delete(mPathToPublishingKitLibSources + LIBS_ANDROID_SUPPORT_V4_PATH_THAT_COMES_WITH_PUBKIT);
			}

			foreach (var dirPath in Directory.GetDirectories(path)) {
				L.W ("new DirectoryInfo(dirPath).Name: {0}", new DirectoryInfo(dirPath).Name);
				L.W ("PlayerSettings.productName: {0}", PlayerSettings.productName);
				if (new DirectoryInfo(dirPath).Name != PlayerSettings.productName) {
					L.W ("!Directory.Exists(" + dirPath + "/libs): {0}", (!Directory.Exists(dirPath + "/libs")));
                    if (!Directory.Exists(dirPath + "/libs")) {
						L.W ("create " + dirPath + "/libs): {0}", (dirPath + "/libs"));
                        Directory.CreateDirectory(dirPath + "/libs");
                    }

					L.W ("new DirectoryInfo(v4supportLibPath).FullName: {0}", new DirectoryInfo(v4supportLibPath).FullName);
					L.W ("new DirectoryInfo(" + dirPath + LIBS_ANDROID_SUPPORT_V4_PATH + ").FullName: {0}", new DirectoryInfo(dirPath + LIBS_ANDROID_SUPPORT_V4_PATH).FullName);
					if (new DirectoryInfo(v4supportLibPath).FullName != new DirectoryInfo(dirPath + LIBS_ANDROID_SUPPORT_V4_PATH).FullName) {
						L.W ("Copy " + v4supportLibPath + " to " + (dirPath + LIBS_ANDROID_SUPPORT_V4_PATH) + "");
						File.Copy(v4supportLibPath, dirPath + LIBS_ANDROID_SUPPORT_V4_PATH, true);
					}

					L.W ("File.Exists(" + mPathToMainProjcetSources + LIBS_UNITY_CLASSES_PATH + "): {0}", File.Exists(mPathToMainProjcetSources + LIBS_UNITY_CLASSES_PATH ));
                    if (File.Exists(mPathToMainProjcetSources + LIBS_UNITY_CLASSES_PATH )) {
						L.W ("Copy " + mPathToMainProjcetSources + LIBS_UNITY_CLASSES_PATH + " to " + (dirPath + LIBS_UNITY_CLASSES_PATH) + "");
                        File.Copy(mPathToMainProjcetSources + LIBS_UNITY_CLASSES_PATH, dirPath + LIBS_UNITY_CLASSES_PATH, true);
                    }
				}
			}
		}

		private void copyDependencyToApk() {
			List<string> origin;
			List<string> patch;
			List<string> result;


		}
	}
	                              
	/// <summary>
	/// Handles the OnPostProcessBuild callback, in which we apply configurations to the sdks in the various platforms
	/// </summary>
}
#endif