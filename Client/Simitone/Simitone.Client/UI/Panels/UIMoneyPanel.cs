using FSO.Client;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Common.Utils;
using Simitone.Client.UI.Model;
using Simitone.Client.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.Common.Rendering.Framework.Model;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using FSO.Content;

namespace Simitone.Client.UI.Panels
{
    public class UIMoneyPanel : UIContainer
    {
        public int LastMoney = 0;
        private TS1GameScreen Game;
        private UILabel MoneyLabel;
        private Texture2D Bg;

        public UIMoneyPanel(TS1GameScreen game) : base()
        {
            Game = game;
            LastMoney = GetMoney();

            MoneyLabel = new UILabel();
            MoneyLabel.CaptionStyle = MoneyLabel.CaptionStyle.Clone();
            MoneyLabel.CaptionStyle.Size = 15;
            MoneyLabel.CaptionStyle.Color = UIStyle.Current.Text;
            MoneyLabel.Alignment = FSO.Client.UI.Framework.TextAlignment.Center | FSO.Client.UI.Framework.TextAlignment.Middle;
            MoneyLabel.Size = new Microsoft.Xna.Framework.Vector2(128, 24);
            Add(MoneyLabel);

            Bg = Content.Get().CustomUI.Get("money_bg.png").Get(GameFacade.GraphicsDevice);

            UpdateMoneyDisplay();
        }

        public void DisplayChange(int change)
        {
            var newLabel = new UILabel();
            newLabel.Y = -20f;
            newLabel.CaptionStyle = MoneyLabel.CaptionStyle.Clone();
            newLabel.CaptionStyle.Size = 15;
            newLabel.CaptionStyle.Color = (change > 0) ? UIStyle.Current.PosMoney : UIStyle.Current.NegMoney;
            newLabel.Alignment = FSO.Client.UI.Framework.TextAlignment.Right | FSO.Client.UI.Framework.TextAlignment.Middle;
            newLabel.Size = new Microsoft.Xna.Framework.Vector2(128, 24);

            newLabel.Caption = ((change > 0) ? "+" : "-") + "§" + Math.Abs(change);
            Add(newLabel);

            GameFacade.Screens.Tween.To(newLabel, 1.5f, new Dictionary<string, float>() { { "Y", -50 }, { "Opacity", 0 } });
            GameThread.SetTimeout(() => { Remove(newLabel); }, 1500);
        }

        private void UpdateMoneyDisplay()
        {
            MoneyLabel.Caption = "§" + LastMoney.ToString("##,#0");
        }

        private int GetMoney()
        {
            return Game.ActiveFamily?.Budget ?? 0;
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);
            Visible = Game.LotControl.ActiveEntity != null;
            var money = GetMoney();
            if (LastMoney != money)
            {
                DisplayChange(money - LastMoney);
                LastMoney = money;
                UpdateMoneyDisplay();
            }
        }

        public override void Draw(UISpriteBatch batch)
        {
            if (!Visible) return;
            DrawLocalTexture(batch, Bg, new Rectangle(0, 0, 12, 24), Vector2.Zero, Vector2.One, UIStyle.Current.Bg);
            DrawLocalTexture(batch, Bg, new Rectangle(12, 0, 12, 24), new Vector2(12, 0), new Vector2(8.666667f, 1), UIStyle.Current.Bg);
            DrawLocalTexture(batch, Bg, new Rectangle(24, 0, 12, 24), new Vector2(116, 0), Vector2.One, UIStyle.Current.Bg);
            base.Draw(batch);
        }
    }
}
