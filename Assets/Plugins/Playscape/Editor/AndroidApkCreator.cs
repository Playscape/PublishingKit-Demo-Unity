#if UNITY_EDITOR
using System;
using System.Text;
using System.Diagnostics;
using System.IO;
using Playscape.Internal;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using Ionic.Zip;
using System.Xml;
using System.Text.RegularExpressions;

namespace Playscape.Editor
{
    /// <summary>
    /// A class providing helper methods for hanlding APK files
    /// </summary>
	public class AndroidApkCreator
	{
		private const string APK_TOOL_PATH = "Assets/Plugins/Playscape/ThirdParty/apktool.jar" ;
		private const string ZIPALIGN_TOOL_PATH = "/Assets/Plugins/Playscape/ThirdParty/zipalign";
		private const string DEX_2_JAR_TOOL_HOME_PATH = "Assets/Plugins/Playscape/ThirdParty/dex2jar";
		private const string ASPECT_HOME_PATH = "Assets/Plugins/Playscape/ThirdParty/aspectsj/";
		private const string DEFAULT_ANDROID_PLATFORM = "android-19";

		private static string ANDROID_HOME = PlatformUtils.isWindows() ? System.Environment.GetEnvironmentVariable("ANDROID_HOME") : AndroidSDKFolder.Path;

        private static string sJavaCommand = (PlatformUtils.isWindows()) ? System.Environment.GetEnvironmentVariable("JAVA_HOME") + @"/bin/java.exe" : "/usr/bin/java";
        private static string sDelimeter = (PlatformUtils.isWindows()) ? ";" : ":";
        private static string sClasspath = Path.Combine(DEX_2_JAR_TOOL_HOME_PATH, "lib/asm-all-3.3.1.jar") + sDelimeter
        + Path.Combine(DEX_2_JAR_TOOL_HOME_PATH, "lib/commons-lite-1.15.jar") + sDelimeter
            + Path.Combine(DEX_2_JAR_TOOL_HOME_PATH, "lib/dex-ir-1.12.jar") + sDelimeter
            + Path.Combine(DEX_2_JAR_TOOL_HOME_PATH, "lib/dex-reader-1.15.jar") + sDelimeter
            + Path.Combine(DEX_2_JAR_TOOL_HOME_PATH, "lib/dex-tools-0.0.9.15.jar") + sDelimeter
            + Path.Combine(DEX_2_JAR_TOOL_HOME_PATH, "lib/dex-translator-0.0.9.15.jar") + sDelimeter
            + Path.Combine(DEX_2_JAR_TOOL_HOME_PATH, "lib/dx.jar") + sDelimeter
            + Path.Combine(DEX_2_JAR_TOOL_HOME_PATH, "lib/jar-rename-1.6.jar") + sDelimeter
            + Path.Combine(DEX_2_JAR_TOOL_HOME_PATH, "lib/asmin-p2.5.jar");

        /// <summary>
        /// Holds the reference to the temporary file provider
        /// </summary>
        private ITempFileProvider mTempFileProvider;

        /// <summary>
        /// Holds the build parameters
        /// </summary>
        private BuildParams buildParams { get; set; }


        /// <summary>
        /// Holds a referenec to the logger
        /// </summary>
        private ILogger logger { get; set; }

        /// <summary>
        /// Constructs a new AndroidAPK creator instnace
        /// </summary>
        /// <param name="bp">A reference to the build parameters</param>
        /// <param name="logger">The logger used for tracing</param>
        /// <param name="tempFileProvider">A reference to a service providing temporary folder</param>
        public AndroidApkCreator(BuildParams bp, ILogger logger, ITempFileProvider tempFileProvider)
        {
            this.buildParams = bp;
            this.logger = logger;
            this.mTempFileProvider = tempFileProvider;
        }

        /// <summary>
        /// Decompiles the APK using APK tool
        /// </summary>
        /// <param name="apkPath">The path to the apk</param>
        /// <param name="outputPath">The output folder</param>
		public void decompile(string apkPath, string outputPath)
		{
            useApkTool(false, apkPath, outputPath);
		}

        /// <summary>
        /// Recompiles the folder using apktool
        /// </summary>
        /// <param name="targetPath">The path of the decompiled APK</param>
        /// <param name="outputPath">The destination of the APK to create</param>
        [Obsolete("Re-compiling using APKtool is causing critical issues in many scenarios", true)]
        public void recompile(string targetPath, string outputPath)
        {
            useApkTool(true, targetPath, outputPath);
        }

