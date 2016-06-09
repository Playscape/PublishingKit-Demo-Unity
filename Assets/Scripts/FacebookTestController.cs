using UnityEngine;
using System.Collections;
using Playscape.Internal;
using Facebook;
using System.Collections.Generic;

public class FacebookTestController : MonoBehaviour {

	const string MENU_TITLE = "Facebook";
	const string LOGIN = "LOGIN";
	const string LOGOUT = "LOGOUT";
	const string SHARE = "SHARE";
	const string APP_REQUEST = "SEND REQUEST";
	const string SEND_GO_REQUEST = "SEND GRAPH OBJECT REQUEST";
	const string GET_APP_REQUEST = "GET APP REQUEST";

	private bool fbInitialized = false;

	// Use this for initialization
	void Start () {
		if (FB.IsInitialized && FB.IsLoggedIn) {
			fbInitialized = true;
		} else {
			FB.Init (OnInitComplete, null, null); 
		}
	}

	void OnInitComplete() {
		fbInitialized = true;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI () {
		float boxWidth = Screen.width / 1.5f;
		float boxHeight = Screen.height * 0.95f;
		
		GUI.skin.button.fontSize = (int)(Screen.height * 0.044f);
		GUI.skin.box.fontSize  = (int)(Screen.height * 0.07);
		
		GUI.Box (new Rect (Screen.width / 2 - boxWidth / 2, 10, boxWidth, boxHeight), MENU_TITLE);
		
		float buttonWidth = boxWidth * 0.85f;
		float buttonHeight = boxHeight / 14;
		float marginTop = 80;
		

		if (!FB.IsLoggedIn)                                                                                              
		{                                                                                                                
			if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, marginTop, buttonWidth, buttonHeight), LOGIN)) {                                                                                                            
				FB.Login("email,publish_actions", LoginCallback);                                                        
			}                                                                                                            
		}    

		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight + marginTop, buttonWidth, buttonHeight), LOGOUT)) {
			if(FB.IsLoggedIn) {
				FB.Logout();
			}
		}
		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 2 + marginTop, buttonWidth, buttonHeight), SHARE)) {
			Share();
		}

		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 3 + marginTop, buttonWidth, buttonHeight), APP_REQUEST)) {
			AppRequest();
		}

		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 4 + marginTop, buttonWidth, buttonHeight), SEND_GO_REQUEST)) {
			SendGraphObject();
		}

		if (GUI.Button (new Rect (Screen.width / 2 - buttonWidth / 2, buttonHeight * 5 + marginTop, buttonWidth, buttonHeight), GET_APP_REQUEST)) {
			GetAppRequest();
		}
	}

	void Share () {
		if (!fbInitialized) {
			L.I ("Facebook didn't init");
			return;
		}
		FB.Feed(                                                                                                                 
		        linkCaption: "I just smashed 1000 friends! Can you beat it?",               
		        picture: "http://www.friendsmash.com/images/logo_large.jpg",                                                     
		        linkName: "Checkout my Friend Smash greatness!",                                                                 
		        link: "http://apps.facebook.com/" + FB.AppId + "/?challenge_brag=" + (FB.IsLoggedIn ? FB.UserId : "guest")       
		        ); 
	}

	void AppRequest() {
		if (!fbInitialized) {
			L.I ("Facebook didn't init");
			return;
		}
//		FB.AppRequest ("I'd like to invite you in this awesome app!", new string[] {"449443455241537", "971381496268038"}, null, null, 50, "", "Just title for test invite", null);
		FB.AppRequest ("I'd like to invite you in this awesome app!", 
			new string[] {}, 
			null, null, 50, "", "Just title for test invite", null);
	}

	void GetAppRequest() {
		if (!fbInitialized) {
			L.I ("Facebook didn't init");
			return;
		}
		SocialController socialController = GameObject.Find("menu").GetComponent<SocialController> ();
		if (socialController != null) {
			socialController.CheckForIncomingRequests ();
		}
	}

	void SendGraphObject() {
		if (!fbInitialized) {
			L.I ("Facebook didn't init");
			return;
		}
		
		FB.AppRequest ("Here, take this life!", // A message for the user
				OGActionType.Send, // Can be .Send or .AskFor depending on what you want to do with the object.
				"804653492990357", // Here we put the object id we got as a result before.		             
				new string[]{ }, // The id of the sender.
				"", // Here you can put in any data you want
				"Send a life to your friend", // A title
				AppRequestCallback);
	}

	void AppRequestCallback(FBResult result)                                                        
	{                                                                                          
		L.I("AppRequestCallback: " + result.ToString());                                                                                                                                        
	}  

	void LoginCallback(FBResult result)                                                        
	{                                                                                          
		L.I("LoginCallback");                                                          
		
		if (FB.IsLoggedIn)                                                                     
		{                                                                                      
			OnLoggedIn();                                                                      
		}                                                                                      
	}                                                                                          
	
	void OnLoggedIn()                                                                          
	{                                                                                          
		L.I("Logged in. ID: " + FB.UserId);                                            
	}       
}
