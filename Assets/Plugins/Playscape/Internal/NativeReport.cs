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
	public static NativeReport.CPair<A, B>[] ToCType<A, B>(this IDictionary<A, B> dict)
	{
		NativeReport.CPair<A, B>[] result = new NativeReport.CPair<A, B>[dict.Count];
		int i = 0;
		foreach (var item in dict) {
			
			result[i] = new NativeReport.CPair<A, B>();
			result[i].first = item.Key;
			result[i].second = item.Value;
			
			i++;
		}
		return result;
	}
	
	public static NativeReport.CPurchaseItem ToCType(this PurchaseItem item)
	{
		return new NativeReport.CPurchaseItem(item);
	}
	
	private class MPAnalyticsProviderAdapter {
		public MPAnalyticsProviderAdapter(MPAnalyticsProvider provider)
		{
			mProvider = provider;
		}
		
		public int GetCurrentNetworkTime()
		{
			return mProvider.CurrentNetworkTime;
		}
		
		private MPAnalyticsProvider mProvider;
	}
	
	public static NativeReport.GetCurrentNetworkTimeCallback ToCType(this MPAnalyticsProvider provider)
	{
		return new MPAnalyticsProviderAdapter(provider).GetCurrentNetworkTime;
	}
	
	private class SocialAnalyticsProviderAdapter {
		public SocialAnalyticsProviderAdapter(SocialAnalyticsProvider provider)
		{
			mProvider = provider;
		}
		
		public int GetCurrentNetwork()
		{
			SocialNetwork? network = mProvider.CurrentNetwork;
			
			if (network.HasValue) {
				return (int) network;
			} else {
				return 0; // NoSocialNetwork
			}
		}
		
		private SocialAnalyticsProvider mProvider;
	}
	
	public static NativeReport.GetCurrentSocialNetworkCallback ToCType(this SocialAnalyticsProvider provider)
	{
		return new SocialAnalyticsProviderAdapter(provider).GetCurrentNetwork;
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
		RegisterSetGameSessionId(setGameSessionId);
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
	
	[StructLayout(LayoutKind.Sequential)]
	public struct CPair<A, B>
	{
		public A first;
		public B second;
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
		CPair<string, string>[] gameParameters);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportMPJoinPublicGame(
		string sessionId,
		string gameName,
		int maxPlayers,
		int gameParametersCount,
		CPair<string, string>[] gameParameters);
	
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
		CPair<string, int>[] stepNameToId);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_StartNewFlow(string type);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportFlowStep(
		string flowType,
		string stepName,
		string stepStatus,
		int detailsCount,
		CPair<string, double>[] details);
	
	
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportLevelStarted(
		string level,
		int additionalParamsCount,
		CPair<string, double>[] additionalParams);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportLevelCompleted(
		string level,
		int additionalParamsCount,
		CPair<string, double>[] additionalParams);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportLevelFailed(
		string level,
		int additionalParamsCount,
		CPair<string, double>[] additionalParams);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportAchievementUnlocked(
		string achievement,
		int additionalParamsCount,
		CPair<string, double>[] additionalParams);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportItemUnlocked(
		int itemId,
		int additionalParamsCount,
		CPair<string, double>[] additionalParams);
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportRatingDialogShow();
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportRatingDialogYes();
	
	[DllImport(SHARED_LIBRARY)]
	public static extern void playscape_report_ReportRatingDialogNo();
	
	
	
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
	
	private static void setGameSessionId(string text) {
		RemoteLogger.SetGameSessionId(text);
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
