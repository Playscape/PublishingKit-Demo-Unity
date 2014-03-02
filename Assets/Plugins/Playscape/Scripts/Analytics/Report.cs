using System;
using Playscape.Internal;
using System.Collections.Generic;
using Playscape.Purchase;
using UnityEngine;
using System.Text;

namespace Playscape.Analytics
{
    /// <summary>
    /// A social network that the user can log into
    /// </summary>
    public enum SocialNetwork
	{
		Facebook,
		GooglePlus,
        GameSpecific
	}

    /// <summary>
    /// Subscription service states
    /// </summary>
    public enum ServiceState
    {
        FirstTimeSubscried = 100,
        FirstTimeUnsubscribed = 200,
        ReminderSubscribed = 110,
        ReminderIgnored = 220,
        ManualSubscribe = 120,
        ManualUnsubscribe = 210
    }

    /// <summary>
    /// Interface for multiplayer analytics
    /// </summary>
    public interface MPAnalyticsProvider
	{
        /// <summary>
        /// Gets CurrentNetworkTime.
        /// </summary>
        /// <value>
        /// The current network time.
        /// </value>
        int CurrentNetworkTime { get; }
	}

    /// <summary>
    /// Gives the reporter information about the current social network
    /// </summary>
    public interface SocialAnalyticsProvider
	{
		/// <summary>
		/// Currently logged-in social network. <code>null</code> if not
		/// logged-in to any network.
		/// </summary>
 		SocialNetwork? CurrentNetwork { get; }
	}
	
    /// <summary>
    /// Represents an instance of a flow (see Generic flows below)
    /// </summary>
    public interface FlowInstance
    {
        /// <summary>
        /// Flow instance globally unique identifier
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Flow type, as given to RegisterFlow()
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Retrieves the step identifier by name.
        /// 
        /// Throws an ArgumentException in DEBUG compilations, and returns -1 in
        /// release compilations (without throwing an exception).
        /// </summary>
        /// <param name="stepName">
        /// The step Name.
        /// </param>
        /// <returns>
        /// The get step id.
        /// </returns>
        int GetStepId(string stepName);
    }

	#region FlowInstance implementation

    /// <summary>
    /// Default flow implementation.
    /// </summary>
    internal class DefaultFlowInstance : FlowInstance {
        /// <summary>
        /// Flow type
        /// </summary>
        private readonly string mType;

        /// <summary>
        /// Flow id
        /// </summary>
        private readonly string mId;

        /// <summary>
        /// Maps step names to their corresponding id
        /// </summary>
        private readonly IDictionary<string, int> mStepNameToStepId;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultFlowInstance"/> class.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="stepNameToStepId">
        /// The step name to step id.
        /// </param>
        internal DefaultFlowInstance(string type, IDictionary<string, int> stepNameToStepId) {
			mId = Guid.NewGuid().ToString();
			mType = type;
			mStepNameToStepId = stepNameToStepId;
		}

		/// <summary>
		/// Flow instance globally unique identifier
		/// </summary>
		public string Id { get { return mId; } }
		
		/// <summary>
		/// Flow type, as given to RegisterFlow()
		/// </summary>
		public string Type { get { return mType; } }

        /// <summary>
        /// Gets Steps.
        /// </summary>
        /// <value>
        /// The steps.
        /// </value>
        public IDictionary<string, int> Steps
	    {
	        get { return mStepNameToStepId; }
	        
	    }

	    /// <summary>
	    /// Retrieves the step identifier by name.
	    /// 
	    /// Throws an ArgumentException in DEBUG compilations, and returns -1 in
	    /// release compilations (without throwing an exception).
	    /// </summary>
	    /// <param name="stepName">
	    /// The step Name.
	    /// </param>
	    /// <returns>
	    /// The get step id.
	    /// </returns>
		public int GetStepId(string stepName) {
			if (mStepNameToStepId.ContainsKey(stepName)) {
				return mStepNameToStepId[stepName];
			}
		 
            if (Debug.isDebugBuild) {
		        throw new ArgumentException(string.Format("Invalid stepName: '{0}' given", stepName));
		    }

		    return -1;
		}
	}

    /// <summary>
    /// A stub flow, used as placeholder so that NPE's won't occur.
    /// </summary>
    internal class StubFlowInstance : FlowInstance
    {
        /// <summary>
        /// stub id
        /// </summary>
        private const string STUB_FLOW_ID = "StubFlowId";

        /// <summary>
        /// stub flow type
        /// </summary>
        private const string STUB_FLOW_TYPE = "StubFlowType";

        /// <summary>
        /// Gets Id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public string Id
        {
            get { return STUB_FLOW_ID; }
        }

        /// <summary>
        /// Gets Type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type
        {
            get { return STUB_FLOW_TYPE; }
        }

        /// <summary>
        /// </summary>
        /// <param name="stepName">
        /// The step name.
        /// </param>
        /// <returns>
        /// Step id
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if running in debug mode
        /// </exception>
        public int GetStepId(string stepName)
        {
            if (Debug.isDebugBuild)
            {
                throw new ArgumentException("This is a stub flow, there are no steps!");
            }

            return -1;
        }
    }

	#endregion

    /// <summary>
    /// Report analytics to Playscape for BI Analysis
    /// </summary>
    public class Report
	{
        /// <summary>
        /// Provides information about the currently used social network
        /// </summary>
        private SocialAnalyticsProvider mSocialAnalyticsProvider;

        /// <summary>
        /// Prevents a default instance of the <see cref="Report"/> class from being created.
        /// Use the Instance method instead.
        /// </summary>
        private Report() {
			// Singleton - use Instance
            CustomVariables = new CustomVariablesMap();
		}

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static Report Instance = new Report();
		
		/// <summary>
		/// If this is a multiplayer game, the reporter will try to
		/// report some multiplayer related fields, which are provided
		/// by an external infrastructure.
		/// </summary>
		/// <param name="provider">
		/// The provider.
		/// </param>
		public void InitMultiplayer (MPAnalyticsProvider provider) {
			NetworkTimeUpdate.mMPAnalyticsProvider = provider;
		}