        /// <summary>
        /// Extract an ZIP file to a folder
        /// </summary>
        /// <param name="zipPath">The ZIP path</param>
        /// <param name="outputPath">The output path</param>
        public void ExtractZip(string zipPath, string outputPath)
        {
            logger.V("ExtractZip started");
            using (ZipFile zipFile = new ZipFile(zipPath))
            {
                zipFile.ExtractAll(outputPath);
            }
            logger.V("Extract Zip ended");
        }

        /// <summary>
        /// Extract an entry of a zip file to a folder
        /// </summary>
        /// <param name="zipPath">The ZIP file path</param>
        /// <param name="zipEntry">The entry name</param>
        /// <param name="outputPath">The output path</param>
        public void ExtractEntry(string zipPath, string zipEntry, string outputPath)
        {
            logger.V("ExtractEntry started");
            using (ZipFile zipFile = new ZipFile(zipPath))
            {
                zipFile[zipEntry].Extract(outputPath);
            }
            logger.V("ExtractEntry ended");
        }

        /// <summary>
        /// Adds a file to a zip archive
        /// </summary>
        /// <param name="zipPath">The zip archive to edit</param>
        /// <param name="filePath">The file to add</param>
        /// <param name="location">The location of the file to add in the zip archive</param>
        public void AddFileToZip(string zipPath, string filePath, string location)
        {
            logger.V("AddFileToZip start");

            logger.V("Adding " + filePath);

            try
            {
                if (location == "/" || location == "./") location = string.Empty;
                string fileName = new FileInfo(filePath).Name;
                string zipEntryLocation = !String.IsNullOrEmpty(location) ? location + "/" + fileName : fileName;
                using (ZipFile zipFile = new ZipFile(zipPath))
                {
                    if (zipFile.EntryFileNames.Contains(zipEntryLocation))
                    {
                        zipFile.RemoveEntry(zipEntryLocation);
                    }
                    zipFile.AddFile(filePath, !String.IsNullOrEmpty(location) ? location : "./");
                    zipFile.Save();
                }

                logger.V("AddFileToZip APK successfully");
            }
            catch (Exception e)
            {
                logger.E("could not zip directory: " + e.Message);
                throw;
            }
        }

        /// <summary>
        /// Creates an archive from a given folder
        /// </summary>
        /// <param name="src">The folder to zip</param>
        /// <param name="dst">The zip archive path</param>
        public void ZipFolder(string src, string dst)
        {
            logger.V("Zip to APK start");

            logger.V("Zipping " + src + " to path " + dst);

            try
            {
                using (ZipFile a = new ZipFile())
                {
                    a.AddDirectory(src, "./");
                    a.Save(dst);
                }

                logger.V("ZipFolder successfully");
            }
            catch (Exception e)
            {
                logger.E("could not zip directory: " + e.Message);
                throw;
            }
        }


        /// <summary>
        /// Converts a dexfile to a jar file
        /// </summary>
        /// <param name="src">The path of the .dex file</param>
        /// <param name="dst">The path to the generated .jar file</param>
		public void Dex2jar(string src, string dst) {
			string command = getJavaCommand ();

			string classpath = getClasspathForDex2JarTool ();

			string mainClass = "com.googlecode.dex2jar.tools.Dex2jarCmd";
			string mainClassParams = "-f -o " + PlatformUtils.qualifyPath(dst) + " " + PlatformUtils.qualifyPath(src);

			string arguments = "-classpath " + classpath + " -Xms512m -Xmx1024m " + mainClass + " " + mainClassParams;

            logger.V("Command " + command);
            logger.V("Argumnets " + arguments);

			int exitCode = runProcessWithCommand (command, arguments);
			string message = "executeDex2jar was" + (exitCode == 0 ? "" : " not") + " successfully with code " + exitCode;
            logger.V(message);

            if (exitCode != 0)
            {
                throw new Exception("failed to execture dex2jar");
            }
		}

