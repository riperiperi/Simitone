using FSO.Client;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Content;
using Simitone.Client.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simitone.Client.UI.Panels
{
    public class UINeighbourhoodSwitcher : UIContainer
    {
        public List<UIElasticButton> LeftBtns = new List<UIElasticButton>();
        public List<UIElasticButton> RightBtns = new List<UIElasticButton>();
        private UINeighborhoodSelectionPanel Panel;
        private ushort Mode;
        public bool MoveInMode;

        public UINeighbourhoodSwitcher(UINeighborhoodSelectionPanel panel, ushort mode, bool moveIn)
        {
            Panel = panel;
            SetMode(mode, moveIn);            
        }

        public void SetMode(ushort mode, bool moveIn)
        {
            //wipes to a new mode with a transition
            UITransDialog WithPresetTransition(string icon, Action action) => new UITransDialog(icon, action);

            MoveInMode = moveIn;
            foreach (var btn in LeftBtns) Remove(btn);
            foreach (var btn in RightBtns) Remove(btn);
            LeftBtns.Clear(); RightBtns.Clear();

            AddBtn(LeftBtns, "ngbh_cas.png", (btn) =>
            {
                WithPresetTransition("cas", () =>
                 {
                     GameController.EnterCAS();
                 });
            });
            if (mode != 4) AddBtn(LeftBtns, "ngbh_back.png", (btn) => PopMode("trans_normal.png",4));

            if (mode != 2 && !moveIn) AddBtn(RightBtns, "ngbh_downt.png", (btn) => PopMode("ngbh_downt.png",2));
            if (mode != 3 && !moveIn) AddBtn(RightBtns, "ngbh_vacat.png", (btn) => PopMode("ngbh_vacat.png",3));
            if (mode != 5 && !moveIn) AddBtn(RightBtns, "ngbh_studio.png", (btn) => PopMode("ngbh_studio.png",5));
            if (mode != 7) AddBtn(RightBtns, "ngbh_magic.png", (btn) => PopMode("ngbh_magic.png", 7));
            Mode = mode;

            LayBtns();
        }

        public void PopMode(string neighborhoodIcon, ushort mode)
        {
            UITransDialog WithTransition(string icon, Action action) => UITransDialog.CreateWithCustomUIIcon(icon, action);
            WithTransition(neighborhoodIcon, () =>
            {
                Panel.PopulateScreen(mode);
                SetMode(mode, MoveInMode);
            });
        }

        private void LayBtns()
        {
            int i = 0;
            foreach (var btn in LeftBtns)
            {
                btn.Position = new Microsoft.Xna.Framework.Vector2(17+48, 14+110*i + 48);                
                i++;
            }

            i = 0;
            foreach (var btn in RightBtns)
            {
                btn.Position = new Microsoft.Xna.Framework.Vector2(UIScreen.Current.ScreenWidth - (17+96) + 48, 14 + 110 * i + 48);
                i++;
            }
        }

        public override void GameResized()
        {
            base.GameResized();
            LayBtns();
        }

        private UIElasticButton AddBtn(List<UIElasticButton> targ, string imgname, ButtonClickDelegate onClick)
        {
            var ui = Content.Get().CustomUI;
            var btn = new UIElasticButton(ui.Get(imgname).Get(GameFacade.GraphicsDevice))
            {
                ElementShadow = new UIShadow(ui.Get("ngbh_shadow.png").Get(GameFacade.GraphicsDevice)) {
                    ShadowDepth = 5
                }
            };
            btn.OnButtonClick += onClick;
            targ.Add(btn);
            Add(btn);            
            return btn;
        }
    }
}
