#if UNITY_EDITOR
using System;

namespace Playscape.Editor
{
	/// <summary>
	/// Utility class which provides user's path to Android SDK folder
	/// </summary>
	public static class AndroidSDKFolder
	{

		private const string AndroidSdkKey 				= "AndroidSdkRoot";
		private const string AndroidPathVariableName 	= "ANDROID_HOME";
		private static string sAndroidSdk 				= "";


		/// <summary>
		/// Return user's path to Android SDK foldler
		/// </summary>
		/// <value>The path.</value>
		public static string Path {
			get { 
				if (string.IsNullOrEmpty(sAndroidSdk)) {
					sAndroidSdk = Environment.GetEnvironmentVariable(AndroidPathVariableName);

					if (string.IsNullOrEmpty(sAndroidSdk)) {
						sAndroidSdk = UnityEditor.EditorPrefs.GetString("AndroidSdkRoot");
					}
				}
				return sAndroidSdk;
			}
		}


		/// <summary>
		/// Returns user's path to google play service jar
		/// </summary>
		/// <value>The google play services jar path.</value>
		public static string GooglePlayServicesJarPath {
			get {
				return System.IO.Path.Combine(AndroidSDKFolder.Path, "extras/google/google_play_services/libproject/google-play-services_lib/libs/google-play-services.jar");
			}
		}


		/// <summary>
		/// Returns path to requested android API.
		/// </summary>
		/// <param name="command">Command.</param>
		/// <param name="args">Arguments.</param>
		public static string GetAndroidAPIJarPath(uint androidAPIVersion) {
			return System.IO.Path.Combine(AndroidSDKFolder.Path, string.Format("platforms/android-{0}/android.jar", androidAPIVersion));
		}
	}
}
#endif