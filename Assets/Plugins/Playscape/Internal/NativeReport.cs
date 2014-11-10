using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Playscape;
using Playscape.Internal;
using Playscape.Purchase;
using Playscape.Analytics;

public static class CInterop
{
	// Solution taken from here: http://answers.unity3d.com/questions/191234/unity-ios-function-pointers.html
	public class MonoPInvokeCallbackAttribute : System.Attribute {
		private Type type;
		public MonoPInvokeCallbackAttribute(Type t ) {  type = t; }
	}

	public class KeysAndValues<K, V>
{
		public KeysAndValues(IDictionary<K, V> dict)
	{
			Keys = new K[dict.Count];
			Values = new V[dict.Count];
			int counter = 0;
			
			foreach (var item in dict)
			{
				Keys[counter] = item.Key;
				Values[counter] = item.Value;
			
				counter++;
		}
		}

		public K[] Keys { get; private set; }
		public V[] Values { get; private set; }
	}
	
	public static NativeReport.CPurchaseItem ToCType(this PurchaseItem item)
	{
		return new NativeReport.CPurchaseItem(item);
	}
	
	public static KeysAndValues<K, V> ToKeysAndValues<K, V>(this IDictionary<K, V> dict) {
		if (dict != null) {
					return new KeysAndValues<K, V> (dict);
		}


		return null;
	}

	[MonoPInvokeCallback(typeof(NativeReport.GetCurrentNetworkTimeCallback))]
	public static int GetCurrentNetworkTime()
	{
		return mMpProvider.CurrentNetworkTime;
	}

	private static MPAnalyticsProvider mMpProvider;
	public static void SetMPAnalyticsProvider(MPAnalyticsProvider provider)
	{
		mMpProvider = provider;
	}
	

	private static SocialAnalyticsProvider mSocialProvider;

	[MonoPInvokeCallback(typeof(NativeReport.GetCurrentSocialNetworkCallback))]
	public static int GetCurrentNetwork()
	{
		SocialNetwork? network = mSocialProvider.CurrentNetwork;
		
		if (network.HasValue) {
			return (int) network;
		} else {
			return 0; // NoSocialNetwork
		}
	}

	public static void SetSocialAnalyticsProvider(SocialAnalyticsProvider provider)
	{
		mSocialProvider = provider;
	}
}

public class NativeReport : MonoBehaviour {
	
    #if UNITY_IPHONE
    private const string SHARED_LIBRARY = "__Internal";
    #else
    private const string SHARED_LIBRARY = "playscape_pubkit";
    #endif
    
	public static void Init() {
		#if UNITY_ANDROID && !UNITY_EDITOR
		RegisterGetConnectivityReport(getConnectivityReport);
		RegisterGenerateGuid(generateGuid);
		RegisterGetCurrentTimeMillis(getCurrentTimeMillis);
		RegisterReport(report);
		RegisterSetNetSessionId(setNetSessionId);
		RegisterSetLevelSessionId(setLevelSessionId);
		RegisterSetGameAuxVars(setGameAuxVars);
		RegisterSetNetworkTime(setNetworkTime);
		#endif
	}
	 
	///
	
	public delegate int GetCurrentNetworkTimeCallback();
	public delegate int GetCurrentSocialNetworkCallback();
	
	[StructLayout(LayoutKind.Sequential)]
	public struct CPurchaseItem
	{
		public CPurchaseItem(PurchaseItem purchaseItem)
		{
			name = purchaseItem.Name;
		}
		
		string name;
	}
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_InitMultiplayer(GetCurrentNetworkTimeCallback callback);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_InitSocial(GetCurrentSocialNetworkCallback callback);
	
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportActivation(string referrer);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportReferrer(string referrer);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportAppsFlyerReferrer(string referrer);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportLaunch(int launchCount);
	
	
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportNotificationDisplayed(
		string promotedGame,
		string icon,
		string action,
		string notificationId);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportNotificationClicked(
		string promotedGame,
		string icon,
		string action,
		string notificationId);
	
	
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportInterstitialLoadFailed(string location);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportInterstitialDismissed(string location);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportInterstitialClicked(string location);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportInterstitialShown(string location);
	
	
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportEvent(string customEvent);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_setCustomVariable(string key, string value);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_removeCustomVariable(string key);
    
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_getCustomVariable(string key, int outLength, StringBuilder outValue);

