using UnityEngine;
using System.Collections;
using System.Xml;
using System.IO;
using System;

namespace Playscape.Editor {

	public class Settings {

		private static readonly string SETTINGS_XML_PATH = 
			"Assets/Plugins/Playscape/Editor/Settings.xml";

		public static string DebugRemoteLoggerUrl
		{
			get
			{
				return ReadStringValue("DebugRemoteLoggerUrl");
			}
		}

		public static string ReleaseRemoteLoggerUrl
		{
			get
			{
				return ReadStringValue("ReleaseRemoteLoggerUrl");
			}
		}

		private static string ReadStringValue(string valueName)
		{
			var doc = new XmlDocument();
			doc.LoadXml(File.ReadAllText(SETTINGS_XML_PATH));
			var node = doc.SelectSingleNode("/Settings/" + valueName);
			if (node != null) {
				return node.InnerText.Trim();
			} else {
				throw new Exception("Setting not found: " + valueName);
			}
		}

	}

}