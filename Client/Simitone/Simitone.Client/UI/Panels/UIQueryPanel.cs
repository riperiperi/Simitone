using FSO.Client;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Common.Utils;
using FSO.Content;
using FSO.Files.Formats.IFF.Chunks;
using FSO.LotView;
using FSO.LotView.Components;
using FSO.SimAntics;
using FSO.SimAntics.Model.TSOPlatform;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simitone.Client.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.Common.Rendering.Framework.Model;
using FSO.Common;
using FSO.UI.Panels;

namespace Simitone.Client.UI.Panels
{
    public class UIQueryPanel : UICachedContainer
    {
        public UILabel Title;
        public UILabel Body;
        public UILabel TitleAd;
        public UILabel Ad;
        public Texture2D Thumbnail;
        public bool Disposable;
        public UI3DThumb Thumb3D;
        public VMEntity ActiveEntity;
        public Texture2D TitleTex;
        public Texture2D ThumbTex;
        public int Mode;

        public bool Shown;
        private float _ShowPCT = 0f;
        public float ShowPCT
        {
            get
            {
                return _ShowPCT;
            }
            set
            {
                _ShowPCT = value;
                Size = new Vector2(Size.X, value * FullHeight + (1 - value) * 45);
                Y = -(5 + Size.Y);
                TitleAd.CaptionStyle.Color = UIStyle.Current.SecondaryText * (1-value);
            }
        }

        private bool _Active;
        public bool Active
        {
            get
            {
                return _Active;
            }
            set
            {
                if (_Active != value)
                {
                    GameFacade.Screens.Tween.To(this, 0.5f, new Dictionary<string, float>() { { "Opacity", (value)?1f:0f } }, TweenQuad.EaseOut);
                }
                _Active = value;
            }
        }

        public float FullHeight = 45;
        private string[] AdStrings;
        private World World;

        public UIQueryPanel(World world)
        {
            World = world;

            AdStrings = new string[14];
            for (int i = 0; i < 14; i++)
            {
                string str = GameFacade.Strings.GetString("160", (i).ToString());
                AdStrings[i] = ((i < 7) ? str.Substring(0, str.Length - 2) + "{0}" : str) + "\r\n";
            }

            Title = new UILabel();
            Title.Size = new Vector2(546 - 50, 0);
            Title.Position = new Vector2(25 + 210, 8);
            Title.CaptionStyle = Title.CaptionStyle.Clone();
            Title.CaptionStyle.Size = 19;
            Title.CaptionStyle.Color = UIStyle.Current.Text;
            Add(Title);

            TitleAd = new UILabel();
            TitleAd.Size = new Vector2(546-50, 0);
            TitleAd.Position = new Vector2(25 + 210, 8);
            TitleAd.Alignment = TextAlignment.Top | TextAlignment.Right;
            TitleAd.CaptionStyle = TitleAd.CaptionStyle.Clone();
            TitleAd.CaptionStyle.Size = 12;
            TitleAd.CaptionStyle.Color = UIStyle.Current.SecondaryText;
            Add(TitleAd);

            Body = new UILabel();
            Body.Wrapped = true;
            Body.Size = new Vector2(518, 0);
            Body.Position = new Vector2(14 + 210, 55);
            Body.CaptionStyle = Body.CaptionStyle.Clone();
            Body.CaptionStyle.Size = 15;
            Body.CaptionStyle.Color = UIStyle.Current.Text;
            Add(Body);

            Ad = new UILabel();
            Ad.Wrapped = true;
            Ad.Alignment = TextAlignment.Top | TextAlignment.Right;
            Ad.Size = new Vector2(518, 0);
            Ad.Position = new Vector2(14+210, 55); //placed below body
            Ad.CaptionStyle = Ad.CaptionStyle.Clone();
            Ad.CaptionStyle.Size = 12;
            Ad.CaptionStyle.Color = UIStyle.Current.SecondaryText;
            Add(Ad);
            Size = new Vector2(210 + 546, 45);

            ShowPCT = ShowPCT;
            Opacity = 0;
            Active = Active;

            ThumbTex = Content.Get().CustomUI.Get("cat_thumb_bg.png").Get(GameFacade.GraphicsDevice);
            TitleTex = Content.Get().CustomUI.Get("query_title.png").Get(GameFacade.GraphicsDevice);

            Title.Alignment = TextAlignment.Left | TextAlignment.Top;
            Body.Alignment = TextAlignment.Left | TextAlignment.Top;

            InternalBefore = true;
        }

