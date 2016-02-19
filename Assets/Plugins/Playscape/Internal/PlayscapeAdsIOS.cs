using System;
using System.Runtime.InteropServices;

namespace Playscape.Internal
{
	public class PlayscapeAdsIOS : PlayscapeAdsBase
	{
#if UNITY_IPHONE

		[DllImport ("__Internal")]
		private static extern void __playscape_ad_displayInterstitialAd (int kind, string placement);

		[DllImport ("__Internal")]
		private static extern void __playscape_ad_displayVideoAd (int kind, string placement);

		[DllImport ("__Internal")]
		private static extern void __playscape_ad_displayBannerAd (int alignment, string placement);

		[DllImport ("__Internal")]
		private static extern void __playscape_ad_hideBannerAd ();

		[DllImport ("__Internal")]
		private static extern bool __playscape_ad_is_banner_shown ();

		[DllImport ("__Internal")]
		private static extern bool __playscape_ad_has_interstitial_in_cache (int kind);

		[DllImport ("__Internal")]
		private static extern void __playscape_ad_disableAd ();

		[DllImport ("__Internal")]
		private static extern void __playscape_ad_enableAd ();

#endif


		/// Wrapper for calls PADAdDisplayManager
		/// <param name="kind">ad kind</param>
		/// <param name="placement">define place where ad should be displayed</param>
		/// </summary>
		public override void displayInterstitialAd(int kind, string placement) {
			#if UNITY_IPHONE
			__playscape_ad_displayInterstitialAd(kind, placement);
			#endif
		}

		/// <summary>
		/// Wrapper for calls PADAdDisplayManager
		/// </summary>
		public override void disableAds() {
			#if UNITY_IPHONE
			__playscape_ad_disableAd();
			#endif
		}
		
		/// <summary>
		/// Wrapper for calls PADAdDisplayManager
		/// </summary>
		public override void enableAds() {
			#if UNITY_IPHONE
			__playscape_ad_enableAd();
			#endif
		}
		
		/// <summary>
		/// Wrapper for calls PADAdDisplayManager
		/// <param name="kind">ad kind</param>
		/// <param name="placement">define place where ad should be displayed</param>
		/// </summary>
		public override void displayVideoAdInternal(int kind, string placement) {
			#if UNITY_IPHONE
			__playscape_ad_displayVideoAd(kind, placement);
			#endif
		}
		
		/// <summary>
		/// Wrapper for calls PADAdDisplayManager
		/// <param name="alignment">ad alignment</param>
		/// <param name="placement">define place where ad should be displayed</param>
		/// </summary>
		public override void displayBannerAd (int alignment, string placement) {
			#if UNITY_IPHONE
			__playscape_ad_displayBannerAd(alignment, placement);
			#endif
		}
		
		/// <summary>
		/// Wrapper for calls PADAdDisplayManager
		/// </summary>
		public override void hideBannerAd() {
			#if UNITY_IPHONE
			__playscape_ad_hideBannerAd();
			#endif
		}
		
		/// <summary>
		/// Wrapper for calls PADAdDisplayManager
		/// </summary>
		public override bool isBannerShown () {
			#if UNITY_IPHONE
			return __playscape_ad_is_banner_shown();
			#endif		
			
			return false;
		}
		
		/// <summary>
		/// Wrapper for calls PADAdDisplayManager 
		/// <param name="kind">ad kind</param>
		/// </summary>
		public override bool hasInterstitialInCache (int kind) {
			#if UNITY_IPHONE
			return __playscape_ad_has_interstitial_in_cache(kind);
			#endif

			return false;
		}

		public override bool onBackPressed () {
			return false;
		}

	}
}

