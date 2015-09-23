using System;

namespace Playscape.Internal {
	/// <summary>
	/// Contains warning constants that are reported to the user, 
	/// each warning must have a warning code for easier lookup for how to handle it in the documentation.
	/// 
	/// Warning codes are formatted as follows:
	/// 
	/// WARN-[deviceCode][Number]
	/// 
	/// e.g:
	/// WARN-A001
	/// for Android warning 001
	/// 
	/// WARN-I042
	/// for iOS warning 42
	/// 
	/// WARN-C001
	/// for common warnig 001
	/// 
	/// </summary>
	static class Warnings  {

		#region Android Warnings

		public static readonly string REPORTED_ID_NOT_SET = MakeWarning(
			"Reporter Id not set.",
   			"WARN-G001");

		public static readonly string ADS_API_KEY_NOT_SET = MakeWarning(
			"AdsConfig ApiKey not set.",
			"WARN-G002");

		public static readonly string RELEASE_BUILD_SHOULD_HAS_INTERNET_CONNECTION = MakeWarning(
			"Release build should has internet connection.",
			"WARN-G003");

		public static readonly string JAVA_HOME_ENV_VAR_NOT_SET = MakeWarning (
			"JAVA_HOME environment variable is missed.",
			"WARN-G004");

		public static readonly string JAVA_HOME_ENV_VAR_NOT_EXIST = MakeWarning (
			"JAVA_HOME path does not exist.",
			"WARN-G005");

		public static readonly string JAVA_NOT_SATISFYING_PLAYSCAPE_SDK = MakeWarning (
			"Please make sure you have a suitable JDK installation. Playscape SDK requires at least JDK 7 (1.7) 64-Bit, having JRE only is not enough.  The latest JDK can be obtained from the Oracle website http://www.oracle.com/technetwork/java/javase/downloads/index.html",
			"WARN-G006");

		public static readonly string ANDROID_HOME_ENV_VAR_NOT_SET = MakeWarning (
			"ANDROID_HOME environment variable is missed.",
			"WARN-G007");

		public static readonly string ANDROID_HOME_ENV_VAR_NOT_EXIST = MakeWarning (
			"ANDROID_HOME path does not exist.",
			"WARN-G008");

		public static readonly string ANDROID_NOT_HAS_GOOGLE_PLAY_SERVICES = MakeWarning (
			"Looks like you don't have \"Google Play Services\" library in your Android SDK folder. Please download it.",
			"WARN-G009");


		#endregion

		#region Android Warnings

		public static readonly string ANDROID_NAME_EXISTS_IN_MANIFEST = MakeWarning(
			"application:name already exists in your manifest, " +
			"Playscape Publishing Kit will not function properly.",
			"WARN-A001");

		public static readonly string PLAYSCAPE_ANDROID_APK_BUILD = MakeWarning(
			"Playscape Publishing Kit will not work when compiling to .apk, " +
			"please build Google Android Project",
			"WARN-A002"); 

		public static readonly string PUSH_WOOSH_APP_ID_NOT_SET_ANDROID = MakeWarning(
			"Pushwood Android Id is not set",
			"WARN-A003");

		public static readonly string CHARTBOOT_ID_NOT_SET_ANDROID = MakeWarning(
			"Chartboost Android Id is not set",
			"WARN-A004");

		public static readonly string MAIN_ACTIVITY_REPLACED = 
			MakeWarning("Your main activity, has been replaced by PlayscapeActivity", 
			            "WARN-A005");

		#endregion



		#region iOS Warnings
		public static readonly string PUSH_WOOSH_APP_ID_NOT_SET_IOS = MakeWarning(
			"Pushwood iOS Id is not set",
			"WARN-I001");
		
		public static readonly string CHARTBOOT_ID_NOT_SET_IOS = MakeWarning(
			"Chartboost iOS Id is not set",
			"WARN-I002");

		#endregion

		#region Common Warnings

		public static readonly string PLAYSCAPE_MANAGER_NOT_IN_FIRST_SCENE = MakeWarning(
			"PlayscapeManager prefab must be added ONLY to the first scene! Remove it from any other scenes, but the first and Drag and Drop Plugins/Playscape/Resources/PlayscapeManager into the Hierarchy window in the first scene.",
			"WARN-C002");

		
		public static readonly string ATLEAST_ONE_AD_ID_MUST_BE_FILLED = MakeWarning("Atleast one ad identifier must be filled.", 
		                                                                          "WARN-C003");

		public static readonly string AD_CONFIG_URL_MUST_BE_FILLED = MakeWarning ("Ads Config Url must be filled.", "WARN-C004");

		#endregion

		private static string MakeWarning (string warningText, string warningCode)
		{
			return string.Format("[{0}] {1}", warningCode, warningText);
		}
	}
}