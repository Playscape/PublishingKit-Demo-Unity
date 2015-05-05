#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace Playscape.Editor
{
    /// <summary>
    /// A class responsible of managing the playscape SDK post-build logic
    /// </summary>
    public class BuildProcess : ITempFileProvider
    {
        /// <summary>
        /// The location of the patch file (aspectj hooks and logic)
        /// </summary>
        private const string PATCH_FILE = "Assets/Plugins/Playscape/ThirdParty/playscape_lifecycle.jar";

        /// <summary>
        /// A delegate used to report the build progress
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="info">A string describing the current step</param>
        /// <param name="percentage">The percentage of the progress (from 1 to 100)</param>
        public delegate void BuildProgressChanged(object sender, string info, int percentage);

        /// <summary>
        /// A delegate used to report the build completion
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        public delegate void BuildCompleted(object sender);

        /// <summary>
        /// Invoked when the build is completed
        /// </summary>
        private BuildCompleted mBuildCompleted;

        /// <summary>
        /// Invoked when the build progress changes
        /// </summary>
        private BuildProgressChanged mBuildProgressChanged;

        /// <summary>
        /// Holds this build params
        /// </summary>
        private BuildParams mBuildParams;

        /// <summary>
        /// Holds a reference to the logger
        /// </summary>
        private ILogger mLogger;

        /// <summary>
        /// A list of paths to clean
        /// </summary>
        List<string> mPaths = new List<string>();

        /// <summary>
        /// Constructs a new BuildProcess instance
        /// </summary>
        /// <param name="buildParams">The build parameters</param>
        /// <param name="logger">A reference to the logger</param>
        /// <param name="buildCompleted">The build completed event handler (optional)</param>
        /// <param name="buildProgressChanged">The build progress change event handler (optiopnal)</param>
        public BuildProcess(BuildParams buildParams, 
            ILogger logger,
            BuildCompleted buildCompleted, 
            BuildProgressChanged buildProgressChanged)
        {
            if (logger == null) throw new ArgumentException("logger cannot be null");
            if (buildParams == null) throw new ArgumentException("buildParams cannot be null");

            mLogger = logger;
            mBuildCompleted = buildCompleted;
            mBuildProgressChanged = buildProgressChanged;
            this.mBuildParams = buildParams;
        }

        /// <summary>
        /// Applies the post-build logic on a certain apk
        /// </summary>
        /// <param name="file">The path to the APK</param>
        public void Build(string file)
        {
            try
            {
                AndroidApkCreator apkCreator = new AndroidApkCreator(mBuildParams, mLogger, this);
                Stopwatch sw = new Stopwatch();
                sw.Start();

                // dex2jar can oonly work with .jar extentions
                string extractedPath = GetNewTempFolder();
                string dexFilePath = extractedPath + "/classes.dex";
                string classesJarFile = GetNewTempFolder("jar");
                string unifiedLibJarFile = GetNewTempFolder("jar");
                string patchedClassesJarFile = GetNewTempFolder("jar");
                string alignedFile = GetNewTempFolder();

                //1. unzip the apk
                //2. extract the dex and convert it into .jar file
                //3. unify the libraries with the patcher
                //4. apply ajc to the .jar (aspectj compiler)
                //5. convert back the .jar into dex
                //6. package the apk
                //7. sign and run zipalign
                OnProgress("Extracting resrouces", 10);
                mLogger.V("BuildProcess - Build - UnzipAPK " + sw.ElapsedMilliseconds);
                apkCreator.ExtractEntry(file, "classes.dex", extractedPath);

                OnProgress("Extracting jar files", 20);
                mLogger.V("BuildProcess - Build - executing dex2jar " + sw.ElapsedMilliseconds);
                apkCreator.Dex2jar(dexFilePath, classesJarFile);
                
                OnProgress("Unifying libraries", 20);
                mLogger.V("BuildProcess - Build - unify libs " + sw.ElapsedMilliseconds);
                apkCreator.unifyLibs(classesJarFile, PATCH_FILE, unifiedLibJarFile);
                
                OnProgress("Applying analytics", 30);
                mLogger.V("BuildProcess - Build - Apply Patch " + sw.ElapsedMilliseconds);
                apkCreator.applyPatch(unifiedLibJarFile, unifiedLibJarFile, patchedClassesJarFile);
                
                OnProgress("Re-dexing libraries", 60);
                mLogger.V("BuildProcess - Build - Executing lib2dex " + sw.ElapsedMilliseconds);
                apkCreator.Jar2dex(patchedClassesJarFile, dexFilePath);

                OnProgress("Packaging apk", 80);
                mLogger.V("BuildProcess - Build - archiving sources " + sw.ElapsedMilliseconds);
                apkCreator.AddFileToZip(file, dexFilePath, null);

                OnProgress("Signing apk", 87);
                mLogger.V("BuildProcess - Build - signing APK " + sw.ElapsedMilliseconds);
                apkCreator.signApk(file);

                OnProgress("running zipalign on apk", 95);
                mLogger.V("BuildProcess - Build - zip align " + sw.ElapsedMilliseconds);
                apkCreator.applyZipalign(file, alignedFile);
                File.Delete(file);
                File.Move(alignedFile, file);

                OnProgress("Cleaning up", 98);

                sw.Stop();
            } 
            finally
            {
                // cleanup the files and call the onComplete delegate
                Cleanup();
                OnProgress("Done", 100);
                OnCompleted();
            }            
        }

        /// <summary>
        /// Invokes the mBuildProgressChanged delegate
        /// </summary>
        /// <param name="info">the information of the currnet step</param>
        /// <param name="percentage">The percentage of the work completed</param>
        private void OnProgress(string info, int percentage)
        {
            if (mBuildProgressChanged != null)
            {
                mBuildProgressChanged(this, info, percentage);
            }
        }


        /// <summary>
        /// Invokes the onCompelted delegate
        /// </summary>
        private void OnCompleted()
        {
            if (mBuildCompleted != null)
            {
                mBuildCompleted(this);
            }
        }

        /// <summary>
        /// An internal method used by buildAsync to kick-off the build process
        /// </summary>
        /// <param name="file">A string - the path of the APK</param>
        private void startBuild(object file)
        {
            Build((string)file);
        }

        /// <summary>
        /// Applies the post-build logic in an async manner
        /// </summary>
        /// <param name="file"></param>
        public void BuildAsync(string file)
        {
            Thread t = new Thread(new ParameterizedThreadStart(startBuild));
            t.Start(file);
        }

        /// <summary>
        /// Gets a new temp folder or file and saves it for later deletion
        /// </summary>
        /// <param name="extension">The extention of the requreid file</param>
        /// <returns>A name of a temporary file or folder</returns>
        public string GetNewTempFolder(string extension)
        {
            if (extension != null) extension = "." + extension;
            string newPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + extension);
            mPaths.Add(newPath);
            return newPath;
        }

        /// <summary>
        /// Gets a new temp folder or file and saves it for later deletion
        /// </summary>
        /// <returns>A name of a temporary file or folder</returns>
        public string GetNewTempFolder()
        {
            return GetNewTempFolder(string.Empty);
        }

        /// <summary>
        /// Cleans up all temp folders
        /// </summary>
        private void Cleanup()
        {
            foreach (string path in mPaths)
            {
                try
                {
                    FileAttributes attr = File.GetAttributes(path);
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        Directory.Delete(path, true);
                    }
                    else
                    {
                        File.Delete(path);
                    }
                }
                catch (Exception e)
                {
                    mLogger.W("Cleanup error - could not delete one of the temporary paths " + e.Message);
                }
            }
        }
    }

    public interface ITempFileProvider
    {
        string GetNewTempFolder();

        string GetNewTempFolder(string extension);
    }
}
#endif