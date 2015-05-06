#if UNITY_EDITOR
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEditor;
using Playscape.Internal; 
using System.Xml;
using System.Text;
using System;
using System.Reflection;
using System.Collections.Generic;
using Ionic.Zip;
using System.Threading;

namespace Playscape.Editor {
	
	class AndroidPostProcessor : AbstractPostProcessor {
		public const string PLAYSCAPE_CONFIG_XML_PATH = CommonConsts.PUBLISHING_PATH_ANDROID_LIB_PATH + "/res/values/playscape_config.xml";
		private const string LIBS_ANDROID_SUPPORT_V4_PATH = "/libs/android-support-v4.jar";
		private const string LIBS_ANDROID_SUPPORT_V4_PATH_THAT_COMES_WITH_PUBKIT  = "/libs/android-support-v4.jar_v19.1";
		private const string LIBS_UNITY_CLASSES_PATH = "/libs/unity-classes.jar";
		private const string LIBS_PLAYSCAPE_PUBLISHING_KIT_PATH = "/libs/playscape_lifecycle.jar";

		private const string MANIFEST_FILE_NAME = "AndroidManifest.xml";
		
		private  string mTargetPath;
		
		public AndroidPostProcessor(string targetPath) {
			mTargetPath = targetPath;
		}
		
		private Boolean isApkBuild() {
			return mTargetPath.ToLower ().EndsWith (".apk");
		}

		public void build(bool async, BuildProcess.BuildCompleted completedCallback, BuildProcess.BuildProgressChanged progressCallback) {
            BuildParams bp = ConstructBuildParams();
            BuildProcess process = new BuildProcess(bp, new UnityDebugLogger(), completedCallback, progressCallback);
            

            if (async) {
                process.BuildAsync(mTargetPath);
            } else
            {
                process.Build(mTargetPath);
            }

		}

        private BuildParams ConstructBuildParams()
        {
            BuildParams bp = new BuildParams
            {
                isDebug = Debug.isDebugBuild,
                sdkVersion = PlayerSettings.Android.minSdkVersion.ToString(),
                keysotre_path = PlayerSettings.Android.keystoreName,
                alias = PlayerSettings.Android.keyaliasName,
                storepass = PlayerSettings.Android.keystorePass,
                keypass = PlayerSettings.Android.keyaliasPass
            };

            if (bp.isDebug || string.IsNullOrEmpty(bp.keysotre_path))
            {
                bp.keysotre_path = (PlatformUtils.isWindows() ? System.Environment.GetEnvironmentVariable("USERPROFILE") :
                                 Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)).FullName)
                    + "/.android/debug.keystore";
                bp.alias = "androiddebugkey";
                bp.storepass = "android";
                bp.keypass = "android";
            }
            return bp;
        }



        public override void Run()
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayProgressBar("Publishing kit post-process", "Applying publishing kit logic", 0);

            string publishingKitLibPath = isApkBuild() ?
                Environment.CurrentDirectory + "/Assets/Plugins/Android/PlayscapePublishingKit" :
                    mTargetPath + "/PlayscapePublishingKit";

            try
            {
                if (isApkBuild())
                {
                    build(false, onComplete, onProgress);
                }

                else
                {
                    if (File.Exists(publishingKitLibPath + LIBS_PLAYSCAPE_PUBLISHING_KIT_PATH))
                    {
                        File.Delete(publishingKitLibPath + LIBS_PLAYSCAPE_PUBLISHING_KIT_PATH);
                    }
                }
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Publishing Kit Error", "An error occured while applying post-build logic: " + e.Message, "OK");
            }

        }

        public static void onProgress(object sender, string info, int percentage)
        {
            EditorUtility.DisplayProgressBar("Publishing kit post-process", info, percentage);
        }

        public static void onComplete(object sender)
        {
            EditorUtility.ClearProgressBar();
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
			injectAdConfigs (ConfigurationInEditor.Instance.MyGameConfiguration, configDoc);			
						
			var writerSettings = new XmlWriterSettings();
			using (var writer = XmlWriter.Create(dstConfig)) {
				configDoc.Save(writer);
			}
		}
		
		private static void injectAdConfigs (Configuration.GameConfiguration gameConfig, XmlDocument configDoc)
		{
			gameConfig.EnumerateConfiguration ((category, fieldInfo) => {
				var settingName = new StringBuilder();
				settingName.Length = 0;
				settingName.Append("playscape_")
					.Append(AndroidPostProcessor.CamelToSnake(category.GetType().Name));
				
				settingName.Append("_")
					.Append(AndroidPostProcessor.CamelToSnake(fieldInfo.Name));
				object value = fieldInfo.GetValue(category);
				
				var xmlElement = configDoc.SelectSingleNode(string.Format("resources/string[@name='{0}']", settingName.ToString()));
				
				if (xmlElement == null) {
					throw new ApplicationException(string.Format("Unable to find xml element <string name='{0}'>, please " +
					                                             "verify playscape_config.xml or your ad provider fields naming conventions.", settingName));
				}
				
				xmlElement.InnerText = string.Format("{0}", value);
			});

			configDoc.SelectSingleNode("resources/string[@name='playscape_ads_api_key']").InnerText =  Convert.ToString(ConfigurationInEditor.Instance.MyAds.MyAdsConfig.ApiKey);
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
		public static string CamelToSnake(string camelCase)
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

}
#endif