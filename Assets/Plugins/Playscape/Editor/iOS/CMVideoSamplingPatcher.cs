//-------------------------------------------------------------
//
//  CMVideoSamplingPatcher
//  Copyright © 2014 Egomotion Limited
//
//  When using Xcode 6 and Unity 4.3.4 or earlier, iOS builds
//  fail due to a missing include in CMVideoSampling.mm.
//  
//  This PostProcessBuild script adds the glext.h header
//  to the CMVideoSampling.mm file if it is missing.
//
//-------------------------------------------------------------

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class CMVideoSamplingPatcher : Editor
{
	const string _patchLine    = "#include <OpenGLES/ES2/glext.h>";
	const string _locationLine = "#include <OpenGLES/ES2/gl.h>";
	
	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
	{
		if (target != BuildTarget.iPhone) {
			return;
		}
		
		var dirInfo = Directory.GetFiles(pathToBuiltProject, "CMVideoSampling.mm", SearchOption.AllDirectories);
		
		if (dirInfo == null || dirInfo.Length <= 0) {
			Debug.LogError("Could not find CMVideoSampling.mm");
			return;
		}
		
		var cmSamplingPath = dirInfo[0];
		var content = new List<string>(File.ReadAllLines(cmSamplingPath));
		
		int index = 0;
		var doPatch = true;
		
		for (int ii = 0; ii < content.Count; ++ii)
		{
			var line = content[ii];
			
			if (line.Contains(_patchLine)) {
				doPatch = false;
				break;
			}
			if (line.Contains(_locationLine)) {
				index = ii+1;
			}
		}
		
		if (doPatch)
		{
			Debug.Log("Patching CMVideoSampling.mm");
			content.Insert(index, _patchLine);
			File.WriteAllLines(cmSamplingPath, content.ToArray());
		}
		else {
			Debug.Log("CMVideoSampling.mm patch already applied. Skipping.");
		}
	}
}
#endif