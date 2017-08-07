using FSO.Client.UI.Controls;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simitone.Client.UI.Controls
{
    public class UITwoStateButton : UIButton
    {
        public UITwoStateButton(Texture2D tex) : base(tex)
        {
            ImageStates = 2;
        }
    }
}