        public override void Update(UpdateState state)
        {
            Visible = Opacity > 0;
            if (Thumb3D != null && Visible) Invalidate();
            base.Update(state);
        }

        public void SetShown(bool show)
        {
            if (Shown && show == Shown && FullHeight != Size.Y)
            {
                ShowPCT = Size.Y / FullHeight;
            }
            if (show)
            {
                GameFacade.Screens.Tween.To(this, 0.5f, new Dictionary<string, float>() { { "ShowPCT", 1f } }, TweenQuad.EaseOut);
            } else
            {
                GameFacade.Screens.Tween.To(this, 0.5f, new Dictionary<string, float>() { { "ShowPCT", 0f } }, TweenQuad.EaseOut);
            }

            Shown = show;
        }

        public void SetInfo(VM vm, VMEntity entity, bool bought)
        {
            ActiveEntity = entity;
            var obj = entity.Object;
            var def = entity.MasterDefinition;
            if (def == null) def = entity.Object.OBJ;

            var item = Content.Get().WorldCatalog.GetItemByGUID(def.GUID);

            CTSS catString = obj.Resource.Get<CTSS>(def.CatalogStringsID);
            if (catString != null)
            {
                Body.Caption = catString.GetString(1);
                Title.Caption = catString.GetString(0);
            }
            else
            {
                Body.Caption = "No information available.";
                Title.Caption = entity.ToString();
            }

            StringBuilder motivesString = new StringBuilder();
            if (def.RatingHunger != 0) { motivesString.AppendFormat(AdStrings[0], def.RatingHunger); }
            if (def.RatingComfort != 0) { motivesString.AppendFormat(AdStrings[1], def.RatingComfort); }
            if (def.RatingHygiene != 0) { motivesString.AppendFormat(AdStrings[2], def.RatingHygiene); }
            if (def.RatingBladder != 0) { motivesString.AppendFormat(AdStrings[3], def.RatingBladder); }
            if (def.RatingEnergy != 0) { motivesString.AppendFormat(AdStrings[4], def.RatingEnergy); }
            if (def.RatingFun != 0) { motivesString.AppendFormat(AdStrings[5], def.RatingFun); }
            if (def.RatingRoom != 0) { motivesString.AppendFormat(AdStrings[6], def.RatingRoom); }

            var sFlags = def.RatingSkillFlags;
            for (int i = 0; i < 7; i++)
            {
                if ((sFlags & (1 << i)) > 0) motivesString.Append(AdStrings[i + 7]);
            }

            var strings = motivesString.ToString().Replace("\r", "").Split('\n').TakeWhile(x => x != "");
            TitleAd.Caption = string.Join("\n", strings.Take(2));
            TitleAd.CaptionStyle.LineHeightModifier = -3;
            TitleAd.Y = 22;
            TitleAd.Alignment = TextAlignment.Middle | TextAlignment.Right;
            TitleAd.Wrapped = true;
            Ad.Caption = string.Join(", ", strings);

            SetTitleSize();

            if (entity is VMGameObject)
            {
                var objects = entity.MultitileGroup.Objects;
                ObjectComponent[] objComps = new ObjectComponent[objects.Count];
                for (int i = 0; i < objects.Count; i++)
                {
                    objComps[i] = (ObjectComponent)objects[i].WorldUI;
                }
                if (Thumbnail != null && Disposable) Thumbnail.Dispose();
                if (Thumb3D != null) Thumb3D.Dispose();
                Thumbnail = null; Thumb3D = null;
                if (FSOEnvironment.Enable3D)
                {
                    Thumb3D = new UI3DThumb(entity);
                }
                else
                {
                    var thumb = World.GetObjectThumb(objComps, entity.MultitileGroup.GetBasePositions(), GameFacade.GraphicsDevice);
                    Thumbnail = thumb;
                }
            }
            else
            {
                if (Thumbnail != null && Disposable) Thumbnail.Dispose();
                if (Thumb3D != null) Thumb3D.Dispose();
                Thumbnail = null; Thumb3D = null;
                Thumbnail = null;
            }

            FullHeight = Body.CaptionStyle.LineHeight * Body.NumLines + 45 + 20;
            FullHeight = Math.Max(200, FullHeight);
            if (Ad.Caption != "")
            {
                Ad.Y = FullHeight;
                FullHeight += 25;
            }
            Disposable = true;
        }

