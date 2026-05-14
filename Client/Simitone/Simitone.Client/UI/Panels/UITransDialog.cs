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
using System.Transactions;
using static System.TimeZoneInfo;

namespace Simitone.Client.UI.Panels
{
    public class UITransDialog : UIContainer
    {
        private float _TransPct;
        public float TransPct {
            get
            {
                return _TransPct;
            }
            set
            {
                Diag.X = Math.Max(0, (value - 1) * UIScreen.Current.ScreenWidth);
                Diag.BodySize = new Vector2(Math.Min(1, value) * UIScreen.Current.ScreenWidth, UIScreen.Current.ScreenHeight).ToPoint();
                Diag.DiagSide = (value <= 1) ? UIDiagonalStripeSide.RIGHT : UIDiagonalStripeSide.LEFT;
                if (value == 1 && !FiredTransition)
                {
                    FiredTransition = true;
                    GameThread.NextUpdate((u) =>
                    {
                        UIScreen.RemoveDialog(this);
                        TransAction();
                        UIScreen.GlobalShowDialog(this, true);
                        GameThread.NextUpdate((u2) =>
                        {
                            if (!UIScreen.Current.GetChildren().Contains(this))
                            {
                                DrawsSince = 0;
                                Parent = null;
                                UIScreen.GlobalShowDialog(this, true);
                            }
                        });
                    });
                }
                if (value == 2 && FiredTransition)
                {
                    UIScreen.RemoveDialog(this);
                }
                _TransPct = value;
            }
        }
        public bool FiredTransition = false;

        private UIDiagonalStripe Diag;
        private Texture2D TransImage;
        private Action TransAction;

        /// <summary>
        /// Creates a new <see cref="UITransDialog"/> with the given "trans_{transType}.png" resource as an icon, and action to complete during the transition phase
        /// </summary>
        /// <param name="Icon"></param>
        /// <param name="Callback"></param>
        public UITransDialog(string transType, Action transAction, bool delayedStart = false)
        {
            var ui = Content.Get().CustomUI;
            Setup(ui.Get($"trans_{transType}.png").Get(GameFacade.GraphicsDevice), transAction, delayedStart);
        }

        /// <summary>
        /// Creates a new <see cref="UITransDialog"/> with the given icon and action to complete during the transition phase
        /// </summary>
        /// <param name="Icon"></param>
        /// <param name="Callback"></param>
        public UITransDialog(Texture2D Icon, Action Callback, bool delayedStart = false)
        {
            Setup(Icon, Callback, delayedStart);
        }

        private void Setup(Texture2D Icon, Action Callback, bool delayedStart)
        {
            TransImage = Icon;
            TransAction = Callback;

            Diag = new UIDiagonalStripe(new Point(0, 0), UIDiagonalStripeSide.RIGHT, UIStyle.Current.TransColor);

            if (!delayedStart)
                BeginTransition();
        }

        public static UITransDialog CreateWithCustomUIIcon(string CustomUIIcon, Action Callback)
        {
            var ui = Content.Get().CustomUI;
            return new(ui.Get(CustomUIIcon).Get(GameFacade.GraphicsDevice), Callback);
        }    

        public override void Removed()
        {
            base.Removed();
        }

        /// <summary>
        /// Starts the transition if it has not yet been started / completed.
        /// </summary>
        public void BeginTransition()
        {
            if (FiredTransition || TransPct != 0)            
                throw new InvalidOperationException("This transition object has already completed. Please make a new transition instance to play the transition again.");            
            
            Add(Diag);
            UIScreen.GlobalShowDialog(this, true);
            GameFacade.Screens.Tween.To(this, 0.2f, new Dictionary<string, float>() { { "TransPct", 1f } });

            TransPct = TransPct;
        }

        public int DrawsSince = 0;
        public override void Draw(UISpriteBatch batch)
        {
            base.Draw(batch);
            DrawLocalTexture(batch, TransImage, null, 
                new Vector2(UIScreen.Current.ScreenWidth - TransImage.Width, UIScreen.Current.ScreenHeight - TransImage.Height) / 2,
                Vector2.One, Color.White * (1-Math.Abs(1-TransPct)));

            if (FiredTransition && TransPct == 1)
            {
                if (DrawsSince++ == 2)
                {
                    GameFacade.Screens.Tween.To(this, 0.2f, new Dictionary<string, float>() { { "TransPct", 2f } });
                }
            }
            if (TransPct > 1) { }
        }
    }
}
