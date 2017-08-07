using FSO.Client;
using FSO.Client.UI.Framework;
using FSO.Common.Utils;
using Simitone.Client.UI.Screens;
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
                var screen = new TS1GameScreen();
                var last = GameFacade.Screens.CurrentUIScreen;
                GameFacade.Screens.RemoveCurrent();
                GameFacade.Screens.AddScreen(screen);

                ((LoadingScreen)last).Close();
                var children = new List<UIElement>(last.GetChildren());
                for (int i=0; i<children.Count; i++)
                {
                    last.Remove(children[i]);
                    screen.Add(children[i]);
                }
                screen.Initialize(lotName, external);
            });
        }
    }
}
