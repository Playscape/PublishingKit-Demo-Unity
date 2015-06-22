using System;
using Playscape.Internal;
using System.Collections.Generic;
using Playscape.Purchase;
using UnityEngine;
using System.Text;
using System.Runtime.InteropServices;

namespace Playscape.Analytics
{
    /// <summary>
    /// A social network that the user can log into
    /// </summary>
    public enum SocialNetwork
	{
		// Don't use zero, it means 'NoSocialNetwork' in C API.
		Facebook = 1,
		GooglePlus = 2,
        GameSpecific = 3
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
    public class FlowInstance
    {
        /// <summary>
		/// Constructs a new FlowInstance of the given type
		/// </summary>
		/// <param name="provider">
		/// The provider.
		/// </param>
		/// <param name="id"> 
		/// Identifier.
		/// </param>
		public FlowInstance(string type, string id)
		{
			Type = type;
			Id = id;
		}

        /// <summary>
		/// Gets current type.
		/// </summary>
		public string Type { get; private set; }

		/// <summary>
		/// Gets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public string Id { get; private set; }
    }

    /// <summary>
    /// Report analytics to Playscape for BI Analysis
    /// </summary>
    public class Report
	{

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
        /// </summary>
		private object mMpAnalyticsProvider;
        
        /// <summary>
        /// </summary>
		private object mSocialAnalyticsProvider;

		/// <summary>
		/// If this is a multiplayer game, the reporter will try to
		/// report some multiplayer related fields, which are provided
		/// by an external infrastructure.
		/// </summary>
		/// <param name="provider">
		/// The provider.
		/// </param>
		public void InitMultiplayer(MPAnalyticsProvider provider) {
			#if !UNITY_EDITOR
            
			CInterop.SetMPAnalyticsProvider(provider);
			NativeReport.playscape_report_InitMultiplayer(CInterop.GetCurrentNetworkTime);

			#endif

		}

        /// <summary>
        /// Initializes social network with the provider.
        /// You should call this method prior to using any social related reports.
        /// </summary>
        /// <param name="provider">
        /// The provider.
        /// </param>
        public void InitSocial(SocialAnalyticsProvider provider) {
			#if !UNITY_EDITOR

			CInterop.SetSocialAnalyticsProvider(provider);   
			NativeReport.playscape_report_InitSocial(CInterop.GetCurrentNetwork);
		
			#endif

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
#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportActivation(referrer);
#endif
		}
		
		/// <summary>
		/// Analytics bible section: 5.09
		/// </summary>
		/// <param name="referrer">
		/// The referrer.
		/// </param>
		public void ReportReferrer(string referrer) {
#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportReferrer(referrer);
#endif
		}

        /// <summary>
        /// Reports app flyer referrer (iOS)
        /// </summary>
        /// <param name="referrer">
        /// The referrer.
        /// </param>
        public void ReportAppsFlyerReferrer(string referrer) {
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportAppsFlyerReferrer(referrer);
#endif
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
#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportLaunch(launchCount);
#endif
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
#if !UNITY_EDITOR
            NativeReport.playscape_report_ReportNotificationDisplayed(promotedGame, icon, action, notificationId);
#endif
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
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportNotificationClicked(
                    promotedGame,
			        icon,
			        action,
			        notificationId);
#endif
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
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportInterstitialLoadFailed(location);
#endif
		}

        /// <summary>
        /// Report when interstitial has been dismissed.
        /// </summary>
        /// <param name="location">
        /// The location.
        /// </param>
        public void ReportInterstitialDismissed(string location) {
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportInterstitialDismissed(location);
#endif
		}

        /// <summary>
        /// Report when interstitial display has failed
        /// </summary>
        /// <param name="location">
        /// The location.
        /// </param>
        public void ReportInterstitialClicked(string location) {
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportInterstitialClicked(location);
#endif
		}

        /// <summary>
        /// Re	port when interstitital has been shown
        /// </summary>
        /// <param name="location">
        /// The location.
        /// </param>
        public void ReportInterstitialShown(string location) {
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportInterstitialShown(location);
#endif
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
#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportEvent(customEvent);
#endif
		}

		#region Custom Analyics

        /// <summary>
        /// Represents a map between a custom variable's name and it's value.
        /// </summary>
        public class CustomVariablesMap
        {
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
                    StringBuilder buffer = new StringBuilder(2048);
					#if !UNITY_EDITOR
                    NativeReport.playscape_report_getCustomVariable(key, buffer.Capacity, buffer);
					#endif
                    string theValue = buffer.ToString();
                    
