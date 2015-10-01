using UnityEngine;
using System.Collections;
using System;

namespace Playscape.Internal
{
	public abstract class PlayscapeABTestingBase : MonoBehaviour {

		public event Action<string> OnExperimentDataArrivedInternal;

		public abstract void reportExperimentEvent(string experimentName, string experimentEvent);

		public abstract void getExperimentData (string experimentName);

		/// <summary>
		/// Triggered by internal android code when experiment data arrived.
		/// </summary>
		/// <param name="payload">Payload is a string that looks like this: 
		/// 					  ExperimentName___:ExperimentGroup___:ExperimentKey1___:ExperimentValue1___:ExperimentKey2___:ExperimentValue2___:
		/// </param>
		public void OnExperimentDataArrived(string payload) {
			if (OnExperimentDataArrivedInternal != null) {
				OnExperimentDataArrivedInternal(payload);
			}
		}

	}
}