        /// <summary>
        /// Converts a .jar file to a dex file
        /// </summary>
        /// <param name="targetPath">The jar to convert (must of a .jar extention)</param>
        /// <param name="outputPath"></param>
        public void Jar2dex(string targetPath, string outputPath)
        {
            if (!targetPath.EndsWith(".jar")) throw new ArgumentException("jar file must have a .jar extension");

            string command = getJavaCommand();

            string classpath = getClasspathForDex2JarTool();

            string mainClass = "com.googlecode.dex2jar.tools.Jar2Dex";
            string mainClassParams = "-f -o " + PlatformUtils.qualifyPath(outputPath) + " " + PlatformUtils.qualifyPath(targetPath);

            string arguments = "-classpath " + classpath + " -Xms512m -Xmx1024m " + mainClass + " " + mainClassParams;

            logger.V("Command " + command);
            logger.V("Argumnets " + arguments);

            int exitCode = runProcessWithCommand(command, arguments);
            string message = "executeLib2dex was" + (exitCode == 0 ? "" : " not") + " successfully with code " + exitCode;
            logger.V(message);

            if (exitCode != 0)
            {
                throw new Exception("failed to execture lib2dex");
            }
        }

        /// <summary>
        /// Unifies to .jar file into one archive
        /// </summary>
        /// <param name="origin">The first library</param>
        /// <param name="patch">The second library</param>
        /// <param name="dst">The path to the output unified library</param>
        public void unifyLibs(string origin, string patch, string dst)
        {
            FileStream originStream = null;
            FileStream patchStream = null;
            Stopwatch sw = new Stopwatch();

            logger.V("Woot" + origin);
            sw.Start();

            logger.V("diffing files " + sw.ElapsedMilliseconds);

            string unzipFolderName = this.mTempFileProvider.GetNewTempFolder();

            using (ZipFile originZipFile = new ZipFile(origin))
            using (ZipFile patchZipFile = new ZipFile(patch))
            {
                List<string> originFiles = new List<string>();
                foreach (ZipEntry ze in originZipFile)
                {
                    if (ze.IsDirectory)
                        continue;
                    if (ze.FileName.EndsWith(".class"))
                    {
                        originFiles.Add(ze.FileName);
                    }
                }
                List<string> patchFiles = new List<string>();
                foreach (ZipEntry ze in patchZipFile)
                {
                    if (ze.IsDirectory)
                        continue;
                    if (ze.FileName.EndsWith(".class"))
                    {
                        patchFiles.Add(ze.FileName);
                    }
                }

                logger.V("Creating temporary folder " + sw.ElapsedMilliseconds);

                patchZipFile.ExtractAll(unzipFolderName, ExtractExistingFileAction.OverwriteSilently);
                patchZipFile.Dispose();


                logger.V("Adding files " + sw.ElapsedMilliseconds);
                File.Copy(origin, dst, true);
                IEnumerable<string> differenceQuery = patchFiles.Except(originFiles);
                using (ZipFile dstZipFile = new ZipFile(dst))
                {
                    var differenceQuery_new = differenceQuery.Select(x => x.Insert(0, "/").Insert(0, unzipFolderName));

                    for (int i = 0; i < differenceQuery_new.Count(); i++)
                    {
                        dstZipFile.AddFile(differenceQuery_new.ElementAt(i), Path.GetDirectoryName(differenceQuery.ElementAt(i)));
                    }

                    dstZipFile.Save(dst);

                }
                logger.V("Adding files completed " + sw.ElapsedMilliseconds);
            }

        }

        /// <summary>
        /// Applies an aspectj path onto an existing library
        /// </summary>
        /// <param name="inpath">The library to patch</param>
        /// <param name="aspectpath">The AspectJ patch</param>
        /// <param name="outjar">The path to the output - patched library</param>
        public void applyPatch(string inpath, string aspectpath, string outjar)
        {
            string command = getJavaCommand();

            string ANDROID_JAR;
            string GOOGLE_PLAY_SERVICES_JAR;

            //Getting android platform jar for minSDK setted in PlayerSettings
            string platform = string.Format("{0}-{1}", "android", Regex.Match(buildParams.sdkVersion, @"\d+").Value);
            ANDROID_JAR = Path.Combine(ANDROID_HOME, string.Format("platforms/{0}/android.jar", platform));

            //if android platform jar getted from PlayerSettings doesn't exist will use default
            if (!File.Exists(ANDROID_JAR))
            {
                ANDROID_JAR = Path.Combine(ANDROID_HOME, string.Format("platforms/{0}/android.jar", DEFAULT_ANDROID_PLATFORM));
            }

            GOOGLE_PLAY_SERVICES_JAR = Path.Combine(ANDROID_HOME,
                                        "extras/google/google_play_services/libproject/google-play-services_lib/libs/google-play-services.jar");

            string delimeter = (PlatformUtils.isWindows()) ? ";" : ":";
            string classpath = Path.Combine(ASPECT_HOME_PATH, "aspectjtools.jar") + delimeter
                + Path.Combine(ASPECT_HOME_PATH, "aspectjrt.jar") + delimeter
                    + ANDROID_JAR + delimeter
                + GOOGLE_PLAY_SERVICES_JAR;

            string arguments = "-classpath " + classpath + " -Xmx8g org.aspectj.tools.ajc.Main -source 1.5 -Xlint:ignore -inpath "
                + PlatformUtils.qualifyPath(inpath) + " -aspectpath " + PlatformUtils.qualifyPath(aspectpath) + " -outjar " + PlatformUtils.qualifyPath(outjar);

            logger.V("Command " + command);
            logger.V("Argumnets " + arguments);

            int exitCode = runProcessWithCommand(command, arguments);
            string message = "aspects was applyed" + (exitCode == 0 ? "" : " not") + " successfully with code " + exitCode;
            if (exitCode == 0)
            {
                logger.V(message);
            }
            else
            {
                throw new Exception("failed to apply patch to the .jar file");
            }
        }


