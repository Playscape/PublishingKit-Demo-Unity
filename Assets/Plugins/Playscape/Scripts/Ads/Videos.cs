using UnityEngine;
using System.Collections;
using Playscape.Internal;
using System;

namespace Playscape.Ads {
	/// <summary>
	/// Lets you display incentivised and non incenstivised videos in your game.
	/// </summary>
	public class Videos {
		/// <summary>
		/// Kind of video ad to display.
		/// </summary>
		public enum Kind {
			/// <summary>
			/// Video which if player watches until the end shouldn't grant any prizes.
			/// </summary>
			NonIncentivised,
			/// <summary>
			/// Video which if player watches until the end should grant a prize.
			/// </summary>
			Incentivised
		}

		
		/// <summary>
		/// State ad finished with
		/// </summary>
		public enum State {
			Completed,
			Skipped,
			Failed
		}

		/// <summary>
		/// Occurs when an video  display has ended - either user skipped it, watched until its end or display failed altogether.
		/// </summary>
		public event Action<State, Kind> OnDisplayEnded;

		/// <summary>
		/// </summary>
		private PlayscapeAdsBase mPlayscapeAds;


		/// <summary>
		/// Singleton
		/// </summary>
		Videos() {
			GameObject go = GameObject.Find(PlayscapeManager.PLAYSCAPE_MANAGER_GAMEOBJECT_NAME);
			if (go != null) {
				
				#if UNITY_ANDROID && !UNITY_EDITOR
				mPlayscapeAds = (PlayscapeAdsBase) go.GetComponent(typeof(PlayscapeAdsAndroid));
				
				mPlayscapeAds.OnVideoDisplayEndedInternal += (payload) => dispatchVideoDisplayEnded(payload);
				#elif UNITY_IPHONE && !UNITY_EDITOR
				mPlayscapeAds = (PlayscapeAdsBase) go.GetComponent(typeof(PlayscapeAdsIOS));
				
				mPlayscapeAds.OnVideoDisplayEndedInternal += (payload) => dispatchVideoDisplayEnded(payload);
				#else
				mPlayscapeAds = (PlayscapeAdsBase) go.GetComponent(typeof(PlayscapeAdsMock));
				#endif
			}
		}

		/// <summary>
		/// The instance.
		/// </summary>
		private static Videos mInstance;

		/// <summary>
		/// Gets the instance.
		/// </summary>
		/// <value>The instance.</value>
		public static Videos Instance {
			get {
				if (mInstance == null) {
					mInstance = new Videos();

					if (mInstance.mPlayscapeAds == null) {
						throw new ApplicationException("Initialization failed - Please place PlayscapeManager prefab into " +
							"the scene and access Instance from onStart() or later (NOT from onAwake()).");
					}
				}
				
				return mInstance;
			}
		}

		/// <summary>
		/// Displays the video.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="placement">Extra text added with the analytic report of each event related to this ad, can be null.</param>
		public void Display(Kind kind, string placement) {
			if (placement == null) {
				placement = string.Empty;
			}

			if (mPlayscapeAds != null) {
				mPlayscapeAds.displayVideoAdInternal((int)kind, placement);
			}
		}

		/// <summary>
		/// Dispatchs the video display ended event.
		/// </summary>
		/// <param name="payload">Payload.</param>
		void dispatchVideoDisplayEnded (string payload)
		{
			string [] payloadParts = payload.Split (':');
			if (payloadParts.Length == 2) {
				State? state = Enum.Parse(typeof(State), payloadParts[0]) as State?;
				Kind? kind = Enum.Parse(typeof(Kind), payloadParts[1]) as Kind?;
				
				if (state != null && kind != null) {
					if (OnDisplayEnded != null) {
						OnDisplayEnded(state.Value, kind.Value);
					} else {
						L.I("Missed event with payload: {0}", payload);
					}
				} else {
					L.E ("Payload parse failure, parsed state from '{0}' to '{1}', kind from '{2}' to '{3}'",
					     payloadParts[0], state, payloadParts[1], kind);
				}
			} else {
				L.E ("Payload size is unexpected: {0}, expecting 2", payloadParts.Length);
			}
		}

	}
}