        /// <summary>
        /// Initializes social network with the provider.
        /// You should call this method prior to using any social related reports.
        /// </summary>
        /// <param name="provider">
        /// The provider.
        /// </param>
        public void InitSocial(SocialAnalyticsProvider provider) {
			mSocialAnalyticsProvider = provider;
		}
		
		#region Lifecycle
		
		/// <summary>
		/// Analytics bible section: 5.01
		/// 
		/// Called automatically by plugin (shouldn't be called directly)
		/// </summary>
		/// <param name='referrer'>
		/// Resolved automatically by plugin
		/// </param>
		public void ReportActivation(string referrer) {

			RemoteLogger.ReportAnalytics ("activation/SD=1/{0}", referrer);
		}
		
		/// <summary>
		/// Analytics bible section: 5.09
		/// </summary>
		/// <param name="referrer">
		/// The referrer.
		/// </param>
		public void ReportReferrer(string referrer) {
			RemoteLogger.ReportAnalytics ("Install_Referrer_Intent/{0}", referrer);
		}

        /// <summary>
        /// Reports app flyer referrer (iOS)
        /// </summary>
        /// <param name="referrer">
        /// The referrer.
        /// </param>
        public void ReportAppsFlyerReferrer(string referrer) {
			RemoteLogger.ReportAnalytics ("Install_Referrer_Intent/AppsFlyer/{0}", referrer);
		}
		
		/// <summary>
		/// Analytics bible section: 5.02
		/// 
		/// Called automatically by plugin (shouldn't be called
		/// directly)
		/// </summary>
		/// <param name='count'>
		/// Resolved automatically by plugin
		/// </param>
		public void ReportLaunch(int launchCount) {
			RemoteLogger.ReportAnalytics ("Launches/{0}", launchCount);
		}
		
		#endregion
		
		#region Push notifications
        /// <summary>
        /// Analytics bible section: 1.12
        /// </summary>
        /// <param name="promotedGame">
        /// The promoted Game.
        /// </param>
        /// <param name="icon">
        /// The icon.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <param name="notificationId">
        /// The notification Id.
        /// </param>
        public void ReportNotificationDisplayed(
                string promotedGame,
                string icon,
                string action,
                string notificationId)
        {
            RemoteLogger.ReportAnalytics("PS2/notification_displayed/{0}/{1}/{2}/{3}/PW",
                                         promotedGame, icon, action, notificationId);
        }

		
		/// <summary>
		/// Analytics bible section: 1.13
		/// </summary>
		/// <param name="promotedGame">
		/// The promoted Game.
		/// </param>
		/// <param name="icon">
		/// The icon.
		/// </param>
		/// <param name="action">
		/// The action.
		/// </param>
		/// <param name="notificationId">
		/// The notification Id.
		/// </param>
		public void ReportNotificationClicked(
			string promotedGame,
			string icon,
			string action,
			string notificationId) 
        {

			RemoteLogger.ReportAnalytics ("PS2/notification_clicked/{0}/{1}/{2}/{3}/PW", 
			                              promotedGame,
			                              icon,
			                              action,
			                              notificationId);

		}
		
		#endregion
		
		#region Interstitials (Analytics bible section: 5.30)
        /// <summary>
        /// Report when interstitial load has failed.
        /// </summary>
        /// <param name="location">
        /// The location.
        /// </param>
        public void ReportInterstitialLoadFailed(string location) {
			RemoteLogger.ReportAnalytics("ad:Interstitial/event:RequestFailed/sdk:Chartboost/id:{0}/conn:{1}",
			                             location,
			                             PlayscapeUtilities.GetConnectivityAnalyticsReport());
		}

        /// <summary>
        /// Report when interstitial has been dismissed.
        /// </summary>
        /// <param name="location">
        /// The location.
        /// </param>
        public void ReportInterstitialDismissed(string location) {
			RemoteLogger.ReportAnalytics("ad:Interstitial/event:Dismissed/sdk:Chartboost/id:{0}/conn:{1}",
			                             location,
			                             PlayscapeUtilities.GetConnectivityAnalyticsReport());
		}

        /// <summary>
        /// Report when interstitial display has failed
        /// </summary>
        /// <param name="location">
        /// The location.
        /// </param>
        public void ReportInterstitialClicked(string location) {
			RemoteLogger.ReportAnalytics("ad:Interstitial/event:Clicked/sdk:Chartboost/id:{0}/conn:{1}",
			                             location,
			                             PlayscapeUtilities.GetConnectivityAnalyticsReport());
		}

        /// <summary>
        /// Re	port when interstitital has been shown
        /// </summary>
        /// <param name="location">
        /// The location.
        /// </param>
        public void ReportInterstitialShown(string location) {
			RemoteLogger.ReportAnalytics("ad:Interstitial/event:Impression/impression_type:Unknown/sdk:Chartboost/id:{0}/conn:{1}",
			                             location,
			                             PlayscapeUtilities.GetConnectivityAnalyticsReport());
		}
		#endregion
		
		#region Misc
		
		/// <summary>
		/// Analytics bible section: 5.04
		/// </summary>
		/// <param name="customEvent">
		/// The custom Event.
		/// </param>
		public void ReportEvent(string customEvent) {
			RemoteLogger.ReportAnalytics ("custom/{0}", customEvent);
		}

		#region Custom Analyics

        /// <summary>
        /// Represents a map between a custom variable's name and it's value.
        /// </summary>
        public class CustomVariablesMap
        {
            /// <summary>
            /// Map of aux vars
            /// </summary>
            private IDictionary<string, string> mAuxVariables = new Dictionary<string, string>();

