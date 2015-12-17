#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.IO;
using System.Xml;
using Playscape.Internal;

namespace Playscape.Editor {


	/// <summary>
	/// Publishing kit initialization, this is the entry point class where editor is initialized.
	/// </summary>
	[InitializeOnLoad]
	public static class PublishingKitInit {
		private static int mCurrentlyOpenedSceneNumber = 0;

		static  PublishingKitInit() {
			EditorApplication.playmodeStateChanged += OnPlayModeStateChanged;
			EditorApplication.hierarchyWindowChanged += OnHierarchyWindowChanged;
			UpdateCurrentlyOpenedSceneNumber();
			Upgrader.Upgrade ();

		}

		[MenuItem ("Window/Playscape/" + CommonConsts.CONFIGURE_PUBLISHING_KIT_MENU_ITEM)]
				public static void OpenConfigurePublishingKitWindow() {
					var window = EditorWindow.GetWindow<ConfigurationWindow>(new Type[] {EditorWindow.GetWindow(typeof(SceneView)).GetType()});
					window.Show(true);
		}

		[MenuItem ("Window/Playscape/About")]
		public static void OpenAbout() {
				var version = "1.0";
				var configDoc = new XmlDocument();
				configDoc.LoadXml(File.ReadAllText("Assets/StreamingAssets/playscape/PlayscapeConfig.xml"));

				version = configDoc.SelectSingleNode("resources/string[@name='playscape_publishing_kit_version']").InnerText;

				EditorUtility.DisplayDialog (
					"Playscape SDK Plugin",
					string.Format("For more information, visit the official site on GitHub:\n\nhttps://github.com/Playscape/Documentation-internal/wiki\n\nPlugin version: {0}", version),
					"OK");
		}

		static void OnPlayModeStateChanged () {
			if (EditorApplication.isPlayingOrWillChangePlaymode) {
				var go = GameObject.Find(PlayscapeManager.PLAYSCAPE_MANAGER_GAMEOBJECT_NAME);

				if (go == null && mCurrentlyOpenedSceneNumber == 0) {
					L.W(Warnings.PLAYSCAPE_MANAGER_NOT_IN_FIRST_SCENE);
				} else if (go != null) {
					var playscapeManager = go.GetComponent<PlayscapeManager>();
					if (playscapeManager.SceneNumberCreatedAt != 0) {
						L.W(Warnings.PLAYSCAPE_MANAGER_NOT_IN_FIRST_SCENE);
					}
				}
			}
		}
		
		static void OnHierarchyWindowChanged ()
		{
			UpdateCurrentlyOpenedSceneNumber();
		}
		
		static void UpdateCurrentlyOpenedSceneNumber() {
			int sceneNumber = 0;
			foreach (var s in EditorBuildSettings.scenes) {
				if (EditorApplication.currentScene == s.path) {
					mCurrentlyOpenedSceneNumber = sceneNumber;
					break;
				}
				sceneNumber++;
			}
		}
	}
}
#endif