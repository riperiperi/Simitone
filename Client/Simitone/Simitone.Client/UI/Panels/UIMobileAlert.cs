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
                            buttonText = GameFacade.Strings.GetString("142", "0");
                            break;
                        case UIAlertButtonType.Yes:
                            buttonText = GameFacade.Strings.GetString("142", "2");
                            break;
                        case UIAlertButtonType.No:
                            buttonText = GameFacade.Strings.GetString("142", "3");
                            break;
                        case UIAlertButtonType.Cancel:
                            buttonText = GameFacade.Strings.GetString("142", "1");
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

        public override void GameResized()
        {
            base.GameResized();
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

            h = ResetButtons(h, true);

            if (Height != h)
            {
                SetHeight(h);
            }
            //update bg with height
        }

        private int ResetButtons(int h, bool setY)
        {
            var btnX = Width/2;
            var btnY = h - 125;
            var totalBtnWidth = Buttons.Sum(x => x.Width);
            int runningWidth = 0;
            int start = 0;
            int i = 0;
            for (i=0; i<Buttons.Count; i++)
            {
                var btn = Buttons[i];
                
                if (runningWidth == 0 || (Width-50) - runningWidth > btn.Width)
                {
                    btn.X = btnX + runningWidth;
                } else
                {
                    btnY += 120;
                    h += 120;
                    //center buttons
                    runningWidth -= 25;
                    for (int j=start; j<i; j++)
                    {
                        Buttons[j].X -= runningWidth / 2;
                    }

                    runningWidth = 0;
                    start = i;
                    btn.X = btnX + runningWidth;
                }
                runningWidth += (int)btn.Width + 25;
                if (setY) btn.Y = btnY;
            }
            runningWidth -= 25;
            for (int j = start; j < i; j++)
            {
                Buttons[j].X -= runningWidth / 2;
            }
            return h;
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
        public override void CalculateMatrix()
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
                ResetButtons(Height, false);
                foreach (var btn in Buttons)
                {
                    btn.X += off;
                    btn.Visible = true;
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
