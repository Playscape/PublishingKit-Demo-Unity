using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public class Build : MonoBehaviour {
	static BuildConfig buildConfig = parseArgs();
	  
    private static void Configure() {
        var config = Playscape.Editor.ConfigurationInEditor.Instance;
        EditorUtility.SetDirty (config);
    }

	private static string[] GetAllScenes()
	{
		List<string> temp = new List<string>();
		foreach (UnityEditor.EditorBuildSettingsScene scene in UnityEditor.EditorBuildSettings.scenes)
		{
			if (scene.enabled)
			{

				temp.Add(scene.path);
			}
		}
		return temp.ToArray();
	}
	
	[MenuItem("Window/Build for Android")]
	public static void BuildAndroid() {
		Configure();
		BuildPipeline.BuildPlayer(GetAllScenes(), buildConfig.OutputPath, BuildTarget.Android, BuildOptions.AcceptExternalModificationsToPlayer | BuildOptions.Development);
	}
    
    [MenuItem("Window/Build for IOS")]
	public static void BuildIOS() {
		Configure();
        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.SimulatorSDK;
		BuildPipeline.BuildPlayer(GetAllScenes(), buildConfig.OutputPath, BuildTarget.iPhone, BuildOptions.Development);
	}

	private static BuildConfig parseArgs() {
		string [] args = Environment.GetCommandLineArgs();

		BuildConfig config = new BuildConfig();

		foreach (var arg in args) {
			var argParts = arg.Split('=');
			if (argParts.Length == 2) {
				var argName = argParts[0];
				var argValue = argParts[1];
				
				switch(argName) {
				case "-outputPath":
					config.OutputPath = argValue;
					break;
				}
			}
			
		}

		return config;
	}

	private class BuildConfig {
		public BuildConfig() {
			OutputPath = "c:/temp/output";

		}

		public string OutputPath {
			get;
			set;
		}

	}
}
#endif