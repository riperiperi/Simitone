using FSO.Client;
using FSO.Client.UI.Framework;
using FSO.Content;
using Microsoft.Xna.Framework;
using Simitone.Client.UI.Controls;

namespace Simitone.Client.UI.Panels.Options
{
    public class UIOptionsPanel : UIContainer
    {
        private static int SELECTED_PAGE = 1;

        private const string OptionsTable = UIOptionsSubpanelBase.OptionsTable,
            GFXTitle = "7", PlayTitle = "11", SoundTitle = "9";
        
        private readonly object settingsObject;

        public UIStencilButton GeneralTabButton { get; }
        public UIStencilButton GraphicsTabButton { get; }
        public UIStencilButton AudioTabButton { get; }
        public UIContainer SubpanelContent { get; private set; }

        /// <summary>
        /// Creates a new <see cref="UIOptionsPanel"/> attached to the <paramref name="SettingsObject"/> which is used
        /// to get the current state of each setting, and change the state of the setting on that instance
        /// </summary>
        /// <param name="SettingsObject"></param>
        public UIOptionsPanel(object SettingsObject)
        {
            var ui = Content.Get().CustomUI;
            var gd = GameFacade.GraphicsDevice;

            GeneralTabButton = new UIStencilButton(ui.Get("opt_general.png").Get(gd))
            {
                Tooltip = GameFacade.Strings.GetString(OptionsTable, PlayTitle)
            };
            GeneralTabButton.OnButtonClick += (btn) => { SetTab(0); };
            Add(GeneralTabButton);

            GraphicsTabButton = new UIStencilButton(ui.Get("opt_gfx.png").Get(gd))
            {
                Tooltip = GameFacade.Strings.GetString(OptionsTable, GFXTitle)
            };
            GraphicsTabButton.OnButtonClick += (btn) => { SetTab(1); };
            Add(GraphicsTabButton);

            AudioTabButton = new UIStencilButton(ui.Get("opt_audio.png").Get(gd))
            {
                Tooltip = GameFacade.Strings.GetString(OptionsTable, SoundTitle)
            };
            AudioTabButton.OnButtonClick += (btn) => { SetTab(2); };
            Add(AudioTabButton);
            
            settingsObject = SettingsObject;
            //SetTab(SELECTED_PAGE);
        }

        private void SetTitle(string Title)
        {
            if (Parent == null) return;
            if (Parent is UIMobileDialog dlg)
                dlg.Caption = Title;
        }

        /// <summary>
        /// Sets the currently viewed tab in the tab viewer.
        /// <para/>Passing -1 will set it to the last opened page.
        /// </summary>
        /// <param name="Page"></param>
        public void SetTab(int Page = -1)
        {
            if (Page == -1)
                Page = SELECTED_PAGE;
            SELECTED_PAGE = Page;

            if (SubpanelContent != null)
            {
                SubpanelContent.Visible = false;
                Children.Remove(SubpanelContent);
            }

            GraphicsTabButton.Selected = AudioTabButton.Selected = GeneralTabButton.Selected = false;

            switch (Page)
            {
                case 0:
                    SetTitle(GameFacade.Strings.GetString(OptionsTable,PlayTitle));
                    GeneralTabButton.Selected = true;
                    SubpanelContent = new UIGeneralOptionsSubpanel(settingsObject);
                    break;
                case 1:
                    SetTitle(GameFacade.Strings.GetString(OptionsTable, GFXTitle));
                    GraphicsTabButton.Selected = true;
                    SubpanelContent = new UIGraphicsOptionsSubpanel(settingsObject);
                    break;
                case 2:
                    SetTitle(GameFacade.Strings.GetString(OptionsTable, SoundTitle));
                    AudioTabButton.Selected = true;
                    SubpanelContent = new UIAudioOptionsSubpanel(settingsObject);
                    break;
            }

            if (SubpanelContent == null) return;
            Add(SubpanelContent);

            GameResized();
        }

        public override void GameResized()
        {
            base.GameResized();

            int left = 40;

            GeneralTabButton.Position = new Vector2(left + 0,0);
            AudioTabButton.Position = new Vector2(left, 186);
            GraphicsTabButton.Position = new Vector2(left, 93);
            if (SubpanelContent != null)
                SubpanelContent.Position = new Vector2(113, 0);
        }
    }
}
