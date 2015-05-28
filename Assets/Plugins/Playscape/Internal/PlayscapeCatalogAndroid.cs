using UnityEngine;
using System.Collections;

namespace Playscape.Internal
{
	public class PlayscapeCatalogAndroid : PlayscapeCatalogBase {

		#if UNITY_ANDROID
        const string PLAYSCAPE_EXCHANGE_MNG_CLASS_NAME = "com.playscape.exchange.ExchangeManager";

        private AndroidJavaClass mJavaMethodsClass = new AndroidJavaClass(PLAYSCAPE_EXCHANGE_MNG_CLASS_NAME);
        #endif

        public override void showCatalog() {
			#if UNITY_ANDROID
            var instance = mJavaMethodsClass.CallStatic<AndroidJavaObject>("getInstance");
            instance.Call("showCatalog");
			#endif
		}

		
	}
}