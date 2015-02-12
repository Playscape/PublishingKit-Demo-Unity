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

		// TODO add if for windows and unix 
		private const string KEY_STORE_PATH = "/Users/mac/.android/debug.keystore";


		public static void cleanUselessResources(string pathToRemove) 
		{
			DirectoryInfo directoryToRemove = new DirectoryInfo(pathToRemove);
			if (directoryToRemove != null && directoryToRemove.Exists) {
				directoryToRemove.Delete (true);
			}
		}

		public static void decompile(string targetPath, string outputPath) 
		{
			useApkTool (false, targetPath, outputPath, "");
		}

		public static void recompile(string targetPath, string outputPath, string buildTargetPath) 
		{
			// remove 'origin/metadata' to make apk unsigned
			cleanUselessResources (outputPath + "/original/");
			useApkTool (true, targetPath, outputPath, buildTargetPath);
			// remove folder with decompiled source
			cleanUselessResources (outputPath);
		}

		// at the end auto sing apk calling
		private static void useApkTool(Boolean isCompile, string targetPath, string outputPath, string buildTargetPath) 
		{
			string apkToolPath = Directory.GetParent (Environment.CurrentDirectory).FullName + APK_TOOL_PATH;
			
			string arguments = "";
			if (isCompile) {
//				string buildTargetPath = mPathToMainProjcetSources;
				arguments = "-jar " + apkToolPath + " b -f " + outputPath + " -o " + buildTargetPath;
			} else {
				arguments = "-jar " + apkToolPath + " d -s -f " + targetPath + " -o " + outputPath + "/";
			}

			string command = "java";

			if (isWindows ()) {
				command += ".exe";
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
			// TODO extract to constants
			string alias = "androiddebugkey";
			string storepass = "android";
			string keypass = "android";
			//			jarsigner -verbose -keystore ~/.android/debug.keystore -storepass android -keypass android newUnityProject_out/dist/newUnityProject.apk androiddebugkey
			string arguments = "verbose -keystore " + KEY_STORE_PATH + " -storepass " + storepass + " -keypass " + keypass + targetPath + " " + alias;

			string command = "jarsigner";
			if (isWindows ()) {
				command += ".exe";
			}
			runProcessWithCommand (command, arguments);
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

