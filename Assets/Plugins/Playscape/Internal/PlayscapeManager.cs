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
		private const string REAL_CHARTBOOST_MANAGER_GAME_OBJECT_NAME = "ChartBoostManager";

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
			GenerateGMSID();
			AddPushWooshScripts();
			ReportLaunchOnIos();

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
			if (GameObject.Find(REAL_CHARTBOOST_MANAGER_GAME_OBJECT_NAME) == null) {
				L.W(Warnings.CHARTBOOST_NOT_INITIALIZED);
			}
		}

		/// <summary>
		/// </summary>
		public void OnApplicationPause(bool pauseStatus)
		{
			if (pauseStatus == false)
			{
				// On iOS, this means app gained focus
				ReportLaunchOnIos();
			}

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
			#if UNITY_ANDROID
			gameObject.AddComponent(typeof(PushNotificationsAndroid));
			#elif UNITY_IPHONE
			gameObject.AddComponent(typeof(PushNotificationsIOS ));
			#endif
		}

	    /// <summary>
	    /// Update
	    /// </summary>
	    void Update()
		{
			NetworkTimeUpdate.UpdateNetworkTimeInRemoteLogger();
		}

	    /// <summary>
	    /// Generate game session unique identifier
	    /// </summary>
	    private void GenerateGMSID()
		{
			RemoteLogger.SetGameSessionId(PlayscapeUtilities.GenerateRandomId());
		}

	    /// <summary>
	    /// Reports game launch, relevant for iOS only.
	    /// </summary>
	    private void ReportLaunchOnIos()
		{
			#if UNITY_IPHONE
			int launchCount = PlayerPrefs.GetInt("playscape_launch_count", 0);
			launchCount++;

			if (launchCount == 1) {
				Report.Instance.ReportActivation("iOS");
			}

			Report.Instance.ReportLaunch(launchCount);
			PlayerPrefs.SetInt("playscape_launch_count", launchCount);
			PlayerPrefs.Save();
			#endif
		}

	}

}