using System;
namespace Playscape.Editor
{
	public static class CommonConsts {
		public const string PLUGINS_PATH = "Assets/Plugins";
		public const string PLUGINS_PLAYSCAPE_PATH = PLUGINS_PATH + "/Playscape";
		public const string PLUGINS_PLAYSCAPE_VERSION_PATH = PLUGINS_PLAYSCAPE_PATH + "/Version";
		public const string PLUGINS_PLAYSCAPE_PREFABS_PATH = PLUGINS_PLAYSCAPE_PATH + "/Prefabs";
		public const string PUBLISHING_PATH_ANDROID_LIB_PATH = PLUGINS_PATH + "/Android/PlayscapePublishingKit";
		public const string CONFIGURE_PUBLISHING_KIT_MENU_ITEM = "Playscape Publishing Kit Configuration...";
		public const string MANIFEST_FRAGMENTS_PATH = PLUGINS_PATH + "/Android/PlayscapeManifestFragments";
		public const string PLAYSCAPE_MANIFEST_PATH = MANIFEST_FRAGMENTS_PATH + "/PlayscapeAndroidManifest.xml";
		public const string GAME_CONFIGURATION_API_URL = "https://playscape-gcm-test.appspot.com";
	}
}