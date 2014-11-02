using UnityEngine;
using System.Collections;

namespace Playscape.Internal
{
	// currently, only android is supported, use mock everywhere else
	#if UNITY_EDITOR || !UNITY_ANDROID

	public class PlayscapeABTestingMock : PlayscapeABTestingBase {

		public override void reportExperimentEvent(string experimentName, string experimentEvent) {
			L.I ("Mock reportExperimentEvent({0}, {1}) called", experimentName, experimentEvent);
		}

		public override void getExperimentData(string experimentName) {
			L.I ("Mock getExperimentData({0}) called", experimentName);
			string mockPayload = "Experiment 1___:Group 1___:Roll A Ball 1___:Roll A Ball Value 1___:Roll A Ball 2___:Roll A Ball Value 2";
			OnExperimentDataArrived (mockPayload);
		}

	}

	#endif
}
