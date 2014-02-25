using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Playscape.Internal {

	/// <summary>
	/// A logger that outputs into a file and and dumps the file later to a server.
	/// 
	/// Dump to server triggers:
	///  * Time based - when the time interval has been reached.
	///  * File size exceeds threshold (file size is checked at a fixed rate.
	/// </summary>
	public static class RemoteLogger {

	    private const string UNITY_REMOTE_LOGGER_TAG = "RemoteLogger-Unity";
	    
		public enum LogLevel {
			Verbose,
			Debug,
			Info,
			Warn,
			Error,
			Assert
		}

	    public static void Init() {
	#if !UNITY_EDITOR
	#if UNITY_IPHONE
	        PlayscapeRemoteLogger_init();
	#endif
	#endif
			Application.RegisterLogCallback(OnLog);
	    }

		static void OnLog (string condition, string stackTrace, LogType type)
		{
			if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) {
				Log (LogLevel.Error, "Unity-Error", string.Format(mUsCulture, "Unity-OnLog: Type: {0}\nCondition:{1}\nstackTrace:\n{2}\n", type, condition, stackTrace));
			}
		}

		public static void Log(LogLevel logLevel, string tag, string message) {

			L.D("[{0}] {1}: {2}", logLevel, tag, message);
	

	#if !UNITY_EDITOR
	#if UNITY_ANDROID
	        initializeAndroidRemoteLogger();
			remoteLoggerClass.CallStatic("log", (int) logLevel, tag, message);
	#elif UNITY_IPHONE
	        PlayscapeRemoteLogger_log((int) logLevel, tag, message);
	#endif
	#endif
	    }

		/// <summary>
		/// Dumps log to server.
		/// </summary>
	    public static void DumpNow() {
	#if !UNITY_EDITOR
	#if UNITY_ANDROID
	        initializeAndroidRemoteLogger();
			remoteLoggerClass.CallStatic("dumpNow");
	#elif UNITY_IPHONE
	        PlayscapeRemoteLogger_dumpNow();
	#endif
	#endif
	    }

		/// <summary>
		/// Sets the GMSID field.
		/// </summary>
		/// <param name="sessionId">Session identifier.</param>
		public static void SetGameSessionId(string sessionId) {
			L.D("GMSID updated: {0}", sessionId);
	#if !UNITY_EDITOR
	#if UNITY_ANDROID
			initializeAndroidRemoteLogger();
			remoteLoggerClass.CallStatic("setGameSessionId", sessionId);
	#elif UNITY_IPHONE
			PlayscapeRemoteLogger_setGameSessionId(sessionId);
	#endif
	#endif
		}
		
		// NETSID field
		/// <summary>
		/// Sets the NETSID field..
		/// </summary>
		/// <param name="sessionId">Session identifier.</param>
		public static void SetNetSessionId(string sessionId) {
			L.D("NETSID updated: {0}", sessionId);
	#if !UNITY_EDITOR
	#if UNITY_ANDROID
			initializeAndroidRemoteLogger();
			remoteLoggerClass.CallStatic("setNetSessionId", sessionId);
	#elif UNITY_IPHONE
			PlayscapeRemoteLogger_setNetSessionId(sessionId);
	#endif	
	#endif
		}
		
		// GMAUX field
		/// <summary>
		/// Sets the GMAUX field.
		/// </summary>
		/// <param name="varsJsonInStringified">Variables json in stringified.</param>
		public static void SetGameAuxVars(string varsJsonInStringified) {
			L.D("GMAUX updated: {0}", varsJsonInStringified);
	#if !UNITY_EDITOR
	#if UNITY_ANDROID
			initializeAndroidRemoteLogger();
			remoteLoggerClass.CallStatic("setGameAuxVars", varsJsonInStringified);
	#elif UNITY_IPHONE
			PlayscapeRemoteLogger_setGameAuxVars(varsJsonInStringified);
	#endif
	#endif
		}
		
		/// <summary>
		/// Sets the NETTIME field.
		/// </summary>
		/// <param name="networkTime">Network time.</param>
		public static void SetNetworkTime(int networkTime) {
			L.D("NETTIME updated: {0}", networkTime);
	#if !UNITY_EDITOR
	#if UNITY_ANDROID
			initializeAndroidRemoteLogger();
			remoteLoggerClass.CallStatic("setNetworkTime", networkTime);
	#elif UNITY_IPHONE
			PlayscapeRemoteLogger_setNetworkTime(networkTime);
	#endif
	#endif
		}
		
	    // LVSID field
		/// <summary>
		/// Sets the LVSID field.
		/// </summary>
		/// <param name="levelSessionId">Variables json in stringified.</param>
		public static void SetLevelSessionId(string levelSessionId) {
			L.D("LVSID updated: {0}", levelSessionId);
	    #if !UNITY_EDITOR
	    #if UNITY_ANDROID
	            initializeAndroidRemoteLogger();
	            remoteLoggerClass.CallStatic("setLevelSessionId", levelSessionId);
	    #elif UNITY_IPHONE
	            PlayscapeRemoteLogger_setLevelSessionId(levelSessionId);
	    #endif
	    #endif
	    }
	    
		private static readonly CultureInfo mUsCulture = CultureInfo.CreateSpecificCulture("en-US");

		public static void ReportAnalytics(string fmt, params object[] args) {
	        var userFormat = string.Format(fmt, args);
			var myFormat = string.Format(mUsCulture, "track:/game_{0}/{1}", Configuration.Instance.ReporterId, userFormat);
	            
	        Log(LogLevel.Info, UNITY_REMOTE_LOGGER_TAG, myFormat);

		}

	#if !UNITY_EDITOR
	#if UNITY_ANDROID
	    private static AndroidJavaClass remoteLoggerClass;
	    private static void initializeAndroidRemoteLogger()
	    {
	        if (remoteLoggerClass == null)
	        {
	            remoteLoggerClass = new AndroidJavaClass("com.mominis.logger.RemoteLogger");
	        }
	    }
	#elif UNITY_IPHONE
		[DllImport("__Internal")]
		private static extern void PlayscapeRemoteLogger_init();

		[DllImport("__Internal")]
		private static extern void PlayscapeRemoteLogger_log(
		int logLevel,
		string tag,
		string message);

		[DllImport("__Internal")]
		private static extern void PlayscapeRemoteLogger_dumpNow();

		[DllImport("__Internal", CharSet = CharSet.Ansi)]
		private static extern void PlayscapeRemoteLogger_setGameSessionId(string sessionId);

		[DllImport("__Internal", CharSet = CharSet.Ansi)]
		private static extern void PlayscapeRemoteLogger_setNetSessionId(string sessionId);

		[DllImport("__Internal", CharSet = CharSet.Ansi)]
		private static extern void PlayscapeRemoteLogger_setGameAuxVars(string varsJsonInStringified);

		[DllImport("__Internal")]
		private static extern void PlayscapeRemoteLogger_setNetworkTime(System.Int32 networkTime);
	    
	    [DllImport("__Internal")]
		private static extern void PlayscapeRemoteLogger_setLevelSessionId(string levelSessionId);
	#endif
	#endif

	}

}