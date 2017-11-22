using FSO.Client;
using FSO.Common;
using FSO.LotView;
using Simitone.Client;
using Simitone.Windows.GameLocator;
using System;
using System.IO;
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
            var gameLocator = new WindowsLocator();

            var useDX = false;
            var path = gameLocator.FindTheSimsOnline();

            if (useDX) GlobalSettings.Default.AntiAlias = false;

            FSOEnvironment.Enable3D = false;
            bool ide = false;
            #region User resolution parmeters

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
                        }
                    }
                }
            }

            #endregion

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

                GlobalSettings.Default.StartupPath = path;
                GlobalSettings.Default.TS1HybridEnable = true;
                GlobalSettings.Default.TS1HybridPath = gameLocator.FindTheSims1();
                GlobalSettings.Default.ClientVersion = "0";
                GlobalSettings.Default.AntiAlias = true;

                GameFacade.DirectX = useDX;
                World.DirectX = useDX;

                if (ide) new FSO.IDE.VolcanicStartProxy().InitVolcanic();

                SimitoneGame game = new SimitoneGame();
                var form = (Form)Form.FromHandle(game.Window.Handle);
                if (form != null) form.FormClosing += Form_FormClosing;
                game.Run();
                game.Dispose();
            }
        }

        private static void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !(GameFacade.Screens.CurrentUIScreen?.CloseAttempt() ?? true);
        }
    }
#endif
}
