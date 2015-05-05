#if UNITY_EDITOR
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using Playscape.Internal;
using System.Xml;
using System.Text;
using System;
using System.Collections.Generic;

namespace Playscape.Editor {
	
	abstract class AbstractPostProcessor : IPostProcessor {
		
		public virtual void CheckForWarnings(WarningAccumulator warnings)
		{
 			warnings.WarnIfStringIsEmpty(
				ConfigurationInEditor.Instance.MyAds.MyAdsConfig.ApiKey,
				Warnings.ADS_API_KEY_NOT_SET);

//			bool atleastOneIdFilled = false;
//			ConfigurationInEditor.Instance.TraverseAdsConfig (
//					(category, fieldInfo) => 
//						{
//								if (category.GetType () != typeof(Configuration.Ads.AdsConfig)) {
//										if (!string.IsNullOrEmpty (fieldInfo.GetValue (category) as string)) {
//												atleastOneIdFilled = true;
//										}
//								}
//						});
//
//			if (!atleastOneIdFilled) 
//			{
//				warnings.AddWarning(Warnings.ATLEAST_ONE_AD_ID_MUST_BE_FILLED);
//			}
//
		}

		public abstract void Run();
	}
}
#endif