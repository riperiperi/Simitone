using FSO.Client;
using FSO.Common;
using FSO.LotView;
using Simitone.Client;
using Simitone.Windows.GameLocator;
using Simitone.Windows.Utils;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Simitone.Windows
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            Directory.SetCurrentDirectory(baseDir);
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            OperatingSystem os = Environment.OSVersion;
            PlatformID pid = os.Platform;

            ILocator gameLocator;
            bool linux = pid == PlatformID.MacOSX || pid == PlatformID.Unix;
            if (linux && Directory.Exists("/Users"))
                gameLocator = new MacOSLocator();
            else if (linux)
                gameLocator = new LinuxLocator();
            else
                gameLocator = new WindowsLocator();

            var useDX = !linux;
            string path = null;
            bool pathProvidedViaCommandLine = false;


            FSOEnvironment.Enable3D = false;
            bool ide = false;
            bool aa = false;
            bool jit = false;
            #region User resolution parmeters

            FSOEnvironment.Args = string.Join(" ", args);

            foreach (var arg in args)
            {
                if (arg[0] == '-')
                {
                    var cmd = arg.Substring(1);
                    if (cmd.StartsWith("lang"))
                    {
                        GlobalSettings.Default.LanguageCode = byte.Parse(cmd.Substring(4));
                    }
                    else if (cmd.StartsWith("hz")) GlobalSettings.Default.TargetRefreshRate = int.Parse(cmd.Substring(2));
                    else
                    {
                        //normal style param
                        switch (cmd)
                        {
                            case "ide":
                                ide = true;
                                break;
                            case "3d":
                                FSOEnvironment.Enable3D = true;
                                break;
                            case "aa":
                                aa = true;
                                break;
                            case "jit":
                                jit = true;
                                break;
                            case "dx":
                            case "dx11":
                                useDX = true;
                                break;
                            case "gl":
                            case "ogl":
                                useDX = false;
                                break;
                            case "touch":
                                FSOEnvironment.SoftwareKeyboard = true;
                                break;
                            case "nosound":
                                FSOEnvironment.NoSound = true;
                                break;
                            case string s when s.StartsWith("path"): //The Sims path
                                path = s.Length > 4 ? s.Substring(4).Trim('"').Replace('\\', '/') + "/" : path;
                                pathProvidedViaCommandLine = true;
                                break;
                        }
                    }
                }
            }
            #endregion

            // If no path provided via command line, check if we need to configure installation
            if (!pathProvidedViaCommandLine && !linux)
            {
                var windowsLocator = gameLocator as WindowsLocator;
                if (windowsLocator != null)
                {
                    // Check if installation has already been configured
                    if (!GlobalSettings.Default.TS1InstallationConfigured)
                    {
                        // First time setup - show installation selector
                        var installations = windowsLocator.GetAllTheSims1Installations();
                        
                        if (installations.Count == 0)
                        {
                            MessageBox.Show(
                                "No installation of The Sims was detected.\n\n" +
                                "Please install The Sims or use the -path command line argument to specify the installation directory.\n\n" +
                                "Example: Simitone.exe -path\"C:\\Program Files\\Maxis\\The Sims\"",
                                "The Sims Not Found",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                            return;
                        }

                        // Check if user already has Simitone saves
                        bool hasExistingSaves = Directory.Exists(Path.Combine(FSOEnvironment.UserDir, "UserData/"));
                        if (hasExistingSaves)
                        {
                            var result = MessageBox.Show(
                                "Existing Simitone save data was detected.\n\n" +
                                "If you select a different installation type than before, your saves may not be compatible.\n\n" +
                                "Do you want to continue with installation selection?",
                                "Existing Saves Detected",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning
                            );
                            
                            if (result == DialogResult.No)
                            {
                                return;
                            }
                        }

                        string selectedInstallType = "";
                        
                        if (installations.Count == 1)
                        {
                            // Only one installation found, use it automatically
                            path = installations[0].path;
                            selectedInstallType = installations[0].description;
                            GlobalSettings.Default.TS1IsSteamInstall = (installations[0].type == Simitone.Windows.GameLocator.TS1InstallationType.Steam);
                        }
                        else
                        {
                            // Multiple installations, show selector
                            using (var selector = new Simitone.Windows.GameLocator.InstallationSelector(installations))
                            {
                                if (selector.ShowDialog() == DialogResult.OK)
                                {
                                    path = selector.SelectedPath;
                                    selectedInstallType = installations.Find(i => i.path == path).description;
                                    GlobalSettings.Default.TS1IsSteamInstall = (selector.SelectedType == Simitone.Windows.GameLocator.TS1InstallationType.Steam);
                                }
                                else
                                {
                                    // User cancelled
                                    return;
                                }
                            }
                        }

                        // Mark as configured and save
                        GlobalSettings.Default.TS1InstallationConfigured = true;
                        GlobalSettings.Default.StartupPath = path;
                        GlobalSettings.Default.TS1HybridPath = path;
                        GlobalSettings.Default.Save();

                        // Show information dialog
                        string savesPath = GlobalSettings.Default.TS1IsSteamInstall
                            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                                         "Saved Games", "Electronic Arts", "The Sims 25", "UserData")
                            : Path.Combine(path, "UserData");
                        
                        string simitoneSavesPath = Path.Combine(FSOEnvironment.UserDir, "UserData");

                        using (var infoDialog = new Simitone.Windows.GameLocator.InstallationInfoDialog(
                            selectedInstallType,
                            path,
                            savesPath,
                            simitoneSavesPath))
                        {
                            infoDialog.ShowDialog();
                        }
                    }
                    else
                    {
                        // Already configured, use saved settings
                        path = GlobalSettings.Default.TS1HybridPath;
                        
                        // Validate the path still exists
                        if (!Directory.Exists(path) || !File.Exists(Path.Combine(path, "GameData", "Behavior.iff")))
                        {
                            var result = MessageBox.Show(
                                $"The configured installation path is no longer valid:\n{path}\n\n" +
                                "Would you like to reconfigure your installation?\n\n" +
                                "Click Yes to select a new installation, or No to exit.",
                                "Installation Not Found",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning
                            );
                            
                            if (result == DialogResult.Yes)
                            {
                                // Reset configuration and restart the detection
                                GlobalSettings.Default.TS1InstallationConfigured = false;
                                GlobalSettings.Default.Save();
                                
                                MessageBox.Show(
                                    "Configuration has been reset. Please restart Simitone to select your installation.",
                                    "Configuration Reset",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information
                                );
                            }
                            
                            return;
                        }
                    }
                }
            }
            
            // If still no path (Linux or other edge case), try to detect
            if (path == null)
            {
                path = gameLocator.FindTheSims1();
            }

            useDX = MonogameLinker.Link(useDX);

            FSO.Files.ImageLoaderHelpers.BitmapFunction = BitmapReader;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            FSOEnvironment.SoftwareDepth = false;
            FSOEnvironment.UseMRT = true;

            if (path != null)
            {
                FSOEnvironment.ContentDir = "Content/";
                FSOEnvironment.GFXContentDir = "Content/" + (useDX ? "DX/" : "OGL/");
                FSOEnvironment.UserDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Simitone/").Replace('\\', '/');
                Directory.CreateDirectory(FSOEnvironment.UserDir);
                FSOEnvironment.Linux = false;
                FSOEnvironment.DirectX = useDX;
                FSOEnvironment.GameThread = Thread.CurrentThread;
                if (GlobalSettings.Default.LanguageCode == 0) GlobalSettings.Default.LanguageCode = 1;
                FSO.Files.Formats.IFF.Chunks.STR.DefaultLangCode = (FSO.Files.Formats.IFF.Chunks.STRLangCode)GlobalSettings.Default.LanguageCode;

                // Set runtime settings (these may have been set during first-time config or command line)
                if (string.IsNullOrEmpty(GlobalSettings.Default.StartupPath))
                    GlobalSettings.Default.StartupPath = path;
                if (string.IsNullOrEmpty(GlobalSettings.Default.TS1HybridPath))
                    GlobalSettings.Default.TS1HybridPath = path;
                
                GlobalSettings.Default.TS1HybridEnable = true;
                FSO.Content.Content.TS1SteamInstall = GlobalSettings.Default.TS1IsSteamInstall;
                GlobalSettings.Default.ClientVersion = "0";
                GlobalSettings.Default.LightingMode = 3;
                GlobalSettings.Default.AntiAlias = aa ? 1 : 0;
                GlobalSettings.Default.ComplexShaders = true;
                GlobalSettings.Default.EnableTransitions = true;

                // Save the updated settings to config.ini
                GlobalSettings.Default.Save();

                if (ide) new FSO.IDE.VolcanicStartProxy().InitVolcanic(args);

                var assemblies = new FSO.SimAntics.JIT.Runtime.AssemblyStore();
                //var globals = new TS1.Scripts.Dummy(); //make sure scripts assembly is loaded
                if (jit) assemblies.InitAOT();
                FSO.SimAntics.Engine.VMTranslator.INSTANCE = new FSO.SimAntics.JIT.Runtime.VMAOTTranslator(assemblies);

                var start = new GameStartProxy();
                start.Start(useDX);
            }
        }

        private static System.Reflection.Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                var name = args.Name;
                if (name.StartsWith("FSO.Scripts"))
                {
                    return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName == name);
                }
                else
                {
                    var assemblyPath = Path.Combine(MonogameLinker.AssemblyDir, args.Name.Substring(0, name.IndexOf(',')) + ".dll");
                    var assembly = Assembly.LoadFrom(assemblyPath);
                    return assembly;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject;
            if (exception is OutOfMemoryException)
            {
                MessageBox.Show(e.ExceptionObject.ToString(), "Out of Memory! FreeSO needs to close.");
            }
            else
            {
                MessageBox.Show(e.ExceptionObject.ToString(), "A fatal error occured! Screenshot this dialog and post it on Discord.");
            }
        }

        public static Tuple<byte[], int, int> BitmapReader(Stream str)
        {
            Bitmap image = (Bitmap)Bitmap.FromStream(str);
            try
            {
                // Fix up the Image to match the expected format
                image = (Bitmap)image.RGBToBGR();

                var data = new byte[image.Width * image.Height * 4];

                BitmapData bitmapData = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                if (bitmapData.Stride != image.Width * 4)
                    throw new NotImplementedException();
                Marshal.Copy(bitmapData.Scan0, data, 0, data.Length);
                image.UnlockBits(bitmapData);

                return new Tuple<byte[], int, int>(data, image.Width, image.Height);
            }
            finally
            {
                image.Dispose();
            }
        }

        // RGB to BGR convert Matrix
        private static float[][] rgbtobgr = new float[][]
          {
             new float[] {0, 0, 1, 0, 0},
             new float[] {0, 1, 0, 0, 0},
             new float[] {1, 0, 0, 0, 0},
             new float[] {0, 0, 0, 1, 0},
             new float[] {0, 0, 0, 0, 1}
          };


        internal static Image RGBToBGR(this Image bmp)
        {
            Image newBmp;
            if ((bmp.PixelFormat & System.Drawing.Imaging.PixelFormat.Indexed) != 0)
            {
                newBmp = new Bitmap(bmp.Width, bmp.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            }
            else
            {
                // Need to clone so the call to Clear() below doesn't clear the source before trying to draw it to the target.
                newBmp = (Image)bmp.Clone();
            }

            try
            {
                System.Drawing.Imaging.ImageAttributes ia = new System.Drawing.Imaging.ImageAttributes();
                System.Drawing.Imaging.ColorMatrix cm = new System.Drawing.Imaging.ColorMatrix(rgbtobgr);

                ia.SetColorMatrix(cm);
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newBmp))
                {
                    g.Clear(Color.Transparent);
                    g.DrawImage(bmp, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, bmp.Width, bmp.Height, System.Drawing.GraphicsUnit.Pixel, ia);
                }
            }
            finally
            {
                if (newBmp != bmp)
                {
                    bmp.Dispose();
                }
            }

            return newBmp;
        }
    }
#endif
}
