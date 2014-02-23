using System;
using UnityEngine;
using Playscape.Internal;


namespace Playscape.Notifications
{
	/// <summary>
	/// Platform indepdendant access to PushWoosh
	/// 
	/// Use PushWoosh.Instance
	/// 
	/// Lets you set PushWoosh tags.
	/// </summary>
	public class PushWoosh
	{
	    /// <summary>
	    /// Instance
	    /// </summary>
	    private static PushWooshCommon mPushWoosh;


	    /// <summary>
	    /// Prevents a default instance of the <see cref="PushWoosh"/> class from being created.
	    /// Use the Instance property.
	    /// </summary>
	    PushWoosh() {
			// Use instance
		}

		/// <summary>
		/// Gets the instance.
		/// </summary>
		/// <value>The instance.</value>
		public static PushWooshCommon Instance {
			get {
				if (mPushWoosh == null) {
					GameObject go = GameObject.Find(PlayscapeManager.PLAYSCAPE_MANAGER_GAMEOBJECT_NAME);
					if (go != null) {

						#if UNITY_ANDROID && !UNITY_EDITOR
						mPushWoosh = (PushWooshCommon) go.GetComponent(typeof(PushNotificationsAndroid));
						#elif UNITY_IPHONE && !UNITY_EDITOR
						mPushWoosh = (PushWooshCommon )go.GetComponent(typeof(PushNotificationsIOS));
						#else
						go.AddComponent(typeof(PushWooshStub));
						mPushWoosh = (PushWooshCommon )go.GetComponent(typeof(PushWooshStub)); 
						((PushWooshStub)mPushWoosh).StubInit();
						#endif
					}
				}

				return mPushWoosh;
			}
		}
	}
}

