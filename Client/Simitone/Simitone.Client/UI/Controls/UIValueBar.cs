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
using FSO.Common.Rendering.Framework.Model;

namespace Simitone.Client.UI.Controls
{
    public class UIValueBar : UIElement
    {
        public Texture2D BarBase;
        public int Width;
        public float Value;

        public UIValueBar (Texture2D tex)
        {
            BarBase = tex;
        }

        public void DrawSlice(UISpriteBatch batch, int width, Color col, int drawFrom)
        {
            var w = BarBase.Width / 3;
            if (width < w * 2)
            {
                if (drawFrom <= 0) DrawLocalTexture(batch, BarBase, new Rectangle(0, 0, width / 2, BarBase.Height), Vector2.Zero, Vector2.One, col);
                DrawLocalTexture(batch, BarBase, new Rectangle(BarBase.Width-((width + 1) / 2), 0, (width+1) / 2, BarBase.Height), new Vector2(width/2, 0), Vector2.One, col);
            } else
            {
                if (drawFrom <= 0) DrawLocalTexture(batch, BarBase, new Rectangle(0, 0, w, BarBase.Height), Vector2.Zero, Vector2.One, col);
                if (drawFrom <= 1) DrawLocalTexture(batch, BarBase, new Rectangle(w, 0, w, BarBase.Height), new Vector2(w, 0), new Vector2((float)(width-2*w)/w, 1), col);
                DrawLocalTexture(batch, BarBase, new Rectangle(w*2, 0, w, BarBase.Height), new Vector2(width - w, 0), Vector2.One, col);
            }
        }

        public override void Draw(UISpriteBatch batch)
        {
            var p = Value;
            Color barcol = new Color((byte)(57 * (1 - p)), (byte)(213 * p + 97 * (1 - p)), (byte)(49 * p + 90 * (1 - p)));
            Color bgcol = new Color((byte)(57 * p + 214 * (1 - p)), (byte)(97 * p), (byte)(90 * p));

            DrawSlice(batch, Width, bgcol, 0);

            var activeWidth = (int)Math.Round(p * Width);
            DrawSlice(batch, activeWidth, Color.White, 2);
            DrawSlice(batch, activeWidth-2, barcol, 0);
        }
    }

    public class UIMotiveBar : UIValueBar
    {
        public int TargetArrow;
        public int Arrow;
        public int MotiveValue;
        public int OldMotiveValue = -200;
        private Queue<int> ChangeBuffer = new Queue<int>();

        public Texture2D ArrowGfx;

        public float ArrowCycle;

        public UIMotiveBar() : base(Content.Get().CustomUI.Get("motive_bg.png").Get(GameFacade.GraphicsDevice))
        {
            ArrowGfx = Content.Get().CustomUI.Get("motive_arrow.png").Get(GameFacade.GraphicsDevice);
            Width = 150;
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);

            if (OldMotiveValue != -200) ChangeBuffer.Enqueue(MotiveValue- OldMotiveValue);
            OldMotiveValue = MotiveValue;

            if (ChangeBuffer.Count > 240) ChangeBuffer.Dequeue();

            int sum = 0;
            foreach (var c in ChangeBuffer) sum += c;

            var diff = sum / 2.5;
            if (diff < 0) diff = Math.Floor(diff);
            else if (diff > 0) diff = Math.Ceiling(diff);
            TargetArrow = Math.Max(Math.Min((int)diff, 5), -5) * 60;

            if (Arrow > TargetArrow) Arrow--;
            if (Arrow < TargetArrow) Arrow++;

            Value = (MotiveValue+100) / 200f;

            ArrowCycle += Arrow / 240f;
            if (ArrowCycle < 0) ArrowCycle += 14f;
            ArrowCycle %= 14f;
        }

        public override void Draw(UISpriteBatch batch)
        {
            if (!Visible) return;
            base.Draw(batch);
            var w = BarBase.Width / 3;
            var spanw = (int)(Width * Value) - w * 2;
            var arrows = spanw / 14;
            var xStart = (Arrow > 0) ? 0 : (spanw);
            var dir = (Arrow > 0) ? 1 : -1;
            for (int i=0; i<arrows; i++)
            {
                float alpha = Math.Min(Math.Abs(Arrow) / 300f, 0.33f);
                if (i == 0) alpha *= (ArrowCycle/14f) * dir + (dir-1)/-2;
                else if (i == arrows - 1) alpha *= (1 - (ArrowCycle / 14f)) * dir + (dir - 1) / -2;

                DrawLocalTexture(batch, ArrowGfx, null, new Vector2(xStart + dir * (i * 14) + ArrowCycle, 0), new Vector2(dir, 1), Color.White * alpha);
            }
        }
    }
}
