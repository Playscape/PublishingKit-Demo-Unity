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
	class WarningAccumulator {

		private const string WARNIG_DIALOG_TEXT = 
			"To configure Playscape Publishing Kit go to Window > " + 
				CommonConsts.CONFIGURE_PUBLISHING_KIT_MENU_ITEM;
		private const string WARNING_DIALOG_TITLE =
			"Playscape Misconfigured";

		private StringBuilder warningSb = new StringBuilder();
		
		public void AddWarning(string warning) {
			warningSb.Append("\n- " + warning);
		}

		public void WarnIfStringIsEmpty(string testedString, string resultingWarning)
		{
			WarnIf(string.IsNullOrEmpty(testedString), resultingWarning);
		}

		public void WarnIf(bool condition, string resultingWarning)
		{
			if (condition)
			{
				AddWarning(resultingWarning);
			}
		}
		
		public bool HasWarnings()
		{
			return warningSb.Length > 0;
		}
		
		public void ShowIfNecessary() {
			if (HasWarnings())
			{
				EditorUtility.DisplayDialog(
					WARNING_DIALOG_TITLE,
					WARNIG_DIALOG_TEXT + "\n"+ warningSb.ToString(),
					"OK");
			}
		}
		
	}

	/// <summary>
	/// Handles the OnPostProcessBuild callback, in which we apply configurations to the sdks in the various platforms
	/// </summary>
	
}
#endif