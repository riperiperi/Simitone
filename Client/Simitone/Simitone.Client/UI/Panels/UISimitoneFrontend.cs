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
using FSO.Client.UI.Model;
using Microsoft.Xna.Framework.Input;
using FSO.SimAntics;
using FSO.SimAntics.NetPlay.Model.Commands;
using FSO.HIT;
using Simitone.Client.UI.Panels.Desktop;

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
        public UIModeSwitcher ModeSwitcher;
        public UIDesktopUCP DesktopUCP;
        public UICheatTextbox CheatTextbox;

        public bool PanelActive;
        public int LastCut = 0;

        public UISimitoneFrontend(TS1GameScreen screen)
        {
            var ui = Content.Get().CustomUI;
            Game = screen;

            if (!Game.Desktop)
            {
                CutBtn = new UITwoStateButton(ui.Get("cut_btn_down.png").Get(GameFacade.GraphicsDevice));
                CutBtn.X = screen.ScreenWidth - (256 + 15);
                CutBtn.Y = 15;
                CutBtn.OnButtonClick += CutButton;
                Add(CutBtn);

                Clock = new UIClockPanel(screen.vm);
                Clock.X = screen.ScreenWidth - (334 + 15);
                Clock.Y = 15;
                Add(Clock);

                Money = new UIMoneyPanel(screen);
                Money.Position = new Vector2(15, screen.ScreenHeight - 172);
                Add(Money);

                ExtendPanelBtn = new UIStencilButton(ui.Get("panel_expand.png").Get(GameFacade.GraphicsDevice));
                ExtendPanelBtn.OnButtonClick += ExpandClicked;
                Add(ExtendPanelBtn);
            }

            CheatTextbox = new UICheatTextbox(Game.vm);
            CheatTextbox.Position = new Vector2(10, 10);
            CheatTextbox.Visible = false;
            Add(CheatTextbox);

            MainPanel = new UIMainPanel(screen);
            MainPanel.OnEndSelect += OnEndSelect;
            MainPanel.ModeChanged += ModeChanged;
            Add(MainPanel);

            if (Game.Desktop)
            {
                DesktopUCP = new UIDesktopUCP(screen);
                DesktopUCP.Position = new Vector2(15, screen.ScreenHeight - (278 + 15));
                DesktopUCP.OnModeClick += LiveButtonClicked;
                Add(DesktopUCP);
            }
            else
            {
                var mode = new UIModeSwitcher(screen);
                mode.Position = new Vector2(64 + 15, screen.ScreenHeight - (64 + 15));
                mode.OnModeClick += LiveButtonClicked;
                Add(mode);
                ModeSwitcher = mode;
                ExtendPanelBtn.Position = new Vector2(mode.X + 54, mode.Y - 50);
            }

            MainPanel.X = 64 + 15;
            if (Game.Desktop) MainPanel.X += 100;
            MainPanel.GameResized();
            MainPanel.Y = screen.ScreenHeight - (128 + 15);
            MainPanel.Visible = false;

            if (Game.vm.GetGlobalValue(32) > 0)
            {
                MainPanel.SetMode(UIMainPanelMode.BUY);
                ModeSwitcher?.EndSwitch(MainPanel.Mode);
                MainPanel.Open();
            } else
            {
                FSO.HIT.HITVM.Get().PlaySoundEvent(UIMusic.None);
            }
        }

        private void ModeChanged(UIMainPanelMode obj)
        {
            Clock?.SetHidden(obj != UIMainPanelMode.LIVE);
            DesktopUCP?.SetMode(obj);
            var lotType = MainPanel.GetLotType(true);
            var hit = FSO.HIT.HITVM.Get();
            switch (obj)
            {
                case UIMainPanelMode.LIVE:
                case UIMainPanelMode.OPTIONS:
                    hit.PlaySoundEvent(UIMusic.None); break;
                case UIMainPanelMode.BUY:
                    switch (lotType)
                    {
                        case UICatalogMode.Downtown:
                            hit.PlaySoundEvent(UIMusic.Downtown); break;
                        case UICatalogMode.Vacation:
                            hit.PlaySoundEvent(UIMusic.Vacation); break;
                        case UICatalogMode.Community:
                            hit.PlaySoundEvent(UIMusic.Unleashed); break;
                        case UICatalogMode.Studiotown:
                            hit.PlaySoundEvent(UIMusic.SuperstarTransition); break;
                        case UICatalogMode.Magictown:
                            hit.PlaySoundEvent(UIMusic.MagictownBuy); break;
                        default:
                            hit.PlaySoundEvent(UIMusic.Buy); break;
                    }
                    break;
                case UIMainPanelMode.BUILD:
                    switch (lotType)
                    {
                        case UICatalogMode.Downtown:
                            hit.PlaySoundEvent(UIMusic.Downtown); break;
                        case UICatalogMode.Vacation:
                            hit.PlaySoundEvent(UIMusic.Vacation); break;
                        case UICatalogMode.Community:
                            hit.PlaySoundEvent(UIMusic.Unleashed); break;
                        case UICatalogMode.Studiotown:
                            hit.PlaySoundEvent(UIMusic.SuperstarTransition); break;
                        case UICatalogMode.Magictown:
                            hit.PlaySoundEvent(UIMusic.MagictownBuild); break;
                        default:
                            hit.PlaySoundEvent(UIMusic.Build); break;
                    }
                    break;
            }
        }

        private void ExpandClicked(UIElement button)
        {
            MainPanel.Open();
            MainPanel.Switcher_OnCategorySelect(MainPanel.Switcher.ActiveCategory);
        }

        private bool LiveButtonClicked(UIMainPanelMode mode)
        {
            var deskAuto = Game.Desktop && (mode != UIMainPanelMode.LIVE || MainPanel.Mode != UIMainPanelMode.LIVE);
            if (MainPanel.PanelActive || deskAuto)
            {
                if (MainPanel.ShowingSelect || deskAuto)
                {
                    if (!MainPanel.PanelActive) MainPanel.Open();
                    //switch to the target mode
                    MainPanel.SetMode(mode);
                    MainPanel.SwitchAvatar?.Kill();
                    return false;
                }
                else
                {
                    StartSelect();
                    return true;
                }
            } else
            {
                MainPanel.Open();
                StartSelect();
                return true;
            }
        }

        private void StartSelect()
        {
            MainPanel.ShowSelect();
        }

        private void OnEndSelect()
        {
            ModeSwitcher?.EndSwitch(MainPanel.Mode);
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
            CutPanel?.Kill();
            CutPanel = null;
        }

        public float ClockTween;
        public override void Update(UpdateState state)
        {
            if (state.NewKeys.Contains(Keys.Space))
            {
                var selected = Game.LotControl.ActiveEntity;
                var familyMembers = Game.vm.Context.ObjectQueries.Avatars.Where(x => 
                    ((VMAvatar)x).GetPersonData(
                        FSO.SimAntics.Model.VMPersonDataVariable.TS1FamilyNumber) == (Game.vm.TS1State.CurrentFamily?.ChunkID)
                        ).ToList();
                var index = familyMembers.IndexOf(selected);
                if (familyMembers.Count > 0)
                {
                    index = (index + 1) % (familyMembers.Count);
                    HITVM.Get().PlaySoundEvent(UISounds.QueueAdd);
                    Game.vm.SendCommand(new VMNetChangeControlCmd() { TargetID = familyMembers[index].ObjectID });
                }
            }

            base.Update(state);
            if (!Game.Desktop)
            {
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
                ModeSwitcher.LiveButton.Switching = MainPanel.ShowingSelect;
                ExtendPanelBtn.Visible = !MainPanel.PanelActive;
            }
        }

        public override void GameResized()
        {
            base.GameResized();
            if (!Game.Desktop)
            {
                CutBtn.X = Game.ScreenWidth - (256 + (138f * ClockTween) + 15);
                if (CutPanel != null) CutPanel.X = CutBtn.X - 39;
                Clock.X = Game.ScreenWidth - (334 + 15);
                Clock.Y = 15;
                Money.Position = new Vector2(15, Game.ScreenHeight - 172);
                var mode = ModeSwitcher;
                mode.Position = new Vector2(64 + 15, Game.ScreenHeight - (64 + 15));
                ExtendPanelBtn.Position = new Vector2(mode.X + 54, mode.Y - 50);
            }
            else
            {
                DesktopUCP.Position = new Vector2(15, Game.ScreenHeight - (278 + 15));
            }
            MainPanel.Y = Game.ScreenHeight - (128 + 15);
        }
    }
}
