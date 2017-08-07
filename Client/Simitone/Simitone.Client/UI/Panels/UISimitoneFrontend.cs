using FSO.Client.UI.Framework;
using FSO.Content;
using Simitone.Client.UI.Controls;
using Simitone.Client.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.Common.Rendering.Framework.Model;
using FSO.Client;
using Microsoft.Xna.Framework;
using Simitone.Client.UI.Panels.LiveSubpanels;

namespace Simitone.Client.UI.Panels
{
    public class UISimitoneFrontend : UIContainer
    {
        public UIClockPanel Clock;
        public UITwoStateButton CutBtn;
        public UICutawayPanel CutPanel;
        public TS1GameScreen Game;
        public UIMoneyPanel Money;
        public UIMainPanel MainPanel;
        public UIStencilButton ExtendPanelBtn;
        public UILiveButton LiveButton;

        public bool PanelActive;
        public int LastCut = 0;

        public UISimitoneFrontend(TS1GameScreen screen)
        {
            var ui = Content.Get().CustomUI;

            CutBtn = new UITwoStateButton(ui.Get("cut_btn_down.png").Get(GameFacade.GraphicsDevice));
            CutBtn.X = screen.ScreenWidth - (256 + 15);
            CutBtn.Y = 15;
            CutBtn.OnButtonClick += CutButton;
            Add(CutBtn);

            Clock = new UIClockPanel(screen.vm);
            Clock.X = screen.ScreenWidth - (334 + 15);
            Clock.Y = 15;
            Game = screen;
            Add(Clock);

            Money = new UIMoneyPanel(screen);
            Money.Position = new Vector2(15, screen.ScreenHeight - 172);
            Add(Money);

            MainPanel = new UIMainPanel(screen);
            Add(MainPanel);

            ExtendPanelBtn = new UIStencilButton(ui.Get("panel_expand.png").Get(GameFacade.GraphicsDevice));
            ExtendPanelBtn.OnButtonClick += ExpandClicked;
            Add(ExtendPanelBtn);

            var btn = new UILiveButton(screen);
            btn.MotiveLevel = 0.5f;
            btn.Position = new Vector2(64 + 15, screen.ScreenHeight - (64 + 15));
            btn.OnButtonClick += LiveButtonClicked;
            Add(btn);
            LiveButton = btn;

            ExtendPanelBtn.Position = new Vector2(btn.X + 54, btn.Y - 50);

            MainPanel.X = 64 + 15;
            MainPanel.Y = btn.Y - 64;
            MainPanel.Visible = false;

        }

        private void ExpandClicked(UIElement button)
        {
            MainPanel.Open();
            MainPanel.Switcher_OnCategorySelect(MainPanel.Switcher.ActiveCategory);
        }

        private void LiveButtonClicked(UIElement button)
        {
            if (MainPanel.PanelActive)
            {
                if (MainPanel.ShowingSelect) MainPanel.SwitchAvatar.Kill();
                else MainPanel.ShowSelect();
            } else
            {
                MainPanel.Open();
                MainPanel.ShowSelect();
            }
        }

        private void CutButton(UIElement button)
        {
            if (CutPanel != null)
            {
                CutBtn.Selected = false;
                CutPanel.Kill();
                CutPanel = null;
            } else
            {
                CutBtn.Selected = true;
                CutPanel = new UICutawayPanel(LastCut);
                CutPanel.X = CutBtn.X-39;
                CutPanel.Y = 15;
                CutPanel.OnSelection += CutPanel_OnSelection;
                AddAt(0, CutPanel);
            }
        }

        private void CutPanel_OnSelection(int obj)
        {
            CutBtn.Selected = false;
            Game.LotControl.World.State.DrawRoofs = (obj == 3);
            Game.LotControl.WallsMode = obj;
            CutPanel.Kill();
            CutPanel = null;
        }

        public float ClockTween;
        public override void Update(UpdateState state)
        {
            base.Update(state);
            if (LastCut != Game.LotControl.WallsMode)
            {
                LastCut = Game.LotControl.WallsMode;
                var ui = Content.Get().CustomUI;
                string cutImg = "cut_btn_down.png";
                switch (LastCut)
                {
                    case 1:
                        cutImg = "cut_btn_away.png"; break;
                    case 2:
                        cutImg = "cut_btn_up.png"; break;
                    case 3:
                        cutImg = "cut_btn_roof.png"; break;
                }
                CutBtn.Texture = ui.Get(cutImg).Get(GameFacade.GraphicsDevice);
            }
            if (Clock.TweenHook != ClockTween)
            {
                ClockTween = Clock.TweenHook;
                CutBtn.X = Game.ScreenWidth - (256 + (138f * ClockTween) + 15);
                if (CutPanel != null) CutPanel.X = CutBtn.X - 39;
            }
            LiveButton.Switching = MainPanel.ShowingSelect;
            ExtendPanelBtn.Visible = !MainPanel.PanelActive;
        }

        public override void GameResized()
        {
            base.GameResized();
            CutBtn.X = Game.ScreenWidth - (256 + (138f * ClockTween) + 15);
            if (CutPanel != null) CutPanel.X = CutBtn.X - 39;
            Clock.X = Game.ScreenWidth - (334 + 15);
            Clock.Y = 15;
        }
    }
}
