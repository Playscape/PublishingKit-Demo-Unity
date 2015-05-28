using UnityEngine;
using System.Collections;


namespace Playscape.Internal
{
	/// <summary>
	/// Wrapper for calls ExternalAdsMethods.java methods
	/// </summary>
	public class PlayscapeAdsAndroid : PlayscapeAdsBase {
		
		#if UNITY_ANDROID
		const string ADS_METHODS_CLASS_NAME = "com.playscape.ads.ExternalAdsMethods";
		
		private AndroidJavaClass mJavaMethodsClass = new AndroidJavaClass (ADS_METHODS_CLASS_NAME);
		#endif
		
		
		/// <summary>
		/// Wrapper for calls ExternalAdsMethods.displayInterstitialAd method
		/// <param name="kind">ad kind</param>
		/// <param name="placement">define place where ad should be displayed</param>
		/// </summary>
		public override void displayInterstitialAd(int kind, string placement) {
			#if UNITY_ANDROID
			mJavaMethodsClass.CallStatic ("displayInterstitialAd", new object[] {kind, placement});
			#endif
		}
		
		/// <summary>
		/// Wrapper for calls ExternalAdsMethods.disableAds method
		/// </summary>
		public override void disableAds() {
			#if UNITY_ANDROID
			mJavaMethodsClass.CallStatic ("disableAds");
			#endif
		}
		
		/// <summary>
		/// Wrapper for calls ExternalAdsMethods.enableAds method
		/// </summary>
		public override void enableAds() {
			#if UNITY_ANDROID
			mJavaMethodsClass.CallStatic ("enableAds");
			#endif
		}
		
		/// <summary>
		/// Wrapper for calls ExternalAdsMethods.displayVideoAd method
		/// <param name="kind">ad kind</param>
		/// <param name="placement">define place where ad should be displayed</param>
		/// </summary>
		public override void displayVideoAdInternal(int kind, string placement) {
			#if UNITY_ANDROID
			mJavaMethodsClass.CallStatic ("displayVideoAd", new object[] {kind, placement});
			#endif
		}
		
		/// <summary>
		/// Wrapper for calls ExternalAdsMethods.displayBannerAd method
		/// <param name="alignment">ad alignment</param>
		/// <param name="placement">define place where ad should be displayed</param>
		/// </summary>
		public override void displayBannerAd (int alignment, string placement)
		{
			#if UNITY_ANDROID
			mJavaMethodsClass.CallStatic("displayBannerAd", new object[] {alignment, placement});
			#endif
		}
		
		/// <summary>
		/// Wrapper for calls ExternalAdsMethods.hideBannerAd method
		/// </summary>
		public override void hideBannerAd()
		{
			#if UNITY_ANDROID
			mJavaMethodsClass.CallStatic("hideBannerAd", new object[] {});
			#endif
		}
		
		/// <summary>
		/// Wrapper for calls ExternalAdsMethods.isBannerShown method
		/// </summary>
		public override bool isBannerShown ()
		{
			#if UNITY_ANDROID
			return mJavaMethodsClass.CallStatic<bool>("isBannerShown", new object[] {});
			#else
			return false;
			#endif
		}
		
		/// <summary>
		/// Wrapper for calls ExternalAdsMethods.hasInterstitialInCache method
		/// <param name="kind">ad kind</param>
		/// </summary>
		public override bool hasInterstitialInCache (int kind)
		{
			#if UNITY_ANDROID
			return mJavaMethodsClass.CallStatic<bool>("hasInterstitialInCache", new object[] { kind });
			#else
			return false;
			#endif
		}
		
		/// <summary>
		/// Wrapper for calls ExternalAdsMethods.onBackPressed method
		/// </summary>
		public override bool onBackPressed () {
			#if UNITY_ANDROID
			return mJavaMethodsClass.CallStatic<bool>("onBackPressed", new object[] { });
			#else
			return false;
			#endif
		}
	}
}