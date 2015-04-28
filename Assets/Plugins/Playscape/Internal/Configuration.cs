using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;
using System.Reflection;
using Playscape.Editor;
using Pathfinding.Serialization.JsonFx;

namespace Playscape.Internal {

	[Serializable]
	public class Configuration : ScriptableObject {
		public const string CONFIGURATION_ASSET_NAME = "PlayscapeConfiguration";
		public const string CONFIGURATION_PATH = "Assets/Plugins/Playscape/Resources/" + CONFIGURATION_ASSET_NAME + ".asset";  
		public static int MAX_NUMBER_OF_EXPERIMENTS = 20;
		public static int MAX_NUMBER_OF_VARS_INEXPERIMENT = 50;

		/* ----- AB TESTING ------------------- */
		public ABTesting MyABTesting = new ABTesting();

		[Serializable]
		public class ABTesting
		{
			public bool EnableABTestingSystem;
			public int NumberOfCustomExperiments;
			public String AmazonPublicKey;
			public String AmazonPrivateKey;
			public CustomExperimentConfig[] MyCustomExperimentConfig;

			public ABTesting()
			{
				EnableABTestingSystem =false;
				NumberOfCustomExperiments = 0;
				MyCustomExperimentConfig = new CustomExperimentConfig[MAX_NUMBER_OF_EXPERIMENTS];
				for (int i = 0; i < MAX_NUMBER_OF_EXPERIMENTS; i++)
				{
					MyCustomExperimentConfig[i] = new CustomExperimentConfig();
				}
			}

			[Serializable]
			public class  CustomExperimentConfig {
				public String ExperimentName;
				public String ExperimentType = "com.playscape.abtesting.UnityABTestingCustomSubscriber";
				public String[] ExperimentVars = new String[MAX_NUMBER_OF_VARS_INEXPERIMENT];
				public int NumberOfVarsInExperiment = 1;
			}
		}

		/* --- Reporting --- */
		public String ReporterId;
		
		// Note - to add a new ad provider, simply follow the naming convention in the ads class
		//        make a subclass for your provider and fill it with ids.
		//        GUI and saving/loading of the config will be done automagically for you.
		public Ads MyAds = new Ads();

		public GameConfiguration MyGameConfiguration = new GameConfiguration();

		[Serializable]
		public class Ads
		{
			public AdsConfig MyAdsConfig = new AdsConfig();
			           
			[Serializable]
			public class AdsConfig {
				public string ApiKey;
                public bool EnableAdsSystem = true;
			}
		}

		/* --- Push Woosh --- */
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
		public void TraverseAdsUIConfig(Action<object, FieldInfo> visitor) {

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

		/**
		 * Performing request to GAME API for fetching Game configuration for passed 'apiKey'
		 * 
		 * apiKey - API_KEY for which game configuration should be fetched
		 * 
		 **/
		public GameConfigurationResponse FetchGameConfigurationForApiKey(string apikey) {			
			AsyncRequest<GameConfigurationResponse> request = new AsyncRequest<GameConfigurationResponse> (CommonConsts.GAME_CONFIGURATION_API_URL, System.Net.WebRequestMethods.Http.Get);
			request.addHeader ("X-API-Key", apikey);

			GameConfigurationResponse gameConfiguration = request.Start ();
			
			return gameConfiguration;
		}

		/**
		 * Entity class which represents configuration of game
		 * 
		 **/

		[Serializable]
		public class GameConfiguration
		{
			[JsonName("ads_config")]
			public AdsConfig MyAdsConfig = new AdsConfig();
			
			[JsonName("admob")]
			public Admob MyAdMobIds = new Admob();
			
			[JsonName("chartboost")]
			public Chartboost MyChartboostIds = new Chartboost ();
			
			[JsonName("vungle")]
			public Vungle MyVungle = new Vungle ();
			
			[JsonName("millennialmedia")]
			public MillennialMedia MyMillennialMedia = new MillennialMedia();
			
			[JsonName("startapp")]
			public Startapp MyStartAppIds = new Startapp ();
			
			[JsonName("adcolony")]
			public Adcolony MyAdColonyIds = new Adcolony();


			/**
			 * Traverses the game configuration via given visitor 
			 * 
			 **/
			public void EnumerateConfiguration(Action<object, FieldInfo> visitor) {
				foreach (var categoryFieldInfo in typeof(GameConfiguration).GetFields()) {
					if (categoryFieldInfo.IsPublic) {
						var category = categoryFieldInfo.GetValue(this);
						
						foreach (var settingFieldInfo in category.GetType().GetFields()) {
							if (settingFieldInfo.IsPublic) {							
								visitor(category, settingFieldInfo);
							}
						}
					}
				}
			}
			
			[Serializable]
			public class AdsConfig {
				[JsonName("url")]
				public string Url;
			}
			
			[Serializable]
			public class Vungle {
				[JsonName("app_id")]
				public string AppId;
			}
			
			[Serializable]
			public class MillennialMedia {
				[JsonName("app_id")]
				public string AppId;
			}
			
			[Serializable]
			public class Chartboost
			{
				[JsonName("app_id")]
				public string AppId;
				
				[JsonName("app_signature")]
				public string AppSignature;
			}
			
			[Serializable]	
			public class Admob
			{
				[JsonName("interstitials_ad_unit_id")]
				public string InterstitialsAdUnitId;
				
				[JsonName("banners_ad_unit_id")]
				public string BannersAdUnitId;
				
				[JsonName("test_device_id")]
				public string TestDeviceId;
			}
			
			[Serializable]
			public class Startapp
			{
				[JsonName("developer_id")]
				public string DeveloperId;
				
				[JsonName("app_id")]
				public string AppId;
			}
			
			[Serializable]
			public class Adcolony 
			{
				[JsonName("app_id")]
				public string AppId;
				
				[JsonName("video_zone_id")]
				public string VideoZoneId;
				
				[JsonName("incentivised_video_zone_id")]
				public String IncentivisedVideoZoneId;
			}
		}

		/**
		 * Entity class which represents GAME API response
		 * 
		 **/
		public class GameConfigurationResponse : AsyncResponse {

			[JsonName("success")]
			public bool Success;

			[JsonName("message")]
			public string ErrorDescription;
			
			[JsonName("config")]
			public GameConfiguration GameConfiguration { get; private set; }
			
			public GameConfigurationResponse() {
				this.Success = false;
				this.GameConfiguration = new GameConfiguration ();
			}
		}	
	}	
}