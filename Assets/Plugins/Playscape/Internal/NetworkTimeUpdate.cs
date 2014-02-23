using Playscape.Analytics;


namespace Playscape.Internal
{
	
	public static class NetworkTimeUpdate
	{

		public static MPAnalyticsProvider mMPAnalyticsProvider { set; private get; }

		public static void UpdateNetworkTimeInRemoteLogger()
		{
			if (mMPAnalyticsProvider != null)
			{
				RemoteLogger.SetNetworkTime((int) mMPAnalyticsProvider.CurrentNetworkTime);
			}
		}

	}

}
