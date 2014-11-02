using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Playscape.Internal;
using System;

namespace Playscape.ABTesting {
	public class ABTesing {

		/// <summary>
		/// delegate to retreive experimentData
		/// </summary>
		public delegate void OnExperimentDataArrivedDelegate(ExperimentData ExperimentData);

		/// <summary>
		/// The m playscape abtesting.
		/// </summary>
		private PlayscapeABTestingBase mPlayscapeABTeting;

		/// <summary>
		/// Save callbacks by experimentName
		/// </summary>
		private IDictionary<string, OnExperimentDataArrivedDelegate> mExperimentNameToCallback;

		/// <summary>
		/// Singleton
		/// </summary>
		ABTesing() {
			GameObject go = GameObject.Find(PlayscapeManager.PLAYSCAPE_MANAGER_GAMEOBJECT_NAME);
			if (go != null) {
				
				#if UNITY_ANDROID && !UNITY_EDITOR
				mPlayscapeABTeting = (PlayscapeABTestingBase) go.GetComponent(typeof(PlayscapeABTestingAndroid));
				#else
				mPlayscapeABTeting = (PlayscapeABTestingBase) go.GetComponent(typeof(PlayscapeABTestingMock));
				#endif
                
                mPlayscapeABTeting.OnExperimentDataArrivedInternal += (payload) => dispatchExperimentDataArrived(payload);
			}
			mExperimentNameToCallback = new Dictionary<string, OnExperimentDataArrivedDelegate> ();
		}

		/// <summary>
		/// The instance.
		/// </summary>
		private static ABTesing mInstance;

		/// <summary>
		/// Gets the instance.
		/// </summary>
		/// <value>The instance.</value>
		public static ABTesing Instance {
			get {
				if (mInstance == null) {
					mInstance = new ABTesing();
				}
				return mInstance;
			}
		}


		/// <summary>
		/// Gets the experiment by experiment name async (nonblocking, waiting for experiment data to be loaded from server)
		/// </summary>
		/// <param name="experimentName">The experiment name to get </param>
		/// <param name="callBack">The callbeck to return the experiment data</param>
		/// 
		public void getExperimentDataByExperimentNameAsync (string experimentName, OnExperimentDataArrivedDelegate callBack)
		{
			if (mExperimentNameToCallback.ContainsKey(experimentName))
			{
				mExperimentNameToCallback[experimentName] = callBack;
			}
			else 
			{
				mExperimentNameToCallback.Add(experimentName,callBack);
			}
			mPlayscapeABTeting.getExperimentData (experimentName);
		}

		/// <summary>
		/// Dispatchs the interstitial display ended event.
		/// </summary>
		/// <param name="payload">Payload is a string that looks like this: 
		/// 					  ExperimentName___:ExperimentGroup___:ExperimentKey1___:ExperimentValue1___:ExperimentKey2___:ExperimentValue2___:
		/// </param>
		void dispatchExperimentDataArrived(string payload)
		{
            L.I("dispatchExperimentDataArrived called with payload: {0}", payload);
			ExperimentData experimentData = ExperimentData.fromPayload (payload);
			if (experimentData != null) 
			{
				string experimentName  = experimentData.getExperimentName();
				if (mExperimentNameToCallback.ContainsKey(experimentName))
				{
					OnExperimentDataArrivedDelegate myDelegate = mExperimentNameToCallback[experimentName];
					myDelegate(experimentData);
				} else {
						L.E ("Missed abtesting event with payload: {0}", payload);
				}
			} else {
			  L.E ("Payload parse failed: {0}",payload);
			}
		}
	}
}
