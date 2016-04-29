#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Playscape.Editor;
using Playscape.Internal;


namespace Playscape.Editor
{
	class AndroidBuildProcess : BuildProcess
	{
		private readonly string[] EXCLUDE_ADS_JARS = new string[] {
			"com/chartboost/*",
			"com/jirbo/adcolony/*",
			"com/millennialmedia/*",
			"com/startapp/*",
			"com/vungle/*",
			"javax.inject/*",
			"com.nineoldandroids/*",
			"dagger/*"
		};

		/// <summary>
		/// The name of the configuration file
		/// </summary>
		private const string CONFIG_FILE = "assets/playscape/PlayscapeConfig.xml";
		/// <summary>
		/// The location of the patch file (aspectj hooks and logic)
		/// </summary>
		private const string PATCH_FILE = "Assets/Plugins/Playscape/Editor/ThirdParty/playscape_lifecycle.jar";
		private const string PATCH_FILE_FB_V3 = "Assets/Plugins/Playscape/Editor/ThirdParty/playscape_facebook_v3.jar";
		private const string PATCH_FILE_FB_V4 = "Assets/Plugins/Playscape/Editor/ThirdParty/playscape_facebook_v4.jar";

		public AndroidBuildProcess(BuildParams buildParams, 
			ILogger logger,
			BuildCompleted buildCompleted, 
			BuildProgressChanged buildProgressChanged,
			BuildFailed buildFailed) : base (buildParams, logger, buildCompleted, buildProgressChanged, buildFailed)
		{

		}

		public override void Build (string file)
		{
			try
			{
				AndroidApkCreator apkCreator = new AndroidApkCreator(mBuildParams, mLogger, this);
				Stopwatch sw = new Stopwatch();
				sw.Start();

				// dex2jar can oonly work with .jar extentions
				string extractedPath = GetNewTempFolder();
				string dexFilePath = extractedPath + "/classes.dex";
				string configFilePath = extractedPath + "/" + CONFIG_FILE;
				string classesJarFile = GetNewTempFolder("jar");
				string unifiedLibJarFile = GetNewTempFolder("jar");
				string unifiedLibJarFile2 = GetNewTempFolder("jar");
				string unifiedLibJarFile3 = GetNewTempFolder("jar");
				string patchedClassesJarFile = GetNewTempFolder("jar");
				string alignedFile = GetNewTempFolder();
				string excludedJarFile = GetNewTempFolder("jar");

				//0. download game configuration and apply game configuration
				//1. unzip the apk
				//2. extract the dex and convert it into .jar file
				//3. unify the libraries with the patcher
				//4. apply ajc to the .jar (aspectj compiler)
				//5. convert back the .jar into dex
				//6. package the apk
				//7. sign and run zipalign

				retrieveGameConfig();
							

				OnProgress("Applying configuration", 5);
				apkCreator.ExtractEntry(file, CONFIG_FILE, extractedPath);
				apkCreator.ApplyGameConfiguration(ConfigurationInEditor.Instance.MyGameConfiguration, configFilePath);
				apkCreator.AddFileToZip(file, configFilePath, "assets/playscape");

				OnProgress("Extracting resrouces", 10);
				mLogger.V("BuildProcess - Build - UnzipAPK " + sw.ElapsedMilliseconds);
				apkCreator.ExtractEntry(file, "classes.dex", extractedPath);

				OnProgress("Extracting jar files", 20);
				mLogger.V("BuildProcess - Build - executing dex2jar " + sw.ElapsedMilliseconds);
				apkCreator.Dex2jar(dexFilePath, classesJarFile);

				if (!ConfigurationInEditor.Instance.MyAds.MyAdsConfig.EnableAdsSystem) {
					OnProgress("Excluding ads classes jar files", 20);
					apkCreator.Exclude(classesJarFile, excludedJarFile, EXCLUDE_ADS_JARS);

					classesJarFile = excludedJarFile;
				}

				OnProgress("Unifying libraries", 35);
				mLogger.V("BuildProcess - Build - unify libs " + sw.ElapsedMilliseconds);
				apkCreator.unifyLibs(classesJarFile, PATCH_FILE, unifiedLibJarFile);
				apkCreator.unifyLibs(unifiedLibJarFile, PATCH_FILE_FB_V3, unifiedLibJarFile2);
				apkCreator.unifyLibs(unifiedLibJarFile2, PATCH_FILE_FB_V4, unifiedLibJarFile3);

				OnProgress("Applying analytics", 40);
				mLogger.V("BuildProcess - Build - Apply Patch " + sw.ElapsedMilliseconds);
				apkCreator.applyPatch(unifiedLibJarFile3, unifiedLibJarFile3, patchedClassesJarFile);

				OnProgress("Re-dexing libraries", 60);
				mLogger.V("BuildProcess - Build - Executing lib2dex " + sw.ElapsedMilliseconds);
				apkCreator.Jar2dex(patchedClassesJarFile, dexFilePath);

				OnProgress("Packaging apk", 80);
				mLogger.V("BuildProcess - Build - archiving sources " + sw.ElapsedMilliseconds);
				apkCreator.AddFileToZip(file, dexFilePath, null);

				OnProgress("Signing apk", 87);
				mLogger.V("BuildProcess - Build - signing APK " + sw.ElapsedMilliseconds);
				apkCreator.signApk(file);

				OnProgress("running zipalign on apk", 95);
				mLogger.V("BuildProcess - Build - zip align " + sw.ElapsedMilliseconds);
				apkCreator.applyZipalign(file, alignedFile);
				File.Delete(file);
				File.Move(alignedFile, file);

				OnProgress("Cleaning up", 98);

				sw.Stop();
			} 
			finally
			{
				// cleanup the files and call the onComplete delegate
				Cleanup();
				OnProgress("Done", 100);
				OnCompleted();
			}                  
		}

	}
}
#endif