                    if (theValue == "") {
                        theValue = null;
                    }
                    
                    return theValue;
                }

                set
                {
					#if !UNITY_EDITOR
                    if (value != null)
                    {
                        NativeReport.playscape_report_setCustomVariable(key, value);
                    }
                    else
                    {
						NativeReport.playscape_report_removeCustomVariable(key);
                    }
					#endif
                }
            }
            
			/// <summary>
        	/// Clears all custom variables.
        	/// </summary>
            public void Clear()
            {
                #if !UNITY_EDITOR
                NativeReport.playscape_report_clearCustomVariables();
                #endif
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
        [Obsolete("This API call is now triggered automatically and should not be called directly", true)]
        public void ReportPurchaseStarted(PurchaseItem item)
        {
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
        [Obsolete("This API call is now triggered automatically and should not be called directly", true)]
        public void ReportPurchaseSuccess(
                PurchaseItem item,
                double amount,
                string currency,
                long currencyTimestamp,
                string transactionId)
        {
        }
        /// <summary>
        /// Should be called by purchase plugin
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        [Obsolete("This API call is now triggered automatically and should not be called directly", true)]
        public void ReportPurchaseCancelled(PurchaseItem item)
        {
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
        [Obsolete("This API call is now triggered automatically and should not be called directly", true)]
        public void ReportPurchaseFailed(PurchaseItem item,
                string failureReason)
        {
        }
        /// <summary>
        /// Should be called by purchase plugin
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        public void ReportPurchaseAlreadyPurchased(PurchaseItem item)
        {
			#if !UNITY_EDITOR
            NativeReport.playscape_report_ReportPurchaseAlreadyPurchased(
                item.ToCType());
#endif
        }

        #endregion

        #endregion

		#region Social
		
		#region Bible Section 5.20

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
            
			#if !UNITY_EDITOR
            NativeReport.playscape_report_ReportSocialLoginSuccess(isSilentLogin, whichUserLoggedIn);
#endif
		}

        /// <summary>
        /// Reports failure in login
        /// </summary>
        /// <param name="isSilentLogin">
        /// The is silent login, without any user interventoin (e.g restoring a login session using a stored token)
        /// </param>
        public void ReportSocialLoginFailed(bool isSilentLogin) {
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportSocialLoginFailed(isSilentLogin);
#endif
		}

        /// <summary>
        /// Login has been canceled, e.g user clicked the cancel button in the login dialog.
        /// </summary>
        /// <param name="isSilentLogin">
        /// The is silent login, without any user interventoin (e.g restoring a login session using a stored token)
        /// </param>
        public void ReportSocialLoginCancelled(bool isSilentLogin) {
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportSocialLoginCancelled(isSilentLogin);
#endif
		}

		#endregion
		
		#region Bible Section 5.21

        /// <summary>
        /// Reports implicit social share, without a dialog.
        /// </summary>
        public void ReportSocialShareNoDialog() { // "Auto" in bible
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportSocialShareNoDialog();
#endif
		}

        /// <summary>
        /// Reports a social share done through a dialog.
        /// </summary>
        public void ReportSocialShareDialog() {
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportSocialShareDialog();
#endif
		}

        /// <summary>
        /// Reports social share canceled, e.g user clicked cancel in the share dialog.
        /// </summary>
        public void ReportSocialShareCancelled() {
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportSocialShareCancelled();
#endif
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
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportSocialFriendsLoaded(friendsCount);
#endif
		}
		/// <summary>
		/// Should report the correct variant according to social login
		/// status
		/// </summary>
		public void ReportSocialFriendsLoadFailed() {
			#if !UNITY_EDITOR
            NativeReport.playscape_report_ReportSocialFriendsLoadFailed();
#endif
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
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportSocialSubmitScore(score);
#endif
		}
		
		/// <summary>
		/// Should report the RequestFailed variant
		/// </summary>
		public void ReportSocialSubmitScoreFailed() {
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportSocialSubmitScoreFailed();
#endif
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
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportSocialRequestSent(
                    whichRequest, whichTargetUserId, uniqueRequestId);
#endif
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
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportSocialRequestFailed(whichRequest, failureReason);
#endif
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
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportSocialGetImagesSuccess(numImages);
#endif
		}

        /// <summary>
        /// Reports a failed retrieval of friend image from the social network.
        /// </summary>
        /// <param name="numImages">
        /// The amount of images of which retrieval has failed.
        /// </param>
        public void ReportSocialGetImagesFailed(int numImages) {
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportSocialGetImagesFailed(numImages);
#endif
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
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportSocialRequestsFound(requestsCount);
#endif
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
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportSocialRequestDetails(whichRequest, whoFromUserId, uniqueId);
#endif
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
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportLeaderboardOpened(whichLeaderboard);
			#endif
		}
		#endregion

        #region Bible Section 5.29

        /// <summary>
        /// Reports logout from the social network.
        /// </summary>
        public void ReportSocialLogout()
        {
			#if !UNITY_EDITOR
            NativeReport.playscape_report_ReportSocialLogout();
#endif
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
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportMPServerConnect(serverName);
#endif
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
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportMPServerConnectSuccess(serverName, latency.Milliseconds);
#endif
		}
		/// <summary>
		/// Networking/StartGameWithFriends/Failure/[reason]
		/// 10.31, 10.33
		/// </summary>
		/// <param name="failureReason">11
		/// The failure Reason.
		/// </param>
		public void ReportMPServerConnectFailed(string failureReason) {
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportMPServerConnectFailed(failureReason);
			#endif
		}
		
		/// <summary>
		/// Bible section 10.34
		/// </summary>
		public void ReportMPServerDisconnect() {
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportMPServerDisconnect();
#endif
		}
		/// <summary>
		/// Bible section 10.35
		/// </summary>
		/// <param name="errorReason">
		/// The error Reason.
		/// </param>
		public void ReportMPServerError(string errorReason) {
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportMPServerError(errorReason);
#endif
		}
		#endregion
		
		#region Online friends
		/// <summary>
		/// Bible section 10.01
		/// </summary>
		public void ReportMPLoadOnlineFriends() {
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportMPLoadOnlineFriends();
#endif
		}
		/// <summary>
		/// Bible section 10.02
		/// </summary>
		/// <param name="friendsCount">
		/// The friends Count.
		/// </param>
		public void ReportMPLoadOnlineFriendsSuccess(int friendsCount) {
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportMPLoadOnlineFriendsSuccess(friendsCount);
#endif
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
            

            #if !UNITY_EDITOR
			gameParameters = gameParameters ?? new Dictionary<string, string>();
			var keysAndValues = gameParameters.ToKeysAndValues();
			NativeReport.playscape_report_ReportMPCreatePublicGame(
				sessionId,
				maxPlayers,
				gameParameters.Count,
				keysAndValues.Keys,
               	keysAndValues.Values);
#endif
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
			IDictionary<string, string> gameParameters)
        {   
            
			#if !UNITY_EDITOR
			
			gameParameters = gameParameters ?? new Dictionary<string, string>();
			var keysAndValues = gameParameters.ToKeysAndValues();
			NativeReport.playscape_report_ReportMPJoinPublicGame(
				sessionId,
				gameName,
				maxPlayers,
				gameParameters.Count,
				keysAndValues.Keys,
                keysAndValues.Values);
#endif
		}

		/// <summary>
		/// Networking/StartRandomGame/Failure/[reason]
		/// 
		/// </summary>
		/// <param name="failureReason">
		/// The failure Reason.
		/// </param>
		public void ReportMPJoinPublicGameFailure(string failureReason) {
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportMPJoinPublicGameFailure(failureReason);
#endif
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
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportMPJoinedPublicGame(sessionId, gameName, playerId);
#endif
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
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportMPCreatePrivateGame(sessionId, gameName, maxPlayers);
#endif
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
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportMPJoinPrivateGame(
				gameName,
				friendIds.Count,
				friendIds.ToArray());
#endif
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

			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportMPJoinPrivateGameFailure(gameName, failureReason);
#endif
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
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportMPJoinedPrivateGame(sessionId, gameName, playerId);
#endif
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
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportMPStartGame(numberOfPlayers);
#endif
		}
		
		/// <summary>
		/// Player is leaving the game.
		/// </summary>
		/// <param name="gameName">
		/// The game Name.
		/// </param>
		public void ReportMPLeaveGame(string gameName) {
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportMPLeaveGame(gameName);
#endif
		}


		#endregion

		#endregion

        #region Generic flows

        /// <summary>
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
            IDictionary<string, int> stepNameToId)
        {
            
#if !UNITY_EDITOR
			var keysAndValues = stepNameToId.ToKeysAndValues();
			NativeReport.playscape_report_RegisterFlow(
				type,
				stepNameToId.Count,
				keysAndValues.Keys,
                keysAndValues.Values);
#endif
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
			string id = string.Empty;
			#if !UNITY_EDITOR
			id = Marshal.PtrToStringAnsi(NativeReport.playscape_report_StartNewFlow(type));
			#endif
			return new FlowInstance(type, id);
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
            IDictionary<string, double> details)
        {
            
			#if !UNITY_EDITOR
			var keysAndValues = details.ToKeysAndValues();
        	NativeReport.playscape_report_ReportFlowStep(
				flow.Id,
				stepName,
				stepStatus,
				details.Count,
				keysAndValues.Keys,
                keysAndValues.Values);
#endif
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
            
#if !UNITY_EDITOR
			var keysAndValues = additionalParams.ToKeysAndValues();
			NativeReport.playscape_report_ReportLevelStarted(
				level,
				additionalParams.Count,
				keysAndValues.Keys,
                keysAndValues.Values);
#endif
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
            
#if !UNITY_EDITOR
			var keysAndValues = additionalParams.ToKeysAndValues();
			NativeReport.playscape_report_ReportLevelCompleted(
				level,
		        additionalParams.Count,
		        keysAndValues.Keys,
                keysAndValues.Values);
#endif
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
            
#if !UNITY_EDITOR
			var keysAndValues = additionalParams.ToKeysAndValues();
			NativeReport.playscape_report_ReportLevelFailed(
				level,
				additionalParams.Count,
				keysAndValues.Keys,
                keysAndValues.Values);
#endif
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
            
#if !UNITY_EDITOR
			var keysAndValues = additionalParams.ToKeysAndValues();
			NativeReport.playscape_report_ReportAchievementUnlocked(
				achievement,
				additionalParams.Count,
				keysAndValues.Keys,
                keysAndValues.Values);
#endif
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
            
#if !UNITY_EDITOR
			var keysAndValues = additionalParams.ToKeysAndValues();
			NativeReport.playscape_report_ReportItemUnlocked(
				itemId,
				additionalParams.Count,
				keysAndValues.Keys,
                keysAndValues.Values);
#endif
        }

        /// <summary>
        /// Row 21 in Excel
        /// </summary>
        public void ReportRatingDialogShow()
        {
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportRatingDialogShow();
#endif
        }

        /// <summary>
        /// Row 22 in Excel
        /// </summary>
        public void ReportRatingDialogYes()
        {
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportRatingDialogYes();
#endif
        }

        /// <summary>
        /// Row 23 in Excel
        /// </summary>
        public void ReportRatingDialogNo()
        {
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportRatingDialogNo();
#endif
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
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportSubscriptionState((int) state);
#endif
        }

		/// <summary>
		/// Wallet operations
		/// </summary>
		public enum WalletOperation {
			Deposit,
			Withdraw
		}

		/// <summary>
		/// Wallet results
		/// </summary>
		public enum WalletResult {
			Success,
			Failed,
			Cancel
		}

		/// <summary>
		/// Reports of  a wallet related operation.
		/// </summary>
		/// <param name="operation">Operation.</param>
		/// <param name="dealType">Deal type.</param>
		/// <param name="transactionID">Transaction Id.</param>
		/// <param name="amount">Amount.</param>
		/// <param name="currency">Currency.</param>
		/// <param name="source">Source.</param>
		/// <param name="flow">Flow.</param>
		/// <param name="step">Step.</param>
		/// <param name="item">Item.</param>
		/// <param name="result">Result.</param>
		/// <param name="reason">Reason.</param>
		public void ReportWalletOperation(WalletOperation operation,
		                      string dealType,
		                      string transactionID,
		                      double amount,
		                      string currency,
		                      string source, 
		                      string flow,
		                      string step,
		                      string item,
		                      WalletResult result,
		                      string reason) {
			#if !UNITY_EDITOR
			NativeReport.playscape_report_ReportWalletOperation ((int)operation, dealType, transactionID, amount,
			                                                    currency, source, flow, step, item,
			                                                    (int)result, reason);
			#endif
		}

        #endregion
	}
}

