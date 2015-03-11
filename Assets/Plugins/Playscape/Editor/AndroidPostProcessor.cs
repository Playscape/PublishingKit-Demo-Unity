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
		
		public const string PLAYSCAPE_CONFIG_XML_PATH = CommonConsts.PUBLISHING_PATH_ANDROID_LIB_PATH + "/res/values/playscape_config.xml";
		private const string LIBS_ANDROID_SUPPORT_V4_PATH = "/libs/android-support-v4.jar";
		private const string LIBS_ANDROID_SUPPORT_V4_PATH_THAT_COMES_WITH_PUBKIT  = "/libs/android-support-v4.jar_v19.1";
		private const string LIBS_UNITY_CLASSES_PATH = "/libs/unity-classes.jar";
		private const string LIBS_PLAYSCAPE_PUBLISHING_KIT_PATH = "/libs/playscape_publishing_kit.jar";
		
		private const string MANIFEST_FILE_NAME = "AndroidManifest.xml";
		
		private  string mTargetPath;
		
		public AndroidPostProcessor(string targetPath) {
			mTargetPath = targetPath;
		}
		
		private Boolean isApkBuild() {
			return mTargetPath.ToLower ().EndsWith (".apk");
		}
		
		public override void Run()
		{
			string publishingKitLibPath = isApkBuild() ? 
				Environment.CurrentDirectory + "/Assets/Plugins/Android/PlayscapePublishingKit" : 
					mTargetPath + "/PlayscapePublishingKit";
			if (!isApkBuild()) {
				if(File.Exists(publishingKitLibPath + LIBS_PLAYSCAPE_PUBLISHING_KIT_PATH)) {
					File.Delete(publishingKitLibPath + LIBS_PLAYSCAPE_PUBLISHING_KIT_PATH);
				}
			}

			// the various sdks to the main manifest file
			string sourcesPath = isApkBuild () ? 
				Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ()) : 
					mTargetPath + "/" + PlayerSettings.productName;	

			if (isApkBuild ()) {
				// decompile apk to update resources
				AndroidApkCreator.decompile (mTargetPath, sourcesPath, Debug.isDebugBuild);

				AndroidApkCreator.applyAspects(mTargetPath, sourcesPath, Debug.isDebugBuild);
				
				// recompile apk file and rewrite existing apk
				AndroidApkCreator.recompile (mTargetPath, sourcesPath, Debug.isDebugBuild);
				
				DirectoryInfo directoryToRemove = new DirectoryInfo(sourcesPath);
				if (directoryToRemove != null && directoryToRemove.Exists) {
					directoryToRemove.Delete (true);
				} 

			}

			return;
			// If our manifests are merged then all manifest fragments will reside in the same file and therefore we point
			// the various sdks to the main manifest file
			sourcesPath = isApkBuild () ? 
				Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ()) : 
					mTargetPath + "/" + PlayerSettings.productName;	
			
			var manifestDst = sourcesPath + "/" + MANIFEST_FILE_NAME;
			
			L.D("mTargetPath: " + mTargetPath);
			L.D("sourcesPath: " + sourcesPath);
			
			
			if (isApkBuild()) {
				// decompile apk to update resources
				AndroidApkCreator.decompile (mTargetPath, sourcesPath, Debug.isDebugBuild);
			}
			
			if (ConfigurationInEditor.Instance.MergeAndroidManifests) 
			{
				AndroidManifestMerger.Merge(manifestDst, Debug.isDebugBuild);
			} else {
				// Copy fragments
				File.Copy(CommonConsts.PLAYSCAPE_MANIFEST_PATH, sourcesPath + "/" + new FileInfo(CommonConsts.PLAYSCAPE_MANIFEST_PATH).Name);
			}
			
			
			string manifestContents = File.ReadAllText(manifestDst);
			manifestContents = ApplyCommonAndroidManifestParams(manifestContents);
			File.WriteAllText(manifestDst, manifestContents);
			
			var configBaseName = isApkBuild () ? "strings.xml"  : new FileInfo(PLAYSCAPE_CONFIG_XML_PATH).Name;
			var dstConfig = (isApkBuild() ? sourcesPath : publishingKitLibPath) + "/res/values/" + configBaseName;
			var srcConfig = isApkBuild () ? dstConfig : PLAYSCAPE_CONFIG_XML_PATH;
			
			ApplyPlayscapeAndroidConfiguration(srcConfig,
			                                   dstConfig,
			                                   Debug.isDebugBuild);
			
			if (!isApkBuild()) {
				CopyDepedencyJarsToLibs(mTargetPath, publishingKitLibPath, sourcesPath);
			}
			
			if (isApkBuild ()) {
				// recompile apk file and rewrite existing apk
				AndroidApkCreator.recompile (mTargetPath, sourcesPath, Debug.isDebugBuild);
				
				DirectoryInfo directoryToRemove = new DirectoryInfo(sourcesPath);
				if (directoryToRemove != null && directoryToRemove.Exists) {
					directoryToRemove.Delete (true);
				} 
			}
		}
		
		public static void ApplyPlayscapeAndroidConfiguration(string srcConfig,
		                                                      string dstConfig,
		                                                      bool isDebugBuild)
		{
			// Manipulate Config
			var configDoc = new XmlDocument();
			configDoc.LoadXml(File.ReadAllText (srcConfig));
			
			configDoc.SelectSingleNode("resources/string[@name='playscape_reporter_id']").InnerText = 
				ConfigurationInEditor.Instance.ReporterId;
			
			configDoc.SelectSingleNode("resources/string[@name='playscape_remote_logger_url']").InnerText = 
				isDebugBuild ? Settings.DebugRemoteLoggerUrl
					: Settings.ReleaseRemoteLoggerUrl;
			
			injectABTestingConfig (configDoc);
			injectAdConfigs (configDoc);			
			
			
			var writerSettings = new XmlWriterSettings();
			writerSettings.Indent = true;
			using (var writer = XmlWriter.Create(dstConfig)) {
				configDoc.Save(writer);
			}
		}
		
		private static void injectAdConfigs (XmlDocument configDoc)
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
			configDoc.SelectSingleNode("resources/string[@name='playscape_ads_config_enable_ads_system']").InnerText =  Convert.ToString(ConfigurationInEditor.Instance.MyAds.MyAdsConfig.EnableAdsSystem).ToLower();
		}
		
		private static void injectABTestingConfig(XmlDocument configDoc)
		{
			XmlNode rootResources = configDoc.SelectSingleNode("resources");
			XmlNode lastExperimentsElement = configDoc.SelectSingleNode("resources/string[@name='playscape_enable_ab_testing_system']");
			
			configDoc.SelectSingleNode("resources/string[@name='playscape_amazon_abtesing_public_key']").InnerText = 
				ConfigurationInEditor.Instance.MyABTesting.AmazonPublicKey;
			configDoc.SelectSingleNode("resources/string[@name='playscape_amazon_abtesing_private_key']").InnerText = 
				ConfigurationInEditor.Instance.MyABTesting.AmazonPrivateKey;
			configDoc.SelectSingleNode("resources/string[@name='playscape_enable_ab_testing_system']").InnerText = 
				ConfigurationInEditor.Instance.MyABTesting.EnableABTestingSystem.ToString().ToLower();
			
			//TODO: remove all previous experiments - or simply generate this into a different file
			for (int i = 0; i < ConfigurationInEditor.Instance.MyABTesting.NumberOfCustomExperiments; i++) 
			{
				string elementName = "playscape_experiment_" + (i + 1).ToString();
				XmlElement playscapeExperimentElement = configDoc.CreateElement("string-array");
				playscapeExperimentElement.SetAttribute("name",elementName);
				
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
				
				XmlNode oldNode = configDoc.SelectSingleNode(string.Format ("resources/string-array[@name='{0}']", elementName));
				if (oldNode != null) {
					rootResources.ReplaceChild(playscapeExperimentElement, oldNode);
				} else {
					rootResources.InsertAfter(playscapeExperimentElement, lastExperimentsElement);
				}
				lastExperimentsElement = playscapeExperimentElement;
			}
		}
		
		/// <summary>
		/// Converts C# camel case to lower case snake.
		/// e.g: ThisIsACamel to this_is_a_camel
		/// </summary>
		/// <returns>Snake cased string.</returns>
		/// <param name="camelCase">Camel case string.</param>
		private static string CamelToSnake(string camelCase)
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
		public static string ApplyCommonAndroidManifestParams (string manifestContents)
		{
			return manifestContents.Replace("PACKAGE_NAME", PlayerSettings.bundleIdentifier);
		}
		
		private static void CopyDepedencyJarsToLibs (string targetPath,
		                                             string pathToPublishingKitLibSources,
		                                             string projectSources)
		{
			string v4supportLibPath = string.Empty;
			foreach (var dirPath in Directory.GetDirectories(targetPath)) {
				// Is there a v4 support lib in a project other than the pubkit?
				if (File.Exists(dirPath + LIBS_ANDROID_SUPPORT_V4_PATH) && 
				    new DirectoryInfo(dirPath).FullName != new DirectoryInfo(pathToPublishingKitLibSources).FullName) 
				{
					v4supportLibPath = dirPath + LIBS_ANDROID_SUPPORT_V4_PATH;
					break;
				}
			}
			
			
			// If no v4 support lib, use the one that comes with the pubkit
			if (string.IsNullOrEmpty (v4supportLibPath)) 
			{
				v4supportLibPath = pathToPublishingKitLibSources + LIBS_ANDROID_SUPPORT_V4_PATH;
				File.Move(pathToPublishingKitLibSources + LIBS_ANDROID_SUPPORT_V4_PATH_THAT_COMES_WITH_PUBKIT, v4supportLibPath);
			} else {
				File.Delete(pathToPublishingKitLibSources + LIBS_ANDROID_SUPPORT_V4_PATH_THAT_COMES_WITH_PUBKIT);
			}
			
			foreach (var dirPath in Directory.GetDirectories(targetPath)) {
				if (new DirectoryInfo(dirPath).Name != PlayerSettings.productName) {
					if (!Directory.Exists(dirPath + "/libs")) {
						Directory.CreateDirectory(dirPath + "/libs");
					}
					
					if (new DirectoryInfo(v4supportLibPath).FullName != new DirectoryInfo(dirPath + LIBS_ANDROID_SUPPORT_V4_PATH).FullName) {
						File.Copy(v4supportLibPath, dirPath + LIBS_ANDROID_SUPPORT_V4_PATH, true);
					}
					
					if (File.Exists(projectSources + LIBS_UNITY_CLASSES_PATH )) {
						File.Copy(projectSources + LIBS_UNITY_CLASSES_PATH, dirPath + LIBS_UNITY_CLASSES_PATH, true);
					}
				}
			}
			
			if(File.Exists(pathToPublishingKitLibSources + LIBS_PLAYSCAPE_PUBLISHING_KIT_PATH)) {
				File.Delete(pathToPublishingKitLibSources + LIBS_PLAYSCAPE_PUBLISHING_KIT_PATH);
			}
		}
	}
	
	/// <summary>
	/// Handles the OnPostProcessBuild callback, in which we apply configurations to the sdks in the various platforms
	/// </summary>
	
}
#endif