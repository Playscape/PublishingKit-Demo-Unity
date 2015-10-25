#if UNITY_EDITOR
using System;

namespace Playscape.Editor
{
	/**
	 * Utility class which provides user's path to Android SDK folder
	 **/
	public static class AndroidSDKFolder
	{

		/**
		 * Method return user's path to Android SDK foldler
		 * 
		 **/
		public static string Path {
			get { return UnityEditor.EditorPrefs.GetString("AndroidSdkRoot"); }
		}
	}
}
#endif