        /// <summary>
        /// Signs an APK
        /// </summary>
        /// <param name="unsignedAPKPath">The path of the apk to sign</param>
        public void signApk(string unsignedAPKPath)
        {
            logger.V("entering signApk");
            string keysotre_path = buildParams.keysotre_path;
            string alias = buildParams.alias;
            string storepass = buildParams.storepass;
            string keypass = buildParams.keypass;


            logger.V("Removing current APK signatures");
            // first remove the current sign
            using (ZipFile zipFile = new ZipFile(unsignedAPKPath))
            {

                for (int i = 0; i < zipFile.Count; ++i)
                {
                    ZipEntry e = zipFile[i];
                    if (e.FileName.StartsWith("META-INF/"))
                        zipFile.RemoveEntry(e.FileName);
                }

                zipFile.Save();
            }


            string command;
            if (PlatformUtils.isWindows())
            {
                command = System.Environment.GetEnvironmentVariable("JAVA_HOME") + @"/bin/jarSigner.exe";
            }
            else
            {
                command = "/usr/bin/jarsigner";
            }

            string arguments = "-keystore " + PlatformUtils.qualifyPath(keysotre_path) + " -storepass " + storepass + " -keypass " + keypass + " " + PlatformUtils.qualifyPath(unsignedAPKPath) + " " + alias;
            logger.V("command: {0}", command);
            logger.V("arguments: {0}", arguments);

            logger.V("Runing sign process");
            int exitCode = runProcessWithCommand(command, arguments);
            string message = "apk was " + (exitCode == 0 ? "" : "not") + " signed successfully" + (exitCode == 0 ? "" : " with code " + exitCode);
            logger.V(message);

            if (exitCode != 0)
            {
                throw new Exception("failed to sign apk");
            }


            logger.V("Leaving signApk");


        }

        /// <summary>
        /// Runs zipalign on an APK
        /// </summary>
        /// <param name="src">The APK to align</param>
        /// <param name="dst">The destination path of the aligned zip file</param>
        public void applyZipalign(string src, string dst)
        {

            logger.V("entering ZipAlign");
            string command = Directory.GetCurrentDirectory() + ZIPALIGN_TOOL_PATH;
            if (PlatformUtils.isWindows())
            {
                command += ".exe";
            }

            string arguments = " 4 " + PlatformUtils.qualifyPath(src) + " " + PlatformUtils.qualifyPath(dst);
            logger.V("command: {0}", command);
            logger.V("arguments: {0}", arguments);

            int exitCode = runProcessWithCommand(command, arguments);
            string message = "apk was " + (exitCode == 0 ? "" : "not") + " zipaligned successfully" + (exitCode == 0 ? "" : " with code " + exitCode);
            logger.V(message);


            if (exitCode != 0)
            {
                throw new Exception("failed run zipalign");
            }

            logger.V("leaving ZipAlign");
        }


