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
    public class UIRelationshipSubpanel : UISubpanel
    {
        public List<Tuple<int, int>> Items = new List<Tuple<int, int>>();
        public UITouchScroll ScrollView;
        public int RelSort = -1;

        public UITwoStateButton FriendButton;
        public UITwoStateButton FamButton;
        public UITwoStateButton AllButton;
        public UITwoStateButton FameButton;

        public UIRelationshipSubpanel(TS1GameScreen game) : base(game)
        {
            var ui = Content.Get().CustomUI;
            ScrollView = new UITouchScroll(() => { return Items?.Count ?? 0; }, DisplayProvider);
            ScrollView.X = 148;
            ScrollView.ItemWidth = 90;
            ScrollView.Size = new Vector2(582, 128);
            Add(ScrollView);

            FriendButton = new UITwoStateButton(ui.Get("rel_friend.png").Get(GameFacade.GraphicsDevice));
            FriendButton.Position = new Vector2(21, 9);
            FriendButton.OnButtonClick += (b) => { ChangeCat(0); };
            Add(FriendButton);
            FamButton = new UITwoStateButton(ui.Get("rel_fam.png").Get(GameFacade.GraphicsDevice));
            FamButton.Position = new Vector2(81, 9);
            FamButton.OnButtonClick += (b) => { ChangeCat(1); };
            Add(FamButton);
            AllButton = new UITwoStateButton(ui.Get("rel_all.png").Get(GameFacade.GraphicsDevice));
            AllButton.Position = new Vector2(21, 69);
            AllButton.OnButtonClick += (b) => { ChangeCat(-1); };
            Add(AllButton);
            FameButton = new UITwoStateButton(ui.Get("rel_fame.png").Get(GameFacade.GraphicsDevice));
            FameButton.Position = new Vector2(81, 69);
            FameButton.OnButtonClick += (b) => { ChangeCat(2); };
            Add(FameButton);

            ChangeCat(-1);
        }

        public void ChangeCat(int cat)
        {
            FriendButton.Selected = cat == 0;
            FamButton.Selected = cat == 1;
            AllButton.Selected = cat == -1;
            FameButton.Selected = cat == 2;
            RelSort = cat;
        }

        public UITSContainer DisplayProvider(int index)
        {
            return new UIRelationshipDisplay(Items[index].Item1, Items[index].Item2, Game.vm);
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);
            UpdateRelView();
            Invalidate();
        }

        public void UpdateRelView()
        {
            var sel = Game.SelectedAvatar;
            var neighbourhood = Content.Get().Neighborhood;
            if (sel == null) return;
            var neighbour = sel.GetPersonData(VMPersonDataVariable.NeighborId);

            var n = neighbourhood.GetNeighborByID(neighbour);
            var rel = n.Relationships;

            var rItems = rel.Select(x => new Tuple<int,int>(neighbour, x.Key)).ToList();

            bool difference = false;
            if (rItems.Count == Items.Count)
            {
                for (int i = 0; i < rItems.Count; i++)
                {
                    if (!rItems[i].Equals(Items[i])) {
                        difference = true; break;
                    }
                }
            } else
            {
                difference = true;
            }

            if (difference)
            {
                Items = rItems;
                ScrollView.Reset();
            }
        }
    }

    public class UIRelationshipDisplay : UITSContainer {
        private UILabel NameLabel;
        private Texture2D ItemBg;
        private Texture2D Item;
        private UIMotiveBar Bar1;
        private UIMotiveBar Bar2;

        private int NIDF;
        private int NID;
        public UIRelationshipDisplay(int nidFrom, int nid, VM curVM) : base()
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

            Bar1 = new UIMotiveBar();
            Bar1.ScaleX = Bar1.ScaleY = 0.5f;
            Bar1.Position = new Vector2(8, 94);
            Add(Bar1);

            Bar2 = new UIMotiveBar();
            Bar2.ScaleX = Bar2.ScaleY = 0.5f;
            Bar2.Position = new Vector2(8, 106);
            Add(Bar2);

            ItemBg = Content.Get().CustomUI.Get("inv_item.png").Get(GameFacade.GraphicsDevice);

            var neighbourhood = Content.Get().Neighborhood;
            var n = neighbourhood.GetNeighborByID((short)nid);
            var obj = Content.Get().WorldObjects.Get(n.GUID);
            if (obj != null)
            {
                var aobj = curVM.Context.CreateObjectInstance(n.GUID, LotTilePos.OUT_OF_WORLD, Direction.NORTH, true);
                Item = UIIconCache.GetObject(aobj.BaseObject);
                aobj.Delete(curVM.Context);
                var ctss = obj.Resource.Get<CTSS>(obj.OBJ.CatalogStringsID);
                //todo: family name
                NameLabel.Caption = ctss?.GetString(0) ?? obj.OBJ.ChunkLabel;
            }
            NIDF = nidFrom;
            NID = nid;
        }

        public override void Update(UpdateState state)
        {
            var neighbourhood = Content.Get().Neighborhood;
            var n = neighbourhood.GetNeighborByID((short)NIDF);
            var rel = n.Relationships;

            List<short> values;
            if (rel.TryGetValue(NID, out values))
            {
                if (values.Count > 0) Bar1.MotiveValue = values[0];
                if (values.Count > 2) Bar2.MotiveValue = values[2];
            }
            base.Update(state);
        }

        public override void Draw(UISpriteBatch batch)
        {
            DrawLocalTexture(batch, ItemBg, null, new Vector2(20, 40), Vector2.One, new Color(104, 164, 184, 255));

            if (Item != null)
            {
                DrawLocalTexture(batch, Item, null, new Vector2(45 + 42 / -2, 65 + 42 / -2), new Vector2(42f / Item.Height, 42f / Item.Height));
            }

            base.Draw(batch);
        }
    }
}
