using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simitone.Windows.GameLocator
{
    public class LinuxLocator : ILocator
    {
        public string FindTheSimsOnline()
        {
            return "game/TSOClient/";
        }

        public string FindTheSims1()
        {
            return "game1/";
        }
    }
}
