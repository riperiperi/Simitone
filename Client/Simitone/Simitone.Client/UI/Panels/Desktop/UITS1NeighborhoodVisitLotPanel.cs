using FSO.Client;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Common.Rendering.Framework.Model;
using Microsoft.Xna.Framework;
using Simitone.Client.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simitone.Client.UI.Panels.Desktop
{
    //added by bisquick 5/5/2026

    /// <summary>
    /// Panel that shows the <see cref="UINeighborhoodSelectionPanel"/> in the context of visiting a lot as opposed to playing in one
    /// </summary>
    internal class UITS1NeighborhoodVisitLotPanel : UIContainer
    {
        private const string Title_StringTable = "212"; // 212 Go Downtown Dialog Strings

        private readonly ushort neighborhoodMode;
        private UIDiagonalStripe captionBg;
        private UILabel caption;

        private UINeighborhoodSelectionPanel neighborhoodView { get; set; }

        public event Action<int> OnHouseSelect;

        public UITS1NeighborhoodVisitLotPanel(ushort neighborhoodMode) : base()
        {
            this.neighborhoodMode = neighborhoodMode;
            create();
        }

        void create()
        {
            //neighborhood panel
            neighborhoodView = new UINeighborhoodSelectionPanel(neighborhoodMode, new()
            {
                PrimaryButtonText = "Visit Lot",
                MoreButtonEnabled = false,
                LotZoneExclusionFilter = { 0 } // exclude residential lots since you can't visit those
            });
            neighborhoodView.OnHouseSelect += (id) => OnHouseSelect?.Invoke(id);
            Add(neighborhoodView);

            captionBg = new UIDiagonalStripe(new Point(0, 50), UIDiagonalStripeSide.RIGHT, Color.Black * 0.5f)
            {
                Position = new Vector2(0, 10)
            };
            Add(captionBg);
            caption = new UILabel()
            {
                Caption = getTitle(), 
                Position = new Vector2(20, 20),
            };
            caption.CaptionStyle.Size = 18;
            caption.CaptionStyle.Color = Color.White;
            Add(caption);
        }

        string getTitle()
        {
            int id = Random.Shared.Next(0, 3); // three strings in this table .. assume any new ones may be for another dialog added later
            return GameFacade.Strings.GetString(Title_StringTable, id.ToString()) ?? "***MISSING***"; // should not be null, but just in case ;)
        }

        public void Awake()
        {            
            //do initial control placement now ... Awake() will start all tween animations
            GameResized();
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);
        }

        public override void GameResized()
        {
            base.GameResized();

            // position when sizing changes

            var gameScr = GameFacade.Screens.CurrentUIScreen;
            captionBg.BodySize = new Point(gameScr.ScreenWidth, captionBg.BodySize.Y);
            caption.Caption = getTitle(); // todo: use UIText entry for this so it is localised // bisquick
            var desiredSize = caption.CaptionStyle.MeasureString(caption.Caption);
            caption.Position = new Vector2((gameScr.ScreenWidth / 2) - (desiredSize.X / 2), 20);            
        }
    }
}
