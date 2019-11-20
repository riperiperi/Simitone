using System;
using System.IO;

namespace Simitone.Windows.GameLocator
{
    public class LinuxLocator : ILocator
    {
        public string FindTheSimsOnline()
        {
            return "";
            //string localDir = @"../The Sims Online/TSOClient/";
            //if (File.Exists(Path.Combine(localDir, "tuning.dat"))) return localDir;

            //return "game/TSOClient/";
        }

        public string FindTheSims1()
        {
            string localDir = @"../The Sims/";
            if (File.Exists(Path.Combine(localDir, "GameData", "Behavior.iff"))) return localDir;

            return "game1/";
        }
    }
}
