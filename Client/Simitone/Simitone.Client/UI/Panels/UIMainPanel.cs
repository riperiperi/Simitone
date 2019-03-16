using FSO.Client;
using FSO.Client.UI.Framework;
using FSO.Common.Utils;
using FSO.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simitone.Client.UI.Controls;
using Simitone.Client.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.Common.Rendering.Framework.Model;
using Simitone.Client.UI.Panels.LiveSubpanels;
using Simitone.Client.UI.Screens;
using FSO.Client.UI.Controls;

namespace Simitone.Client.UI.Panels
{
    public class UIMainPanel : UIContainer
    {
        private float _CurWidth;
        public float CurWidth
        {
            get
            {
                return _CurWidth;
            }
            set
            {
                _CurWidth = value;
                UpdateWidth();
            }
        }

        private Texture2D Div;
        private Texture2D WhitePx;
        private Rectangle DivRect;
        private UIDiagonalStripe Diag;
        public UISubpanel SubPanel;
        public TS1GameScreen Game;

        public UIStencilButton FloorUpBtn;
        public UIStencilButton FloorDownBtn;
        public UILabel FloorLabel;
        public UILabel FloorLabelShadow;
        public UICategorySwitcher Switcher;
        public UIImage Divider;
        public UIStencilButton HideButton;
        public bool ShowingSelect;

        public UISwitchAvatarPanel SwitchAvatar;
        public bool PanelActive;
        public UIMainPanelMode Mode;

        public event Action OnEndSelect;
        public event Action<UIMainPanelMode> ModeChanged;

        public string[] FloorNames = new string[]
        {
            "1st",
            "2nd",
            "3rd",
            "4th",
            "5th"
        };
        public int LastFloor = -1;

        private List<UICategory> LiveCategories = new List<UICategory>()
        {
            new UICategory() { ID = 0, IconName = "live_motives.png" },
            new UICategory() { ID = 1, IconName = "live_job.png" },
            new UICategory() { ID = 2, IconName = "live_personality.png" },
            new UICategory() { ID = 3, IconName = "live_relationships.png" },
            new UICategory() { ID = 4, IconName = "live_inventory.png" }
        };


        private List<UICategory> BuyCategories = new List<UICategory>()
        {
            new UICategory() { ID = 0, IconName = "cat_seat.png" },
            new UICategory() { ID = 1, IconName = "cat_surf.png" },
            new UICategory() { ID = 2, IconName = "cat_appl.png" },
            new UICategory() { ID = 3, IconName = "cat_elec.png" },
            new UICategory() { ID = 4, IconName = "cat_plum.png" },
            new UICategory() { ID = 5, IconName = "cat_deco.png" },
            new UICategory() { ID = 6, IconName = "cat_misc.png" },
            new UICategory() { ID = 7, IconName = "cat_ligt.png" },
        };

        private List<UICategory> BuildCategories = new List<UICategory>()
        {
            new UICategory() { ID = 0, IconName = "cat_build_arch.png" },
            new UICategory() { ID = 1, IconName = "cat_build_outs.png" },
            new UICategory() { ID = 2, IconName = "cat_build_objs.png" },
        };

        private List<UICategory> OptionsCategories = new List<UICategory>()
        {
            new UICategory() { ID = 0, IconName = "cat_build_arch.png" },
        };

