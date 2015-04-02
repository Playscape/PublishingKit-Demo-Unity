#if UNITY_EDITOR
#if UNITY_ANDROID
using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;
using Playscape.Internal;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using Ionic.Zip;

namespace Playscape.Editor
{
	public class AndroidApkCreator
	{
		private const string APK_TOOL_PATH = "Assets/Plugins/Playscape/ThirdParty/apktool.jar" ;
		private const string ZIPALIGN_TOOL_PATH = "/Assets/Plugins/Playscape/ThirdParty/zipalign";
		private const string DEX_2_JAR_TOOL_HOME_PATH = "Assets/Plugins/Playscape/ThirdParty/dex2jar";
		private const string ASPECT_HOME_PATH = "Assets/Plugins/Playscape/ThirdParty/aspectsj/";
		private const string ANDROID_JAR = "/usr/local/android-sdk-macosx/platforms/android-19/android.jar";
		private const string GOOGLE_PLAY_SERVICES_JAR = "/usr/local/android-sdk-macosx/extras/google/google_play_services/libproject/google-play-services_lib/libs/google-play-services.jar";

		// temp solution
		private const string PATCH_FILE = "../../../bridges/android/playscape_lifecycle/bin/classes.jar";
		
		static AndroidApkCreator()
		{}
		
		public static void cleanUselessResources(string pathToRemove) 
		{
			L.D("Removing Path: " + pathToRemove);
		}
		
		public static void decompile(string apkPath, string outputPath, bool isDebugBuild) 
		{
			useApkTool (false, apkPath, outputPath, isDebugBuild);
		}
		
		public static void recompile(string targetPath, string outputPath, bool isDebugBuild) 
		{
			useApkTool (true, targetPath, outputPath, isDebugBuild);
		}

