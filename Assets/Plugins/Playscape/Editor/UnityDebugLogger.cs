using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Playscape.Internal;

namespace Playscape.Editor
{
    /// <summary>
    /// An implemetnation of that uses the Playscape wrapper for Unity
    /// </summary>
	class UnityDebugLogger : ILogger
	{
        public L.LogLevel CurrentLogLevel
        {
            get
            {
                return L.CurrentLogLevel;
            }
            set
            {
                L.CurrentLogLevel = value;
            }
        }

        public void D(string fmt, params object[] args)
        {
            L.D(fmt, args);
        }

        public void I(string fmt, params object[] args)
        {
            L.I(fmt, args);
        }

        public void W(string fmt, params object[] args)
        {
            L.W(fmt, args);
        }

        public void E(string fmt, params object[] args)
        {
            L.E(fmt, args);
        }

        public void E(Exception e, string fmt, params object[] args)
        {
            L.E(e, fmt, args);
        }

        public void A(string fmt, params object[] args)
        {
            L.A(fmt, args);
        }

        public void V(string fmt, params object[] args)
        {
            L.V(fmt, args);
        }
    }
}
