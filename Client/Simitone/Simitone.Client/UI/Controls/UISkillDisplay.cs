using FSO.Client;
using FSO.Client.UI.Framework;
using FSO.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simitone.Client.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.Common.Rendering.Framework.Model;

namespace Simitone.Client.UI.Controls
{
    public class UISkillDisplay : UIElement
    {
        public Texture2D Skill;

        public UISkillDisplay() : base()
        {
            Skill = Content.Get().CustomUI.Get("skill.png").Get(GameFacade.GraphicsDevice);
        }

        private int _Value;
        public int Value {
            get
            {
                return _Value;
            }
            set
            {
                if (value != _Value) Invalidate();
                _Value = value;
            }
        }
        private int _Needed;
        public int Needed
        {
            get
            {
                return _Needed;
            }
            set
            {
                if (value != _Needed) Invalidate();
                _Needed = value;
            }
        }

        public override void Draw(UISpriteBatch batch)
        {
            for (int i = 0; i < 10; i++) {
                Color color;
                if (i < Value) color = UIStyle.Current.SkillActive;
                else if (i < Needed) color = UIStyle.Current.SkillNeeded;
                else color = UIStyle.Current.SkillInactive; 
                DrawLocalTexture(batch, Skill, null, new Microsoft.Xna.Framework.Vector2(i * 8, 0), Vector2.One, color);
            }
        }
    }
}
