using UnityEngine;
using System.Collections;
using Playscape.Internal;
using Playscape.Analytics;

public class PlayscapeChartBoostManager : MonoBehaviour {
	private static bool mInitialized;

	void Awake () {
		if (mInitialized) {
			Destroy(gameObject);
		}
	
		mInitialized = true;
		DontDestroyOnLoad(gameObject);
	}

	public void didFailToLoadInterstitial( string location )
	{
		Report.Instance.ReportInterstitialLoadFailed(location);
		
		L.D ("ChartboostSupport didFailToLoadInterstitial " + location);
	}		
	
	public void didDismissInterstitial( string location )
	{
		Report.Instance.ReportInterstitialDismissed(location);
		
		L.D ("ChartboostSupport didDismissInterstitial " + location);
	}
	
	public void didClickInterstitial( string location )
	{
		Report.Instance.ReportInterstitialClicked(location);
		
		L.D ("ChartboostSupport didClickInterstitial " + location);
	}
	
	public void didShowInterstitial( string location )
	{
		Report.Instance.ReportInterstitialShown(location);
		
		L.D ("ChartboostSupport didShowInterstitial " + location);
	}
}
