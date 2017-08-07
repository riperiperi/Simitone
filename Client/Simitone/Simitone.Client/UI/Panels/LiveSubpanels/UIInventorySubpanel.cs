using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simitone.Client.UI.Screens;
using FSO.Content;
using FSO.SimAntics.Model;
using FSO.Files.Formats.IFF.Chunks;
using FSO.Common.Rendering.Framework.Model;
using Simitone.Client.UI.Controls;
using Microsoft.Xna.Framework;
using FSO.SimAntics;
using Microsoft.Xna.Framework.Graphics;
using FSO.LotView.Model;
using FSO.Client;
using FSO.Client.UI.Framework;
using Simitone.Client.UI.Model;
using FSO.Client.UI.Controls;

namespace Simitone.Client.UI.Panels.LiveSubpanels
{
    public class UIInventorySubpanel : UISubpanel
    {
        public List<InventoryItem> Items = new List<InventoryItem>();
        public UITouchScroll ScrollView;
        public int CatSort = -1;

        public UITwoStateButton MagicButton;
        public UITwoStateButton IngButton;
        public UITwoStateButton OtherButton;

        public HashSet<int> HiddenCats = new HashSet<int>()
        {
            //note: section 2 is hidden. guids 7 and 8 seem to contain downtown time in count.
            2, 6, 7, 8
        };

        public UIInventorySubpanel(TS1GameScreen game) : base(game)
        {
            var ui = Content.Get().CustomUI;
            ScrollView = new UITouchScroll(() => { return Items?.Count ?? 0; }, DisplayProvider);
            ScrollView.X = 148;
            ScrollView.ItemWidth = 90;
            ScrollView.Size = new Vector2(582, 128);
            Add(ScrollView);

            MagicButton = new UITwoStateButton(ui.Get("inv_mag_btn.png").Get(GameFacade.GraphicsDevice));
            MagicButton.Position = new Vector2(21, 9);
            MagicButton.OnButtonClick += (b) => { ChangeCat(7); };
            Add(MagicButton);
            IngButton = new UITwoStateButton(ui.Get("inv_ing_btn.png").Get(GameFacade.GraphicsDevice));
            IngButton.Position = new Vector2(81, 9);
            IngButton.OnButtonClick += (b) => { ChangeCat(8); };
            Add(IngButton);
            OtherButton = new UITwoStateButton(ui.Get("inv_other_btn.png").Get(GameFacade.GraphicsDevice));
            OtherButton.Position = new Vector2(21, 69);
            OtherButton.OnButtonClick += (b) => { ChangeCat(-1); };
            Add(OtherButton);

            ChangeCat(-1);
        }

        public void ChangeCat(int cat)
        {
            MagicButton.Selected = cat == 7;
            IngButton.Selected = cat == 8;
            OtherButton.Selected = cat == -1;
            CatSort = cat;
        }

        public UITSContainer DisplayProvider(int index)
        {
            return new UIInventoryDisplay(Items[index]);
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);
            UpdateInventoryView();
            Invalidate();
        }

        public void UpdateInventoryView()
        {
            var sel = Game.SelectedAvatar;
            if (sel == null) return;
            var neighbourhood = Content.Get().Neighborhood;
            var neighbour = sel.GetPersonData(VMPersonDataVariable.NeighborId);
            var inventory = neighbourhood.GetInventoryByNID(neighbour).Where(x => (CatSort == -1 && !HiddenCats.Contains(x.Type)) || CatSort == x.Type).ToList();

            bool difference = false;
            if (inventory.Count == Items.Count)
            {
                for (int i = 0; i < inventory.Count; i++)
                {
                    var i1 = inventory[i]; var i2 = Items[i];
                    if (i1.Count != i2.Count || i1.GUID != i2.GUID || i1.Type != i2.Type)
                    {
                        difference = true; break;
                    }
                }
            } else
            {
                difference = true;
            }

            if (difference)
            {
                Items.Clear();
                foreach (var item in inventory)
                {
                    Items.Add(item.Clone());
                }
                ScrollView.Reset();
            }
        }
    }

    public class UIInventoryDisplay : UITSContainer {
        private UILabel NameLabel;
        private UILabel CountLabel;
        private Texture2D ItemBg;
        private Texture2D Item;
        public UIInventoryDisplay(InventoryItem item) : base()
        {
            NameLabel = new UILabel();
            NameLabel.CaptionStyle = NameLabel.CaptionStyle.Clone();
            NameLabel.CaptionStyle.Color = UIStyle.Current.Text;
            NameLabel.CaptionStyle.Size = 10;
            NameLabel.Wrapped = true;
            NameLabel.Size = new Vector2(80, 33);
            NameLabel.Position = new Vector2(5, 5);
            NameLabel.Alignment = TextAlignment.Middle | TextAlignment.Center;
            Add(NameLabel);

            CountLabel = new UILabel();
            CountLabel.CaptionStyle = NameLabel.CaptionStyle.Clone();
            CountLabel.CaptionStyle.Size = 15;
            CountLabel.Size = new Vector2(80, 30);
            CountLabel.Position = new Vector2(5, 90);
            CountLabel.Alignment = TextAlignment.Middle | TextAlignment.Center;
            Add(CountLabel);

            ItemBg = Content.Get().CustomUI.Get("inv_item.png").Get(GameFacade.GraphicsDevice);

            var obj = Content.Get().WorldObjects.Get(item.GUID);
            if (obj != null)
            {
                Item = obj.Resource.Get<BMP>(obj.OBJ.CatalogStringsID)?.GetTexture(GameFacade.GraphicsDevice);
                NameLabel.Caption = obj.Resource.Get<CTSS>(obj.OBJ.CatalogStringsID)?.GetString(0) ?? obj.OBJ.ChunkLabel;
                CountLabel.Caption = item.Count.ToString();
            }
        }

        public override void Draw(UISpriteBatch batch)
        {
            DrawLocalTexture(batch, ItemBg, null, new Vector2(20, 40), Vector2.One, new Color(104, 164, 184, 255));

            if (Item != null)
            {
                DrawLocalTexture(batch, Item, new Rectangle(0, 0, Item.Width / 2, Item.Height), new Vector2(45 + 42 / -2, 65 + 42 / -2), new Vector2(42f / Item.Height, 42f / Item.Height));
            }

            base.Draw(batch);
        }
    }
}
