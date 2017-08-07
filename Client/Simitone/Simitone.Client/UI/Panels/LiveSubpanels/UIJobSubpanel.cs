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
using FSO.Client;

namespace Simitone.Client.UI.Panels.LiveSubpanels
{
    public class UIJobSubpanel : UISubpanel
    {
        public UILabel PerformanceTitle;
        public UIMotiveBar PerformanceBar;
        public UILabel JobTitle;
        public UILabel SalaryTitle;
        public UITwoStateButton CareerButton;

        private JobLevel LastJobLevel;

        private UISkillDisplay[] Skills;
        private string[] SkillNames = new string[]
        {
            "Cooking",
            "Mechanical",
            "Charisma",
            "Body",
            "Logic",
            "Creativity"
        };

        private VMPersonDataVariable[] SkillInd = new VMPersonDataVariable[]
        {
            VMPersonDataVariable.CookingSkill,
            VMPersonDataVariable.MechanicalSkill,
            VMPersonDataVariable.CharismaSkill,
            VMPersonDataVariable.BodySkill,
            VMPersonDataVariable.LogicSkill,
            VMPersonDataVariable.CreativitySkill
        };

        public UIJobSubpanel(TS1GameScreen game) : base(game)
        {
            PerformanceTitle = new UILabel();
            PerformanceTitle.Caption = "Performance";
            PerformanceTitle.Position = new Vector2(79, 16);
            InitLabel(PerformanceTitle);

            PerformanceBar = new UIMotiveBar();
            PerformanceBar.Position = new Vector2(79, 41);
            DynamicOverlay.Add(PerformanceBar);

            JobTitle = new UILabel();
            JobTitle.Caption = "Subway Musician";
            JobTitle.Position = new Vector2(18, 71);
            InitLabel(JobTitle);

            SalaryTitle = new UILabel();
            SalaryTitle.Caption = "Salary: §90";
            SalaryTitle.Position = new Vector2(18, 94);
            InitLabel(SalaryTitle);
            SalaryTitle.CaptionStyle.Color = UIStyle.Current.BtnActive;

            CareerButton = new UITwoStateButton(Content.Get().CustomUI.Get("blank_blue.png").Get(GameFacade.GraphicsDevice));
            CareerButton.Position = new Vector2(20, 15);
            Add(CareerButton);

            Skills = new UISkillDisplay[6];
            for (int i=0; i<6; i++)
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

        public int LastPerformance;

        public override void Update(UpdateState state)
        {
            base.Update(state);

            if (Opacity < 1)
            {
                if (DynamicOverlay.GetChildren().Count > 0)
                {
                    DynamicOverlay.Remove(PerformanceBar);
                    Add(PerformanceBar);
                }
            } else
            {
                if (DynamicOverlay.GetChildren().Count == 0)
                {
                    Remove(PerformanceBar);
                    DynamicOverlay.Add(PerformanceBar);
                }
            }
            var sel = Game.SelectedAvatar;
            if (sel == null) return;
            var type = sel.GetPersonData(FSO.SimAntics.Model.VMPersonDataVariable.JobType);
            var level = sel.GetPersonData(FSO.SimAntics.Model.VMPersonDataVariable.JobPromotionLevel);
            var performance = sel.GetPersonData(FSO.SimAntics.Model.VMPersonDataVariable.JobPerformance);

            var job = Content.Get().Jobs.GetJob((ushort)type);
            if (job == null)
            {
                if (LastPerformance != -200)
                {
                    JobTitle.Caption = "Unemployed";
                    SalaryTitle.Caption = "";
                    PerformanceBar.Visible = false;
                    PerformanceTitle.Visible = false;

                    LastJobLevel = null;
                    LastPerformance = -200;
                }
            }
            else
            {
                var myLevel = job.JobLevels[level];

                if (myLevel != LastJobLevel)
                {
                    JobTitle.Caption = myLevel.JobName;
                    SalaryTitle.Caption = "Salary: §" + myLevel.Salary + " (" + ToTime(myLevel.StartTime) + "-" + ToTime(myLevel.EndTime) + ")";

                    LastJobLevel = myLevel;
                }

                if (LastPerformance != performance)
                {
                    PerformanceBar.Visible = true;
                    PerformanceTitle.Visible = true;
                    PerformanceBar.MotiveValue = performance;
                    LastPerformance = performance;
                }
                for (int i = 0; i < 6; i++)
                    Skills[i].Needed = myLevel.MinRequired[i + 1] / 100;
            }

            for (int i = 0; i < 6; i++)
            {
                Skills[i].Value = sel.GetPersonData(SkillInd[i]) / 100;
            }
        }

        private string ToTime(int time)
        {
            return ((time > 12) ? (time - 12) : time) + ((time >= 12) ? "pm" : "am");
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
