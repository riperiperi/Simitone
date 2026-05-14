using FSO.Client;
using FSO.Client.UI.Controls;
using FSO.Client.Utils;
using FSO.Common.Rendering.Framework.IO;
using FSO.Common.Rendering.Framework.Model;
using FSO.Content;
using FSO.Files.Formats.IFF.Chunks;
using FSO.SimAntics;
using FSO.SimAntics.Model;
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
    public class UIPersonalitySubpanel : UISubpanel
    {
        private const string STRFile = "live", STRTable = "131";
        private record UIPersonalityDescriptor(string Title, string TitleLow, string TitleNormal, string TitleHigh, string CaptionLow, string CaptionNormal, string CaptionHigh);

        private VMAvatar? lastSelectedAvatar = default;

        private UISkillDisplay[] PersonalityTraits;

        private VMPersonDataVariable[] SkillInd = new VMPersonDataVariable[]
        {
            VMPersonDataVariable.NeatPersonality,
            VMPersonDataVariable.OutgoingPersonality,
            VMPersonDataVariable.ActivePersonality,
            VMPersonDataVariable.PlayfulPersonality,
            VMPersonDataVariable.NicePersonality
        };
        private enum UIPersonalityBarElement
        {
            Neat,
            Outgoing,
            Active,
            Playful,
            Nice,
        }
        private Dictionary<UIPersonalityBarElement, UIPersonalityDescriptor> _stringDescriptions = new();
        private UILabel titleLabel;
        private UILabel captionLabel;

        public UIPersonalitySubpanel(TS1GameScreen game) : base(game)
        {
            int leftMargin = 20;
            int topMargin = 11, topMarginBar = 35;

            //**initialize personality descriptions from STR
            InitDescriptions();

            PersonalityTraits = new UISkillDisplay[5];
            for (int i=0; i <= (int)UIPersonalityBarElement.Nice; i++)
            {
                PersonalityTraits[i] = new UISkillDisplay();
                PersonalityTraits[i].Position = new Vector2(leftMargin + 2 + (i%3)*140, topMarginBar + 60*(i/3));
                Add(PersonalityTraits[i]);

                var name = new UILabel();
                name.Caption = _stringDescriptions[(UIPersonalityBarElement)i].Title;
                name.Position = new Vector2(leftMargin + (i % 3) * 140, topMargin + 60 * (i / 3));
                InitLabel(name);
            }
            titleLabel = new UILabel()
            {
                Position = new(450, topMargin + 0),                
            };
            InitLabel(titleLabel);
            Add(titleLabel);
            captionLabel = new UILabel()
            {
                Position = new(450, topMargin + 20),
                Size = new Vector2(700, 200),
                Wrapped = true,
                Alignment = FSO.Client.UI.Framework.TextAlignment.Left
            };
            InitLabel(captionLabel);
            Add(captionLabel);
        }

        private void InitDescriptions()
        {
            int personalities = (int)UIPersonalityBarElement.Nice + 1;
            int numOfFields = 7; // 7 strings per personality trait

            _stringDescriptions.Clear();

            for (int i = 0; i < personalities; i++)
            {
                int field = 0;
                string GET() => GameFacade.Strings.GetString(STRFile, STRTable, (1 + field++ + (i * numOfFields)).ToString());
                UIPersonalityDescriptor desc = new(GET(),GET(),GET(),GET(),GET(),GET(),GET());
                _stringDescriptions.Add((UIPersonalityBarElement)i, desc);
            }
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);
            if (lastSelectedAvatar != null && lastSelectedAvatar == Game.SelectedAvatar) return;
            lastSelectedAvatar = Game.SelectedAvatar;            
            for (int i = 0; i < 5; i++)
            {
                int value = lastSelectedAvatar.GetPersonData(SkillInd[i]) / 100;
                PersonalityTraits[i].Value = value;                
                SetPersonalityDescription((UIPersonalityBarElement)0,value);
            }
        }

        void SetPersonalityDescription(UIPersonalityBarElement Personality, int Value)
        {
            UIPersonalityDescriptor desc = _stringDescriptions[Personality];
            int traitStrength = Math.Max(Math.Min(Value,10),0) / 3; // 0,1,2
            titleLabel.Caption = traitStrength switch
            {
                0 => $"{desc.TitleLow}",
                1 => $"{desc.TitleNormal}",
                2 => $"{desc.TitleHigh}",
            };
            captionLabel.Caption = traitStrength switch
            {
                0 => $"{desc.CaptionLow}",
                1 => $"{desc.CaptionNormal}",
                2 => $"{desc.CaptionHigh}",
            };
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
