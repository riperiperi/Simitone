using FSO.Client;
using FSO.Client.UI.Framework;
using FSO.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.Common.Rendering.Framework.Model;
using FSO.Common.Rendering.Framework.IO;

namespace Simitone.Client.UI.Controls
{
    public class UITouchScroll : UICachedContainer
    {
        public int ItemWidth;
        public Texture2D ScrollEdgeL;
        public Texture2D ScrollEdgeR;
        public bool DrawBounds = true;
        public int Margin;

        private UIMouseEventRef HitTest;

        private float Scroll;
        private float ScrollVelocity;

        private Func<int> LengthProvider;
        private Func<int, UITSContainer> ElemProvider;

        public UITouchScroll(Func<int> lengthProvider, Func<int, UITSContainer> elemProvider) : base()
        {
            var ui = Content.Get().CustomUI;
            ScrollEdgeL = ui.Get("scroll_edge_l.png").Get(GameFacade.GraphicsDevice);
            ScrollEdgeR = ui.Get("scroll_edge_r.png").Get(GameFacade.GraphicsDevice);
            HitTest = ListenForMouse(new Rectangle(0, 0, (int)Size.X, (int)Size.Y), new UIMouseEvent(MouseEvents));

            LengthProvider = lengthProvider;
            ElemProvider = elemProvider;
        }

        private int MouseDownTime;
        private int MouseDownID = -1;
        private Point MouseDownAt;
        private bool InScroll;
        private UITSContainer LastSelected;
        private List<float> ScrollVelocityHistory = new List<float>();

        public void MouseEvents(UIMouseEventType type, UpdateState state)
        {
            switch (type)
            {
                case UIMouseEventType.MouseDown:
                    MouseDownID = state.CurrentMouseID;
                    MouseDownTime = 0;
                    MouseDownAt = state.MouseState.Position;
                    InScroll = false;
                    ScrollVelocity = 0;
                    break;
                case UIMouseEventType.MouseUp:
                    if (!InScroll)
                    {
                        Select(GlobalPoint(MouseDownAt.ToVector2()).ToPoint());
                    }
                    else
                    {
                        //calculate scroll velocity
                        if (ScrollVelocityHistory.Count > 1)
                        {
                            int total = 0;
                            ScrollVelocity = 0f;
                            for (int i = 1; i < ScrollVelocityHistory.Count; i++)
                            {
                                total++;
                                ScrollVelocity += ScrollVelocityHistory[i];
                            }
                            ScrollVelocity /= total;
                        }
                        ScrollVelocityHistory.Clear();
                    }

                    InScroll = false;
                    MouseDownID = -1;
                    break;
            }
        }

        public void Select(Point at)
        {
            var item = (int)(at.X + Scroll) / ItemWidth;
            if (item >= LengthProvider()) return;
            var rItem = GetOrPrepare(item);
            if (rItem != null)
            {
                LastSelected?.Deselected();
                rItem.Selected();
                LastSelected = rItem;
            }
        }

        public UITSContainer GetOrPrepare(int id)
        {
            var item = Children.Where(x => (x as UITSContainer)?.ItemID == id).FirstOrDefault() as UITSContainer;
            if (item == null)
            {
                item = ElemProvider(id);
                item.Visible = false;
                item.ItemID = id;
                Add(item);
            }
            return item;
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);
            var sizeRect = new Rectangle(0, 0, (int)Size.X, (int)Size.Y);
            if (!sizeRect.Equals(HitTest.Region))
            {
                HitTest.Region = sizeRect;
            }

            var length = LengthProvider();

            //perform scroll and input management.

            if (MouseDownID != -1)
            {
                var lastMouse = state.MouseStates.FirstOrDefault(x => x.ID == MouseDownID);
                if (lastMouse != null)
                {
                    var pos = lastMouse.MouseState.Position;
                    if (!InScroll)
                    {
                        if (Math.Abs((pos - MouseDownAt).X) > 25) InScroll = true;
                    }
                    if (InScroll)
                    {
                        ScrollVelocity = -(pos - MouseDownAt).X;
                        MouseDownAt = pos;
                    }
                }
            }

            ScrollVelocityHistory.Insert(0, ScrollVelocity);
            if (ScrollVelocityHistory.Count > 5) ScrollVelocityHistory.RemoveAt(ScrollVelocityHistory.Count - 1);

            Scroll += ScrollVelocity;
            ScrollVelocity *= 0.9f;
            Scroll = Math.Max(-Margin, Math.Min(length * ItemWidth - Size.X + Margin, Scroll));

            //update children positions.
            //delete ones that are not 

            var untouched = new HashSet<UIElement>(Children);

            var b = (int)(Scroll / ItemWidth);
            var e = b + (Size.X + (ItemWidth - 1)) / ItemWidth;
            for (int i=b; i<e; i++)
            {
                if (i < 0) continue;
                if (i >= length) break;
                var item = GetOrPrepare(i);
                untouched.Remove(item);
                item.X = i * ItemWidth - Scroll;
                item.Visible = true;
            }

            foreach (var child in untouched)
            {
                Children.Remove(child);
            }

            Invalidate();
        }

        public void SetScroll(float value)
        {
            Scroll = value;
        }

        public override void Draw(UISpriteBatch batch)
        {
            base.Draw(batch);
            if (DrawBounds)
            {
                DrawLocalTexture(batch, ScrollEdgeL, new Vector2(0, Size.Y / 2 - 64));
                DrawLocalTexture(batch, ScrollEdgeR, new Vector2(Size.X - 15, Size.Y / 2 - 64));
            }
        }

        public void Reset()
        {
            var children = new List<UIElement>(Children);
            foreach (var child in children)
            {
                Remove(child);
            }
            LastSelected = null;
        }
    }

    public class UITSContainer : UIContainer
    {
        public int ItemID;
        public virtual void Selected()
        {

        }

        public virtual void Deselected()
        {

        }
    }
}
