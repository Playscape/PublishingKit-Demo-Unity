#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;
using System;
using System.IO;
using Playscape.Internal;

namespace Playscape.Editor {

	/// <summary>
	/// When working with configuration via the editor be sure to use this class, it will ensure the creation of the configuration asset file.
	/// </summary>
	public class ConfigurationInEditor : MonoBehaviour {
		
		/** Google Cloud Messanging Id */
		public const string PLAYSCAPE_GCM_SENDER_ID = "786132690907";

		public static Configuration Instance {
			get {

				if (Configuration.Instance == null) {
					// There isn't a serialized file, create a new one (will only happen during Editing, not in runtime)

					if (Configuration.Instance == null) {
						string dirPath = Path.GetDirectoryName(Playscape.Internal.Configuration.CONFIGURATION_PATH);
						if (!Directory.Exists(dirPath)) {
							Directory.CreateDirectory(dirPath);
							AssetDatabase.ImportAsset(Configuration.CONFIGURATION_PATH);

						}

						Configuration.Instance = (Configuration)ScriptableObject.CreateInstance(typeof(Configuration).Name);
						if (Configuration.Instance != null) {
							AssetDatabase.CreateAsset(Configuration.Instance, Configuration.CONFIGURATION_PATH);
							L.D ("Create new configuration file");
						}
					}
				}

				return Configuration.Instance;
			}
		}
	}
}
#endif