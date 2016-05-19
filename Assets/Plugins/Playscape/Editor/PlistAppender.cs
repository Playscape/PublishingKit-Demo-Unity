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

	class PlistAppender {

		private readonly string filePath;
		private string plistContents;

		public PlistAppender(string filePath)
		{
			this.filePath = filePath;
			if (File.Exists(filePath))
			{
				plistContents = File.ReadAllText(filePath);
			}
			else
			{
				plistContents = 
					@"<?xml version=""1.0"" encoding=""UTF-8""?>
					  <!DOCTYPE plist PUBLIC
								""-//Apple//DTD PLIST 1.0//EN""
								""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
					  <plist version=""1.0"">
						<dict>
						</dict>
					  </plist>
					";
			}
		}

		public void AddString(string key, string value)
		{
			AppendFormattedText("\t<key>{0}</key>\n\t<string>{1}</string>", key, value);
		}

		public void AddNumber(string key, int value)
		{
			AppendFormattedText("\t<key>{0}</key>\n\t<integer>{1}</integer>", key, value);
		}

		public void AddBool(string key, bool value)
		{
			AppendFormattedText("\t<key>{0}</key>\n\t{1}", key, value ? "<true/>" : "<false/>");
		}

		public void AppendFragment(string filePath)
		{
			AppendRawText(File.ReadAllText(filePath));
		}

		public void Save()
		{
			File.WriteAllText(filePath, plistContents);
		}

		private void AppendFormattedText(string addedText, params object[] values)
		{
			AppendRawText(String.Format(addedText, values));
		}

		private void AppendRawText(string addedText)
		{
			string dictCloseTag = "</dict>";
			int lastIndex = plistContents.LastIndexOf (dictCloseTag);

			if (lastIndex != -1) {
				plistContents = plistContents.Insert (lastIndex, "\n" + addedText + "\n");
			}				
		}

	}
	
}
#endif