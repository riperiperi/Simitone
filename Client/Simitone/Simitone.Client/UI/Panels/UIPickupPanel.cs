using FSO.Client;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Common;
using FSO.Content;
using FSO.Files.Formats.IFF.Chunks;
using FSO.LotView.Components;
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
using FSO.Common.Rendering.Framework.Model;
using FSO.Common.Rendering.Framework.IO;

namespace Simitone.Client.UI.Panels
{
    public class UIPickupPanel : UIContainer
    {
        public UILabel TitleLabel;
        public UILabel SubtextLabel;
        public Texture2D Thumbnail;
        public UI3DThumb Thumb3D;
        protected UIMouseEventRef ClickHandler;

        public UICatButton CancelButton;

        public event Action<bool> OnResponse;

        public UIPickupPanel()
        {
            TitleLabel = new UILabel();
            TitleLabel.Position = new Vector2(450, 14);
            TitleLabel.CaptionStyle = TitleLabel.CaptionStyle.Clone();
            TitleLabel.CaptionStyle.Size = 19;
            TitleLabel.CaptionStyle.Color = UIStyle.Current.SecondaryText;
            Add(TitleLabel);

            SubtextLabel = new UILabel();
            SubtextLabel.Position = new Vector2(450, 44);
            SubtextLabel.CaptionStyle = SubtextLabel.CaptionStyle.Clone();
            SubtextLabel.CaptionStyle.Size = 12;
            SubtextLabel.CaptionStyle.Color = UIStyle.Current.Text;
            Add(SubtextLabel);

            CancelButton = new UICatButton(Content.Get().CustomUI.Get("cat_cancel.png").Get(GameFacade.GraphicsDevice));
            CancelButton.Position = new Vector2(174, 31);
            CancelButton.OnButtonClick += CancelButton_OnButtonClick;
            Add(CancelButton);

            ClickHandler =
                ListenForMouse(new Rectangle(0, 0, 400, 128), new UIMouseEvent(OnMouseEvent));
        }

        private void OnMouseEvent(UIMouseEventType type, UpdateState state)
        {
            if (type == UIMouseEventType.MouseDown)
            {
                OnResponse?.Invoke(true);
            }
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);
            if (Visible)
            {
                SubtextLabel.CaptionStyle.Color = UIStyle.Current.Text * Opacity;
                TitleLabel.CaptionStyle.Color = UIStyle.Current.SecondaryText * Opacity;
                CancelButton.Opacity = Opacity;
            }
        }

        private void CancelButton_OnButtonClick(UIElement button)
        {
            OnResponse?.Invoke(false);
        }

        public void SetInfo(VM vm, VMEntity entity)
        {
            var obj = entity.Object;
            var def = entity.MasterDefinition;
            if (def == null) def = entity.Object.OBJ;

            CTSS catString = obj.Resource.Get<CTSS>(def.CatalogStringsID);
            if (catString != null)
            {
                TitleLabel.Caption = catString.GetString(0);
            }
            else
            {
                TitleLabel.Caption = entity.ToString();
            }
            var World = vm.Context.World;
            var sellback = entity.MultitileGroup.Price;

            var canDelete = entity.IsUserMovable(vm.Context, true) == FSO.SimAntics.Model.VMPlacementError.Success;
            if (!canDelete)
            {
                SubtextLabel.Caption = GameFacade.Strings.GetString("136", "2").Replace("%", TitleLabel.Caption); //cannot be deleted
            } else
            {
                if (sellback > 0)
                {
                    SubtextLabel.Caption = GameFacade.Strings.GetString("136", "1").Replace("%", TitleLabel.Caption).Replace("$", "§" + sellback.ToString());
                } else
                {
                    SubtextLabel.Caption = GameFacade.Strings.GetString("136", "0").Replace("%", TitleLabel.Caption);
                }
                
            }

            if (entity is VMGameObject)
            {
                var objects = entity.MultitileGroup.Objects;
                ObjectComponent[] objComps = new ObjectComponent[objects.Count];
                for (int i = 0; i < objects.Count; i++)
                {
                    objComps[i] = (ObjectComponent)objects[i].WorldUI;
                }
                if (Thumbnail != null) Thumbnail.Dispose();
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
                if (Thumbnail != null) Thumbnail.Dispose();
                if (Thumb3D != null) Thumb3D.Dispose();
                Thumbnail = null; Thumb3D = null;
                Thumbnail = null;
            }
        }

        public override void PreDraw(UISpriteBatch batch)
        {
            if (!Visible) return;
            base.PreDraw(batch);
            Thumb3D?.Draw();
        }

        public override void Draw(UISpriteBatch batch)
        {
            if (!Visible) return;
            base.Draw(batch);
            var targSize = 180f;

            float scale = 1f;
            Texture2D thumb = null;
            if (Thumb3D != null)
            {
                thumb = Thumb3D.Tex;
            }
            else if (Thumbnail != null)
            {
                scale = targSize / (float)Math.Sqrt(Thumbnail.Width * Thumbnail.Width + Thumbnail.Height * Thumbnail.Height);
                thumb = Thumbnail;
            }

            if (thumb != null)
            {
                var pos = new Vector2(thumb.Width * scale - 350 * 2, thumb.Height * scale - 128) / -2;
                DrawLocalTexture(batch, thumb, null, pos, new Vector2(scale));
            }
        }

        public override void Removed()
        {
            if (Thumbnail != null) Thumbnail.Dispose();
            if (Thumb3D != null) Thumb3D.Dispose();
        }
    }
}
