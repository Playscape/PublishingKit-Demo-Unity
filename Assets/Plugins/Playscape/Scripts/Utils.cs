using System;
using System.Runtime.InteropServices;


namespace Playscape
{
	/// <summary>
	/// Handy helper methods.
	/// </summary>
	public static class Utils {
		/// <summary>
		/// Represents the date 01/01/1970 00:00:00
		/// </summary>
		private static readonly DateTime Jan1st1970 = new DateTime
			(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		
		/// <summary>
		/// Retrieves the epoch (unix-timestamp) in milliseconds.
		/// </summary>
		/// <returns>Unix timestamp in milliseconds.</returns>
		public static long CurrentTimeMillis {
			get {
				// iOS's DateTime subtraction is b0n3d so we used this workaround instead...
				#if !UNITY_IPHONE
				return (long) (DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
				#else
				return PlayscapeUtils_currentTimeMillis();
				#endif
			}
		}

		#if UNITY_IPHONE
		[DllImport("__Internal")]
		/// <summary>
		/// Playscapes the utils_current time millis.
		/// </summary>
		/// <returns>The utils_current time millis.</returns>
		private static extern long PlayscapeUtils_currentTimeMillis();
		#endif
	}
}

