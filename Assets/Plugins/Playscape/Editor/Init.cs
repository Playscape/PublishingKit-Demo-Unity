#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.IO;
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

		[MenuItem ("Window/" + CommonConsts.CONFIGURE_PUBLISHING_KIT_MENU_ITEM)]
		public static void ConfigurePublishingKit() {
			var window = EditorWindow.GetWindow<ConfigurationWindow>(new Type[] {EditorWindow.GetWindow(typeof(SceneView)).GetType()});
			window.Show(true);
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