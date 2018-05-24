using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.Client.UI.Framework;
using FSO.Client.UI.Controls;
using System.Threading;
using FSO.Client;
using FSO.Common.Rendering.Framework.Model;
using FSO.Content;
using Simitone.Client.UI.Panels;
using Simitone.Client.UI.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FSO.Common.Utils;
using FSO.SimAntics;

namespace Simitone.Client.UI.Screens
{
    public class LoadingScreen : FSO.Client.UI.Framework.GameScreen
    {
        public string[] LoadText = new string[] {
            "", //Started = 0,
            "Identifying Works...", //ScanningFiles = 1,
            "Categorising Works...", //InitGlobal = 2,
            "Arranging Wardrobes...", //InitBCF = 3,
            "Reticulating Splines...", //InitAvatars = 4,
            "Performing Sound Check...", //InitAudio = 5,
            "Arranging Furniture...", //InitObjects = 6,
            "Building Concert Hall...", //InitArch = 7,
            " " //Done = 8,
        };

        public UISimitoneBg Bg;
        public UIDiagonalStripe ProgressDiag;
        public UIDiagonalStripe TextDiag;
        public UILoadProgress LoadProgress;
        public UISimitoneLogo Logo;
        public UISimitoneLoadLabel LastLabel;
        public bool LoadingComplete;

        private bool Closing = false;
        private float _i;
        public float InterpolatedAnimation
        {
            set
            {
                TextDiag.X = (Closing) ? 0 : (ScreenWidth * (1 - value));
                TextDiag.BodySize = new Point((int)(value * ScreenWidth), 75);
                TextDiag.Y = ScreenHeight*0.75f - 37;

                ProgressDiag.X = (Closing) ? (ScreenWidth * (1 - value)) : 0;
                ProgressDiag.BodySize = new Point((int)(value * ScreenWidth), 150);

                LoadProgress.Opacity = value;

                _i = value;
            }

            get
            {
                return _i;
            }
        }

        public LoadingScreen()
        {
            Content.InitBasic(GlobalSettings.Default.StartupPath, GameFacade.GraphicsDevice);

            Bg = new UISimitoneBg();
            Bg.Position = (new Vector2(ScreenWidth, ScreenHeight)) / 2;
            Add(Bg);

            ProgressDiag = new UIDiagonalStripe(new Point(0, 150), UIDiagonalStripeSide.RIGHT, Color.Black * 0.75f);
            ProgressDiag.Position = new Vector2(0, ScreenHeight / 2 - 75);
            Add(ProgressDiag);

            TextDiag = new UIDiagonalStripe(new Point(0, 150), UIDiagonalStripeSide.LEFT, Color.Black * 0.5f);
            TextDiag.Position = new Vector2(0, ScreenHeight * 0.75f - 37);
            Add(TextDiag);

            GameFacade.Screens.Tween.To(this, 0.5f, new Dictionary<string, float>() { { "InterpolatedAnimation", 1f } }, TweenQuad.EaseOut);

            LoadProgress = new UILoadProgress();
            LoadProgress.Position = (new Vector2(ScreenWidth, ScreenHeight) - new Vector2(938, 112))/2;
            Add(LoadProgress);

            Logo = new UISimitoneLogo();
            Logo.Position = new Vector2(ScreenWidth, ScreenHeight) / 2;
            Add(Logo);
            GameFacade.Screens.Tween.To(Logo, 1f, new Dictionary<string, float>() { { "Y", ScreenHeight/4 }, { "ScaleX", 0.5f }, { "ScaleY", 0.5f } }, TweenQuad.EaseOut);

            InterpolatedAnimation = InterpolatedAnimation;

            (new Thread(() => {
                FSO.Content.Content.Init(GlobalSettings.Default.StartupPath, GameFacade.GraphicsDevice);
                VMContext.InitVMConfig();
                lock (this)
                {
                    LoadingComplete = true;
                }
            })).Start();
        }

