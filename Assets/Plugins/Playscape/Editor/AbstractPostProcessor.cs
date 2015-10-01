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
		}

		public abstract void Run();
	}
}
#endif