            /// <summary>
            /// Get/Set custom var by it's name.
            /// </summary>
            /// <param name="key">
            /// The key.
            /// </param>
            /// <returns>value of custom variable or null if it doesn't exist</returns>
            public string this [string key]
            {
                get
                {
                    if (mAuxVariables.ContainsKey(key))
                    {
                        return mAuxVariables[key];
                    }

                    return null;
                }

                set
                {
                    if (value != null)
                    {
                        SetAuxVar(key, value);
                    }
                    else
                    {
                        RemoveAuxVar(key);
                    }
                }
            }

            /// <summary>
            /// Sets a custom auxilary variable
            /// </summary>
            /// <param name="name">Name.</param>
            /// <param name="value">Value.</param>
            private void SetAuxVar(string name, string value)
            {
                mAuxVariables[name] = value;

                setAuxVariablesInRemoteLogger();
            }

            /// <returns><c>true</c> if aux var is set
            /// <param name="name">Name.</param>
            private Boolean IsAuxVarSet(string name)
            {
                return mAuxVariables.ContainsKey(name);
            }

            /// <summary>
            /// Removes the aux variable.
            /// </summary>
            /// <param name="name">Name.</param>
            private void RemoveAuxVar(string name)
            {
                if (IsAuxVarSet(name))
                {
                    mAuxVariables.Remove(name);
                    setAuxVariablesInRemoteLogger();
                }
            }

            /// <summary>
            /// Compares keys of KeyValuePairs using standard string's compareTo.
            /// </summary>
            private class SimpleStringComparer : IComparer<KeyValuePair<string, string>>
            {
                /// <summary>
                /// Compares strings in given key/value pairs
                /// </summary>
                /// <param name="x">
                /// First KeyValuePair.
                /// </param>
                /// <param name="y">
                /// Second KeyValuePair.
                /// </param>
                /// <returns>
                /// CompareTo method result.
                /// </returns>
                public int Compare(KeyValuePair<string, string> x, KeyValuePair<string, string> y)
                {
                    return x.Key.CompareTo(y.Key);
                }
            }

            /// <summary>
            /// Comparer
            /// </summary>
            private SimpleStringComparer mSimpleStringComparer = new SimpleStringComparer();

            /// <summary>
            /// Updates aux variables in the remote logger.
            /// </summary>
            private void setAuxVariablesInRemoteLogger()
            {
                if (mAuxVariables.Count > 0)
                {
                    SimpleJson.JsonObject jsonObject = new SimpleJson.JsonObject();
                    foreach (var variable in mAuxVariables)
                    {
                        jsonObject.Add(variable.Key, variable.Value);
                    }

                    string json = jsonObject.ToString();

                    // We replace double quotes with single quote to make big query parsing easier and remove any escape characters (which shouldn't be anyway as we disallow this in lua)

                    json = json.Replace("\"", "'").Replace("\\", "");
                    json = json.Substring(1, json.Length - 2); // stip { and }
                    // Now our json string looks like this: 'key':'value', 'key2':value2', ..., 'keyN':valueN
                    // we have to sort it

                    var entriesVector = new List<KeyValuePair<string, string>>();

                    string[] keysAndValuesSplit = json.Split(',');

                    foreach (var keyValueJoined in keysAndValuesSplit)
                    {
                        string[] keyValueSplit = keyValueJoined.Split(':');

                        if (keyValueSplit.Length == 2)
                        {
                            string key = keyValueSplit[0];
                            string value = keyValueSplit[1];
                            var entry = new KeyValuePair<string, string>(key, value);

                            entriesVector.Add(entry);
                        }
                    }

                    entriesVector.Sort(mSimpleStringComparer);

                    var auxVars = new StringBuilder();

                    bool isFirst = true;
                    auxVars.Append("{");
                    foreach (var item in entriesVector)
                    {

                        if (!isFirst)
                        {
                            auxVars.Append(",");
                        }

                        auxVars.Append(item.Key);
                        auxVars.Append(":");
                        auxVars.Append(item.Value);

                        isFirst = false;
                    }
                    auxVars.Append("}");

                    RemoteLogger.SetGameAuxVars(auxVars.ToString());

                }
                else
                {
                    RemoteLogger.SetGameAuxVars(null);
                }
            }

        }

        /// <summary>
        /// Gets CustomVariables.
        /// Custom variables contain auxilary data which is sent with each report.
        /// </summary>
        public CustomVariablesMap CustomVariables { get; private set; }

		#endregion

		#endregion

        #region Purchase

        /// <summary>
        /// Analytics bible section: 5.07
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        public void ReportPurchaseStarted(PurchaseItem item)
        {
            RemoteLogger.ReportAnalytics("{0}/BuyProduct", item.Name);
        }

        #region Bible Section 5.08

        /// <summary>
        /// Should be called by purchase plugin
        /// 
        /// In addition, in Android, onGooglePlayActivityResult
        /// analytics event should be fired with
        /// orderId from result JSON as described in:
        /// http://m-virt/doku.php?id=features:billing:inapp_billing
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <param name="amount">
        /// The amount.
        /// </param>
        /// <param name="currency">
        /// The currency.
        /// </param>
        /// <param name="currencyTimestamp">
        /// The currency Timestamp.
        /// </param>
        /// <param name="transactionId">
        /// The transaction Id.
        /// </param>
        public void ReportPurchaseSuccess(
                PurchaseItem item,
                double amount,
                string currency,
                long currencyTimestamp,
                string transactionId)
        {
            RemoteLogger.ReportAnalytics("{0}/BuyProductResult/Success/currency={1}/amount={2:0.00}/currencyTimestamp={3}/orderId={4}", 
                item.Name, currency, amount, currencyTimestamp, transactionId);
        }
        /// <summary>
        /// Should be called by purchase plugin
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        public void ReportPurchaseCancelled(PurchaseItem item)
        {
            RemoteLogger.ReportAnalytics("{0}/BuyProductResult/Cancelled",
                item.Name);
        }
        /// <summary>
        /// Should be called by purchase plugin
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <param name="failureReason">
        /// The failure Reason.
        /// </param>
        public void ReportPurchaseFailed(PurchaseItem item,
                string failureReason)
        {
            RemoteLogger.ReportAnalytics("{0}/BuyProductResult/Failed/{1}",
                item.Name, failureReason);
        }
        /// <summary>
        /// Should be called by purchase plugin
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        public void ReportPurchaseAlreadyPurchased(PurchaseItem item)
        {
            RemoteLogger.ReportAnalytics("{0}/BuyProductResult/Already_purchased",
                item.Name);
        }