		/// <summary>
		/// Runs ApkTool on a given compiled APK or decompiled APK folder
		/// </summary>
		/// <param name="isCompile">true if this is a compilation action. false for decompile action</param>
		/// <param name="targetPath">The path of the APK or decompiled APK folder</param>
		/// <param name="outputPath">The path of the uncompiled APK folder or re-compiled APK</param>
		private void useApkTool(Boolean isCompile, string targetPath, string outputPath)
		{
			//UnityEngine.Debug.LogError (APKTOOL_FOLDER);
			string unsignedPath = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			unsignedPath = unsignedPath.Substring(0, unsignedPath.Length - 4) + ".apk";

			string arguments = "";
			string apkToolPath = APK_TOOL_PATH;

			if (isCompile) {
				arguments = "-jar " + PlatformUtils.qualifyPath(apkToolPath) + " b -f " + PlatformUtils.qualifyPath(outputPath) + " -o " + PlatformUtils.qualifyPath(unsignedPath);
			} else {
				arguments = "-jar " + PlatformUtils.qualifyPath(apkToolPath) + " d -s -f " + PlatformUtils.qualifyPath(targetPath) + " -o " + PlatformUtils.qualifyPath(outputPath);
			}

			string command = getJavaCommand ();

            logger.V("Command " + command);
            logger.V("Argumnets " + arguments);

			int exitCode = runProcessWithCommand (command, arguments);
			string message = (isCompile ? "compile" : "decompile") + " was " + (exitCode == 0 ? "" : "not") + " successfully";

            if (exitCode != 0)
            {
                throw new Exception("failed to run apk tool");
            }
		}


		private int runProcessWithCommand(string command, string args)
		{
            logger.V(string.Format("running {0} with arguments {1}", command, args));

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

            if (exitCode != 0)
            {
                string output = proc.StandardOutput.ReadToEnd();
                logger.W("process failed with error {0}", output);
            }
			proc.Close();

			return exitCode;
		}


		private static string getJavaCommand() {
            return sJavaCommand;
		}


		private static string getClasspathForDex2JarTool() {
            return sClasspath;
		}


        public void ApplyGameConfiguration(Configuration.GameConfiguration gameConfiguration, string filePathToApplyConfiguration)
        {
            var configDoc = new XmlDocument();
            configDoc.LoadXml(File.ReadAllText(filePathToApplyConfiguration));

            injectAdsConfig (gameConfiguration, configDoc);
			injectABTestingConfig (configDoc);

            var writerSettings = new XmlWriterSettings();
            writerSettings.Indent = true;
            using (var writer = XmlWriter.Create(filePathToApplyConfiguration))
            {
                configDoc.Save(writer);
            }
        }

        private void injectAdsConfig(Configuration.GameConfiguration gameConfiguration, XmlDocument configDoc)
        {
            gameConfiguration.EnumerateConfiguration((category, fieldInfo) =>
            {
                var settingName = new StringBuilder();
                settingName.Length = 0;
                settingName.Append("playscape_")
                    .Append(AndroidPostProcessor.CamelToSnake(category.GetType().Name));

                settingName.Append("_")
                    .Append(AndroidPostProcessor.CamelToSnake(fieldInfo.Name));
                object value = fieldInfo.GetValue(category);

                var xmlElement = configDoc.SelectSingleNode(string.Format("resources/string[@name='{0}']", settingName.ToString()));

                if (xmlElement == null)
                {
                    throw new ApplicationException(string.Format("Unable to find xml element <string name='{0}'>, please " +
                                                                 "verify playscape_config.xml or your ad provider fields naming conventions.", settingName));
                }

                xmlElement.InnerText = string.Format("{0}", value);
            });

            configDoc.SelectSingleNode("resources/string[@name='playscape_ads_api_key']").InnerText = Convert.ToString(ConfigurationInEditor.Instance.MyAds.MyAdsConfig.ApiKey);
            configDoc.SelectSingleNode("resources/string[@name='playscape_ads_config_enable_ads_system']").InnerText = Convert.ToString(ConfigurationInEditor.Instance.MyAds.MyAdsConfig.EnableAdsSystem).ToLower();
            configDoc.SelectSingleNode("resources/string[@name='playscape_reporter_id']").InnerText = Convert.ToString(ConfigurationInEditor.Instance.ReporterId);
			configDoc.SelectSingleNode("resources/string[@name='playscape_is_published_by_playscape']").InnerText =  Convert.ToString(ConfigurationInEditor.Instance.MyGameConfiguration.PublishedByPlayscape);
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
    }

    /// <summary>
    /// A class representing a build process parameters
    /// </summary>
    public class BuildParams
    {
        public bool isDebug { get; set; }
        public string sdkVersion { get; set; }
        public string keysotre_path { get; set; }
        public string alias { get; set; }
        public string storepass { get; set; }
        public string keypass { get; set; }

    }
}
#endif
