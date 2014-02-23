using System.Collections.Generic;
using Playscape.Analytics;
using UnityEngine;
using Playscape.Internal;
using System;

namespace Playscape
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
	    /// </summary>
	    public void Awake()
		{
			if (Debug.isDebugBuild) {
				L.CurrentLogLevel = L.LogLevel.Debug;
			}

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
			ReportLaunch();

			// Makes this game object live forever
			DontDestroyOnLoad(gameObject);

			L.D ("PlayscapeManager Initialized");
			mInitialized = true;
		}

		/// <summary>
		/// Start this instance.
		/// </summary>
		void Start() {
			if (GameObject.Find(REAL_CHARTBOOST_MANAGER_GAME_OBJECT_NAME) == null) {
				L.W(Warnings.CHARTBOOST_NOT_INITIALIZED);
			}
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
	    private void ReportLaunch()
		{
			#if UNITY_IPHONE
			int launchCount = PlayerPrefs.GetInt("playscape_launch_count", 0);
			launchCount++;

			if (launchCount == 1) {
				Report.Instance.ReportActivation("TODO TODO");
			}

			Report.Instance.ReportLaunch(launchCount);
			PlayerPrefs.SetInt("playscape_launch_count", launchCount);
			PlayerPrefs.Save();
			#endif

		}

	}

}