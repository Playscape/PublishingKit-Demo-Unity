#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
#if UNITY_5
using UnityEditor.iOS.Xcode;
#else 
using UnityEditor.XCodeEditor;
#endif
using System.IO;
using System.Collections;
using System.Collections.Generic;
 
namespace Playscape.Internal {

	/// <summary>
	/// Xcode project mod. Edits xcode project files
	/// </summary>
	public class XcodeProjectMod 
	{
		private string path;
		private List<string> frameworks;
		private string folderName;
		private List<string> files;
		private List<string> otherLinkerFlags;
		private List<string> libs;

		public XcodeProjectMod(string path) {
			this.path = path;
			this.frameworks = new List<string> ();
			this.files = new List<string> ();
			this.otherLinkerFlags = new List<string> ();
			this.libs = new List<string> ();
			this.folderName = "Frameworks";
		}

		/// <summary>
		/// Sets the folder.
		/// </summary>
		/// <param name="folderName">Folder name.</param>
		public void setFolder(string folderName) {
			this.folderName = folderName;
		}

		/// <summary>
		/// Adds the framework.
		/// </summary>
		/// <param name="framework">Framework.</param>
		public void AddFramework(string framework) {
			if (!string.IsNullOrEmpty (framework)) {
				frameworks.Add(framework);
			}
		}

		/// <summary>
		/// Adds the file.
		/// </summary>
		/// <param name="file">File.</param>
		public void addFile(string file) {
			if (!string.IsNullOrEmpty (file)) {
				files.Add(file);
			}
		}
	 	 
		/// <summary>
		/// Adds the linker flag.
		/// </summary>
		/// <param name="flag">Flag.</param>
		public void AddLinkerFlag(string flag) {
			if (!string.IsNullOrEmpty (flag)) {
				otherLinkerFlags.Add(flag);
			}
		}

		/// <summary>
		/// Copies the and replace directory.
		/// </summary>
		/// <param name="srcPath">Source path.</param>
		/// <param name="dstPath">Dst path.</param>
	    void CopyAndReplaceDirectory(string srcPath, string dstPath)
	    {
	        if (Directory.Exists(dstPath))
	            Directory.Delete(dstPath);
	        if (File.Exists(dstPath))
	            File.Delete(dstPath);
	 
	        Directory.CreateDirectory(dstPath);
	 
	        foreach (var file in Directory.GetFiles(srcPath))
	            File.Copy(file, Path.Combine(dstPath, Path.GetFileName(file)));
	 
	        foreach (var dir in Directory.GetDirectories(srcPath))
	            CopyAndReplaceDirectory(dir, Path.Combine(dstPath, Path.GetFileName(dir)));
	    }

		/// <summary>
		/// Adds the usr lib.
		/// </summary>
		/// <param name="lib">Lib.</param>
		public void AddUsrLib(string lib) {
			if (!string.IsNullOrEmpty (lib)) {
				libs.Add(lib);
			}
		}
		 
		/// <summary>
		/// Apply this instance.
		/// </summary>
	    public void Apply()
	    {
#if UNITY_5
			ApplyWithUnity5 (path);
#else
			ApplyWithUnity4 (path);
#endif
						      
	    }

		private void ApplyWithUnity4(string path) {
#if UNITY_4
			XCProject proj = new XCProject (path);

			PBXGroup group = proj.GetGroup(folderName);
			PBXGroup frameworkGroup = proj.GetGroup (folderName);		
					
			foreach (string usrlib in libs) {
				string completeLibPath = System.IO.Path.Combine( "usr/lib", usrlib);
				proj.AddFile( completeLibPath, group, "SDKROOT", true, false);
			}
					
			//add system frameworks to ios project
			foreach (string framework in frameworks) {
				string completePath = System.IO.Path.Combine( "System/Library/Frameworks", framework);
				proj.AddFile(completePath, frameworkGroup, "SDKROOT", true, false);
			}

			//add files to project
			string folderInProject = Path.Combine(path, folderName);
			foreach (string file in files) {
				string filename = Path.GetFileName(file);
				string absoluteFilePath = System.IO.Path.Combine(folderInProject, filename);
								
				CopyAndReplaceDirectory(file, absoluteFilePath);

				if( file.EndsWith(".framework") )
					proj.AddFile ( absoluteFilePath, frameworkGroup, "GROUP", true, false);
				else
					proj.AddFile ( absoluteFilePath, group);
			}

			proj.AddFrameworkSearchPaths(Path.Combine("$(SRCROOT)", folderName));
		
			//adding other linker flagas to ios proejct build settings
			foreach (string flag in otherLinkerFlags) {
				proj.AddOtherLDFlags (flag);
			}

			proj.Save ();
#endif
		}

		private void ApplyWithUnity5(string path) {
#if UNITY_5
			string projPath = Path.Combine(path, "Unity-iPhone.xcodeproj/project.pbxproj");
			PBXProject proj = new PBXProject();
			
			proj.ReadFromString(File.ReadAllText(projPath));
			string target = proj.TargetGuidByName("Unity-iPhone");
			
			//add system frameworks to ios project
			foreach (string framework in frameworks) {
				proj.AddFrameworkToProject(target, framework, false);
			}
			
			//add files to project
			string folderInProject = Path.Combine(path, folderName);
			foreach (string file in files) {
				string filename = Path.GetFileName(file);
				string projFilename = Path.Combine(folderName, filename);
				
				CopyAndReplaceDirectory(file, Path.Combine(folderInProject, filename));
				proj.AddFileToBuild(target, proj.AddFile(projFilename, projFilename, PBXSourceTree.Source));
			}
			
			//add framework search path
			proj.SetBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", "$(inherited)");
			proj.AddBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", string.Format("$(PROJECT_DIR)/{0}", folderName));
			
			
			foreach (string usrlib in libs) {
				string fileGuid = proj.AddFile("usr/lib/" + usrlib, string.Format("{0}/{1}", folderName, usrlib), PBXSourceTree.Sdk);
				proj.AddFileToBuild(target, fileGuid);
			}
			
			//adding other linker flagas to ios proejct build settings
			foreach (string flag in otherLinkerFlags) {
				proj.AddBuildProperty (target, "OTHER_LDFLAGS", flag);
			}
			
			//save project
			File.WriteAllText(projPath, proj.WriteToString());
#endif
		}
	}
}
#endif
