using FSO.Client;
using FSO.Client.UI.Framework;
using FSO.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simitone.Client.UI.Controls
{
    public class UICatButton : UIStencilButton
    {
        private Texture2D CatBase;
        private Texture2D Replaced;
        public UICatButton(Texture2D tex) : base(tex)
        {
            Alpha = 1f;
            ImageStates = 1;
            CatBase = Content.Get().CustomUI.Get("cat_btn_base.png").Get(GameFacade.GraphicsDevice);
        }

        public void ReplaceImage(Texture2D tex)
        {
            if (Replaced == null)
            {
                Replaced = Texture;
            }
            Texture = tex;
        }

        public void RestoreImage()
        {
            if (Replaced != null)
            {
                Texture = Replaced;
                Replaced = null;
            }
        }

        public override void Draw(UISpriteBatch SBatch)
        {
            if (!Visible) return;
            base.Draw(SBatch);
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
            DrawLocalTexture(SBatch, CatBase, null, new Vector2(-5), Vector2.One, color * Alpha);
        }
    }
}
