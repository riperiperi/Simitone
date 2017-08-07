using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simitone.Client.UI.Model
{
    public class UIStyle
    {
        public static UIStyle DARK = new UIStyle();
        
        public static UIStyle Current = DARK;


        //class definition

        public Color Bg = Color.Black * 0.75f;
        public Color TitleBg = Color.Black * 0.85f;

        public Color BtnNormal = Color.White;
        public Color BtnActive = new Color(0, 255, 128, 255);
        public Color BtnDisable = new Color(128, 128, 128, 255);

        public Color ActiveSelection = Color.Yellow;

        public Color Text = Color.White;
        public Color SecondaryText = new Color(0, 255, 128, 255);

        public Color DialogBg = Color.Black * 0.8f;
        public Color DialogText = Color.White;
        public Color DialogTitle = Color.Black;

        public Color BtnTxt = new Color(0, 31, 63);
        public Color GreenBtnTxt = new Color(0, 63, 16);
        public Color BtnTxtShadow = Color.White * 0.5f;

        public Color PosMoney = new Color(0, 255, 128, 255);
        public Color NegMoney = new Color(255, 128, 0, 255);

        public Color SkillInactive = new Color(99, 109, 242, 255);
        public Color SkillActive = new Color(0, 255, 255, 255);
        public Color SkillNeeded = new Color(255, 191, 0, 255);
    }
}
