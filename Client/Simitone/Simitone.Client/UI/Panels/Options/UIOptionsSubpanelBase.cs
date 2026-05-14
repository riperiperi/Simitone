using FSO.Client;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Common;
using FSO.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simitone.Client.UI.Screens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Simitone.Client.UI.Panels.Options
{
    public abstract class UIOptionsSubpanelBase : UIContainer
    {
        internal const string OptionsTable = "145", LowTitle = "39", MedTitle = "40", HighTitle = "41";
        
        public enum SettingType
        {
            None,
            Variable,
            ToggleVariable,
            BoolProp,

        }

        public struct UIOptionLocalizationDescription
        {
            public string TitleID;
            public string AboutTitleTextID;
            public string AboutDescriptionID;

            public UIOptionLocalizationDescription(string titleID, string aboutTitleTextID, string aboutDescriptionID)
            {
                TitleID = titleID;
                AboutTitleTextID = aboutTitleTextID;
                AboutDescriptionID = aboutDescriptionID;
            }
        }

        public abstract class UIOptionDescriprion
        {
            public abstract SettingType Type { get; }
            public PropertyInfo AttachedProperty { get; }
            public UIOptionLocalizationDescription? LocalizationInfo { get; set; }

            protected UIOptionDescriprion(PropertyInfo AttachedProperty)
            {
                this.AttachedProperty = AttachedProperty;
            }
            /// <summary>
            /// Convert To is used to convert from the value assigned to the RadioButton (e.g. LowBound -> Highbound) to the value of the setting.
            /// </summary>
            public Func<object, object> ConvertTo;
            /// <summary>
            /// Convert From is used to convert from the value of the setting to the value assigned to the RadioButton (e.g. LowBound -> Highbound)
            /// </summary>
            public Func<object, object> ConvertFrom;

            public object GetValue(object Instance)
            {
                var value = AttachedProperty.GetValue(Instance);
                return ConvertFrom != null ? ConvertFrom(value) : value;
            }

            public void SetValue(object Instance, object value)
            {
                value = ConvertTo != null ? ConvertTo(value) : value;
                AttachedProperty.SetValue(Instance, value);
            }

            public string GetTitle() =>
                LocalizationInfo.HasValue ? GameFacade.Strings.GetString(OptionsTable, LocalizationInfo.Value.TitleID) : AttachedProperty?.Name ?? "No Name";

            public string GetAboutInfo() =>
                LocalizationInfo.HasValue ? $"{GameFacade.Strings.GetString(OptionsTable, LocalizationInfo.Value.AboutTitleTextID)}\n" +
                $"{GameFacade.Strings.GetString(OptionsTable, LocalizationInfo.Value.AboutDescriptionID)}" : null;
        }

        public class UIBoolPropOptionDescription : UIOptionDescriprion
        {
            public override SettingType Type => SettingType.BoolProp;
            public UIBoolPropOptionDescription(PropertyInfo AttachedProperty) : base(AttachedProperty)
            {
                if (AttachedProperty.PropertyType != typeof(bool)) throw new ArgumentException("Property must be of type bool");
            }

            public UIBoolPropOptionDescription(PropertyInfo AttachedProperty, Func<object, object> ConvertTo, Func<object, object> ConvertFrom) : base(AttachedProperty)
            {
                this.ConvertTo = ConvertTo;
                this.ConvertFrom = ConvertFrom;
            }            
        }

        public class UIIntPropOptionDescription : UIOptionDescriprion
        {
            public override SettingType Type => SettingType.Variable;

            public int LowBound { get; }
            public int Highbound { get; }

            public UIIntPropOptionDescription(PropertyInfo AttachedProperty, int LowBound = 0, int Highbound = 2) : base(AttachedProperty)
            {
                if (!AttachedProperty.PropertyType.IsEnum)
                    if (AttachedProperty.PropertyType != typeof(int)) throw new ArgumentException("Property must be of type int");
                this.LowBound = LowBound;
                this.Highbound = Highbound;
            }
        }

        public class UITogglableIntPropOptionDescription : UIIntPropOptionDescription
        {
            public override SettingType Type => SettingType.ToggleVariable;

            public PropertyInfo ToggleProperty { get; }

            public UITogglableIntPropOptionDescription(PropertyInfo AttachedProperty, PropertyInfo ToggleProperty, int LowBound = 0, int Highbound = 2) :
                base(AttachedProperty, LowBound, Highbound)
            {
                Debug.Assert(AttachedProperty.PropertyType == typeof(int), "Toggle property must be of type int");
                Debug.Assert(ToggleProperty.PropertyType == typeof(bool), "Toggle property must be of type bool");
                this.ToggleProperty = ToggleProperty;
            }
        }

        IEnumerable<UIOptionDescriprion> settings => GetSettingsDescriptors();
        protected abstract IEnumerable<UIOptionDescriprion> GetSettingsDescriptors();

        public Texture2D CheckboxTexture { get; }
        public Texture2D RadioTexture { get; }

        protected float Width => Parent?.Size.X ?? 800;
        private string LocalLowText, LocalMedText, LocalHighText;

        protected object SettingsObjectInstance { get; }

        protected UIOptionsSubpanelBase(object SettingsObjectInstance) : base()
        {
            CheckboxTexture = Content.Get().CustomUI.Get("check.png").Get(GameFacade.GraphicsDevice);
            RadioTexture = Content.Get().CustomUI.Get("radio.png").Get(GameFacade.GraphicsDevice);

            LocalLowText = GameFacade.Strings.GetString(OptionsTable, LowTitle);
            LocalMedText = GameFacade.Strings.GetString(OptionsTable, MedTitle);
            LocalHighText = GameFacade.Strings.GetString(OptionsTable, HighTitle);

            this.SettingsObjectInstance = SettingsObjectInstance;
            CreateSettings();            
        }
         
        /// <summary>
        /// Clears all controls and redraws controls from the provided <see cref="GetSettingsDescriptors"/> descriptors.
        /// </summary>
        protected void CreateSettings()
        {
            int COLUMNS = 2, column1Y = 50, column2Y = 50;
            int COLX(int c) => ((int)(Width / 2) * c) + 30;
            string GetString(string id) => GameFacade.Strings.GetString(OptionsTable, id);

            bool any = false;

            Children.Clear(); // any controls from prior session should be removed.

            if (settings.Any(x => x.Type == SettingType.BoolProp))
            {
                CreateColumnHeader("Toggle Settings", COLX(0), 10);
                any = true;
            }
            if (settings.Any(x => x.Type != SettingType.BoolProp))
            {
                CreateColumnHeader($"Description", COLX(1), 10);
                CreateColumnHeader($"{GetString(LowTitle)}/{GetString(MedTitle)}/{GetString(HighTitle)}", COLX(1) + 260, 14, 12);
                any = true;
            }
            if (!any)
            {
                CreateColumnHeader("There are no settings here.", COLX(0), 10);
                return;
            }

            foreach (var setting in settings.OrderByDescending(x => x.Type))
            {
                int ColumnX = COLX((setting.Type == SettingType.BoolProp) ? 0 : 1); // ON/OFF on right
                switch (setting.Type)
                {
                    case SettingType.BoolProp:
                        {
                            CreateCheckBox(setting as UIBoolPropOptionDescription, ColumnX, column2Y);
                            column2Y += 25;
                        }
                        continue;
                    case SettingType.ToggleVariable:
                        {
                            CreateToggleRadios(setting as UITogglableIntPropOptionDescription, ColumnX, column1Y);
                        }
                        break;
                    case SettingType.Variable:
                        {
                            CreateRadios(setting as UIIntPropOptionDescription, ColumnX, column1Y);
                        }
                        break;
                }
                // ** not boolprop from beyond this point
                column1Y += 25;
            }
            //allow derived classes to update their layout 
            OnControlsPopulated(new Rectangle(COLX(0),00,(int)(Width / (column2Y!=50?1:2)),Math.Max(column1Y,column2Y)));
        }

        /// <summary>
        /// This method is called after <see cref="CreateSettings"/> is called.
        /// <para/>See: <see cref="CreateSettings"/>:
        /// <para/>
        /// <paramref name="ReservedSpace"/> is the region of the control that is occupied by generated controls, please add controls outside this area
        /// if manually creating controls.
        /// </summary>
        protected virtual void OnControlsPopulated(Rectangle ReservedSpace)
        {
            ;
        }

        private void CreateColumnHeader(string Header, int ColumnX, int ColumnY, int FontSize = 18)
        {
            var label = new UILabel()
            {
                Caption = Header,
                CaptionStyle = TextStyle.DefaultTitle.Clone(),
                Position = new Microsoft.Xna.Framework.Vector2(ColumnX, ColumnY)
            };
            label.CaptionStyle.Size = FontSize;
            Add(label);
        }

        private (UILabel Label, UIButton Button) CreateCheckBox(UIBoolPropOptionDescription Descriptor, int ColumnX, int ColumnY)
        {
            // get current setting state using reflection
            UIBoolPropOptionDescription setting = Descriptor;
            bool value = (bool)Descriptor.GetValue(GlobalSettings.Default);

            //get title
            string Caption = Descriptor.GetTitle();

            //create checkbox and label
            int left = 200 + ColumnX;
            var check = new UIButton(CheckboxTexture)
            {
                Position = new Microsoft.Xna.Framework.Vector2(left, ColumnY),
                Tooltip = Descriptor.GetAboutInfo(),
                Selected = value
            };
            // ** set new setting value on click
            check.OnButtonClick += (elem) =>
            {
                Descriptor.SetValue(SettingsObjectInstance, !check.Selected);
                OnSettingsChanged(Descriptor);
            };
            Add(check);

            var label = new UILabel()
            {
                Caption = Caption,
                Position = new Microsoft.Xna.Framework.Vector2(ColumnX, ColumnY),
                Tooltip = Descriptor.GetAboutInfo()
            };
            Add(label);
            return (label, check);
        }

        private void CreateToggleRadios(UITogglableIntPropOptionDescription ToggleSetting, int ColumnX, int ColumnY)
        {
            int left = 200 + ColumnX, margin = 60, labelLeft = ColumnX;

            // get current setting state using reflection
            UITogglableIntPropOptionDescription setting = ToggleSetting;
            bool enabled = (bool)setting.ToggleProperty.GetValue(GlobalSettings.Default);
            int value = (int)setting.GetValue(GlobalSettings.Default);

            //get title
            string Caption = ToggleSetting.GetTitle();

            // ON/OFF TOGGLE
            var check = new UIButton(CheckboxTexture)
            {
                Position = new(left, ColumnY),
                Tooltip = "On/Off",
                Selected = enabled
            };
            // set the ON/OFF setting value on click
            check.OnButtonClick += (elem) =>
            {
                ToggleSetting.ToggleProperty.SetValue(SettingsObjectInstance, !check.Selected);
                OnSettingsChanged(ToggleSetting);
            };
            Add(check);

            //RADIOs
            for (int i = setting.LowBound; i <= setting.Highbound; i++)
            {
                var radio = new UIButton(RadioTexture)
                {
                    Position = new(left + (margin * ((i - setting.LowBound) + 1)), ColumnY),
                    Disabled = !enabled,
                    Tooltip = (i == setting.LowBound) ? LocalLowText : (i == setting.Highbound) ? LocalHighText : LocalMedText,
                    Selected = value == i,
                    ID = i.ToString(), // store the value here
                };
                // set the variable value equal to the current RadioButton value on click
                radio.OnButtonClick += (elem) =>
                {
                    int valueToApply = int.Parse(elem.ID);
                    setting.SetValue(SettingsObjectInstance, valueToApply);
                    OnSettingsChanged(ToggleSetting);
                };
                Add(radio);
            }
            var label = new UILabel()
            {
                Position = new Microsoft.Xna.Framework.Vector2(labelLeft, ColumnY),
                Caption = Caption,
                Tooltip = setting.GetAboutInfo()
            };
            Add(label);
        }

        private void CreateRadios(UIIntPropOptionDescription ToggleSetting, int ColumnX, int ColumnY)
        {
            int left = 200 + ColumnX, margin = 60, labelLeft = ColumnX;

            // get current setting state using reflection
            UIIntPropOptionDescription setting = ToggleSetting;
            int value = (int)setting.GetValue(GlobalSettings.Default);

            //get title
            string Caption = ToggleSetting.GetTitle();

            //RADIOs
            for (int i = setting.LowBound; i <= setting.Highbound; i++)
            {
                var radio = new UIButton(RadioTexture)
                {
                    Position = new(left + (margin * ((i - setting.LowBound) + 1)), ColumnY),
                    Tooltip = (i == setting.LowBound) ? LocalLowText : (i == setting.Highbound) ? LocalHighText : LocalMedText,
                    ID = i.ToString(), // store the value here
                    Selected = value == i
                };
                // set the variable value equal to the current RadioButton value on click
                radio.OnButtonClick += (elem) =>
                {
                    int valueToApply = int.Parse(elem.ID);
                    setting.SetValue(SettingsObjectInstance, valueToApply);
                    OnSettingsChanged(ToggleSetting);
                };
                Add(radio);
            }
            var label = new UILabel()
            {
                Position = new Microsoft.Xna.Framework.Vector2(labelLeft, ColumnY),
                Caption = Caption,
                Tooltip = setting.GetAboutInfo()
            };
            Add(label);
        }

        /// <summary>
        /// When any settings values are changed, this method is called to refresh the UI, but can be overridden to allow additional functionality        
        /// </summary>
        /// <param name="ChangedSettings">List of settings that have changed since the last time this was called.</param>
        protected virtual void OnSettingsChanged(params UIOptionDescriprion[] ChangedSettings)
        {
            if (SettingsObjectInstance is not GlobalSettings settings) return;

            for (int i = 0; i < ChangedSettings.Length; i++)
                OnSettingVerifying(ChangedSettings[i]); // show any warnings now.

            //save settings to file
            settings.Save();

            //set tex compression
            FSOEnvironment.TexCompress = (settings.TexCompression & 1) > 0;

            FSO.LotView.WorldConfig.Current = new FSO.LotView.WorldConfig()
            {
                LightingMode = settings.LightingMode,
                SmoothZoom = settings.SmoothZoom,
                //SurroundingLots = settings.SurroundingLotMode,
                AA = settings.AntiAlias,
                Weather = settings.Weather,
                Directional = settings.DirectionalLight3D,
                Complex = settings.ComplexShaders,
                EnableTransitions = settings.EnableTransitions
            };

            var vm = (GameFacade.Screens.CurrentUIScreen as TS1GameScreen)?.vm;
            if (vm != null)
                vm.Context.World.ChangedWorldConfig(GameFacade.GraphicsDevice);            

            CreateSettings();
        }

        /// <summary>
        /// Before a setting is applied in-game, this method can be used to show the user information about the setting they changed, 
        /// like a potential side-effect or recommendation to restart.
        /// </summary>
        /// <param name="VerifyOption">The option being verified.</param>
        protected virtual void OnSettingVerifying(UIOptionDescriprion VerifyOption)
        {

        }
    }
}
