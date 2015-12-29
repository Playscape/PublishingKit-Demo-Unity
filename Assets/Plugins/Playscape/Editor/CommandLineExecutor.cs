using System;
using System.Diagnostics;
using Playscape.Editor;


namespace Playscape.Internal
{
	/// <summary>
	/// Output.
	/// </summary>
	public class Output {

		/// <summary>
		/// Gets the exit code.
		/// </summary>
		/// <value>The exit code.</value>
		public int ExitCode { get; private set; }

		/// <summary>
		/// Gets the error description.
		/// </summary>
		/// <value>The error description.</value>
		public string ErrorDescription { get; private set; }

		public Output(int exitCode, string errorDescription) {
			this.ExitCode = exitCode;
			this.ErrorDescription = errorDescription;
		}
	}

	/// <summary>
	/// Command line executor.
	/// </summary>
	public class CommandLineExecutor
	{
		private ILogger mLogger;

		/// <summary>
		/// Initializes a new instance of the <see cref="Playscape.Internal.CommandLineExecutor"/> class.
		/// </summary>
		/// <param name="logger">Logger.</param>
		public CommandLineExecutor (ILogger logger)
		{
			mLogger = logger;
		}


		/// <summary>
		/// Execute the specified command and args.
		/// </summary>
		/// <param name="command">Command.</param>
		/// <param name="args">Arguments.</param>
		public Output Execute(string command, string args) {
			mLogger.V(string.Format("Running {0} with arguments {1}", command, args));
			
			var processInfo = new ProcessStartInfo (command, args)
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};
			Process proc;
			
			if ((proc = Process.Start(processInfo)) == null)
			{
				throw new InvalidOperationException("Can not start new process with command: " + command + " in AndroidApkCreator.");
			}
			
			string standardOutput = proc.StandardOutput.ReadToEnd();
			
			if (string.IsNullOrEmpty(standardOutput)) {
				standardOutput = proc.StandardError.ReadToEnd();
			}
			
			proc.WaitForExit();
			
			int exitCode = proc.ExitCode;
			
			if (exitCode != 0)
			{
				mLogger.W("process failed with error {0}", standardOutput);
			}
			proc.Close();

			return new Output(exitCode, string.IsNullOrEmpty(standardOutput) ? "Unknown" : standardOutput);
		}
	}
}

