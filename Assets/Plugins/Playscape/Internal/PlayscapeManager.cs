using System.Collections.Generic;
using Playscape.Analytics;
using UnityEngine;
using Playscape.Internal;
using System;

namespace Playscape.Internal
{
    /// <summary>
    /// Initialization script for the Playscape Publishing Kit
    /// </summary>
    public class PlayscapeManager : MonoBehaviour
	{
	    /// <summary>
	    /// </summary>
	    public const string PLAYSCAPE_MANAGER_GAMEOBJECT_NAME = "PlayscapeManager";

	    /// <summary>
	    /// Has this game object ever initialized?
	    /// Makes this game object singleton.
	    /// </summary>
	    private static bool mInitialized;

		/// <summary>
		/// Gets the scene number this object created at.
		/// </summary>
		/// <value>The scene number created at.</value>
		public int SceneNumberCreatedAt { get; private set; }

	    /// <summary>
	    /// </summary>
	    public void Awake()
		{
			if (Debug.isDebugBuild) {
				L.CurrentLogLevel = L.LogLevel.Debug;
			}
			#if UNITY_EDITOR
			L.CurrentLogLevel = L.LogLevel.Debug;
			#endif

			if (mInitialized)
			{
				// This ensures that there are no two PlayscapeManager objects in the scene.
				// This scenario can occur if we're back in the same scene
				Destroy(gameObject);
				return;
			}
            
			RemoteLogger.Init();
			NativeReport.Init();
			AddABTesingScripts();
			AddPushWooshScripts();
			AddAdScripts();

			// Makes this game object live forever
			DontDestroyOnLoad(gameObject);

			SceneNumberCreatedAt = Application.loadedLevel;

			L.D ("PlayscapeManager Initialized");
			mInitialized = true;
		}

		/// <summary>
		/// Start this instance.
		/// </summary>
		public void Start() {
		}

		/// <summary>
		/// </summary>
		public void OnApplicationQuit()
		{
			RemoteLogger.DumpNow();
		}

	    /// <summary>
	    /// </summary>
	    private void AddPushWooshScripts()
		{
			#if UNITY_IPHONE
			gameObject.AddComponent(typeof(PushNotificationsIOS ));
			PushWooshCommon pushWoosh = (PushWooshCommon) gameObject.GetComponent(typeof(PushNotificationsIOS));
			pushWoosh.ReportAllTags();
			#endif
		}

		/// <summary>
		/// Adds the ad scripts.
		/// </summary>
		void AddAdScripts ()
		{
			#if UNITY_ANDROID && !UNITY_EDITOR
			gameObject.AddComponent(typeof(PlayscapeAdsAndroid));
			#else
			gameObject.AddComponent(typeof(PlayscapeAdsMock));
			#endif
		}

		/// <summary>
		/// Adds the abtesting scripts.
		/// </summary>
		private void AddABTesingScripts()
		{
			#if UNITY_ANDROID && !UNITY_EDITOR
			gameObject.AddComponent(typeof(PlayscapeABTestingAndroid));
			#else
			gameObject.AddComponent(typeof(PlayscapeABTestingMock));
			#endif
		}

	    /// <summary>
	    /// Update
	    /// </summary>
	    void Update()
		{
			NetworkTimeUpdate.UpdateNetworkTimeInRemoteLogger();
		}
	}

}