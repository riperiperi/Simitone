using FSO.Client.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.Common.Rendering.Framework.Model;
using FSO.Client;
using FSO.Client.UI.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Simitone.Client.UI.Controls
{
    public class UIElasticButton : UIButton
    {
        public UIElasticButton(Texture2D tex) : base (tex)
        {
            ImageStates = 1;

            if (ClickHandler != null)
            {
                ClickHandler.Region.Width = Texture.Width;
                ClickHandler.Region.Height = Texture.Height;
                ClickHandler.Region.X = Texture.Width / -2;
                ClickHandler.Region.Y = Texture.Height / -2;
            }
        }

        private int LastState;
        private int DownTime = 5;
        public override void Update(UpdateState state)
        {
            base.Update(state);
            if (CurrentFrame != LastState)
            {
                if (CurrentFrame == 2)
                {
                    if (LastState == 1)
                    {
                        //elsatically snap back to normal scale (pressed)
                        ScaleX = ScaleY = 0.75f;
                        GameFacade.Screens.Tween.To(this, 0.75f, new Dictionary<string, float>() { { "ScaleX", 1.0f }, { "ScaleY", 1.0f } }, TweenElastic.EaseOut);
                    }
                    else
                    {
                        //hover (get slightly bigger)
                        GameFacade.Screens.Tween.To(this, 0.3f, new Dictionary<string, float>() { { "ScaleX", 1.1f }, { "ScaleY", 1.1f } }, TweenQuad.EaseOut);
                    }
                }
                else if (CurrentFrame == 0)
                {
                        //return to normal size
                        GameFacade.Screens.Tween.To(this, 0.3f, new Dictionary<string, float>() { { "ScaleX", 1.0f }, { "ScaleY", 1.0f } }, TweenQuad.EaseOut);
                } else if (CurrentFrame == 1)
                {
                    //down (get smaller)
                    DownTime = 0;
                    GameFacade.Screens.Tween.To(this, 0.2f, new Dictionary<string, float>() { { "ScaleX", 0.8f }, { "ScaleY", 0.8f } }, TweenQuad.EaseOut);
                } else
                {
                    ScaleX = ScaleY = 1; //disabled
                }
                LastState = CurrentFrame;
            }
            if (CurrentFrame != 1 && DownTime < 5) DownTime++;
        }

        public override void Draw(UISpriteBatch SBatch)
        {
            DrawLocalTexture(SBatch, Texture, new Vector2(Texture.Width, Texture.Height) / -2);
        }
    }
}
