using FSO.Client;
using FSO.Client.UI.Framework;
using FSO.Common.Utils;
using FSO.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simitone.Client.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.Common.Rendering.Framework.Model;

namespace Simitone.Client.UI.Controls
{
    public class UICategorySwitcher : UIContainer
    {
        public UICatButton MainButton;
        public UIDiagonalStripe Stripe;
        public UIVertGrad Grad;
        public event Action<int> OnCategorySelect;
        public event Action OnOpen;
        public int ActiveCategory;
        public List<UICategory> Categories;
        public List<UIStencilButton> CatSwitchButtons = new List<UIStencilButton>();

        private float _ce;
        public float CategoryExpand
        {
            get
            {
                return _ce;
            }
            set
            {
                var scrHeight = GameFacade.Screens.CurrentUIScreen.ScreenHeight;
                var size = (scrHeight - (128 + 15));
                Stripe.Y = (-value) * size;
                Stripe.BodySize = new Point(85, (int)(value*size));

                var i = 0;
                foreach (var btn in CatSwitchButtons)
                {
                    btn.Y = i++ * -70 * value - 75;
                    btn.Opacity = value;
                    btn.Visible = value > 0;
                }

                Grad.Visible = value > 0;
                Grad.GSize = new Vector2(size, 75*value);
                Stripe.Visible = value > 0;
                _ce = value;
            }
        }

        public UICategorySwitcher()
        {
            Stripe = new UIDiagonalStripe(new Point(), UIDiagonalStripeSide.UP, UIStyle.Current.Bg);
            Add(Stripe);

            Grad = new UIVertGrad();
            Grad.Position = new Vector2(43, 0);
            Grad.Visible = false;
            Add(Grad);

            MainButton = new UICatButton(TextureGenerator.GetPxWhite(GameFacade.GraphicsDevice));
            MainButton.Position = new Microsoft.Xna.Framework.Vector2(10, 31);
            MainButton.OnButtonClick += (b) => { Open(); };
            MainButton.Selected = true;
            Add(MainButton);

            CategoryExpand = CategoryExpand;
        }

        public void Select(int cat)
        {
            foreach (var item in CatSwitchButtons)
            {
                Remove(item);
            }
            CatSwitchButtons.Clear();
            foreach (var catG in Categories)
            {
                var id = catG.ID;
                if (catG.ID == cat)
                {
                    MainButton.Texture = Content.Get().CustomUI.Get(catG.IconName).Get(GameFacade.GraphicsDevice);
                }
                else
                {
                    var btn = new UIStencilButton(Content.Get().CustomUI.Get(catG.IconName).Get(GameFacade.GraphicsDevice));
                    btn.Shadow = true;
                    btn.X = 10;
                    btn.Visible = false;
                    btn.OnButtonClick += (b) => { Select(id); };
                    Add(btn);
                    CatSwitchButtons.Add(btn);
                }
            }
            if (CategoryExpand > 0)
            {
                CategoryExpand = CategoryExpand;
                Close();
            }
            OnCategorySelect?.Invoke(cat);
            ActiveCategory = cat;
        }

        public void InitCategories(List<UICategory> cats)
        {
            Categories = cats;
            Select(Categories[0].ID);
        }

        public void Open()
        {
            if (CategoryExpand > 0)
            {
                Close(); return;
            }
            OnOpen?.Invoke();
            GameFacade.Screens.Tween.To(this, 0.3f, new Dictionary<string, float>() { { "CategoryExpand", 1f } }, TweenQuad.EaseOut);
        }

        public void Close()
        {
            GameFacade.Screens.Tween.To(this, 0.3f, new Dictionary<string, float>() { { "CategoryExpand", 0f } }, TweenQuad.EaseOut);
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);
        }
    }

    public class UICategory
    {
        public int ID;
        public string IconName;
    }

    public class UIVertGrad : UIElement
    {
        public Texture2D Grad;
        public Vector2 GSize;

        public UIVertGrad() : base()
        {
            Grad = Content.Get().CustomUI.Get("dialog_title_grad.png").Get(GameFacade.GraphicsDevice);
        }

        public override void Draw(UISpriteBatch batch)
        {
            DrawLocalTexture(batch, Grad, null, new Vector2(0, 0), new Vector2(GSize.X / Grad.Width, GSize.Y), Color.White, (float)Math.PI / -2, new Vector2(0, 0.5f));
        }
    }
}
