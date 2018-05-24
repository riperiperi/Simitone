using FSO.Client;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Common.Utils;
using FSO.Content;
using FSO.Files.Formats.IFF.Chunks;
using FSO.LotView.Model;
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
    public class UIFamilyCASItem : UITSContainer
    {
        public Texture2D PxWhite;
        public Texture2D Grad;
        public UIImage Background;
        public UIImage SelectedGrad;
        public UILabel FamilyTitle;
        public UIFamiliesCASPanel FParent;
        public List<UIAvatarSelectButton> Btns = new List<UIAvatarSelectButton>();

        public int MaxWidth = 810;
        public int Width;
        public int TitleWidth;
        public float SelectPct { get; set; }

        public UIFamilyCASItem(UIFamiliesCASPanel parent, FAMI family, VM vm)
        {
            FParent = parent;
            var ui = Content.Get().CustomUI;
            var gd = GameFacade.GraphicsDevice;

            PxWhite = TextureGenerator.GetPxWhite(gd);
            Grad = ui.Get("dialog_title_grad.png").Get(gd);
            Background = new UIImage(ui.Get("circle10px.png").Get(gd)).With9Slice(19, 19, 19, 19);
            //set the width based on number of family members
            //30 margin on family thumbs
            Width = 100 * family.FamilyGUIDs.Length + 10;
            Background.SetSize(Width, 120);
            Background.Position = new Vector2((MaxWidth - Width) / 2, 40);
            Add(Background);

            //max width is 100*8+10, 810

            var fams = family.ChunkParent.Get<FAMs>(family.ChunkID);

            FamilyTitle = new UILabel();
            FamilyTitle.CaptionStyle = FamilyTitle.CaptionStyle.Clone();
            FamilyTitle.CaptionStyle.Color = UIStyle.Current.Text;
            FamilyTitle.CaptionStyle.Size = 19;
            FamilyTitle.Size = new Vector2(MaxWidth, 40);
            FamilyTitle.Alignment = TextAlignment.Center | TextAlignment.Middle;
            FamilyTitle.Caption = fams?.GetString(0) ?? "";
            Add(FamilyTitle);

            TitleWidth = (int)FamilyTitle.CaptionStyle.MeasureString(FamilyTitle.Caption).X + 30;

            //display heads of all family members
            InitAvatarList(family.FamilyGUIDs, vm);
            SelectPct = SelectPct;
        }

        public override void Draw(UISpriteBatch batch)
        {
            DrawLocalTexture(batch, PxWhite, null, new Vector2((MaxWidth - TitleWidth) / 2, 0), new Vector2(TitleWidth, 40), UIStyle.Current.Bg);
            Background.Visible = true;
            Background.Draw(batch);
            DrawLocalTexture(batch, Grad, null, new Vector2((MaxWidth - Width) / 2, 54), new Vector2((Width / (float)Grad.Width)*SelectPct, 66), Color.White);
            Background.Visible = false;
            base.Draw(batch);
        }

        public void InitAvatarList(uint[] guids, VM vm)
        {
            int i = 0;
            foreach (var btn in Btns)
            {
                Remove(btn);
            }
            Btns.Clear();

            i = 0;
            var baseX = MaxWidth / 2 - (guids.Length-1) * 50;
            foreach (var sim in guids)
            {
                var fam = vm.Context.CreateObjectInstance(sim, LotTilePos.OUT_OF_WORLD, Direction.NORTH).BaseObject;
                fam.Tick();
                var btn = new UIAvatarSelectButton(UIIconCache.GetObject(fam));
                btn.Opacity = 1f;
                var id = i;
                btn.Name = fam.Name;
                btn.X = baseX + (i++) * 100;
                btn.Y = 88;
                btn.DeregisterHandler();
                Btns.Add(btn);
                Add(btn);
                fam.Delete(true, vm.Context);
            }
        }

        public override void Selected()
        {
            base.Selected();
            FamilyTitle.CaptionStyle.Color = UIStyle.Current.SecondaryText;
            GameFacade.Screens.Tween.To(this, 0.33f, new Dictionary<string, float>() { { "SelectPct", 1f } }, TweenQuad.EaseOut);
            FParent.SetSelection(ItemID);
        }

        public override void Deselected()
        {
            base.Deselected();
            FamilyTitle.CaptionStyle.Color = UIStyle.Current.Text;
            GameFacade.Screens.Tween.To(this, 0.33f, new Dictionary<string, float>() { { "SelectPct", 0f } }, TweenQuad.EaseOut);
        }
    }
}
