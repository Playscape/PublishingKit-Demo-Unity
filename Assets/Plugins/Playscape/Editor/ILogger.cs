using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Playscape.Editor
{

    /// <summary>
    /// An interface for printing logs to an output
    /// </summary>
	public interface ILogger
	{    
        /// <summary>
        /// Sets or gets the current log level
        /// </summary>
		Playscape.Internal.L.LogLevel CurrentLogLevel { set; get; }
		
        /// <summary>
        /// Print a log in a severity level of Debug
        /// </summary>
        /// <param name="fmt">The format string</param>
        /// <param name="args">The arguments of the format string</param>
        void D(string fmt, params object [] args);

        /// <summary>
        /// Print a log in a severity level of Info
        /// </summary>
        /// <param name="fmt">The format string</param>
        /// <param name="args">The arguments of the format string</param>
        void I(string fmt, params object [] args);

        /// <summary>
        /// Print a log in a severity level of Warning
        /// </summary>
        /// <param name="fmt">The format string</param>
        /// <param name="args">The arguments of the format string</param>
        void W(string fmt, params object [] args);

        /// <summary>
        /// Print a log in a severity level of Error
        /// </summary>
        /// <param name="fmt">The format string</param>
        /// <param name="args">The arguments of the format string</param>
		void E(string fmt, params object [] args);

        /// <summary>
        /// Print a log in a severity level of Error
        /// </summary>
        /// <param name="e">The exception associated with the error</param>
        /// <param name="fmt">The format string</param>
        /// <param name="args">The arguments of the format string</param>
		void E(Exception e, string fmt, params object [] args);

        /// <summary>
        /// Print a log in a severity level of Assert
        /// </summary>
        /// <param name="fmt">The format string</param>
        /// <param name="args">The arguments of the format string</param>
		void A(string fmt, params object [] args);

        /// <summary>
        /// Print a log in a severity level of Verbose
        /// </summary>
        /// <param name="fmt">The format string</param>
        /// <param name="args">The arguments of the format string</param>
		void V(string fmt, params object [] args);
    }
}
