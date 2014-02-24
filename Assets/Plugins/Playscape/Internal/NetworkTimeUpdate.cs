using Playscape.Analytics;


namespace Playscape.Internal
{
	
	public static class NetworkTimeUpdate
	{

		public static MPAnalyticsProvider mMPAnalyticsProvider { set; private get; }

		private static int mCurrentNetworkTime;
		public static void UpdateNetworkTimeInRemoteLogger()
		{
			if (mMPAnalyticsProvider != null)
			{
				int newNetTime = mMPAnalyticsProvider.CurrentNetworkTime;

				if (mCurrentNetworkTime != newNetTime) {
					mCurrentNetworkTime = newNetTime;
					RemoteLogger.SetNetworkTime(newNetTime);
				}
			}
		}

	}

}