        #endregion

        #endregion

		#region Social
		
		#region Bible Section 5.20

        /// <summary>
        /// true if called ReportSocialLoginSuccess() method.
        /// </summary>
        private bool mIsLoggedIn = false;

        /// <summary>
        /// Reports a succesful login to a social network.
        /// 
        /// Be sure to call InitSocial() prior to using this method.
        /// </summary>
        /// <param name="isSilentLogin">
        /// The is silent login, without any user interventoin (e.g restoring a login session using a stored token)
        /// </param>
        /// <param name="whichUserLoggedIn">
        /// The which user logged in.
        /// </param>
        public void ReportSocialLoginSuccess(
			bool isSilentLogin,
			string whichUserLoggedIn) {
            
            mIsLoggedIn = true;
			if (mSocialAnalyticsProvider.CurrentNetwork != null) {
				RemoteLogger.ReportAnalytics ("{0}/Login/success/silent={1}/User:{2}",
				                          mSocialAnalyticsProvider.CurrentNetwork,
			                              PlayscapeUtilities.JsonBool(isSilentLogin),
			                              whichUserLoggedIn);
			}
		}

        /// <summary>
        /// Reports failure in login
        /// </summary>
        /// <param name="isSilentLogin">
        /// The is silent login, without any user interventoin (e.g restoring a login session using a stored token)
        /// </param>
        public void ReportSocialLoginFailed(bool isSilentLogin) {
			if (mSocialAnalyticsProvider.CurrentNetwork != null) {
				RemoteLogger.ReportAnalytics ("{0}/Login/failed/silent={1}",
				                             mSocialAnalyticsProvider.CurrentNetwork,
				                             PlayscapeUtilities.JsonBool(isSilentLogin));
			}
		}

        /// <summary>
        /// Login has been canceled, e.g user clicked the cancel button in the login dialog.
        /// </summary>
        /// <param name="isSilentLogin">
        /// The is silent login, without any user interventoin (e.g restoring a login session using a stored token)
        /// </param>
        public void ReportSocialLoginCancelled(bool isSilentLogin) {
			if (mSocialAnalyticsProvider.CurrentNetwork != null) {
				RemoteLogger.ReportAnalytics ("{0}/Login/canceled/silent={1}",
				                              mSocialAnalyticsProvider.CurrentNetwork,
				                              PlayscapeUtilities.JsonBool(isSilentLogin));
			}
		}

		#endregion
		
		#region Bible Section 5.21

        /// <summary>
        /// Reports implicit social share, without a dialog.
        /// </summary>
        public void ReportSocialShareNoDialog() { // "Auto" in bible
			if (mSocialAnalyticsProvider.CurrentNetwork != null) {
				RemoteLogger.ReportAnalytics ("{0}/Share/Auto",
				                              mSocialAnalyticsProvider.CurrentNetwork);
			}
		}

        /// <summary>
        /// Reports a social share done through a dialog.
        /// </summary>
        public void ReportSocialShareDialog() {
			if (mSocialAnalyticsProvider.CurrentNetwork != null) {
				RemoteLogger.ReportAnalytics ("{0}/Share/Dialog",
				                              mSocialAnalyticsProvider.CurrentNetwork);
			}
		}

        /// <summary>
        /// Reports social share canceled, e.g user clicked cancel in the share dialog.
        /// </summary>
        public void ReportSocialShareCancelled() {
			if (mSocialAnalyticsProvider.CurrentNetwork != null) {
				RemoteLogger.ReportAnalytics ("{0}/Share/Dialog/Canceled",
				                              mSocialAnalyticsProvider.CurrentNetwork);
			}
		}
		#endregion
		
		#region Bible Section 5.22

        /// <summary>
        /// A friends list load request has finished.
        /// </summary>
        /// <param name="friendsCount">
        /// The friends count loaded.
        /// </param>
        public void ReportSocialFriendsLoaded(int friendsCount) {
			if (mSocialAnalyticsProvider.CurrentNetwork != null){
				RemoteLogger.ReportAnalytics ("{0}/LoadFriends/Success/FriendsUpdated:{1}",
				                              mSocialAnalyticsProvider.CurrentNetwork,
				                              friendsCount);
			}
		}
		/// <summary>
		/// Should report the correct variant according to social login
		/// status
		/// </summary>
		public void ReportSocialFriendsLoadFailed() {

			string reasonText = mIsLoggedIn ? "UpdateFailed" : "NotLoggedIn";

			if (mSocialAnalyticsProvider.CurrentNetwork != null){
				RemoteLogger.ReportAnalytics ("{0}/LoadFriends/Failure/{1}",
				                              mSocialAnalyticsProvider.CurrentNetwork,
				                              reasonText);
			}
		}
		#endregion
		
		#region Bible Section 5.23

        /// <summary>
        /// Reports the amount of score submitted to the social network.
        /// </summary>
        /// <param name="score">
        /// The score.
        /// </param>
        public void ReportSocialSubmitScore(long score) {
			if (mSocialAnalyticsProvider.CurrentNetwork != null) {
				RemoteLogger.ReportAnalytics ("{0}/SubmitScore/Success/{1}",
				                              mSocialAnalyticsProvider.CurrentNetwork,
				                              score);
			}
		}
		
		/// <summary>
		/// Should report the RequestFailed variant
		/// </summary>
		public void ReportSocialSubmitScoreFailed() {
			if (mSocialAnalyticsProvider.CurrentNetwork != null) {
				RemoteLogger.ReportAnalytics ("{0}/SubmitScore/Failure/RequestFailed",
				                              mSocialAnalyticsProvider.CurrentNetwork);
			}
		}
		#endregion
		
