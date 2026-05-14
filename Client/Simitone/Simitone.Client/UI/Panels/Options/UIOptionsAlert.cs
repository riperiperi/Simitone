using FSO.Client;
using Simitone.Client.UI.Controls;
using System;

namespace Simitone.Client.UI.Panels.Options
{
    public class UIOptionsAlert : UIMobileDialog
    {        
        public UIOptionsPanel OptTabViewer;

        public UIBigButton OKButton { get; }

        public event Action<short> OnResult;

        /// <summary>
        /// Creates a new <see cref="UIOptionsAlert"/> that will display the current state of each setting on the
        /// <paramref name="SettingsObjectInstance"/> provided.
        /// <para/>Should be of type <see cref="GlobalSettings"/> for now.
        /// </summary>
        /// <param name="SettingsObjectInstance"></param>
        public UIOptionsAlert(object SettingsObjectInstance) : base()
        {
            //set title text to default value, selected tab page changes title
            string title = "Options";

            Caption = title;
            SetHeight(400);

            OptTabViewer = new UIOptionsPanel(SettingsObjectInstance);
            //GfxPanel.OnResult += (res) => { OnResult?.Invoke(res); Close(); };
            Add(OptTabViewer);
            OptTabViewer.SetTab();

            OKButton = new UIBigButton(true)
            {
                Caption = "OK"
            };
            OKButton.OnButtonClick += (elem) =>
            {
                //OnResult?.Invoke(GfxPanel.GetSelectedOption());
                Close();
            };
            Add(OKButton);

            GameResized();
        }

        public override void GameResized()
        {
            base.GameResized();
            OptTabViewer.Position = new Microsoft.Xna.Framework.Vector2((Width - 1030) / 2, 90);
            OKButton.Position = new((Width / 2) - (OKButton.Width / 2), Height - 10 - OKButton.Size.Y);
        }
    }
}
