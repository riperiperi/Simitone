using FSO.Client;
using FSO.Client.GameContent;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Client.UI.Model;
using FSO.Common.Rendering.Framework.Model;
using FSO.SimAntics;
using FSO.SimAntics.NetPlay.Model.Commands;
using FSO.SimAntics.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simitone.Client.UI.Panels
{
    public class UICheatTextbox : UIContainer
    {
        private Dictionary<string, VMCheatContext.VMCheatType> cheatDefinitions = new Dictionary<string, VMCheatContext.VMCheatType>()
        {
            { "moveobjects", VMCheatContext.VMCheatType.MoveObjects},
            { "motherlode", VMCheatContext.VMCheatType.Budget },
            { "klapaucius", VMCheatContext.VMCheatType.Budget },
            { "rosebud", VMCheatContext.VMCheatType.Budget },
            // gives the user the submitted amount of money
            { "giveMoney", VMCheatContext.VMCheatType.Budget }
        };

        private UITextBox baseTextbox;
        private Texture2D baseTexture;
        private VM ts1VM;

        /// <summary>
        /// An empty UICheatTextbox
        /// </summary>
        public UICheatTextbox(FSO.SimAntics.VM vm) : this(vm, "")
        {
            
        }
        /// <summary>
        /// A UICheatTextbox with text
        /// </summary>
        /// <param name="initialText"></param>
        public UICheatTextbox(FSO.SimAntics.VM vm, string initialText)
        {
            ts1VM = vm;
            baseTextbox = new UITextBox()
            {
                CurrentText = initialText,
                FlashOnEmpty = true,
                MaxLines = 1,
                //Tooltip = "Cheaters never win",
                //FrameColor = Color.Transparent,
                ID = "UICheatTextboxBase",
                
            };
            baseTexture = new Texture2D(GameFacade.GraphicsDevice, 1, 1);
            baseTexture.SetData(new Color[] { new Color((byte)67, (byte)93, (byte)90, (byte)255) });
            baseTextbox.SetBackgroundTexture(baseTexture, 0, 0, 0, 0);
            Size = new Vector2(200, 23);
            baseTextbox.SetSize(200, 23);
            Add(baseTextbox);
        }
        public override void Update(UpdateState state)
        {
            base.Update(state);
            var pressedKeys = state.KeyboardState.GetPressedKeys();         
            //not sure if ts1 allowed you to use right ctrl or right shift but i dont discriminate
            if ((pressedKeys.Contains(Keys.LeftControl) || pressedKeys.Contains(Keys.RightControl))
                && (pressedKeys.Contains(Keys.LeftShift) || pressedKeys.Contains(Keys.RightShift))
                && state.NewKeys.Contains(Keys.C)) //prevent change over multiple frames
            {
                Visible = !Visible;
            }
            baseTextbox.Visible = Visible;
            if (Visible)
            {
                if (state.NewKeys.Contains(Keys.Enter))
                {
                    commandEntered(baseTextbox.CurrentText, out bool shouldHide);
                    Visible = !shouldHide;
                }
            }               
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandString"></param>
        private void commandEntered(string commandString, out bool shouldHide)
        {
            shouldHide = true;
            if (string.IsNullOrWhiteSpace(commandString)) return; // a blank textbox should close after hitting enter -- even if a command was never run.

            var cheat = new VMNetCheatCmd();
            var context = new VMCheatContext();
            var repetitions = getRepetitions(commandString);
            switch (trimRepetitions(commandString))
            {
                //These three are special cheatcodes that don't really match the parameterized cheats e.g. moveobjects on
                case "klapaucius":
                case "rosebud":
                    context.Amount = (int)VMCheatContext.BudgetCheatPresetAmount.KLAPAUCIUS;
                    context.CheatBehavior = VMCheatContext.VMCheatType.Budget;
                    break;
                case "motherlode":
                    context.Amount = (int)VMCheatContext.BudgetCheatPresetAmount.MOTHERLODE;
                    context.CheatBehavior = VMCheatContext.VMCheatType.Budget;
                    break;
                default: context = parseCommandString(commandString); break;                                       
            }
            cheat.Context = context;
            shouldHide = false;
            if (cheat.Context == null) // the command was not recognized
            {
                FSO.HIT.HITVM.Get().PlaySoundEvent(UISounds.Error); // in TS1 this was a dialog but a sound may be less intrusive
                return;
            }
            context.Repetitions = (byte)repetitions;
            var sndEvent = UISounds.Error;
            if (context.CheatBehavior != VMCheatContext.VMCheatType.InvalidCheat)
            {
                ts1VM.SendCommand(cheat);
                switch (context.CheatBehavior) // sound feedback
                {
                    case VMCheatContext.VMCheatType.Budget: sndEvent = UISounds.BuyPlace; break;
                    default: sndEvent = UISounds.Click; break;
                }
                shouldHide = true;
            }
            FSO.HIT.HITVM.Get().PlaySoundEvent(sndEvent);
        } 

        private String trimRepetitions(string input)
        {
            return input.Replace(";", "").Replace("!", "");
        }

        private int getRepetitions(string input)
        {
            var repetitions = input.Count(x => x == '!');
            if (repetitions == input.Count(x => x == ';'))
                return repetitions;
            return 0;
        }

        /// <summary>
        /// Creates a VMCheatContext which includes repetitions, parameters, and CheatBehavior
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private VMCheatContext parseCommandString(string command)
        {
            if (command.Length == 0)
                return null;
            var baseCmd = command;

            int NumberOfParameters = 1; // in the future if we want multiple parameters per cheat...
            
            string[] parameters = new string[NumberOfParameters];
            if (command.Contains(' ')) {
                baseCmd = command.Substring(0, command.IndexOf(' '));
                string parameterString = command.Substring(command.IndexOf(' ') + 1);
                for (int i = 0; i < NumberOfParameters; i++)
                {
                    var individualParameter = parameterString;
                    if (string.IsNullOrWhiteSpace(individualParameter))
                        break;
                    if (individualParameter.Contains(' '))
                    {
                        individualParameter = individualParameter.Substring(0, individualParameter.IndexOf(' '));
                        parameters[i] = individualParameter;
                        parameterString = parameterString.Substring(parameterString.IndexOf(' ') + 1);
                        continue;
                    }                    
                    parameters[i] = individualParameter;
                    break;
                }
            }
            trimRepetitions(baseCmd);
            if (!cheatDefinitions.TryGetValue(baseCmd, out VMCheatContext.VMCheatType cheatType))
            {
                // cheat not defined in cheatDefinitions
                return null;
            }
            VMCheatContext context = new VMCheatContext()
            {
                CheatBehavior = cheatType,                
            };
            foreach(var parameter in parameters)
            {
                switch (parameter)
                {
                    case "on": context.Modifier = true; break; // set modifier true
                    case "off": context.Modifier = false; break; // set modifer false
                    default:
                        if (int.TryParse(parameter, out int amount)) //check if the parameter is a number
                            context.Amount = amount; // if it is amount is set
                        break;
                }
            }
            return context;
        }        
    }
}