		#region Bible Section 5.24
        /// <param name="whichRequest">
        /// Request id.
        /// </param>
        /// <param name="whichTargetUserId">
        /// Id of user the request was targeted to.
        /// </param>
        /// <param name="uniqueRequestId">
        /// 
        ///     A randomly generated number.
        ///     This should be sent in the request, so the receiving end may call
        ///     ReportSocialRequestDetails with the same ID.
        /// </param>
		public void ReportSocialRequestSent(
			string whichRequest,
			string whichTargetUserId,
			long uniqueRequestId)
		{
			if (mSocialAnalyticsProvider.CurrentNetwork != null) {
				RemoteLogger.ReportAnalytics ("{0}/RequestSent/Id:{1}/Success/To:{2}/UniqueID:{3}",
				                              mSocialAnalyticsProvider.CurrentNetwork,
				                              whichRequest,
				                              whichTargetUserId,
				                              uniqueRequestId);
			}
		}

        /// <summary>
        /// Request send failed.
        /// </summary>
        /// <param name="whichRequest">
        /// Request identifier.
        /// </param>
        /// <param name="failureReason">
        /// The failure reason.
        /// </param>
        public void ReportSocialRequestFailed(
			string whichRequest,
			string failureReason) {

			if (mSocialAnalyticsProvider.CurrentNetwork != null) {
				RemoteLogger.ReportAnalytics ("{0}/RequestSent/Id:{1}/Failure/{2}",
				                              mSocialAnalyticsProvider.CurrentNetwork,
				                              whichRequest,
				                              failureReason);
			}
		}
		#endregion
		
		#region Bible Section 5.25

        /// <summary>
        /// Reports succesful retrieval of friend image from the social network.
        /// </summary>
        /// <param name="numImages">
        /// The amount of images retrieved succesfuly.
        /// </param>
        public void ReportSocialGetImagesSuccess(int numImages) {
			if (mSocialAnalyticsProvider.CurrentNetwork != null) {
				RemoteLogger.ReportAnalytics ("{0}/GetImages/Count:{1}/Success",
				                              mSocialAnalyticsProvider.CurrentNetwork,
				                              numImages);
			}
		}

        /// <summary>
        /// Reports a failed retrieval of friend image from the social network.
        /// </summary>
        /// <param name="numImages">
        /// The amount of images of which retrieval has failed.
        /// </param>
        public void ReportSocialGetImagesFailed(int numImages) {
			if (mSocialAnalyticsProvider.CurrentNetwork != null) {
				RemoteLogger.ReportAnalytics ("{0}/GetImages/Count:{1}/Failure",
				                              mSocialAnalyticsProvider.CurrentNetwork,
				                              numImages);
			}
		}
		#endregion
		
		#region Bible Section 5.26 and 5.27
		/// <summary>
		/// Should not report when requestsCount == 0
		/// </summary>
		/// <param name="requestsCount">
		/// The requests Count.
		/// </param>
		public void ReportSocialRequestsFound(int requestsCount) {
			if ((mSocialAnalyticsProvider.CurrentNetwork != null) && (requestsCount > 0)) {
				RemoteLogger.ReportAnalytics ("{0}/RequestsReceived/Count:{1}",
				                              mSocialAnalyticsProvider.CurrentNetwork,
				                              requestsCount);
				}
		}
		
        /// <param name="whichRequest">
        /// Reuqest identifier.
        /// </param>
        /// <param name="whoFromUserId">
        /// Id of the user who sent the request.
        /// </param>
        /// <param name="uniqueId">
        /// Must be the same value passed to the uniqueRequestId parameter of
        /// ReportSocialRequestSent at the sending side.
        /// </param>
		public void ReportSocialRequestDetails(
			string whichRequest,
			string whoFromUserId,
			long uniqueId) {

			if (mSocialAnalyticsProvider.CurrentNetwork != null) {
				RemoteLogger.ReportAnalytics ("{0}/RequestReceived/Id:{1}/From:{2}/UniqueID:{3}",
				                              mSocialAnalyticsProvider.CurrentNetwork,
				                              whichRequest,
				                              whoFromUserId,
				                              uniqueId);
			}
		}
		#endregion
		
		#region Bible Section 5.28

        /// <summary>
        /// Reports that a leaderboard UI has been opened.
        /// </summary>
        /// <param name="whichLeaderboard">
        /// The which leaderboard.
        /// </param>
        public void ReportLeaderboardOpened(string whichLeaderboard) {
			if (mSocialAnalyticsProvider.CurrentNetwork == SocialNetwork.GooglePlus) {
				RemoteLogger.ReportAnalytics ("{0}/LeaderboardOpened/Name:{1}",
				                              SocialNetwork.GooglePlus,
				                              whichLeaderboard);
			}
		}
		#endregion

        #region Bible Section 5.29

        /// <summary>
        /// Reports logout from the social network.
        /// </summary>
        public void ReportSocialLogout()
        {
            mIsLoggedIn = false;
            RemoteLogger.ReportAnalytics("{0}/Logout",
                                          mSocialAnalyticsProvider.CurrentNetwork);
        }

        #endregion

        #endregion

        #region Multiplayer

        #region Server connection
        /// <summary>
        /// Bible section 10.30
        /// </summary>
        /// <param name="serverName">
        /// The server Name.
        /// </param>
		public void ReportMPServerConnect(string serverName) {
			RemoteLogger.ReportAnalytics ("Networking/ConnectToOptimalServer/Connecting/ServerName:{0}", serverName);
		}
		/// <summary>
		/// Bible section 10.32
		/// </summary>
		/// <param name="serverName">
		/// The server Name.
		/// </param>
		/// <param name="latency">
		/// The latency.
		/// </param>
		public void ReportMPServerConnectSuccess(
			string serverName,
			TimeSpan latency) {
			RemoteLogger.ReportAnalytics ("Networking/ConnectToOptimalServer/Success/Server:{0}/Latency:{1}",
			                              serverName,
			                              latency.TotalMilliseconds);
		}
		/// <summary>
		/// Networking/StartGameWithFriends/Failure/[reason]
		/// 10.31, 10.33
		/// </summary>
		/// <param name="failureReason">
		/// The failure Reason.
		/// </param>
		public void ReportMPServerConnectFailed(string failureReason) {
			RemoteLogger.SetNetSessionId(null);
			RemoteLogger.ReportAnalytics ("Networking/ConnectToOptimalServer/Failure/{0}", failureReason);
		}
		
