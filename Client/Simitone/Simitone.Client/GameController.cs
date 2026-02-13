using FSO.Client;
using FSO.Client.UI.Framework;
using FSO.Client.UI.Controls;
using FSO.Common.Utils;
using FSO.Content;
using Simitone.Client.UI.Screens;
using Simitone.Client.UI.Panels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simitone.Client
{
    public static class GameController
    {
        public static void EnterLoading()
        {
            var screen = new LoadingScreen();
            GameFacade.Screens.RemoveCurrent();
            GameFacade.Screens.AddScreen(screen);
        }

        public static void EnterGameMode(string lotName, bool external)
        {
            GameThread.NextUpdate((x) =>
            {
                var mode = NeighSelectionMode.Normal;
                if (lotName.Length > 1 && lotName[0] == '!')
                {
                    switch (lotName[1])
                    {
                        case 'n':
                            mode = NeighSelectionMode.MoveIn; break;
                        case 'm':
                            mode = NeighSelectionMode.MoveInMagic; break;
                    }
                }
                var screen = new TS1GameScreen(mode);
                if (mode != NeighSelectionMode.Normal)
                {
                    screen.StartMoveIn(int.Parse(lotName.Substring(2)));
                }
                var last = GameFacade.Screens.CurrentUIScreen;
                GameFacade.Screens.RemoveCurrent();
                GameFacade.Screens.AddScreen(screen);

                var load = (last as LoadingScreen);

                if (load != null)
                {
                    load.Close();
                    var children = new List<UIElement>(last.GetChildren());
                    for (int i = 0; i < children.Count; i++)
                    {
                        last.Remove(children[i]);
                        screen.Add(children[i]);
                    }
                }
                screen.Initialize(lotName, external);
                
                // Show notification about any failed content files
                ShowFailedContentNotification();
            });
        }
        
        /// <summary>
        /// Shows a notification to the user about any content files that failed to load.
        /// This is called after entering game mode so the user can see which custom content
        /// files are problematic.
        /// </summary>
        private static void ShowFailedContentNotification()
        {
            var failedFiles = Content.FailedContentFiles;
            if (failedFiles == null || failedFiles.Count == 0)
                return;
            
            var realFailures = failedFiles
                .Where(f => f.ErrorType != "DebugInfo")
                .GroupBy(f => f.Filename)
                .Select(g => g.First())
                .ToList();
            
            if (realFailures.Count == 0)
                return;
            
            var message = new StringBuilder();
            message.AppendLine("Some custom content files could not be loaded:");
            message.AppendLine();
            
            int displayCount = Math.Min(realFailures.Count, 3);
            for (int i = 0; i < displayCount; i++)
            {
                var file = realFailures[i];
                string shortReason = file.ErrorType switch
                {
                    "EndOfStream" => "truncated",
                    "InvalidData" => "invalid format",
                    "IOException" => "I/O error",
                    "CatalogError" => "catalog error",
                    "DecodeError" => "decode failed",
                    _ => file.ErrorType
                };
                message.AppendLine($"• {file.Filename} ({shortReason})");
            }
            
            if (realFailures.Count > 3)
            {
                message.AppendLine($"... and {realFailures.Count - 3} more");
            }
            
            message.AppendLine();
            message.AppendLine("Common causes: corrupted files, missing .cfp files, or wrong format.");
            message.AppendLine("The game will continue without these items.");
            
            // Show the alert
            UIMobileAlert alert = null;
            alert = new UIMobileAlert(new UIAlertOptions
            {
                Title = "Custom Content Warning",
                Message = message.ToString(),
                Buttons = UIAlertButton.Ok((btn) => 
                { 
                    alert.Close();
                })
            });
            
            UIScreen.GlobalShowDialog(alert, true);
        }

        public static void EnterCAS()
        {
            //GameThread.NextUpdate((x) =>
            //{
                var screen = new TS1CASScreen();
                var last = GameFacade.Screens.CurrentUIScreen;
                if (last is TS1GameScreen) ((TS1GameScreen)last).CleanupLastWorld();
                GameFacade.Screens.RemoveCurrent();
                GameFacade.Screens.AddScreen(screen);
            //});
        }
    }
}
