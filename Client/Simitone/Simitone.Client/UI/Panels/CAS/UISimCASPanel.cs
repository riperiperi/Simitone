using FSO.Client;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Client.UI.Model;
using FSO.Common.Rendering.Framework.IO;
using FSO.Common.Rendering.Framework.Model;
using FSO.Common.Utils;
using FSO.Content;
using FSO.HIT;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simitone.Client.UI.Controls;
using Simitone.Client.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simitone.Client.UI.Panels.CAS
{
    public class UISimCASPanel : UIContainer
    {
        public UILabel FirstNameTitle;
        public UITextBox FirstNameTextBox;
        public UIStencilButton SimTabButton;
        public UIStencilButton PersonalityTabButton;
        public UIStencilButton BioTabButton;

        public UILabel SimGenderTitle;
        public UILabel SimAgeTitle;
        public UILabel SimSkinTitle;
        public UIStencilButton SimMaleBtn;
        public UIStencilButton SimFemaleBtn;
        public UIStencilButton SimAdultBtn;
        public UIStencilButton SimChildBtn;
        public UIStencilButton SimSkinLgtBtn;
        public UIStencilButton SimSkinMedBtn;
        public UIStencilButton SimSkinDrkBtn;
        public UIStencilButton SimRandomBtn;

        public UILabel PerNeatTitle;
        public UILabel PerOutgoingTitle;
        public UILabel PerActiveTitle;
        public UILabel PerPlayfulTitle;
        public UILabel PerNiceTitle;
        private Texture2D PersonalityTex;
        public UICASPersonalityBar[] Personalities = new UICASPersonalityBar[5];

        public UILabel BioTitle;
        public UIImage BioBG;
        public UITextEdit BioEdit;

        public string AType = "ma";
        public string SkinType = "lgt";

        public event Action OnCollectionChange;
        public event Action OnRandom;

        public int ActiveTab;

        public UISimCASPanel()
        {
            var ui = Content.Get().CustomUI;
            var gd = GameFacade.GraphicsDevice;

            SimTabButton = new UIStencilButton(ui.Get("cas_sim.png").Get(gd));
            SimTabButton.Position = new Vector2();
            SimTabButton.OnButtonClick += (btn) => { SetTab(0); };
            Add(SimTabButton);

            PersonalityTabButton = new UIStencilButton(ui.Get("cas_per.png").Get(gd));
            PersonalityTabButton.Position = new Vector2(0, 93);
            PersonalityTabButton.OnButtonClick += (btn) => { SetTab(1); };
            Add(PersonalityTabButton);

            BioTabButton = new UIStencilButton(ui.Get("cas_bio.png").Get(gd));
            BioTabButton.Position = new Vector2(0, 186);
            BioTabButton.OnButtonClick += (btn) => { SetTab(2); };
            Add(BioTabButton);

            FirstNameTitle = new UILabel();
            FirstNameTitle.Caption = "First Name";
            FirstNameTitle.Position = new Vector2(96, 6);
            InitLabel(FirstNameTitle, 15);
            FirstNameTitle.CaptionStyle.Color = UIStyle.Current.SecondaryText;

            FirstNameTextBox = new UITextBox();
            FirstNameTextBox.Position = new Vector2(94, 26);
            FirstNameTextBox.Alignment = TextAlignment.Center;
            FirstNameTextBox.SetSize(401, 48);
            FirstNameTextBox.TextStyle = FirstNameTextBox.TextStyle.Clone();
            FirstNameTextBox.TextStyle.Color = UIStyle.Current.Text;
            FirstNameTextBox.TextStyle.Size = 25;
            Add(FirstNameTextBox);

            // SIM TAB

            SimGenderTitle = new UILabel();
            SimGenderTitle.Caption = "Gender";
            SimGenderTitle.Size = Vector2.One;
            SimGenderTitle.Position = new Vector2(113 + 32, 95);
            SimGenderTitle.Alignment = TextAlignment.Middle | TextAlignment.Center;
            InitLabel(SimGenderTitle, 15);

            SimAgeTitle = new UILabel();
            SimAgeTitle.Caption = "Age";
            SimAgeTitle.Size = Vector2.One;
            SimAgeTitle.Position = new Vector2(212 + 32, 95);
            SimAgeTitle.Alignment = TextAlignment.Middle | TextAlignment.Center;
            InitLabel(SimAgeTitle, 15);

            SimSkinTitle = new UILabel();
            SimSkinTitle.Caption = "Skin Color";
            SimSkinTitle.Size = Vector2.One;
            SimSkinTitle.Position = new Vector2(302 + 45, 95);
            SimSkinTitle.Alignment = TextAlignment.Middle | TextAlignment.Center;
            InitLabel(SimSkinTitle, 15);

            SimMaleBtn = new UIStencilButton(ui.Get("cas_male.png").Get(gd));
            SimMaleBtn.OnButtonClick += (btn) => ChangeType(0, 'm');
            SimMaleBtn.Position = new Vector2(113, 114);
            Add(SimMaleBtn);
            SimFemaleBtn = new UIStencilButton(ui.Get("cas_female.png").Get(gd));
            SimFemaleBtn.OnButtonClick += (btn) => ChangeType(0, 'f');
            SimFemaleBtn.Position = new Vector2(113, 191);
            Add(SimFemaleBtn);

            SimAdultBtn = new UIStencilButton(ui.Get("cas_adult.png").Get(gd));
            SimAdultBtn.OnButtonClick += (btn) => ChangeType(1, 'a');
            SimAdultBtn.Position = new Vector2(212, 114);
            Add(SimAdultBtn);
            SimChildBtn = new UIStencilButton(ui.Get("cas_child.png").Get(gd));
            SimChildBtn.OnButtonClick += (btn) => ChangeType(1, 'c');
            SimChildBtn.Position = new Vector2(212, 191);
            Add(SimChildBtn);

            SimSkinLgtBtn = new UIStencilButton(ui.Get("cas_skinlgt.png").Get(gd));
            SimSkinLgtBtn.OnButtonClick += (btn) => ChangeSkin("lgt");
            SimSkinLgtBtn.Position = new Vector2(302, 114);
            Add(SimSkinLgtBtn);
            SimSkinMedBtn = new UIStencilButton(ui.Get("cas_skinmed.png").Get(gd));
            SimSkinMedBtn.OnButtonClick += (btn) => ChangeSkin("med");
            SimSkinMedBtn.Position = new Vector2(302, 164);
            Add(SimSkinMedBtn);
            SimSkinDrkBtn = new UIStencilButton(ui.Get("cas_skindrk.png").Get(gd));
            SimSkinDrkBtn.OnButtonClick += (btn) => ChangeSkin("drk");
            SimSkinDrkBtn.Position = new Vector2(302, 214);
            Add(SimSkinDrkBtn);

            SimRandomBtn = new UIStencilButton(ui.Get("cas_rand.png").Get(gd));
            SimRandomBtn.Position = new Vector2(405, 142);
            SimRandomBtn.OnButtonClick += SimRandomBtn_OnButtonClick;
            Add(SimRandomBtn);

            //PERSONALITY TAB

            PerNeatTitle = new UILabel();
            PerNeatTitle.Position = new Vector2(99, 85);
            PerNeatTitle.Size = new Vector2(110, 1);
            PerNeatTitle.Alignment = TextAlignment.Right;
            PerNeatTitle.Caption = "Neat:";
            InitLabel(PerNeatTitle, 15);

            PerOutgoingTitle = new UILabel();
            PerOutgoingTitle.Position = new Vector2(99, 122);
            PerOutgoingTitle.Size = new Vector2(110, 1);
            PerOutgoingTitle.Alignment = TextAlignment.Right;
            PerOutgoingTitle.Caption = "Outgoing:";
            InitLabel(PerOutgoingTitle, 15);

            PerActiveTitle = new UILabel();
            PerActiveTitle.Position = new Vector2(99, 160);
            PerActiveTitle.Size = new Vector2(110, 1);
            PerActiveTitle.Alignment = TextAlignment.Right;
            PerActiveTitle.Caption = "Active:";
            InitLabel(PerActiveTitle, 15);

            PerPlayfulTitle = new UILabel();
            PerPlayfulTitle.Position = new Vector2(99, 200);
            PerPlayfulTitle.Size = new Vector2(110, 1);
            PerPlayfulTitle.Alignment = TextAlignment.Right;
            PerPlayfulTitle.Caption = "Playful:";
            InitLabel(PerPlayfulTitle, 15);

            PerNiceTitle = new UILabel();
            PerNiceTitle.Position = new Vector2(99, 238);
            PerNiceTitle.Size = new Vector2(110, 1);
            PerNiceTitle.Alignment = TextAlignment.Right;
            PerNiceTitle.Caption = "Nice:";
            InitLabel(PerNiceTitle, 15);

            PersonalityTex = ui.Get("skill.png").Get(gd);

            for (int i=0; i<5; i++)
            {
                Personalities[i] = new UICASPersonalityBar(this);
                Personalities[i].Position = new Vector2(215, 87 + i*38);
                Add(Personalities[i]);
            }

            //BIO TAB
            BioTitle = new UILabel();
            BioTitle.Position = new Vector2(99, 85);
            BioTitle.Caption = "Bio:";
            InitLabel(BioTitle, 15);

            BioBG = new UIImage(ui.Get("cas_bio_bg.png").Get(gd)).With9Slice(15, 15, 15, 15);
            BioBG.Position = new Vector2(99, 112);
            BioBG.SetSize(390, 152);
            Add(BioBG);

            BioEdit = new UITextEdit();
            BioEdit.Position = new Vector2(107, 122);
            BioEdit.SetSize(371, 133);
            BioEdit.TextStyle = BioEdit.TextStyle.Clone();
            BioEdit.TextStyle.Color = UIStyle.Current.Text;
            BioEdit.TextStyle.Size = 12;
            BioEdit.MaxLines = 7;
            Add(BioEdit);

            UpdateType();

            SetTab(0);
        }

        public void SetTab(int tab)
        {
            var sim = tab == 0;
            var per = tab == 1;
            var bio = tab == 2;

            SimGenderTitle.Visible = sim;
            SimAgeTitle.Visible = sim;
            SimSkinTitle.Visible = sim;
            SimMaleBtn.Visible = sim;
            SimFemaleBtn.Visible = sim;
            SimAdultBtn.Visible = sim;
            SimChildBtn.Visible = sim;
            SimSkinLgtBtn.Visible = sim;
            SimSkinMedBtn.Visible = sim;
            SimSkinDrkBtn.Visible = sim;
            SimRandomBtn.Visible = sim;

            PerNeatTitle.Visible = per;
            PerOutgoingTitle.Visible = per;
            PerActiveTitle.Visible = per;
            PerPlayfulTitle.Visible = per;
            PerNiceTitle.Visible = per;
            for (int i = 0; i < 5; i++)
            {
                Personalities[i].Visible = per;
            }

            BioTitle.Visible = bio;
            BioBG.Visible = bio;
            BioEdit.Visible = bio;

            SimTabButton.Selected = sim;
            PersonalityTabButton.Selected = per;
            BioTabButton.Selected = bio;

            ActiveTab = tab;
        }

        private void SimRandomBtn_OnButtonClick(UIElement button)
        {
            OnRandom?.Invoke();
        }

        public int AllowedPoints = 25;
        public int TotalPoints = 0;

        public void UpdateTotalPoints()
        {
            TotalPoints = 0;
            for (int i = 0; i < 5; i++)
            {
                TotalPoints += Personalities[i].Points;
            }
        }

        public override void Draw(UISpriteBatch batch)
        {
            //draw backgrounds
            var px = TextureGenerator.GetPxWhite(GameFacade.GraphicsDevice);
            DrawLocalTexture(batch, px, null, new Vector2(90, 0), new Vector2(410, 70), UIStyle.Current.Bg);
            DrawLocalTexture(batch, px, null, new Vector2(90, 75), new Vector2(410, 196), UIStyle.Current.Bg);

            //tab bgs
            DrawLocalTexture(batch, px, null, SimTabButton.Position, new Vector2(85, 85), UIStyle.Current.Bg);
            DrawLocalTexture(batch, px, null, PersonalityTabButton.Position, new Vector2(85, 85), UIStyle.Current.Bg);
            DrawLocalTexture(batch, px, null, BioTabButton.Position, new Vector2(85, 85), UIStyle.Current.Bg);
            base.Draw(batch);

            if (ActiveTab == 1)
            {
                //personality tab. draw remaining points
                for (int i = 0; i < AllowedPoints; i++)
                {
                    var col = (i < AllowedPoints-TotalPoints) ? UIStyle.Current.SkillActive : UIStyle.Current.SkillInactive;
                    DrawLocalTexture(batch, PersonalityTex, null, new Vector2(490, 255-i*7), Vector2.One, col, (float)(Math.PI / 2));
                }
            }
        }

        private void InitLabel(UILabel label, int fontSize)
        {
            label.CaptionStyle = label.CaptionStyle.Clone();
            label.CaptionStyle.Size = fontSize;
            label.CaptionStyle.Color = UIStyle.Current.Text;
            Add(label);
        }

        private void ChangeType(int i, char targ)
        {
            var temp = AType.ToCharArray();
            temp[i] = targ;
            AType = new string(temp);
            UpdateType();
        }

        private void ChangeSkin(string skin)
        {
            SkinType = skin;
            UpdateType();
        }

        public void UpdateType()
        {
            SimAdultBtn.Selected = AType[1] == 'a';
            SimChildBtn.Selected = AType[1] == 'c';

            SimMaleBtn.Selected = AType[0] == 'm';
            SimFemaleBtn.Selected = AType[0] == 'f';

            SimSkinLgtBtn.Selected = SkinType == "lgt";
            SimSkinMedBtn.Selected = SkinType == "med";
            SimSkinDrkBtn.Selected = SkinType == "drk";

            OnCollectionChange?.Invoke();
        }
    }

    public class UICASPersonalityBar : UIElement
    {
        private Texture2D BarItem;
        public int Points;
        private UIMouseEventRef ClickHandler;
        private bool MouseOn;
        private UISimCASPanel CAS;

        public UICASPersonalityBar(UISimCASPanel cas)
        {
            BarItem = Content.Get().CustomUI.Get("bar.png").Get(GameFacade.GraphicsDevice);
            ClickHandler = ListenForMouse(new Rectangle(-20, -6, 274, 32), new UIMouseEvent(MouseEvt));
            CAS = cas;
        }

        public void MouseEvt(UIMouseEventType type, UpdateState state)
        {
            if (type == UIMouseEventType.MouseDown) MouseOn = true;
            else if (type == UIMouseEventType.MouseUp) MouseOn = false;
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);
            if (MouseOn)
            {
                //how much allowance do we have on this specifically
                var allowance = CAS.AllowedPoints - (CAS.TotalPoints - Points);
                var newPTs = Math.Max(0, Math.Min(allowance, (int)Math.Ceiling(GlobalPoint(state.MouseState.Position.ToVector2()).X / 24)));
                if (newPTs != Points)
                {
                    HITVM.Get().PlaySoundEvent(UISounds.CreateCharacterPersonality);
                    Points = newPTs;
                    CAS.UpdateTotalPoints();
                }
            }
        }

        public override void Draw(UISpriteBatch batch)
        {
            if (!Visible) return;
            for (int i=0; i<10; i++)
            {
                var col = (i < Points) ? UIStyle.Current.SkillActive : UIStyle.Current.SkillInactive;
                DrawLocalTexture(batch, BarItem, new Rectangle(0, 0, 10, 20), new Vector2(i * 24, 0), Vector2.One, col);
                DrawLocalTexture(batch, BarItem, new Rectangle(20, 0, 10, 20), new Vector2(i * 24 + 10, 0), Vector2.One, col);
            }
        }
    }
}
