using FSO.Client;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Common.Utils;
using FSO.Content;
using FSO.Files.Formats.IFF.Chunks;
using FSO.SimAntics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simitone.Client.UI.Controls;
using Simitone.Client.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simitone.Client.UI.Panels.CAS
{
    /// <summary>
    /// Lists all of the families that can be moved in or deleted.
    /// </summary>
    public class UIFamiliesCASPanel : UIContainer
    {
        public UITouchScroll FamilyList;
        public UILabel Title;
        public UITwoStateButton DeleteButton;
        public UITwoStateButton NewButton;

        public event Action OnNewFamily;

        public List<FAMI> Families = new List<FAMI>();
        private VM vm;
        private Texture2D WhitePx;

        public int Selection;
        public float TitleI { get; set; }
        
        public UIFamiliesCASPanel()
        {
            var gd = GameFacade.GraphicsDevice;
            var ui = Content.Get().CustomUI;
            var sh = UIScreen.Current.ScreenHeight;
            var sw = UIScreen.Current.ScreenWidth;
            FamilyList = new UITouchScroll(FamilyLength, FamilyProvider);
            FamilyList.VerticalMode = true;
            FamilyList.Size = new Vector2(810, sh);
            FamilyList.X = (sw - 810) / 2;
            FamilyList.ItemWidth = 180;
            FamilyList.Margin = 90;
            FamilyList.DrawBounds = false;
            Add(FamilyList);
            WhitePx = TextureGenerator.GetPxWhite(GameFacade.GraphicsDevice);

            Title = new UILabel();
            Title.NewStyle(UIStyle.Current.Text, 37);
            Title.Caption = "Select a Family";
            Title.Size = new Vector2(sw, 60);
            Title.Alignment = TextAlignment.Middle | TextAlignment.Center;
            Title.Y = -85;
            Add(Title);

            DeleteButton = new UITwoStateButton(ui.Get("btn_deletefam.png").Get(gd));
            DeleteButton.Position = new Vector2(sw - 140, sh - 260);
            Add(DeleteButton);

            NewButton = new UITwoStateButton(ui.Get("btn_createfam.png").Get(gd));
            NewButton.Position = new Vector2(sw - 140, sh - 380);
            Add(NewButton);
            NewButton.OnButtonClick += (btn) => OnNewFamily?.Invoke();

            TitleI = TitleI;
            SetSelection(-1);
        }

        public void SetSelection(int i)
        {
            Selection = i;
            DeleteButton.Disabled = Selection == -1;
            NewButton.Disabled = false;
        }

        public override void GameResized()
        {
            var sh = UIScreen.Current.ScreenHeight;
            var sw = UIScreen.Current.ScreenWidth;
            FamilyList.Size = new Vector2(810, sh);
            FamilyList.X = (sw - 810) / 2;
            FamilyList.Reset();
            Title.Size = new Vector2(sw, 60);
            DeleteButton.Position = new Vector2(sw - 140, sh - 260);
            NewButton.Position = new Vector2(sw - 140, sh - 380);
            base.GameResized();
        }

        public int FamilyLength()
        {
            return Families.Count;
        }

        public UITSContainer FamilyProvider(int id)
        {
            var item = new UIFamilyCASItem(this, Families[id], vm);
            return item;
        }

        public void UpdateFamilies(List<FAMI> families, VM vm)
        {
            this.vm = vm;
            Families = families;
            FamilyList.Reset();
        }

        public override void Draw(UISpriteBatch batch)
        {
            var y = TitleI * 100 - 85;
            Title.Y = y;
            NewButton.X = UIScreen.Current.ScreenWidth - 140 * TitleI;
            DeleteButton.X = NewButton.X;

            FamilyList.Opacity = TitleI;
            FamilyList.Visible = FamilyList.Opacity > 0;
            Title.Visible = false;
            base.Draw(batch);
            DrawLocalTexture(batch, WhitePx, null, new Vector2(0, y), new Vector2(UIScreen.Current.ScreenWidth, 60), UIStyle.Current.Bg);
            Title.Visible = true;
            Title.Draw(batch);
        }
    }
}
