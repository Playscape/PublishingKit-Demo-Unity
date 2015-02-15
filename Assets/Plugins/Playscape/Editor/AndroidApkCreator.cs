using System;
using System.Diagnostics;
using System.IO;
using Playscape.Internal; 

namespace Playscape.Editor
{
	public class AndroidApkCreator
	{
		private const string APK_CONFIG_STUFF_PATH = "/Apk_config_stuff";
		private const string APK_TOOL_PATH = APK_CONFIG_STUFF_PATH + "/apktool.jar";

		private const string DEFAULT_ALIAS = "androiddebugkey";
		private const string DEFAULT_STOREPASS = "android";
		private const string DEFAULT_KEYPASS = "android";


		public static void cleanUselessResources(string pathToRemove) 
		{
			DirectoryInfo directoryToRemove = new DirectoryInfo(pathToRemove);
			if (directoryToRemove != null && directoryToRemove.Exists) {
				directoryToRemove.Delete (true);
			} else {
				FileInfo fileToRemove = new FileInfo (pathToRemove);
				if(fileToRemove != null && fileToRemove.Exists) {
					fileToRemove.Delete();
				}
			}
		}

		public static void decompile(string targetPath, string outputPath) 
		{
			useApkTool (false, targetPath, outputPath);
		}

		public static void recompile(string targetPath, string outputPath) 
		{
			// remove origin apk
			cleanUselessResources (targetPath);
			useApkTool (true, targetPath, outputPath);
			// remove folder with decompiled source
			cleanUselessResources (outputPath);
		}

		// at the end auto sing apk calling
		private static void useApkTool(Boolean isCompile, string targetPath, string outputPath) 
		{
			string apkToolPath = Directory.GetParent (Environment.CurrentDirectory).FullName + APK_TOOL_PATH;
			
			string arguments = "";
			if (isCompile) {
				arguments = "-jar " + apkToolPath + " b -f " + outputPath + " -o " + targetPath;
			} else {
				arguments = "-jar " + apkToolPath + " d -s -f " + targetPath + " -o " + outputPath + "/";
			}

			string command;

			if (isWindows ()) {
				command = "%JAVA_HOME%/bin/java.exe";
			} else {
				command = "/usr/bin/java";
			}
			
			int exitCode = runProcessWithCommand (command, arguments);
			string message = (isCompile ? "compile" : "decompile") + " was " + (exitCode == 0 ? "" : "not") + " successfully"; 
			L.W (message);

			if (exitCode == 0 && isCompile) {
				signApk (targetPath);
			}
		}
		
		private static void signApk(string targetPath) 
		{

			string keystore = "/.android/debug.keystore";
			string keyStorePath;
			string keyStorePath1;

			keyStorePath = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)).FullName + keystore;

			string arguments = "-verbose -keystore " + keyStorePath + " -storepass " + DEFAULT_STOREPASS + " -keypass " + DEFAULT_KEYPASS + " " + targetPath + " " + DEFAULT_ALIAS;

			string command;
			
			if (isWindows ()) {
				command = "%JAVA_HOME%/bin/jarsigner.exe";
			} else {
				command = "/usr/bin/jarsigner";
			}

			int exitCode = runProcessWithCommand (command, arguments);
			string message = "apk was " + (exitCode == 0 ? "" : "not") + " signed successfully" + (exitCode == 0 ? "" : " with code " + exitCode); 
			L.W (message);
		}
		
		public static int runProcessWithCommand(string command, string args)
		{
			var processInfo = new ProcessStartInfo (command, args)
			{
				CreateNoWindow = true,
				UseShellExecute = false
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

