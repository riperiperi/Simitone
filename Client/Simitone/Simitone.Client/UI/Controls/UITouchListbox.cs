using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using FSO.Client;
using FSO.Content;
using Simitone.Client.UI.Model;
using Microsoft.Xna.Framework.Graphics;
using FSO.Common.Utils;

namespace Simitone.Client.UI.Controls
{
    public class UITouchStringList : UIContainer
    {
        public UIImage Outline;
        public UITouchScroll ScrollElem;
        public event Action<int> OnSelectionChange;

        private Vector2 _Size;
        public override Vector2 Size
        {
            get
            {
                return _Size;
            }

            set
            {
                _Size = value;
                Outline.Width = value.X;
                Outline.Height = value.Y;
                ScrollElem.Size = new Vector2(value.X - 12, value.Y - 12);
            }
        }

        public List<string> BackingList = new List<string>();

        public UITouchStringList()
        {
            var gd = GameFacade.GraphicsDevice;
            var ui = Content.Get().CustomUI;

            ScrollElem = new UITouchScroll(GetLength, GetElemAt);
            ScrollElem.VerticalMode = true;
            ScrollElem.Position = new Vector2(6);
            ScrollElem.ItemWidth = 38;
            ScrollElem.DrawBounds = false;
            ScrollElem.Margin = 6;
            ScrollElem.SetScroll(-6);
            Add(ScrollElem);

            Outline = new UIImage(ui.Get("cat_btn_base.png").Get(gd)).With9Slice(25, 25, 25, 25);
            Add(Outline);
        }

        public void SelectionChanged(int id)
        {
            OnSelectionChange?.Invoke(id);
        }

        public void Refresh()
        {
            ScrollElem.Reset();
        }

        public int GetLength()
        {
            return BackingList.Count;
        }

        public UITSContainer GetElemAt(int i)
        {
            return new UITouchStringListItem(BackingList[i], new Point((int)Size.X-12, ScrollElem.ItemWidth), this);
        }
    }

    public class UITouchStringListItem : UITSContainer
    {
        public string Value;
        public Point ESize;
        public bool Outlined;
        public UITouchStringList TParent;
        public UILabel Label;
        private Texture2D Px;
        public float SelectPct { get; set; }
        
        public UITouchStringListItem(string value, Point esize, UITouchStringList parent)
        {
            TParent = parent;
            Value = value;
            ESize = esize;
            Px = TextureGenerator.GetPxWhite(GameFacade.GraphicsDevice);
            Label = new UILabel();
            Label.CaptionStyle = Label.CaptionStyle.Clone();
            Label.CaptionStyle.Size = 19;
            Label.CaptionStyle.Color = UIStyle.Current.Text;
            Label.Alignment = TextAlignment.Middle | TextAlignment.Left;
            Label.Caption = Label.CaptionStyle.TruncateToWidth(value, esize.X-20);
            Label.Size = esize.ToVector2();
            Label.X += 10;
            Add(Label);

            SelectPct = SelectPct;
        }
        
        public override void Selected()
        {
            Outlined = true;
            Label.CaptionStyle.Color = UIStyle.Current.Bg;
            GameFacade.Screens.Tween.To(this, 0.3f, new Dictionary<string, float>() { { "SelectPct", 1f } }, TweenQuad.EaseOut);
            TParent.SelectionChanged(ItemID);
        }

        public override void Deselected()
        {
            Label.CaptionStyle.Color = UIStyle.Current.Text;
            GameFacade.Screens.Tween.To(this, 0.3f, new Dictionary<string, float>() { { "SelectPct", 0f } }, TweenQuad.EaseOut);
            Outlined = false;
        }

        public override void Draw(UISpriteBatch batch)
        {
            DrawLocalTexture(batch, Px, null, Vector2.Zero, new Vector2(ESize.X*SelectPct, ESize.Y), UIStyle.Current.SecondaryText);
            base.Draw(batch);
        }
    }
}
