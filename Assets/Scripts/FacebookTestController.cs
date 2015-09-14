using UnityEngine;
using System.Collections;
using Playscape.Internal;

public class FacebookTestController : MonoBehaviour {

	const string MENU_TITLE = "Facebook";
	const string LOGIN = "LOGIN";
	const string LOGOUT = "LOGOUT";
	const string SHARE = "SHARE";

	// Use this for initialization
	void Start () {
	
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
	}

	void Share () {
		FB.Feed(                                                                                                                 
		        linkCaption: "I just smashed 1000 friends! Can you beat it?",               
		        picture: "http://www.friendsmash.com/images/logo_large.jpg",                                                     
		        linkName: "Checkout my Friend Smash greatness!",                                                                 
		        link: "http://apps.facebook.com/" + FB.AppId + "/?challenge_brag=" + (FB.IsLoggedIn ? FB.UserId : "guest")       
		        ); 
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
