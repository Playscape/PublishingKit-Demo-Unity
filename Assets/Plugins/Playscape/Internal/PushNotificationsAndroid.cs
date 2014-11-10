using UnityEngine;
using System.Collections;

public class PushNotificationsAndroid : Playscape.Internal.PushWooshCommon {
	#if UNITY_ANDROID && !UNITY_EDITOR
	// Use this for initialization
	void Start () {
		InitPushwoosh();
	}
	
	// Update is called once per frame
	void Update () {

	}
	
	private static AndroidJavaObject pushwoosh = null;
	
	void InitPushwoosh() {
		if (pushwoosh != null) {
			return;
		}

		using(var pluginClass = new AndroidJavaClass("com.arellomobile.android.push.PushwooshProxy")) {
			pushwoosh = pluginClass.CallStatic<AndroidJavaObject>("instance");
		}
		
		pushwoosh.Call("setListenerName", this.gameObject.name);

	}
 
	public void setIntTag(string tagName, int tagValue)
	{
		pushwoosh.Call("setIntTag", tagName, tagValue);
	}

	public void unregisterDevice()
	{
		pushwoosh.Call("unregisterFromPushNotifications");
	}

	public void setStringTag(string tagName, string tagValue)
	{
		pushwoosh.Call("setStringTag", tagName, tagValue);
	}

	public void sendLocation(double lat, double lon)
	{
		pushwoosh.Call("sendLocation", lat, lon);
	}

	public void startTrackingGeoPushes()
	{
		pushwoosh.Call("startTrackingGeoPushes");
	}

	public void stopTrackingGeoPushes()
	{
		pushwoosh.Call("stopTrackingGeoPushes");
	}

	public void clearLocalNotifications()
	{
		pushwoosh.Call("clearLocalNotifications");
	}

	public void scheduleLocalNotification(string message, int seconds)
	{
		pushwoosh.Call("scheduleLocalNotification", message, seconds);
	}

	public void scheduleLocalNotification(string message, int seconds, string userdata)
	{
		pushwoosh.Call("scheduleLocalNotification", message, seconds, userdata);
	}
	
	public string getPushToken()
	{
		return pushwoosh.Call<string>("getPushToken");
	}


	void OnApplicationPause(bool paused)
	{
		if (Application.platform == RuntimePlatform.Android) {
            InitPushwoosh();
			if(paused)
			{
				pushwoosh.Call("onPause");
			}
			else
			{
				pushwoosh.Call("onResume");
			}
		}
	}

	#endif

	#region implemented abstract members of PushWooshCommon
	
	protected override void SetTagImpl (string name, string value)
	{
		#if UNITY_ANDROID && !UNITY_EDITOR
		setStringTag(name, value);
		#endif
	}
	
	protected override void SetTagImpl (string name, int value)
	{
		#if UNITY_ANDROID && !UNITY_EDITOR
		setIntTag(name, value);
		#endif
	}
	#endregion
}