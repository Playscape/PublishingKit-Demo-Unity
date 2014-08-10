using UnityEngine;
using System.Collections;


namespace Playscape.Internal
{
	public class PlayscapeAdsAndroid : PlayscapeAdsBase {

		#if UNITY_ANDROID
		const string ADS_METHODS_CLASS_NAME = "com.playscape.ads.UnityAdsMethods";

		private AndroidJavaClass mJavaMethodsClass;
		#endif

		// Use this for initialization
		void Start () {
			#if UNITY_ANDROID
			mJavaMethodsClass = new AndroidJavaClass (ADS_METHODS_CLASS_NAME);
			#endif
		}

		public override void displayInterstitialAd(int kind, string placement) {
			#if UNITY_ANDROID
			mJavaMethodsClass.CallStatic ("displayInterstitialAd", new object[] {kind, placement});
			#endif
		}

		public override void displayVideoAdInternal(int kind, string placement) {
			#if UNITY_ANDROID
			mJavaMethodsClass.CallStatic ("displayVideoAd", new object[] {kind, placement});
			#endif
		}

		public override void displayBannerAd (int alignment, string placement)
		{
			#if UNITY_ANDROID
			mJavaMethodsClass.CallStatic("displayBannerAd", new object[] {alignment, placement});
			#endif
		}

		public override void hideBannerAd() 
		{
			#if UNITY_ANDROID
			mJavaMethodsClass.CallStatic("hideBannerAd", new object[] {});
			#endif
		}

		public override bool isBannerShown () 
		{
			#if UNITY_ANDROID
			return mJavaMethodsClass.CallStatic<bool>("isBannerShown", new object[] {});
			#else
			return false;
			#endif
		}

		public override bool hasInterstitialInCache (int kind)
		{
			#if UNITY_ANDROID
			return mJavaMethodsClass.CallStatic<bool>("hasInterstitialInCache", new object[] { kind });
			#else
			return false;
			#endif
		}

		public override bool onBackPressed () {
			#if UNITY_ANDROID
			return mJavaMethodsClass.CallStatic<bool>("onBackPressed", new object[] { });
			#else
			return false;
			#endif
		}
	}
}