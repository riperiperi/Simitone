using FSO.Client;
using FSO.Client.UI.Framework;
using FSO.Common.Utils;
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
    public class UIDiagonalStripe : UIElement
    {
        public UIDiagonalStripeSide DiagSide;
        public Point BodySize { get; set; }
        public Color Color { get; set; }
        public int StartOff;
        private Texture2D Diag;

        public UIDiagonalStripe(Point size, UIDiagonalStripeSide diagSide, Color color)
        {
            BodySize = size;
            Color = color;
            DiagSide = diagSide;
            Diag = Content.Get().CustomUI.Get("diag.png").Get(GameFacade.GraphicsDevice);
        }

        public override void Draw(UISpriteBatch batch)
        {
            if (!Visible) return;
            var whitepx = TextureGenerator.GetPxWhite(batch.GraphicsDevice);

            DrawLocalTexture(batch, whitepx, null, new Vector2(0, StartOff), BodySize.ToVector2() - new Vector2(0, StartOff), Color);

            Rectangle lastRect;
            Point start;
            Point inc;
            int total;
            float rotate = 0;

            switch (DiagSide)
            {
                case UIDiagonalStripeSide.RIGHT:
                    start = new Point(BodySize.X, 0);
                    inc = new Point(0, 64);
                    total = (int)Math.Ceiling(BodySize.Y / 64f);
                    lastRect = new Rectangle(0, 0, 32, BodySize.Y % 64);
                    break;
                case UIDiagonalStripeSide.BOTTOM:
                    start = new Point(BodySize.X, BodySize.Y);
                    inc = new Point(-64, 0);
                    total = (int)Math.Ceiling(BodySize.X / 64f);
                    lastRect = new Rectangle(0, 0, 32, BodySize.X % 64);
                    rotate = (float)(Math.PI / 2);
                    break;
                case UIDiagonalStripeSide.LEFT:
                    start = new Point(0, BodySize.Y);
                    inc = new Point(0, -64);
                    total = (BodySize.Y + 63) / 64;
                    lastRect = new Rectangle(0, 0, 32, BodySize.Y % 64);
                    rotate = (float)(Math.PI);
                    break;
                default:
                    start = new Point(0, 0);
                    inc = new Point(64, 0);
                    total = (int)Math.Ceiling(BodySize.X / 64f);
                    lastRect = new Rectangle(0, 0, 32, BodySize.X % 64);
                    rotate = (float)(Math.PI*3 / 2);
                    break;
            }
            if (lastRect.Width == 0) lastRect.Width = 64;
            if (lastRect.Height == 0) lastRect.Height = 64;
            for (int i=0; i<total; i++)
            {
                if (i == 0 && StartOff != 0)
                    DrawLocalTexture(batch, Diag, new Rectangle(0, StartOff, 32, 64-StartOff), start.ToVector2() + new Vector2(0, StartOff), Vector2.One, Color, rotate);
                else
                    DrawLocalTexture(batch, Diag, (i == total - 1)?(Rectangle?)lastRect:null, start.ToVector2(), Vector2.One, Color, rotate);
                start += inc;
            }
        }

        public override Rectangle GetBounds()
        {
            return new Rectangle(0, 0, BodySize.X, BodySize.Y);
        }
    }

    public enum UIDiagonalStripeSide
    {
        RIGHT,
        BOTTOM,
        LEFT,
        UP
    }
}
