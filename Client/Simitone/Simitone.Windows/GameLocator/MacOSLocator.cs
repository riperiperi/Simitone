using System;

namespace Simitone.Windows.GameLocator
{
    public class MacOSLocator : ILocator
    {
        public string FindTheSimsOnline()
        {
            return string.Format("{0}/Documents/The Sims Online/TSOClient/", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        }

        public string FindTheSims1()
        {
            return "game/The Sims";
        }
    }
}
