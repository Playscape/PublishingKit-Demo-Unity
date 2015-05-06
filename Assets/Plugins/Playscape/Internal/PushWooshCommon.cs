using UnityEngine;
using System;
using Playscape.Analytics;

/// <summary>
/// Handles messages incoming from within the platform (iOS/Android)
/// 
/// And exposes public methods for platform-independant tag setting.
/// </summary>
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Playscape.Internal {
	public abstract class PushWooshCommon : MonoBehaviour
	{
		private enum PWTagReportSource {
			PushWooshRegistrationService,
			GCMIntentService
		}

		private const string PUSHWOOSH_RANDOM_ID_TAG = "RandomID";
		private const string PUSHWOOSH_LAST_NOTIFICATION_RECEIVED_TAG = "LastNotificationReceived";
		private const string PUSHWOOSH_LAST_NOTIFICATION_ID_TAG = "LastNotificationId";
		private const string CUSTOM_DATA_JSON_KEY = "u";
		
		private const string PREF_PREFIX = "Playscape.PushWooshCommon";
		private const string PREF_RANDOM_ID = PREF_PREFIX + "." + PUSHWOOSH_RANDOM_ID_TAG;
		private const string PREF_LAST_NOTIFICATION_RECEIVED = PREF_PREFIX + "." + PUSHWOOSH_LAST_NOTIFICATION_RECEIVED_TAG;
		private const string PREF_LAST_NOTIFICATION_ID = PREF_PREFIX + "." + PUSHWOOSH_LAST_NOTIFICATION_ID_TAG;

		private const string TAG = "PushWooshCommon-Unity";


		void onRegisteredForPushNotifications(string token)
		{
			int randomId = PlayerPrefs.GetInt(PREF_RANDOM_ID, 0);
			int lastNotificationReceived = PlayerPrefs.GetInt(PREF_LAST_NOTIFICATION_RECEIVED, 0);
			int lastNotificationId = PlayerPrefs.GetInt(PREF_LAST_NOTIFICATION_ID, 0);

			if (randomId == 0) {
				randomId = generateRandomId();
				PlayerPrefs.SetInt(PREF_RANDOM_ID, randomId);
				
				PlayerPrefs.Save();
			}
			
			SetTag(PUSHWOOSH_RANDOM_ID_TAG, randomId);
			SetTag(PUSHWOOSH_LAST_NOTIFICATION_ID_TAG, lastNotificationId);
			SetTag(PUSHWOOSH_LAST_NOTIFICATION_RECEIVED_TAG, lastNotificationReceived);

			L.D ("GCM and PushWoosh registered successfully!");
		}
		
		void onFailedToRegisteredForPushNotifications(string error)
		{
			RemoteLogger.Log(RemoteLogger.LogLevel.Error, TAG, "***PushWoosh Registration error occurred - PushWoosh initialization failed");
			L.E("{0}", error);
		}
		
		void onPushNotificationsReceived(string payload)
		{
			string promotedGame = Configuration.Instance.ReporterId;
			string action = "NotSet";
			string notificationId = "0";
			int numericNotificationId = 0;
			int lastNotificationReceived = 0;

			if (payload != null) {
				var payloadParsed = parseJson(payload);
				if (payloadParsed != null) {
					string customDataJson = getValueFromJson(payloadParsed, CUSTOM_DATA_JSON_KEY);
					if (customDataJson != null) {
						var customData = parseJson(customDataJson);
						
						promotedGame = getValueFromJson(customData, "promotedPackageName") ?? promotedGame;
						action = getValueFromJson(customData, "action") ?? action;
						notificationId = getValueFromJson(customData, "notificationId") ?? notificationId;
						if (int.TryParse(notificationId, out numericNotificationId)) {
							lastNotificationReceived = (int)(Utils.CurrentTimeMillis / 1000 / 60 / 60);

							PlayerPrefs.SetInt(PREF_LAST_NOTIFICATION_ID, numericNotificationId);
							PlayerPrefs.SetInt(PREF_LAST_NOTIFICATION_RECEIVED, lastNotificationReceived);
							PlayerPrefs.Save();

							SetTag(PUSHWOOSH_LAST_NOTIFICATION_ID_TAG, numericNotificationId);
							SetTag(PUSHWOOSH_LAST_NOTIFICATION_RECEIVED_TAG, lastNotificationReceived);
						}
					}
				}
			}

			L.D("PushWoosh Payload: {0}", payload);
		}

		public void ReportAllTags() {
			#if REMOVE_FROM_LINKAGE
			#if !UNITY_EDITOR
			playscape_TagReporter_reportAllTags();
			#endif
			#endif
		}

		private void ReportSentTags(string tagName, string tagValue) {
			#if !UNITY_EDITOR
			#if !UNITY_EDITOR
			playscape_TagReporter_setTag(tagName, tagValue);
			#endif
			#endif

			var sb = new StringBuilder();
			sb.Append("Sent");

			bool isCustom = tagName != PUSHWOOSH_RANDOM_ID_TAG && 
							tagName != PUSHWOOSH_LAST_NOTIFICATION_ID_TAG && 
							tagName != PUSHWOOSH_LAST_NOTIFICATION_RECEIVED_TAG;

			PWTagReportSource source = PWTagReportSource.PushWooshRegistrationService;
			if (isCustom) {
				sb.Append(" CUSTOM");
				source = PWTagReportSource.GCMIntentService;
			}

			sb.Append(" tags to pushwoosh from ").Append(source.ToString());
			sb.Append(" : ").Append(tagName).Append(" = ").Append(tagValue);

			RemoteLogger.Log(RemoteLogger.LogLevel.Info, TAG, sb.ToString());
		}
		
		private SimpleJson.JsonObject parseJson(string json) {
			
			SimpleJson.JsonObject jsonObj = null;
			object o;
			SimpleJson.SimpleJson.TryDeserializeObject(json, out o);
			if (o is SimpleJson.JsonObject) {
				jsonObj = (SimpleJson.JsonObject)o;
			}
			
			return jsonObj;
		}
		
		private string getValueFromJson(SimpleJson.JsonObject jsonObj, string key) {
			object value = null;
			if (jsonObj.TryGetValue(key, out value)) {
				return value.ToString();
			}
			
			return null;
		}
		
		private int generateRandomId() {
			string guid = Guid.NewGuid().ToString();
			return ((Math.Abs(guid.GetHashCode()))%100) + 1;
		}
		

		public void SetTag(string name, string value)
		{
			L.D("PushwooshTag: {0} = {1}", name, value);

			SetTagImpl(name, value);

			ReportSentTags(name, value);
		}

		public void SetTag(string name, int value)
		{
			L.D("PushwooshTag: {0} = {1}", name, value);

			SetTagImpl(name, value);

			ReportSentTags(name, value.ToString());
		}

		protected abstract void SetTagImpl(string name, string value);
		protected abstract void SetTagImpl(string name, int value); 

		#if UNITY_IPHONE
		private const string SHARED_LIBRARY = "__Internal";
		#else
		private const string SHARED_LIBRARY = "playscape_pubkit";
		#endif

		[DllImport(SHARED_LIBRARY)]
		public static extern void playscape_TagReporter_setTag(string key, string value);

		[DllImport(SHARED_LIBRARY)]
		public static extern void playscape_TagReporter_reportAllTags();
	}
}