using System;
using UnityEngine;
using Playscape.Internal;

namespace Playscape.Ads {
	/// <summary>
	/// Helper for locking and unclocking ads displaying
	/// </summary>
	public class AdManager {

		/// <summary>
		/// Communicates with the internal ads part.
		/// </summary>
		private PlayscapeAdsBase mPlayscapeAds;

		/// <summary>
		/// Singleton
		/// </summary>
		AdManager() {
			GameObject go = GameObject.Find(PlayscapeManager.PLAYSCAPE_MANAGER_GAMEOBJECT_NAME);
			if (go != null) {

				#if UNITY_ANDROID && !UNITY_EDITOR
				mPlayscapeAds = (PlayscapeAdsBase) go.GetComponent(typeof(PlayscapeAdsAndroid));
				#else
				mPlayscapeAds = (PlayscapeAdsBase) go.GetComponent(typeof(PlayscapeAdsMock));
				#endif
			}
		}

		/// <summary>
		/// The instance.
		/// </summary>
		private static AdManager mInstance;

		/// <summary>
		/// Gets the instance.
		/// </summary>
		/// <value>The instance.</value>
		public static AdManager Instance {
			get {
				if (mInstance == null) {
					mInstance = new AdManager();

					if (mInstance.mPlayscapeAds == null) {
						throw new ApplicationException("Initialization failed - Please place PlayscapeManager prefab into " +
						                               "the scene and access Instance from onStart() or later (NOT from onAwake()).");
					}
				}

				return mInstance;
			}
		}

		/// <summary>
		/// Wrapper to lock ads displaying
		/// </summary>
		public void DisableAds() {
			mPlayscapeAds.disableAds();
		}

		/// <summary>
		/// Wrapper to unlock ads displaying
		/// </summary>
		public void EnableAds() {
			mPlayscapeAds.enableAds();
		}

	}
}
