using UnityEngine;
using System.Collections;

namespace Playscape.Internal
{
	public class PlayscapeABTestingAndroid : PlayscapeABTestingBase {

		#if UNITY_ANDROID
		const string ABTESTING_METHODS_CLASS_NAME = "com.playscape.ads.ExternalABTestingMethods";
	
		private AndroidJavaClass mJavaMethodsClass = new AndroidJavaClass (ABTESTING_METHODS_CLASS_NAME);
		#endif

		public override void reportExperimentEvent(string experimentName, string experimentEvent) {
			#if UNITY_ANDROID
			mJavaMethodsClass.CallStatic ("reportExperimentEvent", new object[] {experimentName, experimentEvent});
			#endif
		}

		public override void getExperimentData (string experimentName)
		{
			#if UNITY_ANDROID
			mJavaMethodsClass.CallStatic ("getExperimentByNameAsync", new object[] {experimentName});
			#endif
		}
	}
}