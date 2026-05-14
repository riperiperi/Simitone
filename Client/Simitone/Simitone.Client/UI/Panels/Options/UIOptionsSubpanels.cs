using FSO.Client;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Common;
using FSO.Content;
using FSO.Files.Formats.IFF.Chunks;
using FSO.LotView.Model;
using Microsoft.Xna.Framework;
using Simitone.Client.UI.Controls;
using Simitone.Client.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Simitone.Client.UI.Panels.Options
{

    public class UIGraphicsOptionsSubpanel : UIOptionsSubpanelBase
    {
        private const string AATitle = "14", AboutAA = "15", AADesc = "16",
            ShadowTitle = "17", AboutShadows = "18", ShadowDesc = "19",
            LightTitle = "20", AboutLight = "21", LightDesc = "22";

        public UIGraphicsOptionsSubpanel(object SettingsObject) : base(SettingsObject)
        {
            
        }

        protected override IEnumerable<UIOptionDescriprion> GetSettingsDescriptors() => [
                    new UIIntPropOptionDescription(typeof(GlobalSettings).GetProperty("AntiAlias"), 0, 2){
                        LocalizationInfo = new(AATitle, AboutAA, AADesc)
                    },
                    new UIBoolPropOptionDescription(typeof(GlobalSettings).GetProperty("EnableTransitions")) { },
                    new UIBoolPropOptionDescription(typeof(GlobalSettings).GetProperty("Weather")) { },
                    new UIBoolPropOptionDescription(typeof(GlobalSettings).GetProperty("SmoothZoom")) { },
                    new UIBoolPropOptionDescription(typeof(GlobalSettings).GetProperty("DirectionalLight3D")) { },
                    new UIBoolPropOptionDescription(typeof(GlobalSettings).GetProperty("ComplexShaders")) { },
                    new UIBoolPropOptionDescription(typeof(GlobalSettings).GetProperty("TexCompression"),
                        (obj) => (((((GlobalSettings)SettingsObjectInstance).TexCompression) & 1) ^ 1) | 2, (obj) => FSOEnvironment.TexCompress) { },
                    new UIBoolPropOptionDescription(typeof(GlobalSettings).GetProperty("Windowed")) { },
                    /*new UITogglableIntPropOptionDescription(typeof(GlobalSettings).GetProperty("LightingMode"),
                        typeof(GlobalSettings).GetProperty("Lighting"),0,3){
                        LocalizationInfo = new(LightTitle, AboutLight, LightDesc)
                    },*/
                    new UIIntPropOptionDescription(typeof(GlobalSettings).GetProperty("LightingMode"),0,3)
                    {
                        LocalizationInfo = new(LightTitle, AboutLight, LightDesc)
                    },
                    new UIIntPropOptionDescription(typeof(GlobalSettings).GetProperty("ShadowQuality")){
                        LocalizationInfo = new(ShadowTitle, AboutShadows, ShadowDesc),
                        ConvertTo = (radio) => (int)radio switch { 0 => 512, 1=>1024, 2=>2048, _ => 512 },
                        ConvertFrom = (setting) => ((int)setting) switch { 512 => 0, 1024 => 1, 2048 => 2, _ => 0 }
                    },
                    new UIIntPropOptionDescription(typeof(GlobalSettings).GetProperty("GlobalGraphicsMode"), (int)GlobalGraphicsMode.Full2D, (int)GlobalGraphicsMode.Full3D) {
                        ConvertTo = (radio) => (GlobalGraphicsMode)((int)radio), // convert to enum
                        ConvertFrom = (setting) => (int)(GlobalGraphicsMode)setting // convert from enum
                    },
                    //new UIIntPropOptionDescription(typeof(GlobalSettings).GetProperty("DPIScaleFactor"), 1, 3),
        ];

        protected override void OnSettingVerifying(UIOptionDescriprion VerifyOption)
        {
            if (VerifyOption.AttachedProperty.Name == "TexCompression") {
                UIAlert alert = null; // show restart alert
                alert = UIScreen.GlobalShowAlert(new UIAlertOptions()
                {
                    Message = GameFacade.Strings.GetString("f103", "25"),
                    Buttons = UIAlertButton.Ok(x => {
                        UIScreen.RemoveDialog(alert);
                    })
                }, true);
            }
            base.OnSettingVerifying(VerifyOption);
        }
    }

    public class UIGeneralOptionsSubpanel : UIOptionsSubpanelBase
    {
        internal const string FreeWillTitle = "59", AboutFreeWill = "60", FreeWillDesc = "61",
            EdgeScrollTitle = "62", AboutEdgeScroll = "63", EdgeScrollDesc = "64";

        private STRLangCode[] hiddenLanguages =
        {
            STRLangCode.Default,
            STRLangCode.Japanese,
            STRLangCode.SimplifiedChinese,
            STRLangCode.TraditionalChinese,
            STRLangCode.Thai,
            STRLangCode.Korean,
            STRLangCode.Slovak            
        };

        public UIGeneralOptionsSubpanel(object SettingsObject) : base(SettingsObject)
        {

        }

        protected override void OnControlsPopulated(Rectangle ReservedSpace)
        {
            base.OnControlsPopulated(ReservedSpace);
            PopulateLanguages(ReservedSpace.Bottom);
        }

        private void PopulateLanguages(int Y)
        {
            int X = 30;
            UILabel titleLabel = new()
            {
                Caption = "Language",
                Position = new(X, Y),
                CaptionStyle = TextStyle.DefaultTitle.Clone()
            };
            titleLabel.CaptionStyle.Size = 18;
            Add(titleLabel);
            Y += 35;            
            STRLangCode[] DataSource = Enum.GetValues<STRLangCode>();
            for(int i = 0; i < DataSource.Length; i++)
            {
                var code = DataSource[i];
                if (hiddenLanguages.Contains(code)) continue;
                UITwoStateButton button = new UITwoStateButton(Content.Get().CustomUI.Get("blank_blue.png").Get(GameFacade.GraphicsDevice))
                {
                    Caption = code.ToString(),
                    Position = new(X, Y)
                };
                Add(button);
                if ((button.Position.X + button.Width) > Width)
                { // every five buttons, nextline
                    Y += (int)button.Size.Y + 10;
                    X = 30;
                    button.Position = new(X, Y);
                }                
                button.OnButtonClick += (elem) => OnTranslationChanging(code);
                X += 10 + (int)button.Width;
            }
        }

        private void OnTranslationChanging(STRLangCode code)
        {
            if ((UIScreen.Current is not TS1GameScreen game)) return;
            game.RuntimeChangeLanguage(code);
            if (SettingsObjectInstance is GlobalSettings settings)
            {
                settings.LanguageCode = (byte)code;
                settings.CurrentLang = Enum.GetName(typeof(STRLangCode), code);
                settings.Save();
            }
            if ((Parent.Parent) is UIMobileDialog dlg)// reload the page in new language
                dlg.Close();
        }

        protected override IEnumerable<UIOptionDescriprion> GetSettingsDescriptors() => [
                    new UIBoolPropOptionDescription(typeof(GlobalSettings).GetProperty("EdgeScroll")){
                        LocalizationInfo = new(EdgeScrollTitle,AboutEdgeScroll,EdgeScrollDesc)
                    },
        ];
    }

    public class UIAudioOptionsSubpanel : UIOptionsSubpanelBase
    {
        public UIAudioOptionsSubpanel(object SettingsObject) : base(SettingsObject)
        {

        }

        protected override IEnumerable<UIOptionDescriprion> GetSettingsDescriptors() => [

        ];
    }
}
