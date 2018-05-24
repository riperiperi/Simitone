using FSO.SimAntics.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.LotView;
using Microsoft.Xna.Framework.Graphics;
using FSO.Client.UI.Framework;
using Microsoft.Xna.Framework;
using FSO.Client;

namespace Simitone.Client.UI.Panels.WorldUI
{
    public class UIMoneyHeadline : VMHeadlineRenderer
    {
        private RenderTarget2D MoneyTarget;
        private TextStyle Style;
        private Texture2D MoneyBG;
        private string Text;

        public UIMoneyHeadline(VMRuntimeHeadline headline) : base(headline)
        {
            Style = TextStyle.DefaultLabel.Clone();
            Style.Size = 12;
            var value = (int)(headline.Operand.Flags2 | (headline.Operand.Duration << 16));
            if (value < -10000)
            {
                Text = (-10000-value).ToString();
                Style.Color = Model.UIStyle.Current.SecondaryText;
            }
            else
            {
                Text = (value > 0) ? ("§" + value) : ("-§" + value);
                Style.Color = Model.UIStyle.Current.Text;
            }
            var measure = Style.MeasureString(Text);


            var GD = GameFacade.GraphicsDevice;
            MoneyTarget = new RenderTarget2D(GD, (int)measure.X+10, (int)measure.Y+30);
            MoneyBG = FSO.Content.Content.Get().CustomUI.Get("money_bg.png").Get(GD);

            DrawNewFrame();
        }

        public void DrawNewFrame()
        {
            var GD = GameFacade.GraphicsDevice;
            GD.SetRenderTarget(MoneyTarget);
            GD.Clear(Color.TransparentBlack);
            var batch = GameFacade.Screens.SpriteBatch;
            var opacity = (Headline.Duration / 60f);
            batch.Begin();
            batch.Draw(MoneyBG, new Vector2(0, Headline.Duration / 2), new Rectangle(0, 0, 12, 24), Model.UIStyle.Current.Bg * opacity,
                0, Vector2.Zero, new Vector2(0.8f, 0.8f), SpriteEffects.None, 0);
            batch.Draw(MoneyBG, new Vector2(9.6f, Headline.Duration / 2), new Rectangle(12, 0, 12, 24), Model.UIStyle.Current.Bg * opacity,
                0, Vector2.Zero, new Vector2(((MoneyTarget.Width-19.2f)/12f), 0.8f), SpriteEffects.None, 0);
            batch.Draw(MoneyBG, new Vector2(MoneyTarget.Width-9.6f, Headline.Duration / 2), new Rectangle(24, 0, 12, 24), Model.UIStyle.Current.Bg * opacity,
                0, Vector2.Zero, new Vector2(0.8f, 0.8f), SpriteEffects.None, 0);
            Style.Color.A = (byte)(opacity*255);
            batch.DrawString(Style.SpriteFont, Text, new Vector2(5, Headline.Duration/2), Style.Color);
            batch.End();
            GD.SetRenderTarget(null);
        }

        public override Texture2D DrawFrame(World world)
        {
            DrawNewFrame();
            return MoneyTarget;
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
