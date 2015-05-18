using UnityEngine;
using System.Collections;

namespace Playscape.Internal
{
	public class PlayscapeCatalogAndroid : PlayscapeCatalogBase {

		#if UNITY_ANDROID
		const string CATALOG_METHODS_CLASS_NAME = "mominis.common.PlayscapeSdk";
	
		private AndroidJavaClass mJavaMethodsClass = new AndroidJavaClass (CATALOG_METHODS_CLASS_NAME);
        #endif

        public override void showCatalog() {
			#if UNITY_ANDROID
			mJavaMethodsClass.CallStatic ("showCatalog", new object[] {});
			#endif
		}

		
	}
}