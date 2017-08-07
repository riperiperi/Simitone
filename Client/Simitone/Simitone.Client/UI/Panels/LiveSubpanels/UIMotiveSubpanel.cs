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
        public UIMotiveBar[] MotiveDisplays;

        public UIMotiveSubpanel(TS1GameScreen game) : base (game)
        {
            MotiveDisplays = new UIMotiveBar[8];
            for (int i=0; i<8; i++)
            {
                var d = new UIMotiveBar();
                d.Position = new Vector2(17 + (i%4)*180, 36+(i/4) * 60);
                Add(d);
                MotiveDisplays[i] = d;

                var l = new UILabel();
                l.CaptionStyle = l.CaptionStyle.Clone();
                l.CaptionStyle.Size = 15;
                l.CaptionStyle.Color = UIStyle.Current.Text;
                l.Alignment = FSO.Client.UI.Framework.TextAlignment.Bottom;
                l.Size = new Vector2(1);
                l.Position = new Vector2(17 + (i % 4) * 180, 30 + (i / 4) * 60);
                l.Caption = GameFacade.Strings.GetString("f102", (i+1).ToString());
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
            } else
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
            MotiveDisplays[0].MotiveValue = Game.SelectedAvatar.GetMotiveData(VMMotive.Hunger);
            MotiveDisplays[1].MotiveValue = Game.SelectedAvatar.GetMotiveData(VMMotive.Comfort);
            MotiveDisplays[2].MotiveValue = Game.SelectedAvatar.GetMotiveData(VMMotive.Hygiene);
            MotiveDisplays[3].MotiveValue = Game.SelectedAvatar.GetMotiveData(VMMotive.Bladder);
            MotiveDisplays[4].MotiveValue = Game.SelectedAvatar.GetMotiveData(VMMotive.Energy);
            MotiveDisplays[5].MotiveValue = Game.SelectedAvatar.GetMotiveData(VMMotive.Fun);
            MotiveDisplays[6].MotiveValue = Game.SelectedAvatar.GetMotiveData(VMMotive.Social);
            MotiveDisplays[7].MotiveValue = Game.SelectedAvatar.GetMotiveData(VMMotive.Room);
        }
    }
}
