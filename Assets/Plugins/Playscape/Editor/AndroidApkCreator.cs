#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;
using Playscape.Internal;
using UnityEditor;
using System.Collections;

namespace Playscape.Editor
{
	public class AndroidApkCreator
	{
		//private static readonly string APKTOOL_FOLDER;
		//private static readonly string APK_TOOL_PATH;
		private const string APK_TOOL_PATH = "Assets/Plugins/Playscape/ThirdParty/apktool.jar" ;
		private const string ZIPALIGN_TOOL_PATH = "/Assets/Plugins/Playscape/ThirdParty/zipalign" ;
		
		static AndroidApkCreator()
		{}
		
		public static void cleanUselessResources(string pathToRemove) 
		{
			L.D("Removing Path: " + pathToRemove);
		}
		
		public static void decompile(string apkPath, string outputPath) 
		{
			useApkTool (false, apkPath, outputPath);
		}
		
		public static void recompile(string targetPath, string outputPath) 
		{
			useApkTool (true, targetPath, outputPath);
		}
		
		// at the end auto sing apk calling
		private static void useApkTool(Boolean isCompile, string targetPath, string outputPath) 
		{
			//UnityEngine.Debug.LogError (APKTOOL_FOLDER);
			string unsignedPath = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			unsignedPath = unsignedPath.Substring(0, unsignedPath.Length - 4) + ".apk";
			
			string arguments = "";
			string apkToolPath = APK_TOOL_PATH;
			if (isWindows ()) {
				apkToolPath = "\"" + APK_TOOL_PATH + "\"";
				outputPath = "\"" + outputPath + "\"";
				unsignedPath = "\"" + unsignedPath + "\"";
				targetPath = "\"" + targetPath + "\"";
			}
			
			if (isCompile) {
				arguments = "-jar " + apkToolPath + " b -f " + outputPath + " -o " + unsignedPath;
			} else {
				arguments = "-jar " + apkToolPath + " d -s -f " + targetPath + " -o " + outputPath + "/";
			}
			
			string command;
			
			if (isWindows ()) {
				command = System.Environment.GetEnvironmentVariable("JAVA_HOME") + @"/bin/java.exe";
			}
			else {
				command = "/usr/bin/java";
			}
			
			L.D ("Command " + command);
			L.D ("Argumnets " + arguments);
			
			int exitCode = runProcessWithCommand (command, arguments);
			string message = (isCompile ? "compile" : "decompile") + " was " + (exitCode == 0 ? "" : "not") + " successfully"; 
			L.W (message);
			
			if (exitCode == 0 && isCompile) {
				signApk (unsignedPath, targetPath);
			}
		}
		
		private static void signApk(string unsignedAPKPath, string targetPath) 
		{
			string keysotre_path = PlayerSettings.Android.keystoreName;
			string alias = PlayerSettings.Android.keyaliasName;
			string storepass = PlayerSettings.Android.keystorePass;
			string keypass = PlayerSettings.Android.keyaliasPass;
			
			if (UnityEngine.Debug.isDebugBuild && (keysotre_path == null || (keysotre_path != null && keysotre_path.Length == 0))) {
				keysotre_path = (isWindows () ? System.Environment.GetEnvironmentVariable ("USERPROFILE") : 
				                 Directory.GetParent (Environment.GetFolderPath (Environment.SpecialFolder.Desktop)).FullName) 
					+ "/.android/debug.keystore";
				alias = "androiddebugkey";
				storepass = "android";
				keypass = "android";
			}
			
			string command;
			if (isWindows ()) {
				command = System.Environment.GetEnvironmentVariable("JAVA_HOME") + @"/bin/jarSigner.exe";
			} else {
				command = "/usr/bin/jarsigner";
			}
			
			string arguments = "-verbose -keystore " + keysotre_path + " -storepass " + storepass + " -keypass " + keypass + " " + unsignedAPKPath + " " + alias;
			L.D ("command: {0}", command);
			L.D ("arguments: {0}", arguments);
			int exitCode = runProcessWithCommand (command, arguments);
			string message = "apk was " + (exitCode == 0 ? "" : "not") + " signed successfully" + (exitCode == 0 ? "" : " with code " + exitCode); 
			L.W (message);
			
			string signedAPKPATH = applyZipalign (unsignedAPKPath);
			
			if (File.Exists(targetPath)) {
				File.Delete (targetPath);
			}
			
			File.Move(signedAPKPATH, targetPath);
		}
		
		private static string applyZipalign(string unsignedAPKPath) {
			string command = Directory.GetCurrentDirectory() + ZIPALIGN_TOOL_PATH;
			if (isWindows ()) {
				command += ".exe";
			} 
			
			string signedAPKPATH = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			signedAPKPATH = signedAPKPATH.Substring(0, signedAPKPATH.Length - 4) + ".apk";
			
			string arguments = "-v 4 " + unsignedAPKPath + " " + signedAPKPATH;
			L.D ("command: {0}", command);
			L.D ("arguments: {0}", arguments);
			int exitCode = runProcessWithCommand (command, arguments);
			string message = "apk was " + (exitCode == 0 ? "" : "not") + " zipaligned successfully" + (exitCode == 0 ? "" : " with code " + exitCode); 
			L.W (message);
			
			return signedAPKPATH;
		}
		
		public static int runProcessWithCommand(string command, string args)
		{
			var processInfo = new ProcessStartInfo (command, args)
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				//				RedirectStandardOutput = true
			};
			Process proc;
			
			if ((proc = Process.Start(processInfo)) == null)
			{
				throw new InvalidOperationException("Can not start new process with command: " + command + " in AndroidApkCreator.");
			}
			
			proc.WaitForExit();
			int exitCode = proc.ExitCode;			
			
			proc.Close();
			
			return exitCode;
		}
		
		private static Boolean isMac() 
		{
			PlatformID pid = getPlatfomId ();
			return pid == PlatformID.MacOSX;
		}
		
		private static Boolean isUnix() 
		{
			PlatformID pid = getPlatfomId ();
			return pid == PlatformID.Unix;
		}
		
		private static Boolean isWindows() 
		{
			PlatformID pid = getPlatfomId ();
			return pid == PlatformID.Win32NT || pid == PlatformID.Win32S || pid == PlatformID.Win32Windows || pid == PlatformID.WinCE;
		}
		
		private static PlatformID getPlatfomId() 
		{
			OperatingSystem os = Environment.OSVersion;
			return os.Platform;
		}
	}
}
#endif