		/// <summary>
		/// Bible section 10.34
		/// </summary>
		public void ReportMPServerDisconnect() {
			RemoteLogger.SetNetSessionId(null);
			RemoteLogger.ReportAnalytics ("Networking/ConnectToOptimalServer/ConnectionStateChanged/Disconnected");
		}
		/// <summary>
		/// Bible section 10.35
		/// </summary>
		/// <param name="errorReason">
		/// The error Reason.
		/// </param>
		public void ReportMPServerError(string errorReason) {
			RemoteLogger.SetNetSessionId(null);
			RemoteLogger.ReportAnalytics ("Networking/ConnectToOptimalServer/Failure/ErrorCode:{0}", errorReason);
		}
		#endregion
		
		#region Online friends
		/// <summary>
		/// Bible section 10.01
		/// </summary>
		public void ReportMPLoadOnlineFriends() {
			RemoteLogger.ReportAnalytics ("Networking/LoadOnlineFriends");
		}
		/// <summary>
		/// Bible section 10.02
		/// </summary>
		/// <param name="friendsCount">
		/// The friends Count.
		/// </param>
		public void ReportMPLoadOnlineFriendsSuccess(int friendsCount) {
			RemoteLogger.ReportAnalytics ("Networking/LoadOnlineFriends/Success/FriendCount:{0}", friendsCount);
		}
		#endregion
		
		#region Public games
		/// <summary>
		/// Bible section 10.11 with parameters
		/// 
		/// </summary>
		/// <param name="sessionID">
		/// The session ID, must be the same value for all players playing together.
		/// Must be globally unique (i.e. it should never be reused by any future game session).
		/// </param>
		/// <param name="maxPlayers">
		/// The max Players.
		/// </param>
		/// <param name="gameParameters">
		/// The game Parameters.
		/// </param>
        public void ReportMPCreatePublicGame(
			string sessionId,
            int maxPlayers,
            IDictionary<string, string> gameParameters)
        {
			RemoteLogger.SetNetSessionId(sessionId);

            RemoteLogger.ReportAnalytics("Networking/StartRandomGame/CreateRoom/MaxPlayers:{0}/CustomProperties:{1}",
                                          maxPlayers,
                                          GetCustomProperties(gameParameters));
        }

        /// <summary>
        /// Bible section 10.03
        /// 
        /// Note: gameName must be unique in the world, but the same for all
        /// players in the same room
		/// </summary>
		/// <param name="sessionId">
		/// Session identifier for create room.
		/// </param>
        /// <param name="gameName">
        /// The game Name.
        /// </param>
        /// <param name="maxPlayers">
        /// The max Players.
        /// </param>
        /// <param name="gameParameters">
        /// The game Parameters.
        /// </param>
		public void ReportMPJoinPublicGame(
			string sessionId,
            string gameName,
			int maxPlayers,
			IDictionary<string, string> gameParameters) {
			RemoteLogger.SetNetSessionId(sessionId);
			RemoteLogger.ReportAnalytics ("Networking/StartRandomGame/MaxPlayers:{0}/CustomProperties:{1}",
			                              maxPlayers,
			                              GetCustomProperties(gameParameters));
		}

		/// <summary>
		/// Customs the properties.
		/// </summary>
		/// <returns>string in the format of {key:value,..}</returns>
		/// <param name="parameters">Room Parameters.</param>
		private string GetCustomProperties (IDictionary<string, string> parameters){
			parameters = parameters ?? new Dictionary<string, string>();

			StringBuilder temp = new StringBuilder("{");
			string sep = "";

			foreach (var entry in parameters) { //KeyValuePair<string,string>
				temp.Append (sep);
				temp.Append (entry.Key);
				temp.Append (":");
				temp.Append (entry.Value);
				sep = ",";
			}
			temp.Append ("}");
			return temp.ToString ();
		}

		/// <summary>
		/// Networking/StartRandomGame/Failure/[reason]
		/// 
		/// </summary>
		/// <param name="failureReason">
		/// The failure Reason.
		/// </param>
		public void ReportMPJoinPublicGameFailure(string failureReason) {
            RemoteLogger.SetNetSessionId(null);
			RemoteLogger.ReportAnalytics ("Networking/StartRandomGame/Failure/{0}", failureReason);
		}
        /// <summary>
        /// Bible section 10.12
        /// 
        /// Note: gameName must be unique in the world, but the same for all
        /// players in the same room
		/// </summary>
		/// <param name="sessionID">
		/// The session ID, must be the same value for all players playing together.
		/// Must be globally unique (i.e. it should never be reused by any future game session).
		/// </param>
        /// <param name="gameName">
        /// The game Name.
        /// </param>
        /// <param name="playerId">
        /// The player Id.
        /// </param>
		public void ReportMPJoinedPublicGame(string sessionId, string gameName, int playerId) {
			RemoteLogger.SetNetSessionId(sessionId);
			RemoteLogger.ReportAnalytics ("Networking/StartRandomGame/JoinedRoom/RoomName:{0}/PlayerId:{1}",
			                              gameName,
			                              playerId);
		}
		#endregion
		
