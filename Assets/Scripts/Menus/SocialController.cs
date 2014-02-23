﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Facebook.MiniJSON;
using System;
using Playscape.Analytics;

public class SocialController : MonoBehaviour {
	
	private const string FRIENDS_GRAPH_QUERY = "me?fields=id,friends.limit(5000).fields(first_name,last_name,picture.type(square).width(120).height(120),devices,scores.limit(5000).fields(score,application),installed),scores.limit(5000).fields(score,application),first_name,last_name,picture.type(square),installed,devices";
	private const string REQUEST_TYPE_INVITE = "INVITE";

	#region Private Members
	private IList<FacebookFriend> mFriends = new List<FacebookFriend>();
	private Dictionary<string, Texture>  mFriendIdToFriendImage = new Dictionary<string, Texture>();
	private static bool mInitialized = false;
	
	private int mNumImagesSucceded;
	private int mNumImagesFailed;
	private bool mDownloadedFriends;

	private bool mShouldEnterLobbyAfterFriendsLoaded;
	private string mOpponentFacebookId;

	/// <summary>
	/// Occurs when on social request received.
	/// </summary>
	public event Action OnSocialRequestReceived;
	#endregion

	#region Properties
	public Dictionary<string, Texture> FriendIdToFriendImage { get { return mFriendIdToFriendImage; } }
	public IList<FacebookFriend> Friends { get { return mFriends; } }
	public FacebookFriend MyFacebookUser {get; private set;}
	public FacebookFriend OpponentFacebookUser {get; private set;}

	private static SocialController mInstance;

	public static SocialController Instance {
		get {
			if (mInstance == null) {
				var go = GameObject.Find("SocialController");
				mInstance = go.GetComponent<SocialController>();
			}

			return mInstance;
		}
	}

	public bool IsLoggedIn {
		get { return FB.IsLoggedIn; }
	}

	#endregion
	
	private class FacebookAnalyticsProvider : SocialAnalyticsProvider {
		#region SocialAnalyticsProvider implementation

		public SocialNetwork? CurrentNetwork {
			get {
				return SocialNetwork.Facebook;
			}
		}

		#endregion
	}

	public class FacebookFriend {
		public string Name { get; private set;}
		public string FacebookId { get; private set; }

		public FacebookFriend (string facebookId, string name) {
			Name = name;
			FacebookId = facebookId;
		}
	}

	void Awake() {
		// Singleton object
		if (mInitialized) {
			Destroy(gameObject);
		}
		mInitialized = true;
		DontDestroyOnLoad(gameObject);

		Report.Instance.InitSocial(new FacebookAnalyticsProvider());


		if (Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer) {
			// Stub data for testing

			mFriends.Add(new FacebookFriend("123", "Alice"));
			mFriends.Add(new FacebookFriend("223", "Bob"));
			mFriends.Add(new FacebookFriend("323", "John"));
			mFriends.Add(new FacebookFriend("423", "Lane"));
			mFriends.Add(new FacebookFriend("523", "Darth"));
			mFriends.Add(new FacebookFriend("623", "Leonard"));
			mFriends.Add(new FacebookFriend("723", "Sheldon"));
			mFriends.Add(new FacebookFriend("823", "Penny"));
			mFriends.Add(new FacebookFriend("923", "Spock"));
			mFriends.Add(new FacebookFriend("121", "George"));

			mDownloadedFriends = true;
		}

#if UNITY_ANDROID || UNITY_IPHONE || UNITY_EDITOR
		enabled = false;
		FB.Init(SetInit, OnHideUnity);
#endif
		// =TEST-Invite
		//ReceiveDummyInvite();
	}

	public void ReceiveDummyInvite() {
		FBResult fbRes = new FBResult("{\"id\":\"456\", \"from\":{\"id\":\"123\"}, \"data\":{\"uniqueRequestId\":\"123456\"}}", null);
		IncomingRequestCallback(fbRes);
	}

	#region FB.Init Callbacks
	void SetInit ()
	{
		enabled = true;
		if (FB.IsLoggedIn) {
			// Silent success
			Report.Instance.ReportSocialLoginSuccess(true, FB.UserId);

			OnLoggedIn();
			Debug.Log("Facebook logged in!");
		} else {
			Debug.Log("Facebook logged NOT in!");
		}
	}

	void OnHideUnity(bool isGameShown) {
	}
	#endregion


	/// <summary>
	/// Always triggered after login or after previosly logged in and the session has been restored
	/// </summary>
	void OnLoggedIn ()
	{
		MyFacebookUser = new FacebookFriend(FB.UserId, "Me");
		retrieveFriends();
	}

	private void retrieveFriends ()
	{
		if (mFriends.Count == 0) {
			FB.API(FRIENDS_GRAPH_QUERY, Facebook.HttpMethod.GET, delegate(FBResult result) {

				DeserializeJSONFriends(result.Text);
				CheckForIncomingRequests();
				DownloadImages();
			});
		}
	}

