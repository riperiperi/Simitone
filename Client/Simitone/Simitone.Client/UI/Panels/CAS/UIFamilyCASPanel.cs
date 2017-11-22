using FSO.Client;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Common.Utils;
using FSO.Content;
using FSO.SimAntics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simitone.Client.UI.Controls;
using Simitone.Client.UI.Model;
using Simitone.Client.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simitone.Client.UI.Panels.CAS
{
    public class UIFamilyCASPanel : UIContainer
    {
        public UIDiagonalStripe NameStripe;
        public UIDiagonalStripe ListStripe;

        public UITextBox SecondName;
        public UIAvatarListPanel AvatarList;
        public UICategorySwitcher AvatarOptions;

        public Action<bool, int> ModifySim;

        private float _ShowI;
        public float ShowI
        {
            get
            {
                return _ShowI;
            }
            set
            {
                if (value < 1 && AvatarOptions.CategoryExpand == 1) AvatarOptions.Close();
                ListStripe.Visible = value > 0;
                NameStripe.Visible = value > 0;
                NameStripe.X = (1 - value) * UIScreen.Current.ScreenWidth;
                NameStripe.Y = 30;
                NameStripe.BodySize = new Point((int)(value * UIScreen.Current.ScreenWidth), NameStripe.BodySize.Y);
                ListStripe.BodySize = new Point((int)(value * UIScreen.Current.ScreenWidth), ListStripe.BodySize.Y);
                AvatarList.X = (1-value) * (-UIScreen.Current.ScreenWidth);
                SecondName.X = (1 - value) * (UIScreen.Current.ScreenWidth);
                _ShowI = value;
            }
        }

        private List<UICategory> AvatarCategories = new List<UICategory>()
        {
            new UICategory() { ID = 0, IconName = "live_motives.png" }, //dummy
            new UICategory() { ID = 1, IconName = "cas_cat_edit.png" },
            new UICategory() { ID = 2, IconName = "cas_cat_del.png" },
        };

        private int ActiveSelection = -1;
        private List<VMAvatar> Avatars;

        public UIFamilyCASPanel(List<VMAvatar> avatar)
        {
            Avatars = avatar;

            AvatarOptions = new UICategorySwitcher();
            AvatarOptions.InitCategories(AvatarCategories);
            AvatarOptions.MainButton.Visible = false;
            AvatarOptions.OnCategorySelect += AvatarOptions_OnCategorySelect;
            Add(AvatarOptions);

            NameStripe = new UIDiagonalStripe(new Point(0, 75), UIDiagonalStripeSide.LEFT, UIStyle.Current.Bg);
            NameStripe.Position = new Vector2(UIScreen.Current.ScreenWidth, 30);
            Add(NameStripe);

            SecondName = new UITextBox();
            SecondName.TextMargin = new Rectangle();
            SecondName.SetSize(UIScreen.Current.ScreenWidth, 60);
            SecondName.Alignment = TextAlignment.Center;
            SecondName.TextStyle = SecondName.TextStyle.Clone();
            SecondName.TextStyle.Size = 37;
            SecondName.TextStyle.Color = UIStyle.Current.SecondaryText;
            SecondName.Position = new Vector2(0, 40);
            Add(SecondName);

            ListStripe = new UIDiagonalStripe(new Point(0, 125), UIDiagonalStripeSide.RIGHT, UIStyle.Current.Bg);
            ListStripe.Position = new Vector2(0, UIScreen.Current.ScreenHeight - 145);
            Add(ListStripe);

            AvatarList = new UIAvatarListPanel(avatar);
            AvatarList.Y = UIScreen.Current.ScreenHeight - 145;
            AvatarList.OnSelection += AvatarList_OnSelection;
            Add(AvatarList);

            Reset();
            ShowI = ShowI;
        }

        private void AvatarOptions_OnCategorySelect(int obj)
        {
            if (obj == 0) return;
            ModifySim?.Invoke((obj == 2), ActiveSelection);
            AvatarOptions.Select(0);
        }

        public void Reset()
        {
            AvatarList.InitAvatarList();
            ActiveSelection = -1;
        }

        private void AvatarList_OnSelection(int obj)
        {
            if (obj >= Avatars.Count)
            {
                ModifySim?.Invoke(false, -1);
            } else
            {
                AvatarOptions.X = AvatarOptions.X = UIScreen.Current.ScreenWidth / 2 - (Avatars.Count()) * 50 + obj * 100 - 44;
                AvatarOptions.Y = UIScreen.Current.ScreenHeight - 145;
                if (AvatarOptions.CategoryExpand < 1 || ActiveSelection == -1 || ActiveSelection == obj)
                    AvatarOptions.Open();
            }
            ActiveSelection = obj;
        }

        public override void GameResized()
        {
            base.GameResized();
            AvatarList.Y = UIScreen.Current.ScreenHeight - 145;
            ListStripe.Position = new Vector2(0, UIScreen.Current.ScreenHeight - 145);
            SecondName.SetSize(UIScreen.Current.ScreenWidth, 60);

            if (ActiveSelection > -1)
            {
                AvatarOptions.X = UIScreen.Current.ScreenWidth / 2 - (Avatars.Count()) * 50 + ActiveSelection*100 - 44;
                AvatarOptions.Y = UIScreen.Current.ScreenHeight - 145;
            }

            AvatarList.InitAvatarList();
        }
    }

    public class UIAvatarListPanel : UIContainer
    {
        private List<VMAvatar> Avatars;
        private List<UIAvatarSelectButton> Btns = new List<UIAvatarSelectButton>();
        private Texture2D Bg;
        public event Action<int> OnSelection;
        public UIAvatarListPanel(List<VMAvatar> avatar)
        {
            Avatars = avatar;

            InitAvatarList();
        }

        public void InitAvatarList()
        {
            int i = 0;
            foreach (var btn in Btns)
            {
                Remove(btn);
            }
            Btns.Clear();

            i = 0;
            var baseX = UIScreen.Current.ScreenWidth/2 - (Avatars.Count()) * 50;
            foreach (var fam in Avatars)
            {
                var btn = new UIAvatarSelectButton(UIIconCache.GetObject(fam));
                btn.Opacity = 1f;
                var id = i;
                btn.OnButtonClick += (b) => { Select(id); };
                btn.Name = fam.Name;
                btn.X = baseX + (i++) * 100;
                btn.Y = 50;
                Btns.Add(btn);
                Add(btn);
            }
            var btn2 = new UIAvatarSelectButton(Content.Get().CustomUI.Get("cas_new_plus.png").Get(GameFacade.GraphicsDevice));
            btn2.Opacity = 1f;
            var id2 = i;
            btn2.OnButtonClick += (b) => { Select(id2); };
            btn2.X = baseX + (i++) * 100;
            btn2.Y = 50;
            Btns.Add(btn2);
            Add(btn2);
        }

        private void Select(int selected)
        {
            int i = 0;
            foreach (var btn in Btns)
            {
                btn.Outlined = (i++) == selected;
            }
            OnSelection?.Invoke(selected);
        }

        public override void Draw(UISpriteBatch batch)
        {
            base.Draw(batch);
        }
    }
}
