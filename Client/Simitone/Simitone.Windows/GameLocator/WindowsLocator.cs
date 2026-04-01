using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Simitone.Windows.GameLocator
{
    public enum TS1InstallationType
    {
        Portable,
        Steam,
        Registry,
        Unknown
    }

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

        /// <summary>
        /// Gets all detected The Sims 1 installations
        /// </summary>
        /// <returns>List of tuples containing (description, path, type) for each found installation</returns>
        public List<(string description, string path, TS1InstallationType type)> GetAllTheSims1Installations()
        {
            var installations = new List<(string, string, TS1InstallationType)>();

            // Check relative directory
            string localDir = @"../The Sims/";
            if (File.Exists(Path.Combine(localDir, "GameData", "Behavior.iff")))
            {
                installations.Add(("Portable Install (Relative Directory)", localDir, TS1InstallationType.Portable));
            }

            // Check for Steam Legacy Collection
            if (FindTheSimsLegacySteam() is string steamInstallDir)
            {
                installations.Add(("Steam - The Sims: Legacy Collection", steamInstallDir, TS1InstallationType.Steam));
            }

            // Check Windows Registry
            using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                RegistryKey softwareKey = hklm.OpenSubKey("SOFTWARE");

                if (Array.Exists(softwareKey.GetSubKeyNames(), delegate (string s) { return s.Equals("Maxis", StringComparison.InvariantCultureIgnoreCase); }))
                {
                    RegistryKey maxisKey = softwareKey.OpenSubKey("Maxis");
                    if (Array.Exists(maxisKey.GetSubKeyNames(), delegate (string s) { return s.Equals("The Sims", StringComparison.InvariantCultureIgnoreCase); }))
                    {
                        RegistryKey tsoKey = maxisKey.OpenSubKey("The Sims");
                        string installDir = (string)tsoKey.GetValue("InstallPath");
                        if (!string.IsNullOrEmpty(installDir))
                        {
                            installDir += "\\";
                            installDir = installDir.Replace('\\', '/');
                            installations.Add(("Registry Install (CD/DVD/GOG)", installDir, TS1InstallationType.Registry));
                        }
                    }
                }
            }

            return installations;
        }

        public string FindTheSims1()
        {
            // Get all installations and return the first one found
            var installations = GetAllTheSims1Installations();
            if (installations.Count > 0)
            {
                return installations[0].path;
            }

            // Fall back to the default install location if nothing found
            return @"C:\Program Files (x86)\Maxis\The Sims\".Replace('\\', '/');
        }

        /// <summary>
        /// Finds The Sims Legacy Collection installed via Steam
        /// </summary>
        /// <param name="steamAppId">Steam App ID, default is The Sims: Legacy Collection</param>
        /// <returns>Full path if found, null otherwise</returns>
        private string FindTheSimsLegacySteam(int steamAppId = 3314060)
        {
            try
            {
                return SteamGameLocator.GetGamePath(steamAppId) ?? null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while finding The Sims: Legacy Collection Steam path: {ex.Message}");
                return null;
            }
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
