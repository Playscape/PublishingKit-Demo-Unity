using UnityEngine;
using System.Collections;


namespace Playscape.Internal
{
// currently, only android is supported, use mock everywhere else
#if UNITY_EDITOR || !UNITY_ANDROID
	public class PlayscapeAdsMock : PlayscapeAdsBase {
		public override void displayInterstitialAd(int kind, string placement) {
			L.I ("Mock displayInterstitialAd({0}, {1}) called", kind, placement);
		}

		public override void disableAds() {
			L.I ("Mock disableAds() called");
		}

		public override void enableAds() {
			L.I ("Mock enableAds() called");
		}

		public override void displayVideoAdInternal(int kind, string placement) {
			L.I ("Mock displayVideoAdInternal({0}, {1}) called", kind, placement);
		}

		public override void displayBannerAd (int alignment, string placement)
		{
			L.I ("Mock displayBannerAd({0}, {1}) called", alignment, placement);
		}

		public override void hideBannerAd ()
		{
			L.D ("Mock hideBannerAd() called");
		}

		public override bool isBannerShown () 
		{
			L.D ("Mock isBannerShown() called");
			return false;
		}

		public override bool hasInterstitialInCache (int kind)
		{
			L.D ("Mock hasInterstitialInCache({0}) called", kind);
			return false;
		}

		public override bool onBackPressed () {
			L.D ("Mock onBackPressed() called");
			return false;
		}
	}
#endif
}