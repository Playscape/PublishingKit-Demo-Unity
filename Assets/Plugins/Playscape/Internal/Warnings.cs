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

		public static readonly string CHARTBOOST_NOT_INITIALIZED = 
			MakeWarning("Chartboost was not initialized, did you integrate Chartboost?", "WARN-C001");

		public static readonly string PLAYSCAPE_MANAGER_NOT_IN_FIRST_SCENE = MakeWarning(
			"PlayscapeManager prefab must be added to the first scene! Drag and Drop Plugins/Playscape/Resources/PlayscapeManager into the Hierarchy window.",
			"WARN-C002");

		#endregion

		private static string MakeWarning (string warningText, string warningCode)
		{
			return string.Format("[{0}] {1}", warningCode, warningText);
		}
	}
}