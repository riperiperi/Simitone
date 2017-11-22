using FSO.Client;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Content;
using Microsoft.Xna.Framework;
using Simitone.Client.UI.Controls;
using Simitone.Client.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.Common.Rendering.Framework.Model;

namespace Simitone.Client.UI.Panels
{
    public class UIModeSwitcher : UIContainer
    {
        public UILiveButton LiveButton;
        public UIElasticButton BuyButton;
        public UIElasticButton BuildButton;
        public UIElasticButton OptionButton;

        private UIButton[] ButtonOrder;
        public Func<UIMainPanelMode, bool> OnModeClick;
        public TS1GameScreen Game;

        public UIModeSwitcher(TS1GameScreen screen)
        {
            Game = screen;
            var ui = Content.Get().CustomUI;

            var btn = new UILiveButton(screen);
            btn.MotiveLevel = 0.5f;
            btn.Position = Vector2.Zero;
            btn.OnButtonClick += (b) => { SwitchMode(UIMainPanelMode.LIVE); };
            Add(btn);
            LiveButton = btn;

            BuildButton = new UIElasticButton(ui.Get("mode_build.png").Get(GameFacade.GraphicsDevice));
            BuildButton.Position = btn.Position;
            BuildButton.OnButtonClick += (b) => { SwitchMode(UIMainPanelMode.BUILD); };
            BuildButton.Opacity = 0;
            Add(BuildButton);

            BuyButton = new UIElasticButton(ui.Get("mode_buy.png").Get(GameFacade.GraphicsDevice));
            BuyButton.Position = btn.Position;
            BuyButton.OnButtonClick += (b) => { SwitchMode(UIMainPanelMode.BUY); };
            BuyButton.Opacity = 0;
            Add(BuyButton);

            OptionButton = new UIElasticButton(ui.Get("mode_options.png").Get(GameFacade.GraphicsDevice));
            OptionButton.Position = btn.Position;
            OptionButton.OnButtonClick += (b) => { SwitchMode(UIMainPanelMode.OPTIONS); };
            OptionButton.Opacity = 0;
            Add(OptionButton);

            ButtonOrder = new UIButton[]
            {
                LiveButton,
                BuyButton,
                BuildButton,
                OptionButton
            };
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);
            foreach (var btn in ButtonOrder)
            {
                btn.Visible = btn.Opacity > 0;
            }
        }

        public void EndSwitch(UIMainPanelMode mode)
        {
            UIButton frontButton;
            switch (mode)
            {
                case UIMainPanelMode.BUILD:
                    frontButton = BuildButton; break;
                case UIMainPanelMode.BUY:
                    frontButton = BuyButton; break;
                case UIMainPanelMode.OPTIONS:
                    frontButton = OptionButton; break;
                default:
                    frontButton = LiveButton; break;
            }
            //become this mode
            SendToFront(frontButton);
            foreach (var button in ButtonOrder)
            {
                GameFacade.Screens.Tween.To(button, 0.5f, new Dictionary<string, float>() { { "Y", 0 } }, TweenQuad.EaseOut);
                GameFacade.Screens.Tween.To(button, 0.5f, new Dictionary<string, float>() { { "Opacity", (button != frontButton)?0f:1f } }, TweenQuad.EaseOut);
            }
        }

        public void SwitchMode(UIMainPanelMode mode)
        {
            UIButton frontButton;
            switch (mode)
            {
                case UIMainPanelMode.BUILD:
                    frontButton = BuildButton; break;
                case UIMainPanelMode.BUY:
                    frontButton = BuyButton; break;
                case UIMainPanelMode.OPTIONS:
                    frontButton = OptionButton; break;
                default:
                    frontButton = LiveButton; break;
            }
            if (OnModeClick?.Invoke(mode) ?? true)
            {
                //switching mode. show the modes.
                int i = 0;
                foreach (var button in ButtonOrder)
                {
                    if (button == LiveButton && Game?.LotControl.ActiveEntity == null) continue;
                    button.Visible = true;
                    GameFacade.Screens.Tween.To(button, 0.5f, new Dictionary<string, float>() { { "Y", (-140)*(i++) } }, TweenQuad.EaseOut);
                    GameFacade.Screens.Tween.To(button, 0.5f, new Dictionary<string, float>() { { "Opacity", 1 } }, TweenQuad.EaseOut);
                }
            }
            else
            {
                //become this mode
                //should happen as part of callback from main panel..
                /*
                SendToFront(frontButton);
                foreach (var button in ButtonOrder)
                {
                    GameFacade.Screens.Tween.To(button, 0.5f, new Dictionary<string, float>() { { "Y", 0 } }, TweenQuad.EaseOut);
                    GameFacade.Screens.Tween.To(button, 0.5f, new Dictionary<string, float>() { { "Opacity", (button != frontButton) ? 0f : 1f } }, TweenQuad.EaseOut);
                }
                */
            }
        }
    }
}
