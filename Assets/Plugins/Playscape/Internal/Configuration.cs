using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;
using System.Reflection;

namespace Playscape.Internal {

	[Serializable]
	public class Configuration : ScriptableObject {
		public const string CONFIGURATION_ASSET_NAME = "PlayscapeConfiguration";
		public const string CONFIGURATION_PATH = "Assets/Plugins/Playscape/Resources/" + CONFIGURATION_ASSET_NAME + ".asset";  

		/* --- Reporting --- */
		public String ReporterId;
		
		// Note - to add a new ad provider, simply follow the naming convention in the ads class
		//        make a subclass for your provider and fill it with ids.
		//        GUI and saving/loading of the config will be done automagically for you.
		public Ads MyAds = new Ads();

		[Serializable]
		public class Ads
		{
			public AdsConfig MyAdsConfig = new AdsConfig();
			public Admob MyAdMobIds = new Admob();
			public Adcolony MyAdColonyIds = new Adcolony();
			public Chartboost MyChartboostIds = new Chartboost();
			public MillennialMedia MyMillennialMedia = new MillennialMedia();
			public Mopub MyMoPubIds = new Mopub();
			public Startapp MyStartAppIds = new Startapp();
			public Vungle MyVungle = new Vungle();


			[Serializable]
			public class AdsConfig {
				public string Url;
			}

			[Serializable]
			public class Vungle {
				public string AppId;
			}

			[Serializable]
			public class MillennialMedia {
				public string AppId;
			}

			[Serializable]
			public class Mopub
			{
				public string InterstitialId;
				public string BannerId;
			}
			
			[Serializable]
			public class Chartboost
			{
				public string AppId;
				public string AppSignature;
			}
			
			[Serializable]
			public class Admob
			{
				public string InterstitialsAdUnitId;
				public string BannersAdUnitId;
				public string TestDeviceId;
			}
			
			[Serializable]
			public class Startapp
			{
				public string DeveloperId;
				public string AppId;
			}
			
			[Serializable]
			public class Adcolony 
			{
				public string AppId;
				public string VideoZoneId;
				public String IncentivisedVideoZoneId;
			}
		}

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

		/// <summary>
		/// Traverses the ads configuration via given visitor.
		/// </summary>
		/// <param name="visitor">Visitor which arguments are: (category: object, field: fieldInfo).
		/// Category is the instance of the category in the ads class.
		/// field is the field info of that category, you can use it to read and/or set that field.
		/// </param>
		public void TraverseAdsConfig(Action<object, FieldInfo> visitor) {

			foreach (var categoryFieldInfo in typeof(Configuration.Ads).GetFields()) {
				if (categoryFieldInfo.IsPublic) {

					var category = categoryFieldInfo.GetValue(Configuration.Instance.MyAds);
					
					foreach (var settingFieldInfo in category.GetType().GetFields()) {
						if (settingFieldInfo.IsPublic) {

							visitor(category, settingFieldInfo);
						}
					}

				}
			}
		}
	}
}