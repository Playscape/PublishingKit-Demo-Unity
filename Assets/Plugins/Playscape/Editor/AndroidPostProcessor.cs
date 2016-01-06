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
		public const string PLAYSCAPE_CONFIG_XML_PATH = "Assets/StreamingAssets/playscape/PlayscapeConfig.xml";
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
		
		public override void CheckForWarnings(WarningAccumulator warnings)
		{
			base.CheckForWarnings (warnings);

			//check JAVA_HOME variable
			bool javaHomeNotSet = string.IsNullOrEmpty(JDKFolder.Path);
			bool javaHomeNotExist = !System.IO.Directory.Exists(JDKFolder.Path);
			bool javaHomeNotSatisfy = !JDKFolder.IsSatisfyingJDKVersion;
			
			warnings.WarnIf (javaHomeNotSet, Playscape.Internal.Warnings.JAVA_HOME_ENV_VAR_NOT_SET);
			warnings.WarnIf (javaHomeNotExist, Playscape.Internal.Warnings.JAVA_HOME_ENV_VAR_NOT_EXIST);
			warnings.WarnIf (javaHomeNotSatisfy, Playscape.Internal.Warnings.JAVA_NOT_SATISFYING_PLAYSCAPE_SDK);
			
			//check ANDROID_HOME variable
			bool androidHomeNotSet = string.IsNullOrEmpty(AndroidSDKFolder.Path);
			bool androidHomeNotExist = !System.IO.Directory.Exists(AndroidSDKFolder.Path);
			bool androidHomeNotHasGooglePlayServices = !System.IO.File.Exists(AndroidSDKFolder.GooglePlayServicesJarPath);
			
			warnings.WarnIf (androidHomeNotSet, Playscape.Internal.Warnings.ANDROID_HOME_ENV_VAR_NOT_SET);
			warnings.WarnIf (androidHomeNotExist, Playscape.Internal.Warnings.ANDROID_HOME_ENV_VAR_NOT_EXIST);
			warnings.WarnIf (androidHomeNotHasGooglePlayServices, Playscape.Internal.Warnings.ANDROID_NOT_HAS_GOOGLE_PLAY_SERVICES);
		}
		
		public void build(bool async, BuildProcess.BuildCompleted completedCallback, BuildProcess.BuildProgressChanged progressCallback, BuildProcess.BuildFailed failedCallback) {
			BuildParams bp = ConstructBuildParams();
			BuildProcess process = new BuildProcess(bp, new UnityDebugLogger(), completedCallback, progressCallback, failedCallback);

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

            if (string.IsNullOrEmpty(bp.keysotre_path))
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
                    build(false, onComplete, onProgress, OnFailed);
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

		public static void OnFailed(object sender, string message) {
			EditorUtility.DisplayDialog (
				"Playscape Post Proccessor",
				message,
				"OK"
			);
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
