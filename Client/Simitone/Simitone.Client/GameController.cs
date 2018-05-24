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
            });
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