		public static void applyAspects(string targetPath, string outputPath, bool isDebugBuild) 
		{
			string dstFile = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ()) + ".jar";
			string targetFile = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ()) + ".jar";
			string dex2jarFile = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ()) + ".jar";
			string pathName = PATCH_FILE;

			executeDex2jar (dex2jarFile, outputPath, isDebugBuild);
			unifyLibs (qualifyPath(dex2jarFile), qualifyPath(pathName), qualifyPath(dstFile));
			applyPatch (qualifyPath(dstFile), qualifyPath(dstFile), qualifyPath(targetFile));
			executeLib2dex (targetFile, outputPath, isDebugBuild);

			File.Delete (qualifyPath(dstFile));
			File.Delete (qualifyPath(targetFile));
		}

		private static void executeDex2jar(string dex2jarFile, string outputPath, bool isDebugBuild) {
			string command = getJavaCommand ();

			string classpath = getClasspathForDex2JarTool ();

			string mainClass = "com.googlecode.dex2jar.tools.Dex2jarCmd";
			string mainClassParams = "-f -o " + qualifyPath(dex2jarFile) + " " + qualifyPath(outputPath + "/classes.dex");

			string arguments = "-classpath " + classpath + " -Xms512m -Xmx1024m " + mainClass + " " + mainClassParams;
			
			L.D ("Command " + command);
			L.D ("Argumnets " + arguments);
			
			int exitCode = runProcessWithCommand (command, arguments);
			string message = "executeDex2jar was" + (exitCode == 0 ? "" : " not") + " successfully with code " + exitCode; 
			L.W (message);
		}

		private static void unifyLibs(string origin, string patch, string dst) {
			FileStream originStream = new FileStream (origin, FileMode.Open, FileAccess.Read, FileShare.Read);
			FileStream patchStream = new FileStream (patch, FileMode.Open, FileAccess.Read, FileShare.Read);
			
			ZipFile originZipFile = new ZipFile(origin);
			ZipFile patchZipFile = new ZipFile(patch);
			List<string> originFiles = new List<string> ();
			foreach (ZipEntry ze in originZipFile)
			{
				if (ze.IsDirectory)
					continue;
				if(ze.FileName.EndsWith(".class")) {
					originFiles.Add(ze.FileName);
				}        
			}
			List<string> patchFiles = new List<string> ();
			foreach (ZipEntry ze in patchZipFile)
			{
				if (ze.IsDirectory)
					continue;
				if(ze.FileName.EndsWith(".class")) {
					patchFiles.Add(ze.FileName);
				}        
			}
			IEnumerable<string> differenceQuery = patchFiles.Except(originFiles);
			
			byte[] buf = new byte[4096];
			
			File.Copy (origin, dst, true);
			string unzipFolderName = Path.Combine(Path.GetDirectoryName(patchZipFile.Name),
			                                      Path.GetFileNameWithoutExtension(patchZipFile.Name));
			patchZipFile.ExtractAll(unzipFolderName, ExtractExistingFileAction.OverwriteSilently);
			patchZipFile.Dispose();

			using (ZipFile dstZipFile = new ZipFile(dst))
			{
				var differenceQuery_new =
					differenceQuery.Select(x => x.Insert(0, "/").Insert(0, Path.Combine(Path.GetDirectoryName(patchZipFile.Name),
					                                                                     Path.GetFileNameWithoutExtension(patchZipFile.Name))));
				
				for (int i = 0; i < differenceQuery_new.Count(); i++)
				{
					dstZipFile.AddFile(differenceQuery_new.ElementAt(i), Path.GetDirectoryName(differenceQuery.ElementAt(i)));
					dstZipFile.Save(dst);
				}
				
			}
			
			originStream.Close ();
			patchStream.Close ();
			File.Delete (origin);

			DirectoryInfo downloadedMessageInfo = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(patch), Path.GetFileNameWithoutExtension (patch)));
			foreach (FileInfo file in downloadedMessageInfo.GetFiles())
			{
				file.Delete(); 
			}
			foreach (DirectoryInfo dir in downloadedMessageInfo.GetDirectories())
			{
				dir.Delete(true); 
			}
			Directory.Delete (Path.Combine(Path.GetDirectoryName(patch), Path.GetFileNameWithoutExtension (patch)), true);
		}

		private static void applyPatch(string inpath, string aspectpath, string outjar) {
			string command = getJavaCommand ();

			string delimeter = (isWindows ()) ? ";" : ":";
			string classpath = Path.Combine (ASPECT_HOME_PATH, "aspectjtools.jar") + delimeter
				+ Path.Combine (ASPECT_HOME_PATH, "aspectjrt.jar") + delimeter 
					+ ANDROID_JAR + delimeter 
				+ GOOGLE_PLAY_SERVICES_JAR;

			string arguments = "-classpath " + classpath + " -Xmx8g org.aspectj.tools.ajc.Main -source 1.5 -Xlint:ignore -inpath "
				+ qualifyPath(inpath) + " -aspectpath " + qualifyPath(aspectpath) + " -outjar " + qualifyPath(outjar);

			L.W ("Command " + command);
			L.W ("Argumnets " + arguments);
			
			int exitCode = runProcessWithCommand (command, arguments);
			string message = "aspects was applyed" + (exitCode == 0 ? "" : " not") + " successfully with code " + exitCode;
			L.W (message);
		}

		private static void executeLib2dex(string targetPath, string outputPath, bool isDebugBuild) {
			string command = getJavaCommand ();
			
			string classpath = getClasspathForDex2JarTool ();
			
			string mainClass = "com.googlecode.dex2jar.tools.Jar2Dex";
			string mainClassParams = "-f -o " + qualifyPath(outputPath + "/classes.dex") + " " + qualifyPath(targetPath);
			
			string arguments = "-classpath " + classpath + " -Xms512m -Xmx1024m " + mainClass + " " + mainClassParams;

			L.D ("Command " + command);
			L.D ("Argumnets " + arguments);
			
			int exitCode = runProcessWithCommand (command, arguments);
			string message = "executeLib2dex was" + (exitCode == 0 ? "" : " not") + " successfully with code " + exitCode;
			L.W (message);
		}

		// at the end auto sing apk calling
		private static void useApkTool(Boolean isCompile, string targetPath, string outputPath, bool isDebugBuild) 
		{
			//UnityEngine.Debug.LogError (APKTOOL_FOLDER);
			string unsignedPath = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			unsignedPath = unsignedPath.Substring(0, unsignedPath.Length - 4) + ".apk";
			
			string arguments = "";
			string apkToolPath = APK_TOOL_PATH;

			if (isCompile) {
				arguments = "-jar " + qualifyPath(apkToolPath) + " b -f " + qualifyPath(outputPath) + " -o " + qualifyPath(unsignedPath);
			} else {
				arguments = "-jar " + qualifyPath(apkToolPath) + " d -s -f " + qualifyPath(targetPath) + " -o " + qualifyPath(outputPath);
			}
			
			string command = getJavaCommand ();
			
			L.D ("Command " + command);
			L.D ("Argumnets " + arguments);
			
			int exitCode = runProcessWithCommand (command, arguments);
			string message = (isCompile ? "compile" : "decompile") + " was " + (exitCode == 0 ? "" : "not") + " successfully"; 
			L.W (message);
			
			if (exitCode == 0 && isCompile) {
				signApk (unsignedPath, targetPath, isDebugBuild);
			}
		}
		
		private static void signApk(string unsignedAPKPath, string targetPath, bool isDebugBuild) 
		{
			string keysotre_path = PlayerSettings.Android.keystoreName;
			string alias = PlayerSettings.Android.keyaliasName;
			string storepass = PlayerSettings.Android.keystorePass;
			string keypass = PlayerSettings.Android.keyaliasPass;
			
			if (isDebugBuild && (keysotre_path == null || (keysotre_path != null && keysotre_path.Length == 0))) {
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
			
			string arguments = "-verbose -keystore " + qualifyPath(keysotre_path) + " -storepass " + storepass + " -keypass " + keypass + " " + qualifyPath(unsignedAPKPath) + " " + alias;
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
			
			string arguments = "-v 4 " + qualifyPath(unsignedAPKPath) + " " + qualifyPath(signedAPKPATH);
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
				RedirectStandardOutput = true
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

		private static string getJavaCommand() {
			return (isWindows ()) ? System.Environment.GetEnvironmentVariable ("JAVA_HOME") + @"/bin/java.exe" : "/usr/bin/java";
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

		private static string qualifyPath(string str) {
			if (isWindows ()) {
				str = "\"" + str + "\"";
			}
			return str;
		}

		private static string getClasspathForDex2JarTool() {
			string delimeter = (isWindows ()) ? ";" : ":";
			string classpath = Path.Combine (DEX_2_JAR_TOOL_HOME_PATH, "lib/asm-all-3.3.1.jar") + delimeter 
				+ Path.Combine (DEX_2_JAR_TOOL_HOME_PATH, "lib/commons-lite-1.15.jar") + delimeter 
					+ Path.Combine (DEX_2_JAR_TOOL_HOME_PATH, "lib/dex-ir-1.12.jar") + delimeter 
					+ Path.Combine (DEX_2_JAR_TOOL_HOME_PATH, "lib/dex-reader-1.15.jar") + delimeter 
					+ Path.Combine (DEX_2_JAR_TOOL_HOME_PATH, "lib/dex-tools-0.0.9.15.jar") + delimeter 
					+ Path.Combine (DEX_2_JAR_TOOL_HOME_PATH, "lib/dex-translator-0.0.9.15.jar") + delimeter 
					+ Path.Combine (DEX_2_JAR_TOOL_HOME_PATH, "lib/dx.jar") + delimeter 
					+ Path.Combine (DEX_2_JAR_TOOL_HOME_PATH, "lib/jar-rename-1.6.jar") + delimeter 
					+ Path.Combine (DEX_2_JAR_TOOL_HOME_PATH, "lib/asmin-p2.5.jar");
			return classpath;
		}
	}
}
#endif
#endif