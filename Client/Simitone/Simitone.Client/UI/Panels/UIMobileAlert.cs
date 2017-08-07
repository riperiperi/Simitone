using FSO.Client;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Content;
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

namespace Simitone.Client.UI.Panels
{
    public class UIMobileAlert : UIMobileDialog
    {
        private UIAlertOptions m_Options;
        private TextRendererResult m_MessageText;
        private TextStyle m_TextStyle;

        private UIImage Icon;
        private Vector2 IconSpace;

        private List<UIButton> Buttons;
        private UITextBox TextBox;

        public string ResponseText
        {
            get
            {
                return (TextBox == null) ? null : TextBox.CurrentText;
            }
            set
            {
                if (TextBox != null) TextBox.CurrentText = value;
            }
        }

        public UIMobileAlert(UIAlertOptions options) : base()
        {
            this.m_Options = options;

            m_TextStyle = TextStyle.DefaultLabel.Clone();
            m_TextStyle.Size = 19;
            m_TextStyle.Color = Color.White;

            Caption = options.Title;

            Icon = new UIImage();
            Icon.Visible = false;
            Icon.Position = new Vector2(32, 32);
            Icon.SetSize(0, 0);
            Add(Icon);

            /** Determine the size **/
            ComputeText();

            /** Add buttons **/
            Buttons = new List<UIButton>();

            foreach (var button in options.Buttons)
            {
                string buttonText = "";
                if (button.Text != null) buttonText = button.Text;
                else
                {
                    switch (button.Type)
                    {
                        case UIAlertButtonType.OK:
                            buttonText = GameFacade.Strings.GetString("142", "ok button");
                            break;
                        case UIAlertButtonType.Yes:
                            buttonText = GameFacade.Strings.GetString("142", "yes button");
                            break;
                        case UIAlertButtonType.No:
                            buttonText = GameFacade.Strings.GetString("142", "no button");
                            break;
                        case UIAlertButtonType.Cancel:
                            buttonText = GameFacade.Strings.GetString("142", "cancel button");
                            break;
                    }
                }
                var btnElem = AddButton(buttonText, button.Type, button.Handler == null);
                Buttons.Add(btnElem);
                if (button.Handler != null) btnElem.OnButtonClick += button.Handler;
            }

            if (options.TextEntry)
            {
                TextBox = new UITextBox();
                TextBox.MaxChars = options.MaxChars;
                this.Add(TextBox);
            }

            /** Position buttons **/
            RefreshSize();
        }

        public void RefreshSize()
        {
            var w = Width;
            var h = m_Options.Height;

            h = Math.Max(h, Math.Max((int)IconSpace.Y - 25, m_MessageText == null ? 0 : m_MessageText.BoundingBox.Height) + 105);

            if (Buttons.Count > 0)
            {
                h += 175;
            }
            else
            {
                h += 32;
            }

            if (m_Options.TextEntry)
            {
                TextBox.X = 32;
                TextBox.Y = h - 54;
                TextBox.SetSize(w - 64, 25);
                h += 45;
            }

            var btnX = (w - ((Buttons.Count-1) * 350)) / 2;
            var btnY = h - 125;
            foreach (UIButton button in Buttons)
            {
                button.Y = btnY;
                button.X = btnX - button.Width/2;
                btnX += 350;
            }

            if (Height != h)
            {
                SetHeight(h);
            }
            //update bg with height
        }

        private float TargetIX;

        public void SetIcon(Texture2D img, int width, int height)
        {
            if (img.Height < 4) return;
            Icon.Texture = img;

            float scale = Math.Min(3, Math.Min((float)height / (float)img.Height, (float)width / (float)img.Width));
            if (scale * img.Height + 20 < height) height = (int)(scale * img.Height + 20);
            IconSpace = new Vector2(width + 30, height);
            Icon.SetSize(img.Width * scale, img.Height * scale);
            Icon.Position = new Vector2(50, 110) + new Vector2(width / 2 - (img.Width * scale / 2), height / 2 - (img.Height * scale / 2));
            TargetIX = Icon.Position.X;

            ComputeText();
            RefreshSize();
        }

        /// <summary>
        /// Map of buttons attached to this message box.
        /// </summary>
        public Dictionary<UIAlertButtonType, UIButton> ButtonMap = new Dictionary<UIAlertButtonType, UIButton>();

        /// <summary>
        /// Adds a button to this message box.
        /// </summary>
        /// <param name="label">Label of the button.</param>
        /// <param name="type">Type of the button to be added.</param>
        /// <param name="InternalHandler">Should the button's click be handled internally?</param>
        /// <returns></returns>
        private UIButton AddButton(string label, UIAlertButtonType type, bool InternalHandler)
        {
            var btn = new UIBigButton(type == UIAlertButtonType.OK || type == UIAlertButtonType.Yes);
            btn.Visible = false;
            btn.Caption = label;
            if (btn.Width < 275) btn.Width = 275;

            if (InternalHandler)
                btn.OnButtonClick += new ButtonClickDelegate(x =>
                {
                    HandleClose();
                });

            ButtonMap.Add(type, btn);

            this.Add(btn);
            return btn;
        }

        private void HandleClose()
        {
            Close();
        }

        private bool m_TextDirty = false;
        protected override void CalculateMatrix()
        {
            base.CalculateMatrix();
            m_TextDirty = true;
        }

        private void ComputeText()
        {
            var margin = (IconSpace.X > 0) ? 50 : 80;
            m_MessageText = TextRenderer.ComputeText(m_Options.Message, new TextRendererOptions
            {
                Alignment = TextAlignment.Left | TextAlignment.Top,
                MaxWidth = Width - margin * 2,
                Position = new Microsoft.Xna.Framework.Vector2(margin, 105),
                Scale = _Scale,
                TextStyle = m_TextStyle,
                WordWrap = true,
                TopLeftIconSpace = IconSpace
            }, this);

            m_TextDirty = false;
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);
            var off = ((Closing) ? 1 : -1) * (1 - InterpolatedAnimation) * Width;
            var newIX = TargetIX + off;
            if (Icon.X != newIX)
            {
                Icon.Visible = true;
                Icon.X = newIX;
                var btnX = (Width - ((Buttons.Count - 1) * 350)) / 2;
                foreach (UIButton button in Buttons)
                {
                    button.X = off + btnX - button.Width / 2;
                    button.Visible = true;
                    btnX += 350;
                }
            }
        }

        public override void Draw(UISpriteBatch batch)
        {
            m_TextStyle.Color = UIStyle.Current.DialogText * InterpolatedAnimation;
            base.Draw(batch);

            if (m_TextDirty)
            {
                ComputeText();
            }

            TextRenderer.DrawText(m_MessageText.DrawingCommands, this, batch);
        }
    }
}
