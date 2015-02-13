#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Xml;
using System.IO;
using Playscape.Internal;
using System;

namespace Playscape.Editor {
	static class AndroidManifestMerger  {
		private const string ANDROID_XMLNS = "http://schemas.android.com/apk/res/android";
		
		/// <summary>
		/// Merges manifests, rules of merging are defined in this method.
		/// </summary>
		/// <param name="destinationManifest">Full path to AndroidManifest.xml created by Unity or copied from Android dir upon build</param>
		public static void Merge (string destinationManifest) {
			var destXdoc = new XmlDocument ();
			var psXdoc = new XmlDocument ();

			string psManifestPath = Directory.GetParent (Directory.GetParent (Environment.CurrentDirectory).FullName).FullName + "/unitypackage/" + CommonConsts.PLAYSCAPE_MANIFEST_PATH;
	
			psXdoc.LoadXml (File.ReadAllText (psManifestPath));
			destXdoc.LoadXml (File.ReadAllText (destinationManifest));
			 
			// ---------- Remove default main activity -------- //
			var mainActivity  = destXdoc.SelectSingleNode ("manifest/application/activity/intent-filter[contains(name,android.intent.action.MAIN)]");
			if (mainActivity != null) {
				mainActivity = mainActivity.ParentNode;
				var oldMainActivityName = "UnknownActivity";
				if (mainActivity.Attributes["android:name"] != null) {
					oldMainActivityName = mainActivity.Attributes["android:name"].Value;
				}

				L.W ("{0}. Previous Activity: {1}", Warnings.MAIN_ACTIVITY_REPLACED, oldMainActivityName); 
				mainActivity.ParentNode.RemoveChild(mainActivity);
			}

			// ---------- Merge Playscape -------------- //
			copyAllManifestTags (psXdoc, destXdoc);
			copyAllApplicationTags (psXdoc, destXdoc);
			
			// Copy application's android:name tag
			var destAppNode = destXdoc.SelectSingleNode ("manifest/application");
			var psAppNode = psXdoc.SelectSingleNode ("manifest/application");
			
			if (destAppNode.Attributes ["android:name"] != null) {
				L.W (Warnings.ANDROID_NAME_EXISTS_IN_MANIFEST);
			} else {
				var destAppNameAttrib = destXdoc.CreateAttribute ("android:name", ANDROID_XMLNS);
				destAppNode.Attributes.Append (destAppNameAttrib);
				destAppNode.Attributes ["android:name"].Value = psAppNode.Attributes ["android:name"].Value;
			}

			if (Debug.isDebugBuild) {
				var debuggableAttrib = destXdoc.CreateAttribute ("android:debuggable", ANDROID_XMLNS);
				destAppNode.Attributes.Append (debuggableAttrib);
				destAppNode.Attributes ["android:debuggable"].Value = "true";
			}
			
			XmlWriterSettings settings = new XmlWriterSettings ();
			settings.Indent = true;

			File.Copy (destinationManifest, destinationManifest + ".bak", true);
			using (var writer = XmlWriter.Create(destinationManifest, settings)) {
				destXdoc.Save (writer);
			}
			
		}
		
		/// <summary>
		/// Clones the xml under new context.
		/// The returned xml node can be later manipulated by the document specified by the newContext argument.
		/// </summary>
		/// <returns>The xml under new context.</returns>
		/// <param name="node">Node.</param>
		/// <param name="newContext">New context.</param>
		private static XmlNode cloneXmlUnderNewContext(XmlNode node, XmlDocument newContext) {
			var newNode = newContext.CreateNode(node.NodeType, node.Prefix, node.Name, node.NamespaceURI);
			if (node.Attributes != null) {
				foreach (XmlAttribute attrib in node.Attributes) {
					var srcAttrib = newContext.CreateAttribute (attrib.Name, attrib.NamespaceURI);
					srcAttrib.Value = attrib.Value;
					newNode.Attributes.Append (srcAttrib);
				}
			}
			if (node.Value != null) {
				newNode.Value = node.Value;
			}
			
			if (string.IsNullOrEmpty(node.InnerXml) == false) {
				newNode.InnerXml = node.InnerXml;
			}
			
			return newNode;
		}
		
		/// <summary>
		/// Copies all manifest tags except application.
		/// </summary>
		/// <param name="fromXdoc">From xdoc.</param>
		/// <param name="toXdoc">To xdoc.</param>
		private static void copyAllManifestTags(XmlDocument fromXdoc, XmlDocument toXdoc) {
			var toAppNode = toXdoc.SelectSingleNode ("manifest/application");
			
			var toManifestNode = toXdoc.SelectSingleNode ("manifest");
			var fromManifestNode = fromXdoc.SelectSingleNode ("manifest");
			
			bool beforeApplication = true;
			foreach (XmlNode node in fromManifestNode.ChildNodes) {
				if (node.Name == "application") {
					beforeApplication = false;
					continue;
				}
				
				var newNode = cloneXmlUnderNewContext(node, toXdoc);
				
				if (!isDirectChildOf(newNode, toManifestNode)) {
					
					// Android's lint get's annoyed if some tags appear after <application> so we maintain order.
					if (beforeApplication) {
						toManifestNode.InsertBefore(newNode, toAppNode);
					} else {
						toManifestNode.InsertAfter(newNode, toAppNode);
					}
				}
			}
		}
		
		private static void copyAllApplicationTags(XmlDocument fromXdoc, XmlDocument toXdoc) {
			var toAppNode = toXdoc.SelectSingleNode ("manifest/application");
			var fromAppNode = fromXdoc.SelectSingleNode ("manifest/application");
			
			foreach (XmlNode node in fromAppNode.ChildNodes) {
				
				var newNode = cloneXmlUnderNewContext(node, toXdoc);
				toAppNode.AppendChild(newNode);
			}
			
		}
		
		/// <summary>
		/// Check if child is a direct descandant of parent
		/// </summary>
		/// <returns><c>true</c>, if parent is a direct parent of child <c>false</c> otherwise.</returns>
		/// <param name="child">Child.</param>
		/// <param name="parent">Parent.</param>
		private static bool isDirectChildOf(XmlNode child, XmlNode parent) {
			foreach (XmlNode c in parent.ChildNodes) {
				if (child.Name == c.Name) {
					
					if (c.Attributes == null && child.Attributes != null) {
						continue;
					}
					
					if (c.Attributes != null && child.Attributes == null) {
						continue;
					}
					
					if (c.Attributes != null) {
						if (c.Attributes.Count != child.Attributes.Count) {
							continue;
						}
						
						bool attribsMatch = true;
						foreach (XmlAttribute a in child.Attributes) {
							if (c.Attributes[a.Name] == null) {
								attribsMatch = false;
							}
							
							if (c.Attributes[a.Name].Value != a.Value) {
								attribsMatch = false;
							}
						}
						
						if (!attribsMatch) {
							continue;
						}
					}
					
					if (c.Value != child.Value) {
						continue;
					}
					
					if (c.NamespaceURI != child.NamespaceURI) {
						continue;
					}
					
					
					return true;
				}
			}
			
			return false;
			
		}
	}
}
#endif