        public override void Removed()
        {
            if (Thumbnail != null && Disposable) Thumbnail.Dispose();
            if (Thumb3D != null) Thumb3D.Dispose();
        }

        public void SetInfo(Texture2D thumb, string name, string description, int price, bool doDispose)
        {
            ActiveEntity = null;
            Body.Caption = description;
            Title.Caption = name;

            SetTitleSize();

            FullHeight = Body.CaptionStyle.LineHeight * Body.NumLines + 45 + 20;
            FullHeight = Math.Max(200, FullHeight);

            TitleAd.Caption = "";
            Ad.Caption = "";

            /*StringBuilder motivesString = new StringBuilder();
            motivesString.AppendFormat(GameFacade.Strings.GetString("206", "19") + "${0}\r\n", price);
            MotivesText.CurrentText = motivesString.ToString();*/

            if (Thumbnail != null && Disposable) Thumbnail.Dispose();
            if (Thumb3D != null) Thumb3D.Dispose();
            Thumbnail = null; Thumb3D = null;
            Thumbnail = thumb;
            //UpdateImagePosition();
            Disposable = doDispose;
        }

        private void SetTitleSize()
        {
            Title.CaptionStyle.Size = 19;
            var calc = Title.CaptionStyle.MeasureString(Title.Caption);
            if (calc.X > 400)
            {
                Title.CaptionStyle.Size = 15;
                Title.Y = 12;
            } else
            {
                Title.Y = 8;
            }
        }

        public override void InternalDraw(UISpriteBatch batch)
        {
            base.InternalDraw(batch);
            DrawLocalTexture(batch, TitleTex, new Rectangle(0, 0, 45, 45), new Vector2(210, 0), Vector2.One, UIStyle.Current.Bg);
            DrawLocalTexture(batch, TextureGenerator.GetPxWhite(batch.GraphicsDevice), null, new Vector2(255, 0), new Vector2(Size.X-(210+90), 45), UIStyle.Current.Bg);
            DrawLocalTexture(batch, TitleTex, new Rectangle(45, 0, 45, 45), new Vector2(Size.X - 45, 0), Vector2.One, UIStyle.Current.Bg);

            DrawLocalTexture(batch, TextureGenerator.GetPxWhite(batch.GraphicsDevice), null, new Vector2(210, 45), new Vector2(Size.X, Size.Y-45), UIStyle.Current.TitleBg);
            DrawLocalTexture(batch, ThumbTex, null, new Vector2(0, FullHeight - 200), Vector2.One, UIStyle.Current.TitleBg * ((255-TitleAd.CaptionStyle.Color.A)/255f));

            var targSize = 180f;

            float scale = 1f;
            Texture2D thumb = null;
            if (Thumb3D != null)
            {
                Thumb3D.Draw();
                thumb = Thumb3D.Tex;
            }
            else if (Thumbnail != null)
            {
                scale = targSize / (float)Math.Sqrt(Thumbnail.Width * Thumbnail.Width + Thumbnail.Height * Thumbnail.Height);
                thumb = Thumbnail;
            }

            if (thumb != null) {
                var pos = new Vector2(thumb.Width * scale - 200, thumb.Height * scale - (FullHeight - 100) * 2) / -2;
                DrawLocalTexture(batch, thumb, null, pos, new Vector2(scale));
            }
        }
    }
}
