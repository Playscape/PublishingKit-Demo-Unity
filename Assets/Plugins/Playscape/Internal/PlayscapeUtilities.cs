using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace Playscape.Internal
{

	public class PlayscapeUtilities
	{

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

	}

}