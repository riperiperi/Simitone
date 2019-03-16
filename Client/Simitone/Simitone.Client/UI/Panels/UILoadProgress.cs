using FSO.Client.UI.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.Common.Rendering.Framework.Model;
using FSO.Common.Utils;
using FSO.Client;
using FSO.Content;

namespace Simitone.Client.UI.Panels
{
    public class UILoadProgress : UIElement
    {
        public int[] Divisors = new int[]
        {
            237,
            328,
            398,
            456,
            507,
            556,
            609,
            680,
            731,
            781,
            832,
            920

            /*
            140,
            243,
            278,
            359,
            405,
            450,
            496,
            587,
            632,
            678,
            723,
            814,
            884
            */
        };

        public float OddTransition { get; set; }
        public float EvenTransition { get; set; }

        public float OverallPercent;

        private int ActiveElem;
        private bool CanFireNext = true;

        private Texture2D Back;
        private Texture2D Front;

        public UILoadProgress()
        {
            var ui = Content.Get().CustomUI;
            Back = ui.Get("load_bar_bg.png").Get(GameFacade.GraphicsDevice);
            Front = ui.Get("load_bar_content.png").Get(GameFacade.GraphicsDevice);
        }

        public override void Update(UpdateState state)
        {
            var targElem = (int)Math.Ceiling(OverallPercent * Divisors.Length);
            Console.WriteLine(targElem);
            if (targElem > ActiveElem && CanFireNext)
            {
                //fire the next
                if (ActiveElem % 2 == 0)
                {
                    //firing an odd
                    OddTransition = 1f;
                    GameFacade.Screens.Tween.To(this, 0.5f, new Dictionary<string, float>() { { "OddTransition", 0f } }, TweenElastic.EaseOut);
                } else
                {
                    EvenTransition = 1f;
                    GameFacade.Screens.Tween.To(this, 0.5f, new Dictionary<string, float>() { { "EvenTransition", 0f } }, TweenElastic.EaseOut);
                }
                ActiveElem++;
                CanFireNext = false;
                GameThread.SetTimeout(() => { CanFireNext = true; }, 260);
            }
            base.Update(state);
        }

        public override void Draw(UISpriteBatch batch)
        {
            DrawLocalTexture(batch, Back, Vector2.Zero);
            for (int i=0; i<Divisors.Length; i++)
            {
                if (i > ActiveElem) return;
                float offset = 0;
                if (i > ActiveElem-2)
                {
                    if (i % 2 == 0) offset = EvenTransition;
                    else offset = OddTransition;
                }

                var last = (i == 0) ? 0 : Divisors[i - 1];
                var t = Divisors[i];

                DrawLocalTexture(batch, Front, new Rectangle(last, 0, t-last, 112), new Vector2(last, offset*(-100)), Vector2.One, Color.White*(1-offset));
            }
        }
    }
}
