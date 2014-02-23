using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using Playscape.Internal;

public class PushNotificationsIOS : Playscape.Internal.PushWooshCommon {
	#if UNITY_IPHONE && !UNITY_EDITOR
	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public void registerForRemoteNotifications();

	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public void setListenerName(string listenerName);

	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public System.IntPtr _getPushToken();
	
	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public void setIntTag(string tagName, int tagValue);

	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public void setStringTag(string tagName, string tagValue);

	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public void startLocationTracking();

	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public void stopLocationTracking();

	// Use this for initialization
	void Start () {
		registerForRemoteNotifications();
		setListenerName(this.gameObject.name);
		L.D(getPushToken());
	}

	
	static public string getPushToken()
	{
		return Marshal.PtrToStringAnsi(_getPushToken());
	}

	#endif

	
	#region implemented abstract members of PushWooshCommon
	
	protected override void SetTagImpl (string name, string value)
	{
		#if UNITY_IPHONE && !UNITY_EDITOR
		setStringTag(name, value);
		#endif
	}
	
	protected override void SetTagImpl (string name, int value)
	{
		#if UNITY_IPHONE && !UNITY_EDITOR
		setIntTag(name, value);
		#endif
	}
	#endregion
}
