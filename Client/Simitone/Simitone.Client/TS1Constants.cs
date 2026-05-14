using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simitone.Client
{
    internal static class GameplaySpeed
    {
        internal const int Pause = 4,
                           Play = 1,
                           FastForward = 2,
                           FastFastForward = 3,
                           Disabled = -1;        

        /// <summary>
        /// Maps <c>VMSpeedMultiplier</c> settings to constants defined in <see cref="GameplaySpeed"/>
        /// </summary>
        internal static Dictionary<int, int> RemapSpeed = new Dictionary<int, int>()
        {
            {0, Pause}, //pause
            {1, Play}, //1 speed
            {3, FastForward}, //2 speed
            {10, FastFastForward}, //3 speed
            {-1, Disabled } // disabled (buy/build,etc.)
        };
        /// <summary>
        /// Maps constants defined in <see cref="GameplaySpeed"/> to <c>VMSpeedMultiplier</c> settings
        /// </summary>
        internal static Dictionary<int, int> ReverseRemap = GameplaySpeed.RemapSpeed.ToDictionary(x => x.Value, x => x.Key);
    }
}
