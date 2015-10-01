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

		// Note - to add a new ad provider, simply follow the naming convention in the ads class
		//        make a subclass for your provider and fill it with ids.
		//        GUI and saving/loading of the config will be done automagically for you.
		public Ads MyAds = new Ads();

		private bool includeArchitecture = false;
		public bool IncludeArchitectures {
			get {
				return includeArchitecture;
			} set {
				includeArchitecture = value;
			}
		}

		[SerializeField]
		private GameConfiguration _gameConfiguration = new GameConfiguration();

		public GameConfiguration MyGameConfiguration {
			set {
				this._gameConfiguration = value;
				this.ReporterId = value.MyAnalyticsReport.ReporterId;
			}
			get {
				return this._gameConfiguration;
			}
		}

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

		/* -- Analytics Reporting -- */
		public string ReporterId;

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
			if (!string.IsNullOrEmpty (apikey)) {
				string url = string.Format ("{0}/config", CommonConsts.GAME_CONFIGURATION_API_URL);
				
				// Star synchronous request for getting game configuration
				GameConfigurationResponse gameConfiguration = null;
				SyncRequest<GameConfigurationResponse> request = new SyncRequest<GameConfigurationResponse> (url, System.Net.WebRequestMethods.Http.Get);
				request.addHeader ("X-API-Key", apikey);
				
				gameConfiguration = request.Start ();
				
				return gameConfiguration;
			}

			return null;
		}

		/**
		 * Entity class which represents configuration of game
		 *
		 **/
		[Serializable]
		public class GameConfiguration
		{
			[PlayscapeJsonName("ads_config")]
			public AdsConfig MyAdsConfig = new AdsConfig();

			[PlayscapeJsonName("admob")]
			public Admob MyAdMobIds = new Admob();

			[PlayscapeJsonName("chartboost")]
			public Chartboost MyChartboostIds = new Chartboost ();

			[PlayscapeJsonName("vungle")]
			public Vungle MyVungle = new Vungle ();

			[PlayscapeJsonName("millennialmedia")]
			public MillennialMedia MyMillennialMedia = new MillennialMedia();

			[PlayscapeJsonName("startapp")]
			public Startapp MyStartAppIds = new Startapp ();

			[PlayscapeJsonName("adcolony")]
			public Adcolony MyAdColonyIds = new Adcolony();

			[PlayscapeJsonName("analytics_report", false)]
			public AnalyticsReport MyAnalyticsReport = new AnalyticsReport ();

			[PlayscapeJsonName("published_by_playscape", false)]
			public bool PublishedByPlayscape;

			/**
			 * Traverses the game configuration via given visitor
			 *
			 **/
			public void EnumerateConfiguration(Action<object, FieldInfo> visitor) {
				foreach (var categoryFieldInfo in typeof(GameConfiguration).GetFields()) {
					if (categoryFieldInfo.IsPublic) {

						//Getting property's attributes list
						object[] attrs = categoryFieldInfo.GetCustomAttributes(typeof(PlayscapeJsonName), false);

						foreach (var attr in attrs) {
							PlayscapeJsonName playscapeAttr = attr as PlayscapeJsonName;

							if (playscapeAttr != null) {
								//check if traverse is allowed for property
								if (playscapeAttr.Traversable) {
									var category = categoryFieldInfo.GetValue(this);

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
			}

			[Serializable]
			public class AdsConfig {
				[PlayscapeJsonName("url")]
				public string Url;
			}

			[Serializable]
			public class Vungle {
				[PlayscapeJsonName("app_id")]
				public string AppId;
			}

			[Serializable]
			public class MillennialMedia {
				[PlayscapeJsonName("app_id")]
				public string AppId;
			}

			[Serializable]
			public class Chartboost
			{
				[PlayscapeJsonName("app_id")]
				public string AppId;

				[PlayscapeJsonName("app_signature")]
				public string AppSignature;
			}

			[Serializable]
			public class Admob
			{
				[PlayscapeJsonName("interstitials_ad_unit_id")]
				public string InterstitialsAdUnitId;

				[PlayscapeJsonName("banners_ad_unit_id")]
				public string BannersAdUnitId;

				[PlayscapeJsonName("test_device_id")]
				public string TestDeviceId;
			}

			[Serializable]
			public class Startapp
			{
				[PlayscapeJsonName("developer_id")]
				public string DeveloperId;

				[PlayscapeJsonName("app_id")]
				public string AppId;
			}

			[Serializable]
			public class Adcolony
			{
				[PlayscapeJsonName("app_id")]
				public string AppId;

				[PlayscapeJsonName("video_zone_id")]
				public string VideoZoneId;

				[PlayscapeJsonName("incentivised_video_zone_id")]
				public String IncentivisedVideoZoneId;
			}

			[Serializable]
			public class AnalyticsReport {

				[PlayscapeJsonName("reporter_id")]
				public string ReporterId;
			}
		}

		/**
		 * Entity class which represents GAME API response
		 *
		 **/
		public class GameConfigurationResponse {

			[PlayscapeJsonName("success")]
			public bool Success;

			[PlayscapeJsonName("message")]
			public string ErrorDescription;

			[PlayscapeJsonName("config")]
			public GameConfiguration GameConfiguration { get; private set; }

			public GameConfigurationResponse() {
				this.Success = false;
				this.GameConfiguration = new GameConfiguration ();
			}
		}
	}
}
