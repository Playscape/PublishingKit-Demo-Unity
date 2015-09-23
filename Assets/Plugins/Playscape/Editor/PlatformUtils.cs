using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Playscape.Editor
{
    public class PlatformUtils
    {

        public static Boolean isMac()
        {
            PlatformID pid = getPlatfomId();
            return pid == PlatformID.MacOSX;
        }

        public static Boolean isUnix()
        {
            PlatformID pid = getPlatfomId();
            return pid == PlatformID.Unix;
        }

        public static Boolean isWindows()
        {
            PlatformID pid = getPlatfomId();
            return pid == PlatformID.Win32NT || pid == PlatformID.Win32S || pid == PlatformID.Win32Windows || pid == PlatformID.WinCE;
        }

        public static PlatformID getPlatfomId()
        {
            OperatingSystem os = Environment.OSVersion;
            return os.Platform;
        }

        public static string qualifyPath(string str)
        {
            if (isWindows())
            {
                str = "\"" + str + "\"";
            }
            return str;
        }

		public static string PathDelimeter {
			get {
				string delimeter = ":";

				if (isWindows()) {
					delimeter = ";";
				}

				return delimeter;
			}
		}
    }
}
