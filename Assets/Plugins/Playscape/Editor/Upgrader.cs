using UnityEngine;
using System.Collections;
using System.IO;

namespace Playscape.Editor {
	public  static class Upgrader {
		/// <summary>
		/// Runs upgrade mechanism if needed.
		/// 
		/// How it works - 
		/// 
		/// 1. Read contents of /Assets/Plugins/Playscape/Version/my_version
		///    The contents are the current version number.
		/// 2. Let's assume we've read "1.3.0". We look for a file named 1.3.0, if its contents are empty proceed with upgrade.
		/// 3. If-else statements which check on previous version files and perform upgrade actions are required.
		/// 
		/// </summary>
		public static void Upgrade() {
			string myVersion = null;
			string upgradeStatus = null;
			try {
				myVersion = File.ReadAllText (CommonConsts.PLUGINS_PLAYSCAPE_VERSION_PATH + "/my_version");
				upgradeStatus = File.ReadAllText (CommonConsts.PLUGINS_PLAYSCAPE_VERSION_PATH + "/" + myVersion);
			} catch {
			}

            if (string.IsNullOrEmpty (upgradeStatus)) {
                if (File.Exists(CommonConsts.PLUGINS_PLAYSCAPE_VERSION_PATH + "/1.3.2")) {
                    //Do nothing
                    DeleteFileForUpgrade(CommonConsts.PUBLISHING_PATH_ANDROID_LIB_PATH + "/libs/com.adience.adience-1.4.1-RELEASE.jar");
                    DeleteFileForUpgrade(CommonConsts.PUBLISHING_PATH_ANDROID_LIB_PATH + "/libs/com.mopub.mobileads.mopub-sdk-1.0.3-RELEASE.jar");
                    File.Delete(CommonConsts.PLUGINS_PLAYSCAPE_VERSION_PATH + "/1.3.2");
                }
                else if (File.Exists(CommonConsts.PLUGINS_PLAYSCAPE_VERSION_PATH + "/1.3.1")) {
                    //Do nothing
                    File.Delete(CommonConsts.PLUGINS_PLAYSCAPE_VERSION_PATH + "/1.3.1");
                }
                else if (File.Exists(CommonConsts.PLUGINS_PLAYSCAPE_VERSION_PATH + "/1.3.0")) {
                    DeleteFileForUpgrade(CommonConsts.PUBLISHING_PATH_ANDROID_LIB_PATH + "/libs/adience.jar");
                    DeleteFileForUpgrade(CommonConsts.PUBLISHING_PATH_ANDROID_LIB_PATH + "/libs/armeabi-v7a/libocs.so");
                    File.Delete(CommonConsts.PLUGINS_PLAYSCAPE_VERSION_PATH + "/1.3.0");
                } else { // Pre 1.3.0
                    DeleteFileForUpgrade(CommonConsts.PUBLISHING_PATH_ANDROID_LIB_PATH + "/libs/com.adience.adience-1.4.1-RELEASE.jar");
                    DeleteFileForUpgrade(CommonConsts.PUBLISHING_PATH_ANDROID_LIB_PATH + "/libs/com.mopub.mobileads.mopub-sdk-1.0.3-RELEASE.jar");
                }
				
				// delete the PlayScapeActivity source file if exist (it is not compiled into a .jar file
				string playScapeActivitySourcePath = "Assets/Plugins/Android/PlayscapePublishingKit/src/com/playscape/publishingkit/PlayscapeActivity.java";
				DeleteFileForUpgrade(playScapeActivitySourcePath);
				DeleteFileForUpgrade(playScapeActivitySourcePath + ".meta");
            }
            
            File.WriteAllText(CommonConsts.PLUGINS_PLAYSCAPE_VERSION_PATH + "/" + myVersion, "upgraded");
		}

		private static void DeleteFileForUpgrade(string filePath) {
			if (File.Exists (filePath)) {
				UpgradeLog("Deleting file: {0}", filePath);
				File.Delete(filePath);
			}
		}

		private static void UpgradeLog(string format, params object[] args) {
			Debug.Log("PlayScape Pubkit Upgrade: " + string.Format(format, args));
		}

	}
}