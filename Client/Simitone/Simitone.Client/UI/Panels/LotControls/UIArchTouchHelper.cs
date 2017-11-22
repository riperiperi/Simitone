using FSO.Client;
using FSO.Client.UI.Framework;
using FSO.Content;
using Simitone.Client.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.Common.Rendering.Framework.Model;
using Microsoft.Xna.Framework;
using FSO.Client.UI.Panels.LotControls;
using Microsoft.Xna.Framework.Graphics;
using FSO.Common;

namespace Simitone.Client.UI.Panels.LotControls
{
    public class UIArchTouchHelper : UIContainer
    {
        public UITwoStateButton ClickButton;
        public UITwoStateButton ShiftClickButton;
        public UITwoStateButton CtrlClickButton;

        public UITwoStateButton RotateCWButton;
        public UITwoStateButton RotateCCWButton;
        private Texture2D Cross;

        public UILotControl Owner;
        public UpdateState LastState;

        public UIArchTouchHelper(UILotControl parent)
        {
            Owner = parent;
            var ui = Content.Get().CustomUI;
            var gd = GameFacade.GraphicsDevice;

            ClickButton = new UITwoStateButton(ui.Get("touch_tool.png").Get(gd));
            Add(ClickButton);
            ClickButton.OnButtonDown += (b) => Owner.SimulateMD(LastState);
            ClickButton.OnButtonClick += (b) => Owner.SimulateMU(LastState);
            ShiftClickButton = new UITwoStateButton(ui.Get("touch_tools.png").Get(gd));
            Add(ShiftClickButton);
            ShiftClickButton.OnButtonDown += (b) => { Owner.AddModifier(UILotControlModifiers.SHIFT); Owner.SimulateMD(LastState); };
            ShiftClickButton.OnButtonClick += (b) => { Owner.SimulateMU(LastState); Owner.RemoveModifier(UILotControlModifiers.SHIFT); };
            CtrlClickButton = new UITwoStateButton(ui.Get("touch_toolc.png").Get(gd));
            Add(CtrlClickButton);
            CtrlClickButton.OnButtonDown += (b) => { Owner.AddModifier(UILotControlModifiers.CTRL); Owner.SimulateMD(LastState); };
            CtrlClickButton.OnButtonClick += (b) => { Owner.SimulateMU(LastState); Owner.RemoveModifier(UILotControlModifiers.CTRL); };

            RotateCWButton = new UITwoStateButton(ui.Get("touch_rotcw.png").Get(gd));
            Add(RotateCWButton);
            RotateCWButton.OnButtonClick += (b) => { parent.ObjectHolder.RotateObject(2); };
            RotateCCWButton = new UITwoStateButton(ui.Get("touch_rotccw.png").Get(gd));
            RotateCCWButton.OnButtonClick += (b) => { parent.ObjectHolder.RotateObject(-2); };
            Add(RotateCCWButton);

            Cross = ui.Get("touch_cross.png").Get(gd);

            GameResized();
        }

        public override void GameResized()
        {
            base.GameResized();
            ClickButton.Position = new Vector2(25, 25);
            ShiftClickButton.Position = new Vector2(25, UIScreen.Current.ScreenHeight - 255);
            CtrlClickButton.Position = new Vector2(UIScreen.Current.ScreenWidth - 100, UIScreen.Current.ScreenHeight - 255);

            RotateCWButton.Position = new Vector2(25, UIScreen.Current.ScreenHeight - 255);
            RotateCCWButton.Position = new Vector2(UIScreen.Current.ScreenWidth - 100, UIScreen.Current.ScreenHeight - 255);
        }

        public override void Update(UpdateState state)
        {
            LastState = state;
            if (Owner.LiveMode || !FSOEnvironment.SoftwareKeyboard) Visible = false;
            else
            {
                var custom = (Owner.CustomControl != null);
                ShiftClickButton.Visible = custom;
                CtrlClickButton.Visible = custom;

                RotateCCWButton.Visible = !custom;
                RotateCWButton.Visible = !custom;

                Visible = custom || Owner.ObjectHolder.Holding != null;
            }
            base.Update(state);
        }

        public override void Draw(UISpriteBatch batch)
        {
            if (!Visible) return;
            base.Draw(batch);
            //DrawLocalTexture(batch, Cross, new Vector2(UIScreen.Current.ScreenWidth - 98, UIScreen.Current.ScreenHeight - 98)/2);
        }


    }
}