        public void Close()
        {
            Closing = true;
            GameThread.SetTimeout(() =>
            {
                ProgressDiag.DiagSide = UIDiagonalStripeSide.LEFT;
                TextDiag.DiagSide = UIDiagonalStripeSide.RIGHT;
                GameFacade.Screens.Tween.To(this, 0.5f, new Dictionary<string, float>() { { "InterpolatedAnimation", 0f } }, TweenQuad.EaseOut);
                GameFacade.Screens.Tween.To(Logo, 0.5f, new Dictionary<string, float>() { { "Y", -200f } }, TweenQuad.EaseIn);
                GameFacade.Screens.Tween.To(Bg, 0.5f, new Dictionary<string, float>() { { "Opacity", 0f } }, TweenQuad.EaseOut);

                GameThread.SetTimeout(() =>
                {
                    ProgressDiag.Parent.Remove(ProgressDiag);
                    TextDiag.Parent.Remove(TextDiag);
                    Logo.Parent.Remove(Logo);
                    LoadProgress.Parent.Remove(LoadProgress);
                    Bg.Parent.Remove(Bg);
                }, 500);
            }, 750);
        }

        public ContentLoadingProgress LastProgress = ContentLoadingProgress.Invalid;

        public override void Update(UpdateState state)
        {
            if (LastProgress != Content.LoadProgress)
            {
                LastProgress = Content.LoadProgress;

                if (LastLabel != null)
                {
                    LastLabel.Kill();
                }
                LastLabel = new UISimitoneLoadLabel(LoadText[(int)LastProgress]);
                LastLabel.Position = new Vector2(ScreenWidth/2, ScreenHeight * 0.75f);
                Add(LastLabel);

                LoadProgress.OverallPercent = (float)LastProgress / (float)ContentLoadingProgress.Done;
            }
            lock (this)
            {
                if (LoadingComplete)
                {
                    GameController.EnterGameMode("", false);
                }
            }
            base.Update(state);
        }
    }

    public class UISimitoneLogo : UIElement
    {
        public Texture2D Logo;

        public UISimitoneLogo() : base()
        {
            var ui = Content.Get().CustomUI;
            Logo = ui.Get("load_logo.png").Get(GameFacade.GraphicsDevice);
        }

        public override void Draw(UISpriteBatch batch)
        {
            DrawLocalTexture(batch, Logo, new Vector2(Logo.Width, Logo.Height) / -2);
        }
    }

    public class UISimitoneBg : UIElement
    {
        public Texture2D Bg;

        public UISimitoneBg() : base()
        {
            var ui = Content.Get().CustomUI;
            Bg = ui.Get("load_static_bg.png").Get(GameFacade.GraphicsDevice);
        }

        public override void Draw(UISpriteBatch batch)
        {
            if (!Visible) return;
            var scale = Math.Max(GameFacade.Screens.CurrentUIScreen.ScreenWidth / 1136f, GameFacade.Screens.CurrentUIScreen.ScreenHeight / 640f);
            DrawLocalTexture(batch, Bg, null, new Vector2(Bg.Width, Bg.Height) / -2 * scale, new Vector2(scale));
        }
    }

    public class UISimitoneLoadLabel : UIContainer
    {
        public UILabel Label;

        private float _Alpha;
        public float Alpha
        {
            get
            {
                return _Alpha;
            }
            set
            {
                Label.CaptionStyle.Color = Color.White * value;
                _Alpha = value;
            }
        }

        public UISimitoneLoadLabel(string text)
        {
            Label = new UILabel();
            Label.CaptionStyle = Label.CaptionStyle.Clone();
            Label.CaptionStyle.Size = 37;
            Label.CaptionStyle.Color = Color.White;
            Label.Caption = text;
            Label.Alignment = TextAlignment.Center | TextAlignment.Middle;
            Label.Size = new Vector2(1, 1);
            Label.Y = 100f;
            Add(Label);
            Alpha = Alpha;
            GameFacade.Screens.Tween.To(Label, 0.4f, new Dictionary<string, float>() { { "Y", 0f } }, TweenQuad.EaseOut);
            GameFacade.Screens.Tween.To(this, 0.4f, new Dictionary<string, float>() { { "Alpha", 1f } }, TweenQuad.EaseOut);
        }

        public void Kill()
        {
            GameFacade.Screens.Tween.To(Label, 0.4f, new Dictionary<string, float>() { { "Y", -100f } }, TweenQuad.EaseIn);
            GameFacade.Screens.Tween.To(this, 0.4f, new Dictionary<string, float>() { { "Alpha", 0f } }, TweenQuad.EaseIn);
            GameThread.SetTimeout(() => { Parent.Remove(this); }, 400);
        }
    }
}
