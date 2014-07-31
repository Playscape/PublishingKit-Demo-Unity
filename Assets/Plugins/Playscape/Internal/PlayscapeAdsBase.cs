﻿using UnityEngine;
using System.Collections;
using System;


namespace Playscape.Internal
{
	public abstract class PlayscapeAdsBase : MonoBehaviour {
		public event Action<string> OnInterstitialShownInternal;
		public event Action<string> OnInterstitialDisplayEndedInternal;
		public event Action<string> OnVideoDisplayEndedInternal;
		
		public abstract void displayInterstitialAd (string kind, string placement);
		public abstract void displayVideoAdInternal (string kind, string placement);
		public abstract void displayBannerAd (string alignment, string placement);
		public abstract void hideBannerAd ();
		public abstract bool isBannerShown ();
		public abstract bool hasInterstitialInCache (string kind);
		public abstract bool onBackPressed ();

		/// <summary>
		/// Triggered by internal android code when interstitial display ends.
		/// </summary>
		/// <param name="state">Payload is a string that looks like this: "state:kind"</param>
		void onInterstitialDisplayEnded(string payload) {
			if (OnInterstitialDisplayEndedInternal != null) {
				OnInterstitialDisplayEndedInternal(payload);
			}
		}

		/// <summary>
		/// Triggered by internal android code when video display end.
		/// </summary>
		/// <param name="payload">Payload.</param>
		void onVideoDisplayEnded(string payload) {
			if (OnVideoDisplayEndedInternal != null) {
				OnVideoDisplayEndedInternal(payload);
			}
		}

		/// <summary>
		/// Triggered by internal android code when an interstitial is shown.
		/// </summary>
		/// <param name="payload">Payload.</param>
		void onInterstitialShown(string payload) {
			if (OnInterstitialShownInternal != null) {
				OnInterstitialShownInternal(payload);
			}
		}
	}
}