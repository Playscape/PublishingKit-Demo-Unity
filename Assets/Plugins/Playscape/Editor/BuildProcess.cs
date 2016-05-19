#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Playscape.Internal;

namespace Playscape.Editor
{
    /// <summary>
    /// A class responsible of managing the playscape SDK post-build logic
    /// </summary>
	abstract class BuildProcess : ITempFileProvider
    {		
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
		/// A delegate used to report the build failed
		/// </summary>
		/// <param name="sender">The sender of the event</param>
		public delegate void BuildFailed (object sender, String failedMessage);

        /// <summary>
        /// Invoked when the build is completed
        /// </summary>
        private BuildCompleted mBuildCompleted;

        /// <summary>
        /// Invoked when the build progress changes
        /// </summary>
        private BuildProgressChanged mBuildProgressChanged;

		/// <summary>
		/// Invoked when the build progress was failed
		/// </summary>
		private BuildFailed mBuildFailed;

        /// <summary>
        /// Holds this build params
        /// </summary>
		protected BuildParams mBuildParams;

        /// <summary>
        /// Holds a reference to the logger
        /// </summary>
		protected ILogger mLogger;

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
            BuildProgressChanged buildProgressChanged,
		    BuildFailed buildFailed)
        {
            if (logger == null) throw new ArgumentException("logger cannot be null");
            if (buildParams == null) throw new ArgumentException("buildParams cannot be null");

            mLogger = logger;
            mBuildCompleted = buildCompleted;
            mBuildProgressChanged = buildProgressChanged;
			mBuildFailed = buildFailed;
            this.mBuildParams = buildParams;
        }

        /// <summary>
        /// Applies the post-build logic on a certain apk
        /// </summary>
        /// <param name="file">The path to the APK</param>
		public abstract void  Build(string path);        

		/// <summary>
		/// Invokes the mBuildFailed delegate
		/// </summary>
		/// <param name="message">The message of the failed reason</param>
		protected void OnFailed(string message) 
		{
			if (mBuildFailed != null) {
				mBuildFailed(this, message);
			}
		}

        /// <summary>
        /// Invokes the mBuildProgressChanged delegate
        /// </summary>
        /// <param name="info">the information of the currnet step</param>
        /// <param name="percentage">The percentage of the work completed</param>
		protected void OnProgress(string info, int percentage)
        {
            if (mBuildProgressChanged != null)
            {
                mBuildProgressChanged(this, info, percentage);
            }
        }


        /// <summary>
        /// Invokes the onCompelted delegate
        /// </summary>
		protected void OnCompleted()
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

		protected virtual void retrieveGameConfig() {
			Configuration.GameConfigurationResponse gameConfigResponse = null;

			try {
				gameConfigResponse = ConfigurationInEditor.Instance.FetchGameConfigurationForApiKey (ConfigurationInEditor.Instance.MyAds.MyAdsConfig.ApiKey);

				//If response from servers is success save fetched configuration to AssetDatabse
				if (gameConfigResponse != null) {
					if (gameConfigResponse.Success) {
						ConfigurationInEditor.Instance.MyGameConfiguration = gameConfigResponse.GameConfiguration;

						//Saving new fetched game configuration to the file-system
						ConfigurationInEditor.Save();
					} else {
						OnFailed("Error!!! Could not retrieve configuration from the server. Message: " + gameConfigResponse.ErrorDescription);
						return;
					}
				}
			} catch (System.Net.WebException e) {
				System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)e.Response;

				if (response == null) {
					mLogger.W("Warning!!! Could not download game configuration. Please check your internet connection");
				} else {
					if ((int)response.StatusCode >= 400) {
						OnFailed("Error!!! Could not retrieve configuration from the server");
						return;
					}
				}
			}				
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
		protected void Cleanup()
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