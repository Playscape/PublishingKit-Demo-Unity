using UnityEngine;
using System.Collections;

public class InviteUI : MonoBehaviour {
	private SocialController mController;
	private readonly Rect PlayerRect = new Rect(0, 0, Screen.width - 50, 128);
	private Vector2 mScrollPosition = Vector2.zero;

	// Use this for initialization
	void Start () {
		mController = SocialController.Instance;
	}
	

	void OnGUI() {
		if (!mController.IsLoggedIn) {
			if (GUI.Button(new Rect (15, Screen.height * 0.05f + 30, Screen.width * 0.20f, Screen.height * 0.08f), "Facebook Login")) {
				mController.FacebookLogin();
			}
		}
		
		GUI.skin.textField.fontSize = (int)(Screen.height * 0.08f);
		
		float scrollViewHeight = (mController.Friends.Count + 1) * PlayerRect.height * 1.2f + Screen.height * 0.05f;
		mScrollPosition = GUI.BeginScrollView(new Rect(25, 50, Screen.width - 50 , Screen.height /1.2f), mScrollPosition, new Rect(0, 0, PlayerRect.width * 0.85f, scrollViewHeight));
		
		float marginTop = 15;
		for (int i = 0; i < mController.Friends.Count; ++i) {
			Rect groupRect = PlayerRect;
			Rect buttonRect = PlayerRect;
			
			buttonRect.width = Screen.width * 0.75f;
			
			groupRect.y = i * groupRect.height + i * marginTop;
			GUI.BeginGroup(groupRect);
			
			if (GUI.Button(buttonRect, "Invite: " + mController.Friends[i].Name)) {

				mController.InviteFriend(mController.Friends[i]);
			}
			
			GUI.EndGroup();
			
			Texture friendImage = null;
			if (mController.FriendIdToFriendImage.TryGetValue(mController.Friends[i].FacebookId, out friendImage)) {
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



	// Update is called once per frame
	void Update () {
	
	}
}