		#region Private games
		/// <summary>
		/// Bible section 10.19
		/// </summary>
		/// <param name="sessionID">
		/// The session ID, must be the same value for all players playing together.
		/// Must be globally unique (i.e. it should never be reused by any future game session).
		/// </param>
		/// <param name="gameName">
		/// The game Name.
		/// </param>
		/// <param name="maxPlayers">
		/// The max Players.
		/// </param>
		public void ReportMPCreatePrivateGame(string sessionId, string gameName, int maxPlayers) {
			RemoteLogger.SetNetSessionId(sessionId);
			RemoteLogger.ReportAnalytics ("Networking/StartGameWithFriends/CreateRoom/RoomName:{0}/MaxPlayers:{1}",
			                              gameName,
			                              maxPlayers);
		}
		
		/// <summary>
		/// Bible section 10.14
		/// </summary>
		/// <param name="gameName">
		/// The game Name.
		/// </param>
		/// <param name="friendIds">
		/// List of friends invited to the private game.
		/// </param>
		public void ReportMPJoinPrivateGame(string gameName, List<string> friendIds) {
			RemoteLogger.ReportAnalytics("Networking/StartGameWithFriends/RequestId:{0}/FriendIds:{1}",
                                         gameName,
			                             string.Join(",", friendIds.ToArray()));
		}
		
		/// <summary>
		/// Networking/StartGameWithFriends/Failure/[reason]
		/// </summary>
		/// <param name="gameName">
		/// The game Name.
		/// </param>
		/// <param name="failureReason">
		/// The failure Reason.
		/// </param>
		public void ReportMPJoinPrivateGameFailure(
			string gameName,
			string failureReason) {
			RemoteLogger.SetNetSessionId(null);
            RemoteLogger.ReportAnalytics ("Networking/StartGameWithFriends/Failure/{0}",
			                              failureReason);
		}
		
		/// <summary>
		/// Bible section 10.22
		/// </summary>
		/// <param name="gameName">
		/// The game Name.
		/// </param>
		/// <param name="sessionID">
		/// The session ID, must be the same value for all players playing together.
		/// Must be globally unique (i.e. it should never be reused by any future game session).
		/// </param>
		/// <param name="playerId">
		/// The player Id.
		/// </param>
		public void ReportMPJoinedPrivateGame(string sessionId, string gameName, int playerId) {
			RemoteLogger.SetNetSessionId(sessionId);
			RemoteLogger.ReportAnalytics ("Networking/StartGameWithFriends/JoinedRoom/RoomName:{0}/PlayerId:{1}",
			                              gameName,
			                              playerId);
		}
		#endregion
		
		#region Games common events
		/// <summary>
		/// Bible section 10.25
		/// </summary>
		/// <param name="numberOfPlayers">
		/// The number Of Players.
		/// </param>
		public void ReportMPStartGame(int numberOfPlayers) {
			RemoteLogger.ReportAnalytics ("Networking/StartGame/PlayerCount:{0}", numberOfPlayers);
		}
		
		/// <summary>
		/// Player is leaving the game.
		/// </summary>
		/// <param name="gameName">
		/// The game Name.
		/// </param>
		public void ReportMPLeaveGame(string gameName) {
			RemoteLogger.ReportAnalytics ("Networking/ConnectionStateChanged/LEAVING/RoomName:{0}", gameName);
            RemoteLogger.SetNetSessionId(null);
		}


		#endregion

		#endregion

        #region Generic flows

        /// <summary>
        /// Type to flow instances map
        /// </summary>
        private IDictionary<string, FlowInstance> mTypeToFlowInstance = new Dictionary<string, FlowInstance>();

        /// <summary>
        /// </summary>
        private readonly FlowInstance mStubFlow = new StubFlowInstance();

        /// <summary>
        /// Registers a new flow and all its steps.
        /// 
        /// Doesn't report anything.
		/// 
		/// If the same flow type is registered again then then the previous one is replaced.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="stepNameToId">
        /// The step Name To Id.
        /// </param>
        public void RegisterFlow(
            string type,
            IDictionary<string, int> stepNameToId) {
			mTypeToFlowInstance[type] = new DefaultFlowInstance(type, stepNameToId);
		}

        /// <summary>
        /// Bible section 6.26
        ///
        /// Reports a step in the given flow:
        ///
        /// Reports: Flow/Type:[flow.Type]/SessionID:[flow.Id]/StepName:[stepName]/StepNumber:[flow.GetStepId()]/Status:[stepStatus]/[details]
        ///
        /// [details] is reported as follows: [key1]=[value1]/[key2]=[value2]
        /// where the keys are ordered alphabetically
        ///
        /// If the given FlowInstance is a stub FlowInstance received from
        /// StartNewFlow with an invalid flow type, the method throws
        /// ArgumentException in DEBUG and does nothing in RELEASE.
        ///
        /// If the given stepName isn't found in the given FlowInstance,
        /// ArgumentException is thrown in DEBUG, and nothing is reported in
        /// RELEASE.
        public FlowInstance StartNewFlow(string type) {
            if (mTypeToFlowInstance.ContainsKey(type))
            {
                return mTypeToFlowInstance[type];
            } else
            {
                return mStubFlow;
            }
		}

