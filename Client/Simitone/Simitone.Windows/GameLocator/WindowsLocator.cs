﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Simitone.Windows.GameLocator
{
    public class WindowsLocator : ILocator
    {
        public string FindTheSimsOnline()
        {
            return "";
            //string Software = "";

            //using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            //{
            //    //Find the path to TSO on the user's system.
            //    RegistryKey softwareKey = hklm.OpenSubKey("SOFTWARE");

            //    if (Array.Exists(softwareKey.GetSubKeyNames(), delegate (string s) { return s.Equals("Maxis", StringComparison.InvariantCultureIgnoreCase); }))
            //    {
            //        RegistryKey maxisKey = softwareKey.OpenSubKey("Maxis");
            //        if (Array.Exists(maxisKey.GetSubKeyNames(), delegate (string s) { return s.Equals("The Sims Online", StringComparison.InvariantCultureIgnoreCase); }))
            //        {
            //            RegistryKey tsoKey = maxisKey.OpenSubKey("The Sims Online");
            //            string installDir = (string)tsoKey.GetValue("InstallDir");
            //            installDir += "\\TSOClient\\";
            //            return installDir.Replace('\\', '/');
            //        }
            //    }
            //}
            //return @"C:\Program Files\Maxis\The Sims Online\TSOClient\".Replace('\\', '/');
        }

        public string FindTheSims1()
        {
            // Search relative directory similar to how macOS and Linux works; allows portability
            string localDir = @"../The Sims/";
            if (File.Exists(Path.Combine(localDir, "GameData", "Behavior.iff"))) return localDir;

            using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                //Find the path to TS1 on the user's system.
                RegistryKey softwareKey = hklm.OpenSubKey("SOFTWARE");

                if (Array.Exists(softwareKey.GetSubKeyNames(), delegate (string s) { return s.Equals("Maxis", StringComparison.InvariantCultureIgnoreCase); }))
                {
                    RegistryKey maxisKey = softwareKey.OpenSubKey("Maxis");
                    if (Array.Exists(maxisKey.GetSubKeyNames(), delegate (string s) { return s.Equals("The Sims", StringComparison.InvariantCultureIgnoreCase); }))
                    {
                        RegistryKey tsoKey = maxisKey.OpenSubKey("The Sims");
                        string installDir = (string)tsoKey.GetValue("InstallPath");
                        installDir += "\\";
                        return installDir.Replace('\\', '/');
                    }
                }
            }

        // Fall back to steam folders
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo d in allDrives)
            {
                if (d.Name =="C:")
                {
                    if (Directory.Exists(@"C:\Program Files (x86)\Steam\steamapps\common\The Sims Legacy\"))
                        return @"C:\Program Files (x86)\Steam\steamapps\common\The Sims Legacy\".Replace('\\', '/');

                }
                else if (d.Name == "D:")
                {
                    if (Directory.Exists(@"D:\Program Files (x86)\Steam\steamapps\common\The Sims Legacy\"))
                        return @"D:\Program Files (x86)\Steam\steamapps\common\The Sims Legacy\".Replace('\\', '/');

                }
            }
            // Fall back to the default install location if the other two checks fail
            return @"C:\Program Files (x86)\Maxis\The Sims\".Replace('\\', '/');
        }

        private static bool is64BitProcess = (IntPtr.Size == 8);
        private static bool is64BitOperatingSystem = is64BitProcess || InternalCheckIsWow64();

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(
            [In] IntPtr hProcess,
            [Out] out bool wow64Process
        );

        /// <summary>
        /// Determines if this process is run on a 64bit OS.
        /// </summary>
        /// <returns>True if it is, false otherwise.</returns>
        public static bool InternalCheckIsWow64()
        {
            if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) ||
                Environment.OSVersion.Version.Major >= 6)
            {
                using (Process p = Process.GetCurrentProcess())
                {
                    bool retVal;
                    if (!IsWow64Process(p.Handle, out retVal))
                    {
                        return false;
                    }
                    return retVal;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
