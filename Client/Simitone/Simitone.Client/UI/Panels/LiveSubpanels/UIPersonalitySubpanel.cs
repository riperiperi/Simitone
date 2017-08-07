using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simitone.Client.UI.Screens;
using Simitone.Client.UI.Model;
using Simitone.Client.UI.Controls;
using FSO.Client.UI.Controls;
using Microsoft.Xna.Framework;
using FSO.Content;
using FSO.Files.Formats.IFF.Chunks;
using FSO.SimAntics.Model;
using FSO.Common.Rendering.Framework.Model;

namespace Simitone.Client.UI.Panels.LiveSubpanels
{
    public class UIPersonalitySubpanel : UISubpanel
    {
        private JobLevel LastJobLevel;

        private UISkillDisplay[] Skills;
        private string[] SkillNames = new string[]
        {
            "Neat",
            "Outgoing",
            "Active",
            "Playful",
            "Nice",
        };

        private VMPersonDataVariable[] SkillInd = new VMPersonDataVariable[]
        {
            VMPersonDataVariable.NeatPersonality,
            VMPersonDataVariable.OutgoingPersonality,
            VMPersonDataVariable.ActivePersonality,
            VMPersonDataVariable.PlayfulPersonality,
            VMPersonDataVariable.NicePersonality
        };

        public UIPersonalitySubpanel(TS1GameScreen game) : base(game)
        {
            Skills = new UISkillDisplay[5];
            for (int i=0; i<5; i++)
            {
                Skills[i] = new UISkillDisplay();
                Skills[i].Position = new Vector2(334 + (i%3)*140, 35 + 60*(i/3));
                Add(Skills[i]);

                var name = new UILabel();
                name.Caption = SkillNames[i];
                name.Position = new Vector2(332 + (i % 3) * 140, 11 + 60 * (i / 3));
                InitLabel(name);
            }
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);
            var sel = Game.SelectedAvatar;
            if (sel == null) return;
            for (int i = 0; i < 5; i++)
            {
                Skills[i].Value = sel.GetPersonData(SkillInd[i]) / 100;
            }
        }

        private void InitLabel(UILabel label)
        {
            label.CaptionStyle = label.CaptionStyle.Clone();
            label.CaptionStyle.Color = UIStyle.Current.Text;
            label.CaptionStyle.Size = 15;
            Add(label);
        }
    }
}