        public UIMainPanel(TS1GameScreen game) : base()
        {
            Game = game;
            Diag = new UIDiagonalStripe(new Point(0, 128), UIDiagonalStripeSide.RIGHT, UIStyle.Current.Bg);
            Add(Diag);
            WhitePx = TextureGenerator.GetPxWhite(GameFacade.GraphicsDevice);
            var ui = Content.Get().CustomUI;
            Div = ui.Get("panel_div.png").Get(GameFacade.GraphicsDevice);

            if (!Game.Desktop)
            {
                FloorUpBtn = new UIStencilButton(ui.Get("level_up.png").Get(GameFacade.GraphicsDevice));
                FloorUpBtn.Position = new Vector2(80, 10);
                FloorUpBtn.OnButtonClick += (b) => { if (Game.Level < 5) Game.Level++; };
                Add(FloorUpBtn);

                FloorDownBtn = new UIStencilButton(ui.Get("level_down.png").Get(GameFacade.GraphicsDevice));
                FloorDownBtn.Position = new Vector2(80, 68);
                FloorDownBtn.OnButtonClick += (b) => { if (Game.Level > 1) Game.Level--; };
                Add(FloorDownBtn);

                FloorLabel = new UILabel();
                FloorLabel.CaptionStyle = FloorLabel.CaptionStyle.Clone();
                FloorLabel.CaptionStyle.Size = 15;
                FloorLabel.CaptionStyle.Color = UIStyle.Current.Text;
                FloorLabel.Alignment = TextAlignment.Middle | TextAlignment.Center;
                FloorLabel.Position = new Vector2(80, 64);
                FloorLabel.Size = new Vector2(51, 18);

                FloorLabelShadow = new UILabel();
                FloorLabelShadow.CaptionStyle = FloorLabel.CaptionStyle.Clone();
                FloorLabelShadow.Alignment = TextAlignment.Middle | TextAlignment.Center;
                FloorLabelShadow.Position = new Vector2(83, 67);
                FloorLabelShadow.Size = new Vector2(51, 18);
                FloorLabelShadow.CaptionStyle.Color = Color.Black * 0.5f;
                Add(FloorLabelShadow);
                Add(FloorLabel);

                Divider = new UIImage(ui.Get("divider.png").Get(GameFacade.GraphicsDevice));
                Divider.Position = new Vector2(146, 29);
                Add(Divider);
            }

            HideButton = new UIStencilButton(ui.Get("panel_hide.png").Get(GameFacade.GraphicsDevice));
            HideButton.X = Game.ScreenWidth - (50 + 64 + 15);
            HideButton.Y = 26;
            HideButton.OnButtonClick += (b) => { Close(); };
            Add(HideButton);

            Switcher = new UICategorySwitcher();
            Switcher.Position = new Vector2(164 - (Game.Desktop ? 16 : 0), 0);
            Switcher.InitCategories(LiveCategories);
            Switcher.OnCategorySelect += Switcher_OnCategorySelect;
            Switcher.OnOpen += Switcher_OnOpen;
            Add(Switcher);
            
            foreach (var fade in GetFadeables())
            {
                fade.Opacity = 0;
            }

            Game.LotControl.QueryPanel.Position = new Vector2(53 + (Game.Desktop ? 25 : 0), -5);
            Add(Game.LotControl.QueryPanel);
            Game.LotControl.PickupPanel.Opacity = 0;
            Add(Game.LotControl.PickupPanel);

            CurWidth = 0;
        }

        private void Switcher_OnOpen()
        {
            var panel = SubPanel as UIBuyBrowsePanel;
            if (panel != null)
            {
                panel.Reset();
            }
        }

        public void SetMode(UIMainPanelMode mode)
        {
            if (mode == Mode) return;
            Mode = mode;

            Game.LotControl.World.State.BuildMode = 0;
            switch (mode)
            {
                case UIMainPanelMode.LIVE:
                    Switcher.InitCategories(LiveCategories);
                    break;
                case UIMainPanelMode.BUY:
                    Switcher.InitCategories(BuyCategories);
                    Game.LotControl.World.State.BuildMode = 1;
                    break;
                case UIMainPanelMode.BUILD:
                    Switcher.InitCategories(BuildCategories);
                    Game.LotControl.World.State.BuildMode = 2;
                    break;
                case UIMainPanelMode.OPTIONS:
                    Switcher.InitCategories(OptionsCategories);
                    break;
            }

            var live = (mode == UIMainPanelMode.LIVE);
            Game.LotControl.LiveMode = live;
            HideButton.Visible = live;

            ModeChanged?.Invoke(mode);
        }