        /// <summary>
        /// Bible section 6.26
        ///
        /// Reports a step in the given flow:
        ///
        /// Reports: Flow/Type:[flow.Type]/SessionID:[flow.Id]/StepName:[stepName]/StepNumber:[flow.GetStepId()]/Status:[stepStatus]/[details]
        ///
        /// [details] is reported as follows: [key1]=[value1]/[key2]=[value2]
        /// where the keys are ordered alphabetically
        ///
        /// If the given FlowInstance is a stub FlowInstance received from
        /// StartNewFlow with an invalid flow type, the method throws
        /// ArgumentException in DEBUG and does nothing in RELEASE.
        ///
        /// If the given stepName isn't found in the given FlowInstance,
        /// ArgumentException is thrown in DEBUG, and nothing is reported in
        /// RELEASE.
        public void ReportFlowStep(
            FlowInstance flow,
            string stepName,
            string stepStatus,
            IDictionary<string, double> details) {
                if (flow is StubFlowInstance)
                {
                    if (Debug.isDebugBuild)
                    {
                        throw new ArgumentException("The given flow is invalid!");
                    }
                } else if (flow is DefaultFlowInstance)
                {
                    var detailsString = new StringBuilder();
                    var keysArray = new string[details.Keys.Count];
                    details.Keys.CopyTo(keysArray, 0);
                    Array.Sort(keysArray);

                    bool isFirst = true;
                    foreach (var key in keysArray)
                    {
                        if (!isFirst)
                        {
                            detailsString.Append("/");
                        } else
                        {
                            isFirst = false;
                        }

                        detailsString.Append(key).Append("=").Append(details[key]);
                    }

                    RemoteLogger.ReportAnalytics("Flow/Type:{0}/SessionID:{5}/StepName:{1}/StepNumber:{2}/Status:{3}/{4}",
                                               flow.Type, stepName, flow.GetStepId(stepName), stepStatus, detailsString, flow.Id);
                }
            }

        #endregion

        #region Gameplay-related

        // this section referrs to the following analytics bible:
        // https://svn:8443/svn/Mominis/Trunk/Games/_docs/general_docs/MoMinis_Example_Analytics_Table.xlsx
        // 
        // All the events are prefixed with: short_package_name/custom/

        // the bible assumes two types of virtual currencies: coins and bucks.
        // We cannot make that assumption in Kit games, so we've opted for a
        // generic method of passing additional arguments for an event.
        //
        // Each event can be suffixed with a list of additional parameters. When
        // the report line is built, the additional parameters are appended,
        // like so: [reported line]/[key1][value1]/[key2][value2]
        //
        // The keys should be ordered alphabetically.

        /// <summary>
        /// Level [level]/Started/[params]
        /// 
        /// Should also update PSAUX::LVSID and send bible section 5.03.
        /// </summary>
        /// <param name="level">
        /// The level.
        /// </param>
        /// <param name="additionalParams">
        /// The additional Params.
        /// </param>
        public void ReportLevelStarted(
            string level,
            IDictionary<string, double> additionalParams)
        {
			RemoteLogger.SetLevelSessionId(level);

			RemoteLogger.ReportAnalytics(
				"custom/Level {0}/Started{1}",
				level,
				PlayscapeUtilities.FormatGameplayRelatedAdditionalParams(additionalParams));

			RemoteLogger.ReportAnalytics(
				"room/{0}",
				level);
        }

        /// <summary>
        /// Level [level]/Completed/[params]
        /// 
        /// Should also update PSAUX::LVSID appropriately
        /// </summary>
        /// <param name="level">
        /// The level.
        /// </param>
        /// <param name="additionalParams">
        /// The additional Params.
        /// </param>
        public void ReportLevelCompleted(
            string level,
            IDictionary<string, double> additionalParams)
        {
			RemoteLogger.ReportAnalytics(
				"custom/Level {0}/Completed{1}",
				level,
				PlayscapeUtilities.FormatGameplayRelatedAdditionalParams(additionalParams));
			
			RemoteLogger.SetLevelSessionId(null);
        }

        /// <summary>
        /// Level [level]/Failed/[params]
        /// 
        /// Should also update PSAUX::LVSID appropriately.
        /// </summary>
        /// <param name="level">
        /// The level.
        /// </param>
        /// <param name="additionalParams">
        /// The additional Params.
        /// </param>
        public void ReportLevelFailed(
            string level,
            IDictionary<string, double> additionalParams)
        {
			RemoteLogger.ReportAnalytics(
				"custom/Level {0}/Failed{1}",
				level,
				PlayscapeUtilities.FormatGameplayRelatedAdditionalParams(additionalParams));
			
			RemoteLogger.SetLevelSessionId(null);
        }

        /// <summary>
        /// Achievement[achievement]/Unlocked/[params].
        /// </summary>
        /// <param name="achievement">
        /// The achievement.
        /// </param>
        /// <param name="additionalParams">
        /// The additional Params.
        /// </param>
        public void ReportAchievementUnlocked(
            string achievement,
            IDictionary<string, double> additionalParams)
        {
			RemoteLogger.ReportAnalytics(
				"custom/Achievement{0}/Unlocked{1}",
				achievement,
				PlayscapeUtilities.FormatGameplayRelatedAdditionalParams(additionalParams));
        }

        /// <summary>
        /// Store/Item[item_id]/UnlockSuccessful/[params].
        /// </summary>
        /// <param name="itemId">
        /// The item Id.
        /// </param>
        /// <param name="additionalParams">
        /// The additional Params.
        /// </param>
        public void ReportItemUnlocked(
            int itemId,
            IDictionary<string, double> additionalParams)
        {
			RemoteLogger.ReportAnalytics(
				"custom/Store/Item{0}/UnlockSuccessful{1}",
				itemId,
				PlayscapeUtilities.FormatGameplayRelatedAdditionalParams(additionalParams));
        }

        /// <summary>
        /// Row 21 in Excel
        /// </summary>
        public void ReportRatingDialogShow()
        {
			RemoteLogger.ReportAnalytics("custom/Rating/Displayed");
        }

        /// <summary>
        /// Row 22 in Excel
        /// </summary>
        public void ReportRatingDialogYes()
        {
			RemoteLogger.ReportAnalytics("custom/Rating/Yes");
        }

        /// <summary>
        /// Row 23 in Excel
        /// </summary>
        public void ReportRatingDialogNo()
        {
			RemoteLogger.ReportAnalytics("custom/Rating/NoThanks");
        }

        #endregion

        #region Subscription Service (Bible section 11.1)

	    /// <summary>
		/// Reports subscription
	    /// </summary>
	    /// <param name="state">
	    /// The state.
	    /// </param>
	    public void ReportSubscriptionState(ServiceState state)
        {
            RemoteLogger.ReportAnalytics("custom/SubscriptionService/State:{0}", (int)state);
        }

        #endregion
	}
}