	void CheckForIncomingRequests ()
	{
		FB.GetDeepLink(delegate (FBResult data) {
			if(!string.IsNullOrEmpty(data.Text)) {
				Uri uri = null;

				try {
				uri = new Uri(data.Text);
				} catch (UriFormatException e) {
					Debug.LogException(e);
				}
				if (uri != null) {
					if (!IsLoggedIn) {
						FacebookLogin(CheckForIncomingRequests);
						return;
					}
					if (OnSocialRequestReceived != null) {
						OnSocialRequestReceived();
					}
					var queryString = uri.Query;
					if (queryString.IndexOf("request_ids") != -1) {
						var keysAndValues = queryString.Split('&');
						foreach (var keyValue in keysAndValues) {
							var keyValueParts = keyValue.Split('=');
							if (keyValueParts.Length == 2) {
								var key = keyValueParts[0];
								var value = keyValueParts[1];

								if (key == "request_ids") {

									var requestIds = Uri.UnescapeDataString(value).Split(',');

									// First request will do
									HandleInvite(requestIds[0]);
								}
							}
						}
					}
				}
			}
		});
	}

	void HandleInvite (string requestId)
	{
		Debug.Log ("Requesting request with id: " + requestId);
		FB.API(requestId, Facebook.HttpMethod.GET, IncomingRequestCallback);

	}

	private void DeleteRequest(string requestId) {

		Debug.Log ("Deleting request: " + requestId);
		FB.API(requestId, Facebook.HttpMethod.DELETE, delegate(FBResult result) {
			if (string.IsNullOrEmpty(result.Error)) {
				Debug.Log ("Deleted request: " + requestId);
			} else {
				Debug.LogError("Failed deleting request with id: " + requestId + " reason: " + result.Error);
			}
		});
	}

	private void IncomingRequestCallback(FBResult result) {
		Debug.Log("IncomingRequestCallback Got result: " + result.Text + " error=" + result.Error);
		string requestId = "invalid";
		if (result != null && string.IsNullOrEmpty(result.Error) && !string.IsNullOrEmpty(result.Text)) {

			var requestJson =  Json.Deserialize(result.Text) as Dictionary<string, object>;

			if (requestJson != null) {

				if (requestJson.ContainsKey("id")) {
					requestId = requestJson["id"] as string;
				}

				string fromUserId = null;
				
				if (requestJson.ContainsKey("from")) {
				
					var from = requestJson["from"] as Dictionary<string, object>;

					if (from != null) {
				
						if (from.ContainsKey("id")) {
							fromUserId = from["id"] as string;
						}
					}
				}

				if (fromUserId != null && requestJson.ContainsKey("data")) {
					var requestData = Json.Deserialize(requestJson["data"] as string) as Dictionary<string, object>;
					if (requestData != null) {

						if (requestData.ContainsKey("uniqueRequestId")) {
							long uniqueRequestId = long.Parse(requestData["uniqueRequestId"].ToString());
							Report.Instance.ReportSocialRequestDetails(REQUEST_TYPE_INVITE, fromUserId, uniqueRequestId);

#if !UNITY_EDITOR
							DeleteRequest(requestId);
#endif

							mOpponentFacebookId = fromUserId;

							lock(mFriends) {

								if (mDownloadedFriends == false) {
									Debug.Log("Friends not loaded yet, waiting for friends load...");
									mShouldEnterLobbyAfterFriendsLoaded = true;
								} else {
									Debug.Log("Entering lobby...");
									EnterLobby(false);
								}
							}
						}
					} else {
						Debug.LogWarning("uniqueRequestId is missing from data!");
					}
				} else {
					Debug.LogWarning("HandleInvite failed finding 'data' in request requestId= " + requestId);
				}
			} else {
				Debug.LogWarning("HandleInvite failed parsing request json requestId= " + requestId);
			}
		} else {
			Debug.LogWarning("HandleInvite Invalid result");
		}

	}

	private void EnterLobby (bool isHost)
	{
		Debug.Log("Entering lobby, isHost = " + isHost);

		foreach (var friend in mFriends) {
			if (friend.FacebookId == mOpponentFacebookId) {
				OpponentFacebookUser = friend;
				break;
			}
		}

		GameState.IsHost = isHost;
		GameState.CurrentGameType = GameState.GameType.MultiplayerPrivateGame;
		Application.LoadLevel("lobby");
	}

	void DownloadImages ()
	{
		Debug.Log("Downloading facebook images for " + mFriends.Count + " friends");
		int i = 0;
		foreach (var friend in mFriends) {
			i++;
			FB.API(GetPictureURL(friend.FacebookId, 128, 128), Facebook.HttpMethod.GET, CreateDownloadClosure(friend, i == mFriends.Count));
		}
	}

