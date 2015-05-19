using UnityEngine;
using System.Collections;

namespace Playscape.Internal
{
	// currently, only android is supported, use mock everywhere else
	#if UNITY_EDITOR || !UNITY_ANDROID

	public class PlayscapeCatalogMock : PlayscapeCatalogBase {

		public override void showCatalog() {
			L.I ("Mock showCatalog() called");
		}

		

	}

	#endif
}
