using UnityEngine;
using System.Collections;
using System;
using System.IO;

namespace Playscape.Internal {

	[Serializable]
	public class Configuration : ScriptableObject {
		public const string CONFIGURATION_ASSET_NAME = "PlayscapeConfiguration";
		public const string CONFIGURATION_PATH = "Assets/Plugins/Playscape/Resources/" + CONFIGURATION_ASSET_NAME + ".asset";  

		/* --- Reporting --- */
		public String ReporterId;

		/* --- Push Woosh --- */
		public string PushWooshAndroidId;
		public string PushWooshIosId;

		public bool MergeAndroidManifests = true;

		/* --- In App Billing --- */
		[Serializable]
		public class InAppItem {
			public string Name;
			public bool Consumable = true;
			public string GooglePlayId;
			public string AppleStoreId;
			public string AmazonStoreId;
		}

		// Disabled for now: not supported at this stage
		//public InAppItem [] InAppBilling = new InAppItem[4];
	
		public static Configuration sConfiguration = null;

		public static Configuration Instance {
			get {
				if (sConfiguration == null) {
					// Try to load existing
					sConfiguration = (Configuration)Resources.Load(CONFIGURATION_ASSET_NAME, typeof(Configuration));
				}

				if (Application.isPlaying && sConfiguration == null) {
					L.E(CONFIGURATION_PATH + " has failed to load, did you configure Playscape Publishing Kit?");
				}

				return sConfiguration;
			}

			#if UNITY_EDITOR
			// Only editor is allowed
			set {
				sConfiguration = value;
			}
			#endif
		}

	}
}