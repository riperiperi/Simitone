using System;
using System.IO;

namespace Simitone.Windows.GameLocator
{
    public class MacOSLocator : ILocator
    {
        public string FindTheSimsOnline()
        {
            return "";
            //string localDir = @"../The Sims Online/TSOClient/";
            //if (File.Exists(Path.Combine(localDir, "tuning.dat"))) return localDir;

            //return string.Format("{0}/Documents/The Sims Online/TSOClient/", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        }

        public string FindTheSims1()
        {
            string localDir = @"../The Sims/";
            if (File.Exists(Path.Combine(localDir, "GameData", "Behavior.iff"))) return localDir;

            return "game1/";
        }
    }
}
