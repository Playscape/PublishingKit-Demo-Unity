using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Facebook.MiniJSON;
using System;

public class InviteController : MonoBehaviour {
	private static InviteController sInstance;
	private IList<FacebookFriend> mFriends = new List<FacebookFriend>();
	private readonly Rect PlayerRect = new Rect(0, 0, Screen.width - 50, 128);
	private Vector2 mScrollPosition = Vector2.zero;
	private const string FRIENDS_GRAPH_QUERY = "me?fields=id,friends.limit(5000).fields(first_name,last_name,picture.type(square).width(120).height(120),devices,scores.limit(5000).fields(score,application),installed),scores.limit(5000).fields(score,application),first_name,last_name,picture.type(square),installed,devices";
	private Dictionary<string, Texture>  mFriendIdToFriendImage = new Dictionary<string, Texture>();

	private class FacebookFriend {
		public string Name { get; private set;}
		public string FacebookId { get; private set; }

		public FacebookFriend (string facebookId, string name) {
			Name = name;
			FacebookId = facebookId;
		}
	}

	void Awake() {

		// Singleton object
		if (sInstance == this && sInstance != null) {
			Destroy(gameObject);
		}
		DontDestroyOnLoad(gameObject);

		sInstance = this;
		enabled = false;
		FB.Init(SetInit, OnHideUnity);

		if (Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer) {
			// Stub data for testing

			mFriends.Add(new FacebookFriend("123", "Alice"));
			mFriends.Add(new FacebookFriend("123", "Bob"));
			mFriends.Add(new FacebookFriend("123", "John"));
			mFriends.Add(new FacebookFriend("123", "Lane"));
			mFriends.Add(new FacebookFriend("123", "Darth"));
			mFriends.Add(new FacebookFriend("123", "Leonard"));
			mFriends.Add(new FacebookFriend("123", "Sheldon"));
			mFriends.Add(new FacebookFriend("123", "Penny"));
			mFriends.Add(new FacebookFriend("123", "Spock"));
			mFriends.Add(new FacebookFriend("123", "George"));
		}
	}

	#region FB.Init Callbacks
	void SetInit ()
	{
		enabled = true;
		if (FB.IsLoggedIn) {
			OnLoggedIn();
			Debug.Log("w00t facebook logged in!");
		} else {
			Debug.Log("w00t facebook logged NOT in!");
		}
	}

	void OnHideUnity(bool isGameShown) {
	}
	#endregion

	void OnGUI() {
		if (!FB.IsLoggedIn) {
			if (GUI.Button(new Rect (15, Screen.height * 0.05f + 30, Screen.width * 0.20f, Screen.height * 0.08f), "Facebook Login")) {
				facebookLogin();
			}
		}

		GUI.skin.textField.fontSize = (int)(Screen.height * 0.08f);

		float scrollViewHeight = (mFriends.Count + 1) * PlayerRect.height * 1.2f + Screen.height * 0.05f;
		mScrollPosition = GUI.BeginScrollView(new Rect(25, 50, Screen.width - 50 , Screen.height /1.2f), mScrollPosition, new Rect(0, 0, PlayerRect.width * 0.85f, scrollViewHeight));
	
		float marginTop = 15;
		for (int i = 0; i < mFriends.Count; ++i) {
			Rect groupRect = PlayerRect;
			Rect buttonRect = PlayerRect;

			buttonRect.width = Screen.width * 0.75f;

			groupRect.y = i * groupRect.height + i * marginTop;
			GUI.BeginGroup(groupRect);
				
				if (GUI.Button(buttonRect, "Invite: " + mFriends[i].Name)) {
					inviteFriend(mFriends[i]);
				}

			GUI.EndGroup();
			
			Texture friendImage = null;
			if (mFriendIdToFriendImage.TryGetValue(mFriends[i].FacebookId, out friendImage)) {
				Rect textureRect = buttonRect;
				textureRect.width = 128;
				textureRect.height = 128;
				textureRect.x =  buttonRect.width;
				textureRect.y = groupRect.y;
				GUI.DrawTexture(textureRect, friendImage);
			}
		}
		GUI.EndScrollView();
	}

	void OnLoggedIn ()
	{
		retrieveFriends();
	}

	private void retrieveFriends ()
	{
		if (mFriends.Count == 0) {
			FB.API(FRIENDS_GRAPH_QUERY, Facebook.HttpMethod.GET, delegate(FBResult result) {
				DeserializeJSONFriends(result.Text);
				DownloadImages();
			});
		}
	}

	void DownloadImages ()
	{
		Debug.Log("Downloading facebook images for " + mFriends.Count + " friends");
		foreach (var friend in mFriends) {
			Debug.Log ("Download facebook image for friend: " + friend.FacebookId);
			FB.API(GetPictureURL(friend.FacebookId, 128, 128), Facebook.HttpMethod.GET, CreateDownloadClosure(friend));
		}
	}

	private Facebook.FacebookDelegate CreateDownloadClosure(FacebookFriend friend) {
		return delegate(FBResult pictureResult) {
			if (pictureResult.Error != null)
				{
					FbDebug.Error(pictureResult.Error);
				}
				else
				{
					mFriendIdToFriendImage.Add(friend.FacebookId, pictureResult.Texture);
					Debug.Log ("Downloaded: " + mFriendIdToFriendImage.Count);
				}
		};
	}

	private void inviteFriend (FacebookFriend facebookFriend)
	{
		FB.AppRequest("Let's play Roll A Ball multiplayer", new string[] {facebookFriend.FacebookId}, "", null, null, "", "Invite", 
			delegate(FBResult result) {
			Debug.Log ("Invite finished");
		});
	}	

	private void facebookLogin() {
		FB.Login("", delegate(FBResult result) {
			Debug.Log (result.Error);
			OnLoggedIn();
		});
	}
		
	#region Facebook Utils
	private void DeserializeJSONFriends(string response)
	{
		mFriends.Clear();
		
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