        public void Switcher_OnCategorySelect(int obj)
        {
            UISubpanel panel = null;
            switch (Mode)
            {
                case UIMainPanelMode.LIVE:
                    switch (obj)
                    {
                        case 0:
                            panel = new UIMotiveSubpanel(Game); break;
                        case 1:
                            panel = new UIJobSubpanel(Game); break;
                        case 2:
                            panel = new UIPersonalitySubpanel(Game); break;
                        case 3:
                            panel = new UIRelationshipSubpanel(Game); break;
                        case 4:
                            panel = new UIInventorySubpanel(Game); break;
                    }
                    break;
                case UIMainPanelMode.BUY:
                    panel = new UIBuyBrowsePanel(Game, (sbyte)obj, GetLotType(false));
                    break;
                case UIMainPanelMode.BUILD:
                    panel = new UIBuyBrowsePanel(Game, (sbyte)obj, UICatalogMode.Build);
                    break;
                case UIMainPanelMode.OPTIONS:
                    panel = new UIButtonSubpanel(Game, new UICatFunc[] {
                        new UICatFunc(GameFacade.Strings.GetString("145", "3"), "opt_save.png", () => { Game.Save(); }),
                        new UICatFunc(GameFacade.Strings.GetString("145", "1"), "opt_neigh.png", () => { Game.ReturnToNeighbourhood(); }),
                        new UICatFunc(GameFacade.Strings.GetString("145", "5"), "opt_quit.png", () => { Game.CloseAttempt(); }),
                    });
                    break;
            }
            SetSubpanel(panel);
        }

        public UICatalogMode GetLotType(bool music)
        {
            UICatalogMode mode;
            var house = Game.vm.GetGlobalValue(10);
            var zones = Content.Get().Neighborhood.ZoningDictionary;
            short result = 1;
            zones.TryGetValue(house, out result);
            var community = result == 1;

            if (house >= 21 && house <= 31) mode = UICatalogMode.Downtown;
            else if (house >= 40 && house <= 49) mode = UICatalogMode.Vacation;
            else if (house >= 81 && house <= 90) mode = UICatalogMode.Studiotown;
            else if (house >= 90 && house <= 99 && (music || community)) mode = UICatalogMode.Magictown;
            else if (community) mode = UICatalogMode.Community;
            else mode = (music && Game.vm.GetGlobalValue(32) > 0)?UICatalogMode.Downtown:UICatalogMode.Normal;

            return mode;
        }

        public void SetSubpanel(UISubpanel sub)
        {
            if (SubPanel != null)
            {
                SubPanel.Kill();
            }
            SubPanel = sub;
            if (sub != null)
            {
                SubPanel.Position = new Vector2(263, 0);
                Add(SubPanel);
            }
        }

        public void SetSubpanelPickup(float opacity)
        {
            //used to hide subpanels to make way for the PickupPanel
            if (SubPanel != null) GameFacade.Screens.Tween.To(SubPanel, 0.3f, new Dictionary<string, float>() { { "Opacity", opacity } }, TweenQuad.EaseOut);
            GameFacade.Screens.Tween.To(Switcher.MainButton, 0.3f, new Dictionary<string, float>() { { "Opacity", opacity } }, TweenQuad.EaseOut);
            GameFacade.Screens.Tween.To(Game.LotControl.PickupPanel, 0.3f, new Dictionary<string, float>() { { "Opacity", 1-opacity } }, TweenQuad.EaseOut);
            if (opacity == 0) Switcher.Close();
        }

        private void UpdateWidth()
        {
            //prepanel width is 167
            //div width is 52

            var iWidth = (int)CurWidth;
            if (iWidth < 211)
            {
                Diag.X = 0;
                Diag.BodySize = new Point(iWidth, 128);
                DivRect = new Rectangle();
            } else if (iWidth < 211+52)
            {
                Diag.X = iWidth;
                Diag.BodySize = new Point(0, 128);
                DivRect = new Rectangle(0, 0, iWidth - 211, 128);
            } else
            {
                Diag.X = 211 + 52;
                Diag.BodySize = new Point(iWidth - (211 + 52), 128);
                DivRect = new Rectangle(0, 0, 52, 128);
            }
        }

