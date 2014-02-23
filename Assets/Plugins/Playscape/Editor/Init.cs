#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.IO;

namespace Playscape.Editor {

	/// <summary>
	/// Publishing kit initialization, this is the entry point class where editor is initialized.
	/// </summary>
	[InitializeOnLoad]
	public class PublishingKitInit : MonoBehaviour {
		[MenuItem ("Window/" + CommonConsts.CONFIGURE_PUBLISHING_KIT_MENU_ITEM)]
		public static void ConfigurePublishingKit() {
			var window = EditorWindow.GetWindow<ConfigurationWindow>(new Type[] {EditorWindow.GetWindow(typeof(SceneView)).GetType()});
			window.Show(true);
		}
	}
}
#endif