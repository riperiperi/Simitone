using FSO.Client;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Common.Utils;
using FSO.Content;
using FSO.Files.Formats.IFF.Chunks;
using Microsoft.Xna.Framework;
using Simitone.Client.UI.Controls;
using Simitone.Client.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simitone.Client.UI.Panels
{
    public class UIHouseSelectPanel : UIContainer
    {
        public UIDiagonalStripe Diag;
        public UIDiagonalStripe TitleStripe;

        public UILabel StreetTitle;
        public UILabel LotTitle;
        public UILabel LotDescription;

        public UIBigButton EnterLot;
        public UIBigButton More;

        public event Action<int> OnSelected;
        public int HouseID;

        public UIHouseSelectPanel(int houseID)
        {
            HouseID = houseID;
            var screen = GameFacade.Screens.CurrentUIScreen;
            Diag = new UIDiagonalStripe(new Point(screen.ScreenWidth / 2, screen.ScreenHeight + 16), UIDiagonalStripeSide.RIGHT, UIStyle.Current.Bg);
            Diag.Y = -16;
            Diag.ListenForMouse(Diag.GetBounds(), (e, s) => { });
            Add(Diag);

            TitleStripe = new UIDiagonalStripe(new Point(screen.ScreenWidth / 2, 92 + 8 + 32), UIDiagonalStripeSide.RIGHT, UIStyle.Current.Bg);
            TitleStripe.StartOff = 8 + 32;
            TitleStripe.Y = 82 - 34;
            Add(TitleStripe);
            
            var house = Content.Get().Neighborhood.GetHouse(houseID);

            var street = Content.Get().Neighborhood.StreetNames;
            var assignment = street.Get<STR>(2001).GetString(houseID-1);

            int streetName;
            if (int.TryParse(assignment, out streetName))
            {
                StreetTitle = new UILabel();
                StreetTitle.Position = new Vector2(30, 94);
                InitLabel(StreetTitle);
                StreetTitle.CaptionStyle.Color = UIStyle.Current.BtnActive;
                StreetTitle.Caption = street.Get<STR>(2000).GetString(streetName-1).Replace("%s", houseID.ToString());
            }

            var nameDesc = Content.Get().Neighborhood.GetHouseNameDesc(houseID);
            var name = nameDesc.Item1;
            if (name == "") name = "Unnamed House";

            LotTitle = new UILabel();
            LotTitle.Position = new Vector2(30, 122);
            InitLabel(LotTitle);
            LotTitle.CaptionStyle.Size = 37;
            LotTitle.Caption = name;

            LotDescription = new UILabel();
            LotDescription.Position = new Vector2(30, 206);
            InitLabel(LotDescription);
            //LotDescription.CaptionStyle.Size = 15;
            LotDescription.Size = new Vector2(502, screen.ScreenHeight-415);
            LotDescription.Wrapped = true;
            LotDescription.Alignment = TextAlignment.Top | TextAlignment.Left;
            LotDescription.Caption = nameDesc.Item2;

            EnterLot = new UIBigButton(false);
            EnterLot.Caption = "Enter Lot";
            EnterLot.Width = 275;
            EnterLot.Position = new Vector2(30, screen.ScreenHeight - 160);
            EnterLot.OnButtonClick += (b) => { OnSelected?.Invoke(houseID); Kill(); };
            Add(EnterLot);

            More = new UIBigButton(true);
            More.Caption = "More";
            More.Width = 192;
            More.Position = new Vector2(330, screen.ScreenHeight - 160);
            Add(More);

            X = screen.ScreenWidth / -2;
            GameFacade.Screens.Tween.To(this, 0.5f, new Dictionary<string, float>() { { "X", 0f } }, TweenQuad.EaseOut);
        }

        public void Kill()
        {
            EnterLot.Opacity = 0.99f; //force an unpressable state
            More.Opacity = 0.99f;
            var screen = GameFacade.Screens.CurrentUIScreen;
            GameFacade.Screens.Tween.To(this, 0.5f, new Dictionary<string, float>() { { "X", (screen.ScreenWidth / -2) - 32 } }, TweenQuad.EaseIn);
            GameThread.SetTimeout(() => { Parent.Remove(this); }, 500);
        }

        private void InitLabel(UILabel label)
        {
            label.CaptionStyle = label.CaptionStyle.Clone();
            label.CaptionStyle.Color = UIStyle.Current.Text;
            label.CaptionStyle.Size = 19;
            Add(label);
        }
    }
}