	private Facebook.FacebookDelegate CreateDownloadClosure(FacebookFriend friend, bool isLastTask) {
		return delegate(FBResult pictureResult) {
			if (pictureResult.Error != null)
				{
					FbDebug.Error(pictureResult.Error);
					mNumImagesFailed++;
				}
				else
				{
					mFriendIdToFriendImage.Add(friend.FacebookId, pictureResult.Texture);
					mNumImagesSucceded++;
				}

			if (isLastTask) {
				Report.Instance.ReportSocialGetImagesFailed(mNumImagesFailed);
				Report.Instance.ReportSocialGetImagesSuccess(mNumImagesSucceded);
			}
		};
	}

	public void InviteFriend (FacebookFriend facebookFriend)
	{
		var rand = new System.Random();
		long uniqueRequestId = Math.Abs(rand.Next() | (rand.Next() << 31));

		var onRequest = CreateRequestClosure(REQUEST_TYPE_INVITE, facebookFriend.FacebookId, uniqueRequestId);

		var dataJson = new Dictionary<string, string>(1);
		dataJson["uniqueRequestId"] = uniqueRequestId.ToString();

		FB.AppRequest("Hey! Let's play Roll A Ball multiplayer", 
		              new string[] {facebookFriend.FacebookId}, "", null, null, Json.Serialize(dataJson), "Invite", 
					  onRequest);
	}

	/// <summary>
	/// Creates a closure for incoming requests.
	/// </summary>
	/// <returns>The request closure.</returns>
	/// <param name="requestType">Request type.</param>
	/// <param name="toFriendId">The facebook friend id to whom the request is sent to.</param>
	/// <param name="uniqueRequestId">An identifier which uniquely identifies this request, used for analytics tracking.</param>
	private Facebook.FacebookDelegate CreateRequestClosure(string requestType, string toFriendId, long uniqueRequestId) {
		return delegate(FBResult result) {
			FbDebug.Log("appRequestCallback");

			if (result != null)
			{
				var responseObject = Json.Deserialize(result.Text) as Dictionary<string, object>;
				object obj = 0;
				if (responseObject.TryGetValue ("cancelled", out obj))
				{
					Report.Instance.ReportSocialRequestFailed(REQUEST_TYPE_INVITE, "Request cancelled");
				}
				else if (responseObject.TryGetValue ("request", out obj))
				{
					FbDebug.Log("Request sent");
					Report.Instance.ReportSocialRequestSent(requestType, toFriendId,  uniqueRequestId);

					mOpponentFacebookId = toFriendId;
					EnterLobby(true);
				}                                              
	        } else {
				Report.Instance.ReportSocialRequestFailed(requestType, "result is null");
			}
		};
	}

	public void FacebookLogin(Action actionToPerformAfterLogin = null) {

		FB.Login("", delegate(FBResult result) {
			if (!string.IsNullOrEmpty(result.Error)) {
				Debug.LogError (result.Error);

			} else {
				actionToPerformAfterLogin();
				Report.Instance.ReportSocialLoginSuccess(false, FB.UserId);
			}

			OnLoggedIn();
		});
	}
		
	#region Facebook Utils
	private void DeserializeJSONFriends(string response)
	{
		mFriends.Clear();

		try {
			var responseObject = Json.Deserialize(response) as Dictionary<string, object>;
			object friendsH;
			var friends = new List<object>();
			if (responseObject.TryGetValue("friends", out friendsH))
			{
				friends = (List<object>)(((Dictionary<string, object>)friendsH)["data"]);

				foreach (var friend in friends) {
					var fd = (Dictionary<string, object>)friend;

					var facebookId = (string)fd["id"];
					var firstName = (string)fd["first_name"];
					var lastName = (string)fd["last_name"];

					var facebookFriend = new FacebookFriend(facebookId, firstName + " " + lastName);

					mFriends.Add(facebookFriend);
				}

				Report.Instance.ReportSocialFriendsLoaded(mFriends.Count);

				lock(mFriends) {
					mDownloadedFriends = true;
					if (mShouldEnterLobbyAfterFriendsLoaded) {
						EnterLobby(false);
						mShouldEnterLobbyAfterFriendsLoaded = false;
					}
				}
			} else {
				Report.Instance.ReportSocialFriendsLoadFailed();
			}
		} catch (Exception e) {
			Debug.LogException(e);
			Report.Instance.ReportSocialFriendsLoadFailed();
		}
	}

	private static Dictionary<string, string> DeserializeJSONProfile(string response)
	{
		var responseObject = Json.Deserialize(response) as Dictionary<string, object>;
		object nameH;
		var profile = new Dictionary<string, string>();
		if (responseObject.TryGetValue("first_name", out nameH))
		{
			profile["first_name"] = (string)nameH;
		}
		return profile;
	}

	private static string GetPictureURL(string facebookID, int? width = null, int? height = null, string type = null)
	{
		string url = string.Format("/{0}/picture", facebookID);
		string query = width != null ? "&width=" + width.ToString() : "";
		query += height != null ? "&height=" + height.ToString() : "";
		query += type != null ? "&type=" + type : "";
		if (query != "") url += ("?g" + query);
		return url;
	}

	#endregion

}