        public UIElement[] GetFadeables()
        {
            if (Game.Desktop)
            {
                return new UIElement[]
                {
                Switcher.MainButton,
                HideButton
                };
            }
            else
            {
                return new UIElement[]
                {
                FloorUpBtn,
                FloorDownBtn,
                FloorLabel,
                FloorLabelShadow,
                Switcher.MainButton,
                Divider,
                HideButton
                };
            }
        }

        public override void Draw(UISpriteBatch batch)
        {
            if (CurWidth > 211)
            {
                if (ShowingSelect)
                {
                    DrawLocalTexture(batch, WhitePx, null, new Vector2(0, 0), new Vector2(211+52, 128), UIStyle.Current.Bg);
                }
                else
                {
                    DrawLocalTexture(batch, WhitePx, null, new Vector2(0, 0), new Vector2(211, 128), UIStyle.Current.Bg);
                    DrawLocalTexture(batch, Div, DivRect, new Vector2(211, 0), Vector2.One, UIStyle.Current.Bg);
                }
            }
            base.Draw(batch);

        }

        public override void Update(UpdateState state)
        {
            base.Update(state);
            Visible = _CurWidth > 0;

            if (!Game.Desktop)
            {
                if (Game.Level != LastFloor)
                {
                    LastFloor = Game.Level;
                    FloorLabel.Caption = FloorNames[LastFloor - 1];
                    FloorLabelShadow.Caption = FloorNames[LastFloor - 1];
                    FloorDownBtn.Disabled = LastFloor == 1;
                    FloorUpBtn.Disabled = LastFloor == 5;
                }
            }

            Game.LotControl.PickupPanel.Visible = Game.LotControl.PickupPanel.Opacity > 0;

            if (Mode != UIMainPanelMode.LIVE)
            {
                Game.vm.SpeedMultiplier = -1;
            } else if (Game.vm.SpeedMultiplier < 0)
            {
                Game.vm.SpeedMultiplier = 0;
            }
        }

        public void Open()
        {
            Visible = true;
            GameFacade.Screens.Tween.To(this, 0.5f, new Dictionary<string, float>() { { "CurWidth", GameFacade.Screens.CurrentUIScreen.ScreenWidth-X} }, TweenQuad.EaseOut);
            foreach (var fade in GetFadeables())
            {
                GameFacade.Screens.Tween.To(fade, 0.3f, new Dictionary<string, float>() { { "Opacity", 1f } });
            }
            PanelActive = true;
        }

        public void Close()
        {
            GameFacade.Screens.Tween.To(this, 0.5f, new Dictionary<string, float>() { { "CurWidth", 0 } }, TweenQuad.EaseOut);
            SetSubpanel(null);
            foreach (var fade in GetFadeables())
            {
                GameFacade.Screens.Tween.To(fade, 0.3f, new Dictionary<string, float>() { { "Opacity", 0f } });
            }
            if (Switcher.CategoryExpand > 0) Switcher.Close();

            SwitchAvatar?.Kill();
            SwitchAvatar = null;
            PanelActive = false;
        }

        public void ShowSelect()
        {
            var add = new UISwitchAvatarPanel(Game);
            Add(add);

            SetSubpanel(null);
            foreach (var fade in GetFadeables())
            {
                GameFacade.Screens.Tween.To(fade, 0.3f, new Dictionary<string, float>() { { "Opacity", 0f } });
            }
            if (Switcher.CategoryExpand > 0) Switcher.Close();
            ShowingSelect = true;
            SwitchAvatar = add;

            add.OnEnd += () =>
            {
                Open();
                Switcher_OnCategorySelect(Switcher.ActiveCategory);
                SwitchAvatar = null;
                ShowingSelect = false;
                OnEndSelect?.Invoke();
            };
        }

        public override void GameResized()
        {
            base.GameResized();
            if (PanelActive) CurWidth = Game.ScreenWidth - X;
            HideButton.X = Game.ScreenWidth - (50 + X);
        }
    }

    public enum UIMainPanelMode
    {
        LIVE,
        BUY,
        BUILD,
        OPTIONS
    }
}
