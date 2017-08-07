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

namespace Simitone.Client.UI.Panels
{
    public class UICutawayPanel : UIContainer
    {
        public float BgAnim { get; set; }
        public Texture2D Background;

        public UIStencilButton DownButton;
        public UIStencilButton CutButton;
        public UIStencilButton UpButton;
        public UIStencilButton RoofButton;

        public event Action<int> OnSelection;

        public UICutawayPanel(int cut)
        {
            BgAnim = 0;
            var ui = Content.Get().CustomUI;
            Background = ui.Get("cut_bg.png").Get(GameFacade.GraphicsDevice);

            DownButton = new UIStencilButton(ui.Get("cut_stencil_down.png").Get(GameFacade.GraphicsDevice));
            DownButton.Position = new Vector2(12, 64);
            DownButton.Selected = (cut == 0);
            DownButton.OnButtonClick += (b) => { OnSelection?.Invoke(0); };
            Add(DownButton);
            CutButton = new UIStencilButton(ui.Get("cut_stencil_away.png").Get(GameFacade.GraphicsDevice));
            CutButton.Position = new Vector2(8, 128);
            CutButton.Selected = (cut == 1);
            CutButton.OnButtonClick += (b) => { OnSelection?.Invoke(1); };
            Add(CutButton);
            UpButton = new UIStencilButton(ui.Get("cut_stencil_up.png").Get(GameFacade.GraphicsDevice));
            UpButton.OnButtonClick += (b) => { OnSelection?.Invoke(2); };
            UpButton.Selected = (cut == 2);
            UpButton.Position = new Vector2(24, 196);
            Add(UpButton);
            RoofButton = new UIStencilButton(ui.Get("cut_stencil_roof.png").Get(GameFacade.GraphicsDevice));
            RoofButton.OnButtonClick += (b) => { OnSelection?.Invoke(3); };
            RoofButton.Selected = (cut == 3);
            RoofButton.Position = new Vector2(54, 254);
            Add(RoofButton);

            Opacity = 0f;
            GameFacade.Screens.Tween.To(this, 0.3f, new Dictionary<string, float>() { { "Opacity", 1f }, { "BgAnim", 1f } }, TweenQuad.EaseOut);
            foreach (var child in Children)
            {
                ((UIStencilButton)child).Alpha = 0f;
                ((UIStencilButton)child).InflateHitbox(25, 5);
                GameFacade.Screens.Tween.To(child, 0.3f, new Dictionary<string, float>() { { "Alpha", 1f } }, TweenQuad.EaseOut);
            }
        }

        public override void Draw(UISpriteBatch batch)
        {
            DrawLocalTexture(batch, Background, null, new Vector2(264, 138), Vector2.One, UIStyle.Current.Bg * BgAnim, ((float)Math.PI / 3) * (1-BgAnim), new Vector2(263, 119));
            base.Draw(batch);
        }

        public void Kill()
        {
            GameFacade.Screens.Tween.To(this, 0.3f, new Dictionary<string, float>() { { "Opacity", 0f }, { "BgAnim", 0f } }, TweenQuad.EaseOut);
            foreach (var child in Children)
            {
                GameFacade.Screens.Tween.To(child, 0.3f, new Dictionary<string, float>() { { "Alpha", 0f } }, TweenQuad.EaseOut);
            }

            GameThread.SetTimeout(() => Parent.Remove(this), 300);
        }
    }
}