    [DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_clearCustomVariables();	
	
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportPurchaseStarted(CPurchaseItem item);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportPurchaseSuccess(
		CPurchaseItem item,
		double amount,
		string currency,
		long currencyTimestamp,
		string transactionId);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportPurchaseCancelled(CPurchaseItem item);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportPurchaseFailed(
		CPurchaseItem item,
		string failureReason);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportPurchaseAlreadyPurchased(CPurchaseItem item);
	
	
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportSocialLoginSuccess(
		bool isSilentLogin,
		string whichUserLoggedIn);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportSocialLoginFailed(bool isSilentLogin);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportSocialLoginCancelled(bool isSilentLogin);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportSocialShareNoDialog();
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportSocialShareDialog();
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportSocialShareCancelled();
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportSocialFriendsLoaded(int friendsCount);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportSocialFriendsLoadFailed();
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportSocialSubmitScore(long score);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportSocialSubmitScoreFailed();
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportSocialRequestSent(
		string whichRequest,
		string whichTargetUserId,
		long uniqueRequestId);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportSocialRequestFailed(
		string whichRequest,
		string failureReason);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportSocialGetImagesSuccess(int numImages);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportSocialGetImagesFailed(int numImages);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportSocialRequestsFound(int requestsCount);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportSocialRequestDetails(
		string whichRequest,
		string whoFromUserId,
		long uniqueId);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportLeaderboardOpened(string whichLeaderboard);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportSocialLogout();
	
	
	
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportMPServerConnect(string serverName);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportMPServerConnectSuccess(
		string serverName,
		int latencyInMilliseconds);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportMPServerConnectFailed(string failureReason);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportMPServerDisconnect();
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportMPServerError(string errorReason);
	
	
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportMPLoadOnlineFriends();
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportMPLoadOnlineFriendsSuccess(int friendsCount);
	
	
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportMPCreatePublicGame(
		string sessionId,
		int maxPlayers,
		int gameParametersCount,
		string[] gameParametersKeys,
		string[] gameParametersValues);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportMPJoinPublicGame(
		string sessionId,
		string gameName,
		int maxPlayers,
		int gameParametersCount,
		string[] gameParametersKeys,
		string[] gameParametersValues);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportMPJoinPublicGameFailure(string failureReason);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportMPJoinedPublicGame(
		string sessionId,
		string gameName,
		int playerId);
	
	
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportMPCreatePrivateGame(
		string sessionId,
		string gameName,
		int maxPlayers);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportMPJoinPrivateGame(
		string gameName,
		int friendIdsCount,
		string[] friendIds);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportMPJoinPrivateGameFailure(
		string gameName,
		string failureReason);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportMPJoinedPrivateGame(
		string sessionId,
		string gameName,
		int playerId);
	
	
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportMPStartGame(int numberOfPlayers);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportMPLeaveGame(string gameName);
	
	
	
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_RegisterFlow(
		string type,
		int stepNameToIdCount,
		string[] stepNameToIdKeys,
		int[] stepNameToIdValues);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern IntPtr playscape_report_StartNewFlow(string type);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportFlowStep(
		string flowId,
		string stepName,
		string stepStatus,
		int detailsCount,
		string[] detailsKeys,
		double[] detailsValues);
	
	
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportLevelStarted(
		string level,
		int additionalParamsCount,
		string[] additionalParamsKeys,
		double[] additionalParamsValues);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportLevelCompleted(
		string level,
		int additionalParamsCount,
		string[] additionalParamsKeys,
		double[] additionalParamsValues);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportLevelFailed(
		string level,
		int additionalParamsCount,
		string[] additionalParamsKeys,
		double[] additionalParamsValues);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportAchievementUnlocked(
		string achievement,
		int additionalParamsCount,
		string[] additionalParamsKeys,
		double[] additionalParamsValues);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportItemUnlocked(
		int itemId,
		int additionalParamsCount,
		string[] additionalParamsKeys,
		double[] additionalParamsValues);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportRatingDialogShow();
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportRatingDialogYes();
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportRatingDialogNo();

	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportWalletOperation(int operation,
	                                                            string dealType,
	                                                            string transactionID,
	                                                            double amount,
	                                                            string currency,
	                                                            string source, 
	                                                            string flow,
	                                                            string step,
	                                                            string item,
	                                                            int result,
	                                                            string reason);


	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportSubscriptionState(int state);

	#if UNITY_ANDROID
	private static string getConnectivityReport() {
		return PlayscapeUtilities.GetConnectivityAnalyticsReport();
	}
	
	private static string generateGuid() {
		return Guid.NewGuid().ToString();
	}
	
	private static long getCurrentTimeMillis() {
		return Utils.CurrentTimeMillis;
	}
	
	private static void report(string text) {
		RemoteLogger.Log(RemoteLogger.LogLevel.Info, "RemoteLogger-Unity", text);
	}
	
	private static void setNetSessionId(string text) {
		RemoteLogger.SetNetSessionId(text);
	}
	
	private static void setLevelSessionId(string text) {
		RemoteLogger.SetLevelSessionId(text);
	}
	
	private static void setGameAuxVars(string text) {
		RemoteLogger.SetGameAuxVars(text);
	}
	
	private static void setNetworkTime(int time) {
		RemoteLogger.SetNetworkTime(time);
	}
	
	private delegate string ReturnStringCallback();
	private delegate long ReturnLongCallback();
	private delegate void StringCallback(string value);
	private delegate void IntCallback(int value);

	[DllImport(SHARED_LIBRARY, EntryPoint="playscape_report_register_getConnectivityReport")]
	private static extern void RegisterGetConnectivityReport(ReturnStringCallback callback);
	
	[DllImport(SHARED_LIBRARY, EntryPoint="playscape_report_register_generateGuid")]
	private static extern void RegisterGenerateGuid(ReturnStringCallback callback);
	
	[DllImport(SHARED_LIBRARY, EntryPoint="playscape_report_register_getCurrentTimeMillis")]
	private static extern void RegisterGetCurrentTimeMillis(ReturnLongCallback callback);
	
	[DllImport(SHARED_LIBRARY, EntryPoint="playscape_report_register_report")]
	private static extern void RegisterReport(StringCallback callback);
	
	[DllImport(SHARED_LIBRARY, EntryPoint="playscape_report_register_setGameSessionId")]
	private static extern void RegisterSetGameSessionId(StringCallback callback);
	
	[DllImport(SHARED_LIBRARY, EntryPoint="playscape_report_register_setNetSessionId")]
	private static extern void RegisterSetNetSessionId(StringCallback callback);
	
	[DllImport(SHARED_LIBRARY, EntryPoint="playscape_report_register_setLevelSessionId")]
	private static extern void RegisterSetLevelSessionId(StringCallback callback);
	
	[DllImport(SHARED_LIBRARY, EntryPoint="playscape_report_register_setGameAuxVars")]
	private static extern void RegisterSetGameAuxVars(StringCallback callback);
	
	[DllImport(SHARED_LIBRARY, EntryPoint="playscape_report_register_setNetworkTime")]
	private static extern void RegisterSetNetworkTime(IntCallback callback);
	#endif
}
