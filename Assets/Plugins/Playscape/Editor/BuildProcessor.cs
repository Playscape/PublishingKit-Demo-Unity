﻿#if UNITY_EDITOR
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

	interface IPostProcessor {
		void CheckForWarnings(WarningAccumulator warnings);
		void Run();
	}

	class StubPostProcessor : IPostProcessor {
		public void CheckForWarnings(WarningAccumulator warnings) { }
		public void Run() { }
	}

	/// <summary>
	/// Handles the OnPostProcessBuild callback, in which we apply configurations to the sdks in the various platforms
	/// </summary>
	class BuildProcessor : MonoBehaviour {

		[PostProcessBuild]
		public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
			L.D ("Playscape OnPostprocessBuild...");

			var warnings = new WarningAccumulator();

			var postProcessor = SelectPostProcessor(target, pathToBuiltProject);

			postProcessor.CheckForWarnings(warnings);
			warnings.ShowIfNecessary();
			if (warnings.HasWarnings() == false) {
				postProcessor.Run();
			}
		}


		static IPostProcessor SelectPostProcessor(BuildTarget target, string pathToBuiltProject)
		{
			if (target == BuildTarget.Android) {
				return new AndroidPostProcessor(pathToBuiltProject);
			} else if (target == BuildTarget.iPhone) {
				return new IOSPostProcessor(pathToBuiltProject);
			} else {
				return new StubPostProcessor();
			}
		}
	}
}
#endif