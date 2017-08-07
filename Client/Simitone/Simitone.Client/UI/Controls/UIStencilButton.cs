using FSO.Client.UI.Controls;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.Client.UI.Framework;
using Microsoft.Xna.Framework;
using Simitone.Client.UI.Model;

namespace Simitone.Client.UI.Controls
{
    public class UIStencilButton : UIButton
    {
        public Color Color = UIStyle.Current.BtnNormal;
        public Color ActiveColor = UIStyle.Current.BtnActive;
        public Color HoverColor = Color.Lerp(UIStyle.Current.BtnNormal, UIStyle.Current.BtnActive, 0.5f);
        public Color DisabledColor = new Color(128, 128, 128, 255);
        public bool Shadow;
        public float Alpha { get; set; }

        public UIStencilButton(Texture2D tex) : base(tex)
        {
            Alpha = 1f;
            ImageStates = 1;
        }

        public override void Draw(UISpriteBatch SBatch)
        {
            if (!Visible) return;
            var frame = CurrentFrame;
            if (Disabled)
            {
                frame = 3;
            }
            if (Selected)
            {
                frame = 1;
            }
            if (ForceState > -1) frame = ForceState;
            frame = Math.Min(3, frame);

            Color color;
            switch (frame)
            {
                case 1:
                    color = ActiveColor; break;
                case 2:
                    color = HoverColor; break;
                case 3:
                    color = DisabledColor; break;
                default:
                    color = Color; break;
            }
            if (Shadow)
                DrawLocalTexture(SBatch, Texture, null, new Vector2(3f, 3f), Vector2.One, Color.Black * 0.25f * Alpha);
            DrawLocalTexture(SBatch, Texture, null, Vector2.Zero, Vector2.One, color * Alpha);
        }
    }
}
