#if UNITY_EDITOR
using System;
using UnityEditor;

namespace Playscape.Editor
{
	/// <summary>
	/// Utility class which provides user's path to JDK folder
	/// </summary>
	public static class JDKFolder
	{
		private const string JDKPathKey 			= "JdkPath";
		private const string JDKPathVariableName 	= "JAVA_HOME";
		private const string JDKVersionKey 			= "java version";
		private const string JDKBitKey 				= "64-Bit Server VM";

		private const uint JDKRequiredVersion = 17000;

		private static string sJDKPath = "";


		/// <summary>
		/// Return user's path to JDK Folder
		/// </summary>
		/// <value>The path.</value>
		public static string Path {
			get {
				if (string.IsNullOrEmpty(sJDKPath)) {
					sJDKPath = System.Environment.GetEnvironmentVariable(JDKPathVariableName);

					if (string.IsNullOrEmpty(sJDKPath)) {
						sJDKPath = UnityEditor.EditorPrefs.GetString(JDKPathKey);
					}
				}

				return sJDKPath;
			}
		}


		/// <summary>
		/// Returns path to jarsigner tool
		/// </summary>
		/// <value>The jar signer path.</value>
		public static string JarSignerPath {
			get {
				string suffix = "bin/jarSigner.exe";

				if (PlatformUtils.isMac() || PlatformUtils.isUnix()) {
					suffix = "bin/jarsigner";
				}

				return System.IO.Path.Combine(JDKFolder.Path, suffix);
			}
		}


		/// <summary>
		/// Returns path to java tool
		/// </summary>
		/// <value>The java path.</value>
		public static string JavaPath {
			get {
				string suffix = "bin/java.exe";

				if (PlatformUtils.isMac() || PlatformUtils.isUnix()) {
					suffix = "bin/java";
				}

				return System.IO.Path.Combine(JDKFolder.Path, suffix);
			}
		}


		/// <summary>
		/// Returns flag which indicates is current JDK required to Playscape
		/// </summary>
		/// <value><c>true</c> if is satisfying JDK version; otherwise, <c>false</c>.</value>
		public static bool IsSatisfyingJDKVersion {
			get {
				uint javaVersion = 0;
				bool java64BitVersion = false;

				if (!string.IsNullOrEmpty(JDKFolder.Path)) {
					Playscape.Internal.CommandLineExecutor executor = new Playscape.Internal.CommandLineExecutor (new UnityDebugLogger());
					Playscape.Internal.Output output = executor.Execute (JDKFolder.JavaPath, "-version");
					
					if (output.ExitCode != 0) {
						throw new Exception(string.Format("Error occured while trying to check JDK version. Error: {0}", output.ErrorDescription));
					} else {
						foreach (var line in output.ErrorDescription.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)) {
							if (line.Contains(JDKVersionKey)) {
								int versionIndex = line.IndexOf(JDKVersionKey) + JDKVersionKey.Length;
								
								string version = line.Substring(versionIndex).Replace("\"", "").Replace(".", "").Replace("_", "");
								javaVersion = UInt16.Parse(version);
							}
							
							if (line.Contains(JDKBitKey)) {
								java64BitVersion = true;
							}
						}
					}
				}

				return (javaVersion >= JDKRequiredVersion && java64BitVersion);
			}
		}
	}
}
#endif