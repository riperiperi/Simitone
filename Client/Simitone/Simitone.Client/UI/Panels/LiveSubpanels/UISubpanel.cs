using FSO.Client;
using FSO.Client.UI.Framework;
using FSO.Common.Utils;
using Microsoft.Xna.Framework;
using Simitone.Client.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simitone.Client.UI.Panels.LiveSubpanels
{
    public class UISubpanel : UICachedContainer
    {
        public TS1GameScreen Game;

        public UISubpanel(TS1GameScreen game) : base()
        {
            Opacity = 0;
            var screenWidth = GameFacade.Screens.CurrentUIScreen.ScreenWidth;
            Size = new Vector2(screenWidth - (342 + (game.Desktop?100:0)), 128);
            GameFacade.Screens.Tween.To(this, 0.3f, new Dictionary<string, float>() { { "Opacity", 1f } });
            Game = game;
        }

        public override void GameResized()
        {
            var screenWidth = UIScreen.Current.ScreenWidth;
            Size = new Vector2(screenWidth - (342 + (Game.Desktop ? 100 : 0)), 128);
            base.GameResized();
        }

        public virtual void Kill()
        {
            GameFacade.Screens.Tween.To(this, 0.3f, new Dictionary<string, float>() { { "Opacity", 0f } });
            GameThread.SetTimeout(() =>
            {
                Parent.Remove(this);
            }, 300);
        }
    }
}
