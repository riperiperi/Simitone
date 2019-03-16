using FSO.Client;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Client.UI.Model;
using FSO.Common;
using FSO.Common.Rendering.Framework.Model;
using FSO.Common.Utils;
using FSO.Content;
using FSO.HIT;
using FSO.LotView.RC;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Simitone.Client.UI.Controls;
using Simitone.Client.UI.Model;
using Simitone.Client.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simitone.Client.UI.Panels.Desktop
{
    public class UIDesktopUCP : UICachedContainer
    {
        public UIImage Background;
        public UIImage FriendIcon;

        public UIButton LiveButton;
        public UIButton BuyButton;
        public UIButton BuildButton;
        public UIButton OptionsButton;

        public UILabel MoneyLabel;
        public UILabel TimeLabel;
        public UILabel TimeLabelShadow;
        public UILabel FriendsLabel;
        public UILabel FriendsLabelShadow;
        public UILabel FloorLabel;
        public UILabel FloorLabelShadow;

        public UIButton RoofButton;
        public UIButton WallsUpButton;
        public UIButton WallsCutButton;
        public UIButton WallsDownButton;

        public UIButton FloorUpButton;
        public UIButton FloorDownButton;

        public UIButton ZoomInButton;
        public UIButton ZoomOutButton;
        public UIButton RotateCWButton;
        public UIButton RotateCCWButton;

        public UIButton[] SpeedButtons;

        private TS1GameScreen Game;

        public Func<UIMainPanelMode, bool> OnModeClick;

        public string[] FloorNames = new string[]
        {
            "1st",
            "2nd",
            "3rd",
            "4th",
            "5th"
        };

        public static Dictionary<int, int> RemapSpeed = new Dictionary<int, int>()
        {
            {0, 4}, //pause
            {1, 1}, //1 speed
            {3, 2}, //2 speed
            {10, 3}, //3 speed
        };

        public static Dictionary<int, int> ReverseRemap = RemapSpeed.ToDictionary(x => x.Value, x => x.Key);

        public UIDesktopUCP(TS1GameScreen screen)
        {
            Game = screen;
            var ui = Content.Get().CustomUI;
            var gd = GameFacade.GraphicsDevice;
            var sDir = new Vector3(0, 2, 0.5f);

            Background = new UIImage(ui.Get("d_live_bg.png").Get(gd));
            Add(Background);

            FriendIcon = new UIImage(ui.Get("d_live_friend.png").Get(gd)) { Position = new Vector2(156, 186) };
            Add(FriendIcon);

            Add(LiveButton = new UIButton(ui.Get("d_live_live.png").Get(gd)) { Position = new Vector2(15, 2) });
            Add(BuyButton = new UIButton(ui.Get("d_live_buy.png").Get(gd)) { Position = new Vector2(107, 27) });
            Add(BuildButton = new UIButton(ui.Get("d_live_build.png").Get(gd)) { Position = new Vector2(179, 80) });
            Add(OptionsButton = new UIButton(ui.Get("d_live_opt.png").Get(gd)) { Position = new Vector2(242, 165) });

            Add(FloorUpButton = new UIStencilButton(ui.Get("d_live_floorup.png").Get(gd)) { Position = new Vector2(16, 150), Shadow = true, ShadowParam = sDir });
            Add(FloorDownButton = new UIStencilButton(ui.Get("d_live_floordown.png").Get(gd)) { Position = new Vector2(16, 192), Shadow = true, ShadowParam = sDir });

            Add(RoofButton = new UIStencilButton(ui.Get("d_live_w1.png").Get(gd)) { Position = new Vector2(15, 111), Shadow = true, ShadowParam = sDir });
            Add(WallsUpButton = new UIStencilButton(ui.Get("d_live_w2.png").Get(gd)) { Position = new Vector2(50, 107), Shadow = true, ShadowParam = sDir });
            Add(WallsCutButton = new UIStencilButton(ui.Get("d_live_w3.png").Get(gd)) { Position = new Vector2(86, 112), Shadow = true, ShadowParam = sDir });
            Add(WallsDownButton = new UIStencilButton(ui.Get("d_live_w4.png").Get(gd)) { Position = new Vector2(117, 122), Shadow = true, ShadowParam = sDir });

            Add(ZoomInButton = new UIStencilButton(ui.Get("d_live_zoomp.png").Get(gd)) { Position = new Vector2(87, 154) });
            Add(ZoomOutButton = new UIStencilButton(ui.Get("d_live_zoomm.png").Get(gd)) { Position = new Vector2(87, 196) });
            Add(RotateCWButton = new UIStencilButton(ui.Get("d_live_rotcw.png").Get(gd)) { Position = new Vector2(62, 175) });
            Add(RotateCCWButton = new UIStencilButton(ui.Get("d_live_rotccw.png").Get(gd)) { Position = new Vector2(114, 175) });

            SpeedButtons = new UIButton[4];
            for (int i=0; i<4; i++)
            {
                Add(SpeedButtons[i] = new UIStencilButton(ui.Get($"d_live_speed{i+1}.png").Get(gd))
                {
                    Position = new Vector2(158 + 30 * i, 246),
                    Shadow = true,
                    ShadowParam = sDir
                });
                var speed = i + 1;
                SpeedButtons[i].OnButtonClick += (btn) =>
                {
                    SwitchSpeed(speed);
                };
            }

            var largeStyle = TextStyle.DefaultLabel.Clone();
            largeStyle.Size = 15;
            largeStyle.Color = UIStyle.Current.DialogTitle;

            var whiteStyle = TextStyle.DefaultLabel.Clone();
            whiteStyle.Size = 12;
            whiteStyle.Color = UIStyle.Current.Text;

            var shadowStyle = TextStyle.DefaultLabel.Clone();
            shadowStyle.Size = 12;
            shadowStyle.Color = Color.Black * 0.5f;

            var friendStyle = TextStyle.DefaultLabel.Clone();
            friendStyle.Size = 12;
            friendStyle.Color = UIStyle.Current.SecondaryText;

            Add(MoneyLabel = new UILabel()
            {
                Position = new Vector2(7, 241),
                Size = new Vector2(138, 30),
                Alignment = TextAlignment.Middle | TextAlignment.Center,
                Caption = "$0",
                CaptionStyle = largeStyle
            });

            Add(TimeLabelShadow = new UILabel()
            {
                Position = new Vector2(157, 221+2),
                Size = new Vector2(114, 16),
                Alignment = TextAlignment.Middle | TextAlignment.Center,
                Caption = "12:00AM",
                CaptionStyle = shadowStyle
            });

            Add(TimeLabel = new UILabel()
            {
                Position = new Vector2(157, 221),
                Size = new Vector2(114, 16),
                Alignment = TextAlignment.Middle | TextAlignment.Center,
                Caption = "12:00AM",
                CaptionStyle = whiteStyle
            });

            Add(FriendsLabelShadow = new UILabel()
            {
                Position = new Vector2(176, 184+2),
                Alignment = TextAlignment.Top | TextAlignment.Left,
                Caption = "0",
                CaptionStyle = shadowStyle
            });

            Add(FriendsLabel = new UILabel()
            {
                Position = new Vector2(176, 184),
                Alignment = TextAlignment.Top | TextAlignment.Left,
                Caption = "0",
                CaptionStyle = friendStyle
            });

            Add(FloorLabelShadow = new UILabel()
            {
                Position = new Vector2(22, 184+2),
                Size = new Vector2(24, 15),
                Alignment = TextAlignment.Center | TextAlignment.Middle,
                Caption = "1st",
                CaptionStyle = shadowStyle
            });

            Add(FloorLabel = new UILabel()
            {
                Position = new Vector2(22, 184),
                Size = new Vector2(24, 15),
                Alignment = TextAlignment.Center | TextAlignment.Middle,
                Caption = "1st",
                CaptionStyle = whiteStyle
            });

            RoofButton.OnButtonClick += (btn) => SetCut(3);
            WallsUpButton.OnButtonClick += (btn) => SetCut(2);
            WallsCutButton.OnButtonClick += (btn) => SetCut(1);
            WallsDownButton.OnButtonClick += (btn) => SetCut(0);

            LiveButton.OnButtonClick += (btn) => OnModeClick?.Invoke(UIMainPanelMode.LIVE);
            BuyButton.OnButtonClick += (btn) => OnModeClick?.Invoke(UIMainPanelMode.BUY);
            BuildButton.OnButtonClick += (btn) => OnModeClick?.Invoke(UIMainPanelMode.BUILD);
            OptionsButton.OnButtonClick += (btn) => OnModeClick?.Invoke(UIMainPanelMode.OPTIONS);

            ZoomInButton.OnButtonClick += ZoomControl;
            ZoomOutButton.OnButtonClick += ZoomControl;
            RotateCWButton.OnButtonClick += RotateClockwise;
            RotateCCWButton.OnButtonClick += RotateCounterClockwise;

            FloorUpButton.OnButtonClick += (b) => { if (Game.Level < 5) Game.Level++; };
            FloorDownButton.OnButtonClick += (b) => { if (Game.Level > 1) Game.Level--; };

            Size = new Vector2(Background.Width, Background.Height);

            UpdateBuildBuy();
            UpdateMoneyDisplay();
            UpdateZoomButton();
        }

        private void ZoomControl(UIElement button)
        {
            if (FSOEnvironment.Enable3D) return;
            Game.ZoomLevel = (Game.ZoomLevel + ((button == ZoomInButton) ? -1 : 1));
        }

        private void RotateCounterClockwise(UIElement button)
        {
            if (FSOEnvironment.Enable3D) return;
            var newRot = (Game.Rotation - 1);
            if (newRot < 0) newRot = 3;
            Game.Rotation = newRot;
        }

        private void RotateClockwise(UIElement button)
        {
            if (FSOEnvironment.Enable3D) return;
            Game.Rotation = (Game.Rotation + 1) % 4;
        }

        private string LastClock = "";
        private int LastSpeed = -1;
        private int LastCut;
        private int LastMoney = 0;
        private sbyte LastFloor = 0;
        private int LastZoom;
        public override void Update(UpdateState state)
        {
            var vm = Game.vm;
            var min = vm.Context.Clock.Minutes;
            var hour = vm.Context.Clock.Hours;

            string suffix = (hour > 11) ? "PM" : "AM";
            hour %= 12;
            if (hour == 0) hour = 12;

            var text = hour.ToString() + ":" + min.ToString().PadLeft(2, '0') + " " + suffix;

            if (text != LastClock)
            {
                LastClock = text;
                TimeLabel.Caption = text;
                TimeLabelShadow.Caption = text;
            }

            if (Game.Level != LastFloor)
            {
                LastFloor = Game.Level;
                FloorLabel.Caption = FloorNames[LastFloor - 1];
                FloorLabelShadow.Caption = FloorNames[LastFloor - 1];
                FloorDownButton.Disabled = LastFloor == 1;
                FloorUpButton.Disabled = LastFloor == 5;
            }

            var speed = RemapSpeed[Math.Max(0, vm.SpeedMultiplier)];
            if (speed != LastSpeed)
            {
                /*
                if (speed == 4) InnerBg.Texture = Content.Get().CustomUI.Get("clockinbg_pause.png").Get(GameFacade.GraphicsDevice);
                else if (LastSpeed == 4) InnerBg.Texture = Content.Get().CustomUI.Get("clockinbg.png").Get(GameFacade.GraphicsDevice);
                */

                for (int i = 0; i < 4; i++)
                {
                    SpeedButtons[i].Selected = (i + 1 == speed);
                }
                LastSpeed = speed;
            }
            
            if (LastCut != Game.LotControl.WallsMode)
            {
                LastCut = Game.LotControl.WallsMode;
                var ui = Content.Get().CustomUI;

                RoofButton.Selected = LastCut == 3;
                WallsUpButton.Selected = LastCut == 2;
                WallsCutButton.Selected = LastCut == 1;
                WallsDownButton.Selected = LastCut == 0;
            }

            var money = GetMoney();
            if (LastMoney != money)
            {
                DisplayChange(money - LastMoney);
                LastMoney = money;
                UpdateMoneyDisplay();
            }

            if (LastZoom != Game.ZoomLevel) UpdateZoomButton();

            base.Update(state);

            //KEY SHORTCUTS
            var keys = state.NewKeys;
            var nofocus = true;
            if (Game.InLot)
            {
                if (keys.Contains(Keys.F1) && !LiveButton.Disabled) OnModeClick?.Invoke(UIMainPanelMode.LIVE);
                if (keys.Contains(Keys.F2) && !BuyButton.Disabled) OnModeClick?.Invoke(UIMainPanelMode.BUY);
                if (keys.Contains(Keys.F3) && !BuildButton.Disabled) OnModeClick?.Invoke(UIMainPanelMode.BUILD);
                if (keys.Contains(Keys.F4)) OnModeClick?.Invoke(UIMainPanelMode.OPTIONS); // Options Panel

                if (nofocus)
                {
                    if (FSOEnvironment.Enable3D)
                    {
                        //if the zoom or rotation buttons are down, gradually change their values.
                        if (RotateCWButton.IsDown || state.KeyboardState.IsKeyDown(Keys.OemPeriod)) ((WorldStateRC)Game.vm.Context.World.State).RotationX += 2f / FSOEnvironment.RefreshRate;
                        if (RotateCCWButton.IsDown || state.KeyboardState.IsKeyDown(Keys.OemComma)) ((WorldStateRC)Game.vm.Context.World.State).RotationX -= 2f / FSOEnvironment.RefreshRate;
                        if (ZoomInButton.IsDown || (state.KeyboardState.IsKeyDown(Keys.OemPlus) && !state.CtrlDown)) Game.LotControl.TargetZoom = Math.Max(0.25f, Math.Min(Game.LotControl.TargetZoom + 1f / FSOEnvironment.RefreshRate, 2));
                        if (ZoomOutButton.IsDown || (state.KeyboardState.IsKeyDown(Keys.OemMinus) && !state.CtrlDown)) Game.LotControl.TargetZoom = Math.Max(0.25f, Math.Min(Game.LotControl.TargetZoom - 1f / FSOEnvironment.RefreshRate, 2));
                    }
                    else
                    {
                        if (keys.Contains(Keys.OemPlus) && !state.CtrlDown && !ZoomInButton.Disabled) { Game.ZoomLevel -= 1; UpdateZoomButton(); }
                        if (keys.Contains(Keys.OemMinus) && !state.CtrlDown && !ZoomOutButton.Disabled) { Game.ZoomLevel += 1; UpdateZoomButton(); }
                        if (keys.Contains(Keys.OemComma)) RotateCounterClockwise(null);
                        if (keys.Contains(Keys.OemPeriod)) RotateClockwise(null);
                    }
                    if (keys.Contains(Keys.PageDown)) { if (Game.Level > 1) Game.Level--; }
                    if (keys.Contains(Keys.PageUp)) { if (Game.Level < 5) Game.Level++; }
                    if (keys.Contains(Keys.Home)) UpdateWallsViewKeyHandler(1);
                    if (keys.Contains(Keys.End)) UpdateWallsViewKeyHandler(0);
                }
            }
        }

        private void UpdateWallsViewKeyHandler(int type)
        {
            var mode = Game.LotControl.WallsMode;
            switch (type)
            {
                case 0:
                    if (mode > 0) Game.LotControl.WallsMode -= 1;
                    break;
                case 1:
                    if (mode < 3) Game.LotControl.WallsMode += 1;
                    break;
            }
        }

        public void UpdateZoomButton()
        {
            ZoomInButton.Disabled = (!Game.InLot) || (!FSOEnvironment.Enable3D && (Game.ZoomLevel == 1));
            ZoomOutButton.Disabled = (!FSOEnvironment.Enable3D && (Game.ZoomLevel == 3));
            LastZoom = Game.ZoomLevel;
        }

        public void SetMode(UIMainPanelMode mode)
        {
            LiveButton.Selected = mode == UIMainPanelMode.LIVE;
            BuyButton.Selected = mode == UIMainPanelMode.BUY;
            BuildButton.Selected = mode == UIMainPanelMode.BUILD;
            OptionsButton.Selected = mode == UIMainPanelMode.OPTIONS;
        }

        public void DisplayChange(int change)
        {
            var newLabel = new UILabel();
            newLabel.Position = MoneyLabel.Position;
            newLabel.Y += -20f;
            newLabel.CaptionStyle = MoneyLabel.CaptionStyle.Clone();
            newLabel.CaptionStyle.Size = 15;
            newLabel.CaptionStyle.Color = (change > 0) ? UIStyle.Current.Text : UIStyle.Current.NegMoney;
            newLabel.Alignment = FSO.Client.UI.Framework.TextAlignment.Right | FSO.Client.UI.Framework.TextAlignment.Middle;
            newLabel.Size = MoneyLabel.Size;

            newLabel.Caption = ((change > 0) ? "+" : "-") + "§" + Math.Abs(change);
            DynamicOverlay.Add(newLabel);

            GameFacade.Screens.Tween.To(newLabel, 1.5f, new Dictionary<string, float>() { { "Y", newLabel.Y-30 }, { "Opacity", 0 } });
            GameThread.SetTimeout(() => { Remove(newLabel); }, 1500);
        }

        private void UpdateMoneyDisplay()
        {
            MoneyLabel.Caption = "§" + LastMoney.ToString("##,#0");
            MoneyLabel.Visible = Game.vm.GetGlobalValue(32) == 0;
        }

        private int GetMoney()
        {
            return Game.ActiveFamily?.Budget ?? 0;
        }

        public void UpdateBuildBuy()
        {
            var bbEnable = Game.vm.Context.Architecture.BuildBuyEnabled;
            BuyButton.Disabled = !bbEnable;
            BuildButton.Disabled = !bbEnable;
            LiveButton.Disabled = Game.vm.GetGlobalValue(32) != 0;
        }

        public void SetCut(int cut)
        {
            Game.LotControl.World.State.DrawRoofs = (cut == 3);
            Game.LotControl.WallsMode = cut;
        }

        public void SwitchSpeed(int speed)
        {
            var vm = Game.vm;
            if (vm.SpeedMultiplier == -1) return;
            switch (vm.SpeedMultiplier)
            {
                case 0:
                    switch (speed)
                    {
                        case 1:
                            HITVM.Get().PlaySoundEvent(UISounds.SpeedPTo1); break;
                        case 2:
                            HITVM.Get().PlaySoundEvent(UISounds.SpeedPTo2); break;
                        case 3:
                            HITVM.Get().PlaySoundEvent(UISounds.SpeedPTo3); break;
                    }
                    break;
                case 1:
                    switch (speed)
                    {
                        case 4:
                            HITVM.Get().PlaySoundEvent(UISounds.Speed1ToP); break;
                        case 2:
                            HITVM.Get().PlaySoundEvent(UISounds.Speed1To2); break;
                        case 3:
                            HITVM.Get().PlaySoundEvent(UISounds.Speed1To3); break;
                    }
                    break;
                case 3:
                    switch (speed)
                    {
                        case 4:
                            HITVM.Get().PlaySoundEvent(UISounds.Speed2ToP); break;
                        case 1:
                            HITVM.Get().PlaySoundEvent(UISounds.Speed2To1); break;
                        case 3:
                            HITVM.Get().PlaySoundEvent(UISounds.Speed2To3); break;
                    }
                    break;
                case 10:
                    switch (speed)
                    {
                        case 4:
                            HITVM.Get().PlaySoundEvent(UISounds.Speed3ToP); break;
                        case 1:
                            HITVM.Get().PlaySoundEvent(UISounds.Speed3To1); break;
                        case 2:
                            HITVM.Get().PlaySoundEvent(UISounds.Speed3To2); break;
                    }
                    break;
            }

            switch (speed)
            {
                case 4: vm.SpeedMultiplier = 0; break;
                case 1: vm.SpeedMultiplier = 1; break;
                case 2: vm.SpeedMultiplier = 3; break;
                case 3: vm.SpeedMultiplier = 10; break;
            }
        }
    }
}
