using FSO.Client;
using FSO.Client.UI.Controls;
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
using FSO.Common.Rendering.Framework.Model;

namespace Simitone.Client.UI.Panels.LiveSubpanels
{
    public class UIMotiveSubpanel : UISubpanel
    {
        private string FSOStrTable = "f102", TS1StrTable = "130";

        private UIMotiveBar[] MotiveDisplays;
        public UIMotiveBar GetMotiveDisplay(UIMotiveElement Motive) => MotiveDisplays[Math.Max(Math.Min((int)Motive, MotiveDisplays.Length),0)];
        public enum UIMotiveElement
        {
            Hunger,
            Comfort,
            Hygiene,
            Bladder,
            Energy,
            Fun,
            Social,
            Room
        }
        Dictionary<UIMotiveElement, string> _motiveStrings = new()
        {
            { UIMotiveElement.Hunger, "1" },
            { UIMotiveElement.Comfort, "5" },
            { UIMotiveElement.Hygiene, "9" },
            { UIMotiveElement.Bladder, "13" },
            { UIMotiveElement.Energy, "3" },
            { UIMotiveElement.Fun, "7" },
            { UIMotiveElement.Social, "11" },
            { UIMotiveElement.Room, "15" },
        };

        public UIMotiveSubpanel(TS1GameScreen game) : base (game)
        {
            bool useTS1StrTable = true;
            string file = useTS1StrTable ? "live" : "UIText";
            string table = useTS1StrTable ? TS1StrTable : FSOStrTable;

            MotiveDisplays = new UIMotiveBar[8];
            for (int i=0; i <= ((int)UIMotiveElement.Room); i++)
            {
                var d = new UIMotiveBar
                {
                    Position = new Vector2(17 + (i % 4) * 180, 36 + (i / 4) * 60)
                };
                Add(d);

                MotiveDisplays[i] = d;
                string strIndex = useTS1StrTable ? _motiveStrings[(UIMotiveElement)i] : i.ToString();

                UILabel l = new UILabel() {
                    Alignment = FSO.Client.UI.Framework.TextAlignment.Bottom,
                    Size = new Vector2(1),
                    Position = new Vector2(17 + (i % 4) * 180, 30 + (i / 4) * 60),
                    Caption = GameFacade.Strings.GetString(file, table, strIndex),
                };
                l.CaptionStyle = l.CaptionStyle.Clone();
                l.CaptionStyle.Size = 15;
                l.CaptionStyle.Color = UIStyle.Current.Text;
                Add(l);
            }
            
        }

        public override void Update(UpdateState state)
        {
            UpdateMotives();
            base.Update(state);
            if (Opacity < 1)
            {
                if (DynamicOverlay.GetChildren().Count > 0)
                {
                    foreach (var m in MotiveDisplays)
                    {
                        DynamicOverlay.Remove(m);
                        Add(m);
                    }
                }
                Invalidate();
            } 
            else
            {
                if (DynamicOverlay.GetChildren().Count == 0)
                {
                    foreach (var m in MotiveDisplays)
                    {
                        Remove(m);
                        DynamicOverlay.Add(m);
                        Invalidate();
                    }
                }
            }
        }        

        private void UpdateMotives()
        {
            if (Game.SelectedAvatar == null) return;
            MotiveDisplays[(int)UIMotiveElement.Hunger].MotiveValue = Game.SelectedAvatar.GetMotiveData(VMMotive.Hunger);
            MotiveDisplays[(int)UIMotiveElement.Comfort].MotiveValue = Game.SelectedAvatar.GetMotiveData(VMMotive.Comfort);
            MotiveDisplays[(int)UIMotiveElement.Hygiene].MotiveValue = Game.SelectedAvatar.GetMotiveData(VMMotive.Hygiene);
            MotiveDisplays[(int)UIMotiveElement.Bladder].MotiveValue = Game.SelectedAvatar.GetMotiveData(VMMotive.Bladder);
            MotiveDisplays[(int)UIMotiveElement.Energy].MotiveValue = Game.SelectedAvatar.GetMotiveData(VMMotive.Energy);
            MotiveDisplays[(int)UIMotiveElement.Fun].MotiveValue = Game.SelectedAvatar.GetMotiveData(VMMotive.Fun);
            MotiveDisplays[(int)UIMotiveElement.Social].MotiveValue = Game.SelectedAvatar.GetMotiveData(VMMotive.Social);
            MotiveDisplays[(int)UIMotiveElement.Room].MotiveValue = Game.SelectedAvatar.GetMotiveData(VMMotive.Room);
        }
    }
}
