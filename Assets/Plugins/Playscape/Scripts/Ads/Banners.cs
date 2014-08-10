using UnityEngine;
using System.Collections;
using Playscape.Internal;
using System;

namespace Playscape.Ads {
/// <summary>
/// Lets you display banners in the game.
/// </summary>
public class Banners {
	/// <summary>
	/// Communicates with the internal ads part.
	/// </summary>
	private PlayscapeAdsBase mPlayscapeAds;

	/// <summary>
	/// alignment of the given banners.
	/// </summary>
	public enum BannerAlignment {
		/// <summary>
		/// The top middle.
		/// </summary>
		topMiddle,
		/// <summary>
		/// The top left.
		/// </summary>
		topLeft,
		/// <summary>
		/// The top right.
		/// </summary>
		topRight,
		/// <summary>
		/// The bottom middle.
		/// </summary>
		bottomMiddle,
		/// <summary>
		/// The bottom left.
		/// </summary>
		bottomLeft,
		/// <summary>
		/// The bottom right.
		/// </summary>
		bottomRight

	}

	/// <summary>
	/// Singleton
	/// </summary>
	Banners() {
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
	private static Banners mInstance;
	
	/// <summary>
	/// Gets the instance.
	/// </summary>
	/// <value>The instance.</value>
	public static Banners Instance {
		get {
			if (mInstance == null) {
				mInstance = new Banners();

				if (mInstance.mPlayscapeAds == null) {
					throw new ApplicationException("Initialization failed - Please place PlayscapeManager prefab into " +
					                               "the scene and access Instance from onStart() or later (NOT from onAwake()).");
				}
			}
			
			return mInstance;
		}
	}

	/// <summary>
	/// Display a banner ad.
	/// </summary>
	/// <param name="alignment">How to position the ad.</param>
	/// <param name="placement">Extra text added with the analytic report of each event related to this ad, can be null.</param>
	public void Display(BannerAlignment alignment, string placement) {
		mPlayscapeAds.displayBannerAd ((int)alignment, placement);
	}

	/// <summary>
	/// Hide this instance.
	/// </summary>
	public void Hide() {
		mPlayscapeAds.hideBannerAd ();
	}

	/// <summary>
	/// Determines if banner is shown.
	/// </summary>
	/// <value><c>true</c> if banner is shown.</value>
	public bool IsBannerShown {
		get { return mPlayscapeAds.isBannerShown(); }
	}
}
}