using FSO.Client.UI.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.Common.Rendering.Framework.Model;
using FSO.Content;
using FSO.Client;
using FSO.Common.Utils;

namespace Simitone.Client.UI.Panels
{
    public class UIRotationAnimation : UICachedContainer
    {
        public float Step;
        public Texture2D ArrowBack;
        public Texture2D Arrow;
        public Texture2D Segment;

        public UIRotationAnimation() : base()
        {
            Opacity = 0;
            Visible = false;
            Size = new Vector2(360, 360);

            var ui = Content.Get().CustomUI;
            ArrowBack = ui.Get("rot_arrow_back.png").Get(GameFacade.GraphicsDevice);
            Arrow = ui.Get("rot_arrow_front.png").Get(GameFacade.GraphicsDevice);
            Segment = ui.Get("rot_seg.png").Get(GameFacade.GraphicsDevice);
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);
            Invalidate();
            Visible = true;
        }

        public override void InternalDraw(UISpriteBatch batch)
        {
            base.Draw(batch);
            for (int j = 0; j < 2; j++)
            {
                var baseRot = (float)(Math.PI / 12) * (2+(j*12));

                DrawLocalTexture(batch, ArrowBack, null, new Vector2(165, 165), Vector2.One, Color.White, baseRot + (float)(Math.PI / 14) * Step * 12, new Vector2(51, 169));
                var ceil = Math.Ceiling(Step * 12);
                for (int i = 0; i < ceil; i++)
                {
                    var rotLevels = (i == ceil - 1 && i != 0) ? (Step * 12)-1 : i;
                    DrawLocalTexture(batch, Segment, null, new Vector2(165, 165), Vector2.One, Color.White, baseRot + (float)(Math.PI / 14) * rotLevels, new Vector2(38, 150));
                }
                DrawLocalTexture(batch, Arrow, null, new Vector2(165, 165), Vector2.One, Color.White, baseRot + (float)(Math.PI / 14) * Step * 12, new Vector2(51, 169));
            }
        }

        public void Kill(bool success)
        {
            if (success) Step = 1f;
            GameFacade.Screens.Tween.To(this, 0.3f, new Dictionary<string, float>() {
                { "Opacity", 0f },
                { "ScaleX", ScaleX*(success?1.5f:0.8f) },
                { "ScaleY", (success ? 1.5f : 0.8f) },
                { "X", X + (180 - 180*(success ? 1.5f : 0.8f)) },
                { "Y", Y + (180 - 180*(success ? 1.5f : 0.8f)) }
            }, TweenQuad.EaseIn);
            GameThread.SetTimeout(() => Parent.Remove(this), 300);
        }
    }
}
