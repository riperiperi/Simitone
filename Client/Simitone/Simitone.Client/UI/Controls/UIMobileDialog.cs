using FSO.Client;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Content;
using Microsoft.Xna.Framework;
using Simitone.Client.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simitone.Client.UI.Controls
{
    public class UIMobileDialog : UIContainer
    {
        public UIDiagonalStripe BackStripe;
        public UIDiagonalStripe FrontStripe;

        private UILabel TitleLabel;
        private UIImage TitleBg;

        public string Caption
        {
            set { TitleLabel.Caption = value; }
        }

        public int Width;
        public int Height;
        public int ScrHeight;

        private int BaseY
        {
            get { return (int)FrontStripe.Y; }
        }

        protected bool Closing;

        private float _i;
        public float InterpolatedAnimation
        {
            set
            {
                Position = new Vector2(0, (ScrHeight - Height) / 2);
                BackStripe.X = (Closing)?0:(Width * (1 - value));
                BackStripe.BodySize = new Point((int)(value * Width), ScrHeight);
                BackStripe.Y = -Position.Y;

                var t2 = Math.Max(0, value - 0.2f) / 0.8f;
                FrontStripe.X = (Closing)?(Width * (1 - t2)):0;
                FrontStripe.BodySize = new Point((int)(t2 * Width), Height);

                TitleBg.Y = 45 - 35 * t2;
                TitleLabel.Y = 15;
                TitleLabel.CaptionStyle.Color = UIStyle.Current.DialogTitle * t2;
                TitleBg.SetSize(Width, 70 * t2);
                _i = value;
                if (value == 0f && Closing) UIScreen.RemoveDialog(this);
            }

            get
            {
                return _i;
            }
        }

        public UIMobileDialog() : base()
        {
            Width = GameFacade.Screens.CurrentUIScreen.ScreenWidth;
            ScrHeight = GameFacade.Screens.CurrentUIScreen.ScreenHeight;

            BackStripe = new UIDiagonalStripe(new Point(), UIDiagonalStripeSide.LEFT, new Color(0, 70, 140) * 0.33f);
            Add(BackStripe);
            FrontStripe = new UIDiagonalStripe(new Point(), UIDiagonalStripeSide.RIGHT, UIStyle.Current.DialogBg);
            Add(FrontStripe);

            TitleBg = new UIImage(Content.Get().CustomUI.Get("dialog_title_grad.png").Get(GameFacade.GraphicsDevice));
            TitleBg.SetSize(Width, 70);
            Add(TitleBg);

            TitleLabel = new UILabel();
            TitleLabel.X = 50;
            TitleLabel.CaptionStyle = TitleLabel.CaptionStyle.Clone();
            TitleLabel.CaptionStyle.Size = 37;
            TitleLabel.CaptionStyle.Color = UIStyle.Current.DialogTitle;
            TitleLabel.Alignment = TextAlignment.Top | TextAlignment.Left;
            Add(TitleLabel);

            InterpolatedAnimation = 0f;
            GameFacade.Screens.Tween.To(this, 0.5f, new Dictionary<string, float>() { { "InterpolatedAnimation", 1f} }, TweenQuad.EaseOut);
        }

        public void Close()
        {
            if (!Closing)
            {
                Closing = true;
                BackStripe.DiagSide = UIDiagonalStripeSide.RIGHT;
                FrontStripe.DiagSide = UIDiagonalStripeSide.LEFT;
                GameFacade.Screens.Tween.To(this, 0.3f, new Dictionary<string, float>() { { "InterpolatedAnimation", 0f } }, TweenQuad.EaseIn);
            }
        }

        public void SetHeight(int height)
        {
            Height = height;
            InterpolatedAnimation = _i;
        }
    }
}
