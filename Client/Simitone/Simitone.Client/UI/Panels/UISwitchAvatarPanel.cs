using FSO.Client.UI.Framework;
using FSO.SimAntics;
using Simitone.Client.UI.Controls;
using Simitone.Client.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using FSO.Content;
using Simitone.Client.UI.Model;
using FSO.Client;
using Microsoft.Xna.Framework;
using FSO.SimAntics.NetPlay.Model.Commands;
using FSO.Common.Utils;

namespace Simitone.Client.UI.Panels
{
    public class UISwitchAvatarPanel : UIContainer
    {
        private TS1GameScreen Game;
        private Texture2D Bg;
        public event Action OnEnd;
        public UISwitchAvatarPanel(TS1GameScreen screen)
        {
            Game = screen;
            Bg = Content.Get().CustomUI.Get("pswitch_bg.png").Get(GameFacade.GraphicsDevice);

            var familyMembers = Game.vm.Context.ObjectQueries.Avatars.Where(x => ((VMAvatar)x).GetPersonData(FSO.SimAntics.Model.VMPersonDataVariable.TS1FamilyNumber) == (Game.vm.CurrentFamily.ChunkID));
            int i = 0;
            foreach (var fam in familyMembers)
            {
                var btn = new UIAvatarSelectButton(UIIconCache.GetObject(fam));
                if (fam.PersistID > 0) btn.Outlined = true;
                btn.Opacity = 0f;
                var id = fam.ObjectID;
                btn.OnButtonClick += (b) => { Select(id); };
                btn.Y = 64;
                GameFacade.Screens.Tween.To(btn, 0.3f, new Dictionary<string, float>() { { "X", 185 + (i++) * 100 }, { "Opacity", 1f } }, TweenQuad.EaseOut);
                Add(btn);
            }
        }

        private void Select(short selected)
        {
            Game.vm.SendCommand(new VMNetChangeControlCmd() { TargetID = selected });
            Kill();
        }

        public void Kill()
        {
            foreach (var child in Children)
            {
                GameFacade.Screens.Tween.To(child, 0.3f, new Dictionary<string, float>() { { "Opacity", 0f } }, TweenQuad.EaseOut);
            }
            GameFacade.Screens.Tween.To(this, 0.3f, new Dictionary<string, float>() { { "Opacity", 0f } }, TweenQuad.EaseOut);
            OnEnd?.Invoke();
            GameThread.SetTimeout(() => Parent.Remove(this), 300);
        }

        public override void Draw(UISpriteBatch batch)
        {
            DrawLocalTexture(batch, Bg, new Rectangle(0, 0, Bg.Width / 2, Bg.Height), new Vector2(60, 0));
            DrawLocalTexture(batch, Bg, new Rectangle(Bg.Width / 2, 0, Bg.Width / 2, Bg.Height), new Vector2(60 + Bg.Width / 2, 0), new Vector2(12, 1));
            base.Draw(batch);
        }
    }

    public class UIAvatarSelectButton : UIElasticButton
    {
        public Texture2D Icon;
        public Texture2D Outline;
        public bool Outlined;

        public UIAvatarSelectButton(Texture2D icon) : base(Content.Get().CustomUI.Get("pswitch_icon_bg.png").Get(GameFacade.GraphicsDevice))
        {
            Icon = icon;
            Outline = Content.Get().CustomUI.Get("pswitch_icon_sel.png").Get(GameFacade.GraphicsDevice);
        }

        public override void Draw(UISpriteBatch SBatch)
        {
            DrawLocalTexture(SBatch, Texture, null, new Vector2(Texture.Width, Texture.Height) / -2, Vector2.One, new Color(104, 164, 184, 255));
            if (Icon != null) DrawLocalTexture(SBatch, Icon, new Vector2(Icon.Width, Icon.Height) / -2);
            if (Outlined) DrawLocalTexture(SBatch, Outline, null, new Vector2(Outline.Width, Outline.Height) / -2, Vector2.One, UIStyle.Current.ActiveSelection);
        }
    }
}
