using UnityEngine;
using System.Collections;
using Playscape.Internal;
using System;

namespace Playscape.Ads {
	/// <summary>
	/// Lets you display full screen ads in your game (interstitials).
	/// </summary>
	public class Interstitials {
		/// <summary>
		/// Kinds of interstitials you can request.
		/// </summary>
		public enum Kind {
			/// <summary>
			/// Interstitials which are displayed as a view on top of the game, the game isn't paused automatically.
			/// </summary>
			Overlay,
			/// <summary>
			/// Interstitials which are opened as a new window e.g Activity in Android, which pause the game.
			/// </summary>
			NonOverlay,

			/// <summary>
			/// Both Overlay, and NonOverlay
			/// </summary>
			Both
		}

		/// <summary>
		/// Kinds of interstitials you can request.
		/// </summary>
		public enum State {
			Failure,
			Skipped,
			Clicked
		}

		/// <summary>
		/// The m playscape ads.
		/// </summary>
		private PlayscapeAdsBase mPlayscapeAds;

		/// <summary>
		/// Occurs when an interstitial ad is shown on the screen.
		/// </summary>
		public event Action<Kind> OnShown;

		/// <summary>
		/// Occurs when display has ended - either user closed it, clicked it or display failed altogether.
		/// </summary>
		public event Action<State, Kind> OnDisplayEnded;

		/// <summary>
		/// Singleton
		/// </summary>
		Interstitials() {
			GameObject go = GameObject.Find(PlayscapeManager.PLAYSCAPE_MANAGER_GAMEOBJECT_NAME);
			if (go != null) {
				
				#if UNITY_ANDROID && !UNITY_EDITOR
				mPlayscapeAds = (PlayscapeAdsBase) go.GetComponent(typeof(PlayscapeAdsAndroid));
				
				mPlayscapeAds.OnInterstitialDisplayEndedInternal += (payload) => dispatchInterstitialDisplayEnded(payload);
				mPlayscapeAds.OnInterstitialShownInternal += (payload) => dispatchInterstitialShown(payload);
				#elif UNITY_IPHONE && !UNITY_EDITOR
				mPlayscapeAds = (PlayscapeAdsBase) go.GetComponent(typeof(PlayscapeAdsIOS));
				
				mPlayscapeAds.OnInterstitialDisplayEndedInternal += (payload) => dispatchInterstitialDisplayEnded(payload);
				mPlayscapeAds.OnInterstitialShownInternal += (payload) => dispatchInterstitialShown(payload);
				#else
				mPlayscapeAds = (PlayscapeAdsBase) go.GetComponent(typeof(PlayscapeAdsMock));
				#endif
			}
		}

		/// <summary>
		/// The instance.
		/// </summary>
		private static Interstitials mInstance;

		/// <summary>
		/// Gets the instance.
		/// </summary>
		/// <value>The instance.</value>
		public static Interstitials Instance {
			get {
				if (mInstance == null) {
					mInstance = new Interstitials();

					if (mInstance.mPlayscapeAds == null) {
						throw new ApplicationException("Initialization failed - Please place PlayscapeManager prefab into " +
						                               "the scene and access Instance from onStart() or later (NOT from onAwake()).");
					}
				}
				
				return mInstance;
			}
		}

		/// <summary>
		/// Displays the interstitial.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="placement">Extra text added with the analytic report of each event related to this ad, can be null.</param>
		[Obsolete]
		public void Display(Kind kind, string placement) {
			throw new Exception ("You call deprecated method: 'Interstitials.Instance.Display(Kind kind, string placement)'. Please, use this method: 'Interstitials.Instance.Display(string placement)'");
		}

		/// <summary>
		/// Displays the interstitial.
		/// </summary>
		/// <param name="placement">Extra text added with the analytic report of each event related to this ad, can be null.</param>
		public void Display(string placement) {
			if (placement == null) {
				placement = string.Empty;
			}

			if (mPlayscapeAds != null) {
				mPlayscapeAds.displayInterstitialAd((int)Kind.Both, placement);
			}
		}

		/// <summary>
		/// Dispatchs the interstitial display ended event.
		/// </summary>
		/// <param name="payload">Payload.</param>
		void dispatchInterstitialDisplayEnded (string payload)
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

		/// <summary>
		/// Dispatchs the interstitial display shown.
		/// </summary>
		/// <param name="payload">Payload.</param>
		void dispatchInterstitialShown (string payload)
		{
			Kind? kind = Enum.Parse(typeof(Kind), payload) as Kind?;
			
			if (kind != null) {
				if (OnShown != null) {
					OnShown(kind.Value);
				} else {
					L.I("Missed event with payload: {0}", payload);
				}
			} else {
				L.E ("Payload parse failure, kind from '{0}' to '{1}'",
				     payload, kind);
			}

		}

		/// <summary>
		/// Determines whether there's an interstitial of the given kind in cache.
		/// </summary>
		/// <returns><c>true</c> there's an interstitial of the given kind in cache.</returns>
		/// <param name="kind">Kind.</param>
		public bool HasInterstitialInCache(Kind kind) {
			return mPlayscapeAds.hasInterstitialInCache ((int)kind);
		}

		/// <summary>
		/// Notifies an open Overlay interstitial on Android that the back button has been pressed.
		///
		/// You should call this method like this:
		///
		/// public void Update() {
		///   if (Input.GetKeyUp(KeyCode.Escape)) {
		//	       if (Interstitials.Instance.OnBackPressed()) {
		//	            return;
		//         } else {
		//	       		Application.Quit();
		//         }
		//    }
		/// }
		/// </summary>
		/// <returns><c>true</c> if an Interstitial was dismissed due the back press</returns>
		public bool OnBackPressed() {
			return mPlayscapeAds.onBackPressed ();
		}

	}
}