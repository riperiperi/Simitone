using FSO.Client;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Content;
using Simitone.Client.UI.Model;
using Simitone.Client.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simitone.Client.UI.Controls
{
    public class UIBigButton : UIButton
    {
        float _mySize = 0;
        string _ghostText = string.Empty;

        public UIBigButton(bool green) : base()
        {
            bool Desktop = (UIScreen.Current as TS1GameScreen).Desktop;
            string texRef = Desktop ? (green ? "greenbutton.png" : "button.png") : (green ? "greenbutton.mobile.png" : "button.mobile.png");

            CaptionStyle = CaptionStyle.Clone();
            CaptionStyle.Size = Desktop ? 14 : 37;
            CaptionStyle.Color = green ? UIStyle.Current.GreenBtnTxt : UIStyle.Current.BtnTxt;
            CaptionStyle.DisabledColor = UIStyle.Current.BtnDisable;
            CaptionStyle.HighlightedColor = green ? UIStyle.Current.BtnPrimaryHighlight : UIStyle.Current.BtnNormalHighlight;
            Texture = Content.Get().CustomUI.Get(texRef).Get(GameFacade.GraphicsDevice);               
        }

        private void SetGhostText()
        {
            _ghostText = string.Empty;
            var desiredSize = CaptionStyle.MeasureString(Caption);
            if (desiredSize.X > Width)
            {
                //truncate
                float overage = desiredSize.X - Width;
                float charSize = desiredSize.X / Caption.Length;
                int trimChars = 4 + (int)(overage / charSize); // 3 '.' plus truncating error correction
                int totalChars = Math.Max(0, Caption.Length - trimChars);
                _ghostText = Caption.Substring(0, totalChars) + "...";
            }
        }
        public override void Draw(UISpriteBatch SBatch)
        {
            if (_mySize != Width) // on size changed invalidate size changed
            {
                SetGhostText();
                _mySize = Width;
            }

            string caption = Caption;
            if (!string.IsNullOrWhiteSpace(_ghostText)) 
                Caption = _ghostText;

            base.Draw(SBatch);

            caption = Caption;
        }
    }
}
