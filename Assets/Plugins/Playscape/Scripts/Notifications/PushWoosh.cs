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
		/// This class wraps around this object.
		/// </summary>
	    private static PushWooshCommon mPushWooshCommon;

		/// <summary>
		/// The instance.
		/// </summary>
		private static PushWoosh mInstance;


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
		public static PushWoosh Instance {
			get {
				if (mPushWooshCommon == null) {
					GameObject go = GameObject.Find(PlayscapeManager.PLAYSCAPE_MANAGER_GAMEOBJECT_NAME);
					if (go != null) {

						#if UNITY_ANDROID && !UNITY_EDITOR
						mPushWooshCommon = (PushWooshCommon) go.GetComponent(typeof(PushNotificationsAndroid));
						#elif UNITY_IPHONE && !UNITY_EDITOR
						mPushWooshCommon = (PushWooshCommon )go.GetComponent(typeof(PushNotificationsIOS));
						#else
						go.AddComponent(typeof(PushWooshStub));
						mPushWooshCommon = (PushWooshCommon )go.GetComponent(typeof(PushWooshStub)); 
						((PushWooshStub)mPushWooshCommon).StubInit();
						#endif

						mInstance = new PushWoosh();
					}
				}

				return mInstance;
			}
		}

		/// <summary>
		/// Sets the tag.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="value">Value.</param>
		public void SetTag(string name, string value)
		{
			mPushWooshCommon.SetTag(name, value);
		}

		/// <summary>
		/// Sets the tag.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="value">Value.</param>
		public void SetTag(string name, int value)
		{
			mPushWooshCommon.SetTag(name, value);
		}
	}
}

