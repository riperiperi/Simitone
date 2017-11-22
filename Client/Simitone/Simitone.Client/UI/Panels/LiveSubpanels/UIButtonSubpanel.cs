using FSO.Client;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Content;
using Microsoft.Xna.Framework;
using Simitone.Client.UI.Controls;
using Simitone.Client.UI.Model;
using Simitone.Client.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simitone.Client.UI.Panels.LiveSubpanels
{
    public class UIButtonSubpanel : UISubpanel
    {
        private float _InitShow;
        public float InitShow
        {
            get
            {
                return _InitShow;
            }
            set
            {
                Invalidate();
                _InitShow = value;
            }
        }
        public UIButtonSubpanel(TS1GameScreen game, UICatFunc[] funcs) : base(game)
        {
            for (int i = 0; i < funcs.Length; i++)
            {
                var func = funcs[i];

                var label = new UILabel();
                label.Caption = func.Name;
                label.Alignment = TextAlignment.Middle | TextAlignment.Center;
                label.Wrapped = true;
                label.Position = new Vector2(-77, 106);
                label.Size = new Vector2(120, 1);
                label.CaptionStyle = label.CaptionStyle.Clone();
                label.CaptionStyle.Size = 12;
                label.CaptionStyle.Color = UIStyle.Current.Text;
                Add(label);

                var subbutton = new UICatButton(Content.Get().CustomUI.Get(func.IconName).Get(GameFacade.GraphicsDevice));
                subbutton.OnButtonClick += (btn) => { func.Func(); };
                subbutton.Position = new Vector2(-50, 16);
                Add(subbutton);

                GameFacade.Screens.Tween.To(label, 0.5f, new Dictionary<string, float>() { { "X", 50 + i * 120f - 27 } }, TweenQuad.EaseOut);
                GameFacade.Screens.Tween.To(subbutton, 0.5f, new Dictionary<string, float>() { { "X", 50 + i * 120f } }, TweenQuad.EaseOut);
                GameFacade.Screens.Tween.To(this, 0.5f, new Dictionary<string, float>() { { "InitShow", 1f } }, TweenQuad.EaseOut);
            }

            InitShow = InitShow;
        }
    }

    public class UICatFunc
    {
        public string Name;
        public string IconName;
        public Action Func;

        public UICatFunc(string name, string iconName, Action func)
        {
            Name = name;
            IconName = iconName;
            Func = func;
        }
    }
}
