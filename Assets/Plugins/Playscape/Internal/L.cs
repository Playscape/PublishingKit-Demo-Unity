using UnityEngine;
using System;

namespace Playscape.Internal
{
	public static class L {

		public enum LogLevel {
			Verbose,
			Debug,
			Info,
			Warn,
			Error,
			Assert
		}

		private static LogLevel mCurrentLogLevel = LogLevel.Info;

		public static LogLevel CurrentLogLevel {
            get
            {
                return mCurrentLogLevel;
            }
			set {
				mCurrentLogLevel = value;
			}
		}

		public static void D(string fmt, params object [] args) {
			if (mCurrentLogLevel <= LogLevel.Debug) {
				Debug.Log(FormatLog(LogLevel.Debug, fmt, args));
			}
		}

		public static void I(string fmt, params object [] args) {
			if (mCurrentLogLevel <= LogLevel.Info) {
				Debug.Log(FormatLog(LogLevel.Info, fmt, args));
			}
		}

		public static void W(string fmt, params object [] args) {
			if (mCurrentLogLevel <= LogLevel.Warn) {
				Debug.LogWarning(FormatLog(LogLevel.Warn, fmt, args));
			}
		}

		public static void E(string fmt, params object [] args) {
			if (mCurrentLogLevel <= LogLevel.Error) {
				Debug.LogError(FormatLog(LogLevel.Error, fmt, args));
			}
		}

		
		public static void E(Exception e, string fmt, params object [] args) {
			if (mCurrentLogLevel <= LogLevel.Error) {
				Debug.LogError(FormatLog(LogLevel.Error, fmt, args));
				Debug.LogException(e);
			}
		}

		public static void A(string fmt, params object [] args) {
			if (mCurrentLogLevel <= LogLevel.Assert) {
				Debug.Log(FormatLog(LogLevel.Assert, fmt, args));
			}
		}

		public static void V(string fmt, params object [] args) {
			if (mCurrentLogLevel <= LogLevel.Verbose) {
				Debug.Log(FormatLog(LogLevel.Verbose, fmt, args));
			}
		}

		private static string FormatLog(LogLevel logLevel, string fmt, params object [] args) {
			string message = "null";
			if (fmt != null) {
				message = string.Format(fmt, args);
			}

			return string.Format("[PS-{0}] {1}", logLevel, message);
		}
	}
}

