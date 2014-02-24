using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace Playscape.Internal
{

	public class PlayscapeUtilities
	{
		private static IDictionary<string, double> EMPTY_DICT = new Dictionary<string, double>();
		private static System.Random sRandom = new System.Random();

		public static string GenerateRandomId()
		{
			byte[] bytes = new byte[8];
			sRandom.NextBytes(bytes);
			return Math.Abs(BitConverter.ToInt64(bytes, 0)).ToString();
		}

		public static string FormatGameplayRelatedAdditionalParams(
			IDictionary<string, double> additionalParams)
		{
			additionalParams = additionalParams ?? EMPTY_DICT;
			StringBuilder formattedText = new StringBuilder();
			
			foreach (var item in additionalParams)
			{
				formattedText
						.Append("/")
						.Append(item.Key)
						.Append(item.Value);
			}
			
			return formattedText.ToString();
		}

		public static string JsonBool(bool value)
		{
			return value ? "true" : "false";
		}

		/// <summary>
		/// Returns a string suitable for the "conn" analytic field.
		/// </summary>
		/// <returns>"wifi", "mobile" or "offline"</returns>
		public static string GetConnectivityAnalyticsReport()
		{
			switch (Application.internetReachability)
			{
			case NetworkReachability.NotReachable:
				return "offline";
			case NetworkReachability.ReachableViaCarrierDataNetwork:
				return "mobile";
			case NetworkReachability.ReachableViaLocalAreaNetwork:
				return "wifi";
			default:
				return "uknown";
			}
		}
	}

}