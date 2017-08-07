using FSO.Client;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Client.UI.Model;
using FSO.Common.Rendering.Framework.IO;
using FSO.Common.Rendering.Framework.Model;
using FSO.Content;
using FSO.HIT;
using FSO.SimAntics;
using Microsoft.Xna.Framework;
using Simitone.Client.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simitone.Client.UI.Panels
{
    public class UIClockPanel : UICachedContainer
    {
        public UIImage OuterBg;
        public UIImage InnerBg;

        public UILabel TimeLabel;
        public UILabel TimeLabelShadow;

        public UIButton Speed1;
        public UIButton Speed2;
        public UIButton Speed3;
        public UIButton SpeedP;

        public UIButton SpeedCurrent;

        public UIMouseEventRef MouseEvent;

        public static Dictionary<int, int> RemapSpeed = new Dictionary<int, int>()
        {
            {0, 4}, //pause
            {1, 1}, //1 speed
            {3, 2}, //2 speed
            {10, 3}, //3 speed
        };

        public static Dictionary<int, int> ReverseRemap = RemapSpeed.ToDictionary(x => x.Value, x => x.Key);

        public UIButton[] Btns;

        public bool Expand;
        public VM VM;

        public UIClockPanel(VM vm) : base()
        {
            VM = vm;
            OuterBg = new UIImage(Content.Get().CustomUI.Get("clockbg.png").Get(GameFacade.GraphicsDevice))
                .With9Slice(25, 25, 0, 0);
            Add(OuterBg);
            OuterBg.X = 138;
            OuterBg.Size = new Point(196, 50);
            OuterBg.Width = OuterBg.Width;

            MouseEvent = OuterBg.ListenForMouse(OuterBg.GetBounds(), HandleMouseEvent);

            InnerBg = new UIImage(Content.Get().CustomUI.Get("clockinbg.png").Get(GameFacade.GraphicsDevice))
                .With9Slice(19, 19, 0, 0);
            Add(InnerBg);
            InnerBg.X = 148;
            InnerBg.Y = 6;
            InnerBg.Size = new Point(133, 38);

            TimeLabel = new UILabel();
            TimeLabel.Alignment = TextAlignment.Middle | TextAlignment.Center;
            TimeLabel.CaptionStyle = TimeLabel.CaptionStyle.Clone();
            TimeLabel.CaptionStyle.Size = 15;
            TimeLabel.CaptionStyle.Color = Color.White;
            TimeLabel.Size = new Vector2(133, 38);
            TimeLabel.Position = InnerBg.Position;

            TimeLabelShadow = new UILabel();
            TimeLabelShadow.Alignment = TextAlignment.Middle | TextAlignment.Center;
            TimeLabelShadow.CaptionStyle = TimeLabel.CaptionStyle.Clone();
            TimeLabelShadow.CaptionStyle.Color = Color.Black * 0.25f;
            TimeLabelShadow.Size = new Vector2(133, 38);
            TimeLabelShadow.Position = InnerBg.Position + new Vector2(2);

            Add(TimeLabelShadow);
            Add(TimeLabel);

            Size = new Microsoft.Xna.Framework.Vector2(334, 50);
            //full size is 334 wide
            //small: 138 x offset at 196

            Speed1 = new UITwoStateButton(Content.Get().CustomUI.Get("speedbtn_1.png").Get(GameFacade.GraphicsDevice));
            Speed2 = new UITwoStateButton(Content.Get().CustomUI.Get("speedbtn_2.png").Get(GameFacade.GraphicsDevice));
            Speed3 = new UITwoStateButton(Content.Get().CustomUI.Get("speedbtn_3.png").Get(GameFacade.GraphicsDevice));
            SpeedP = new UITwoStateButton(Content.Get().CustomUI.Get("speedbtn_4.png").Get(GameFacade.GraphicsDevice));

            Btns = new UIButton[] { Speed1, Speed2, Speed3, SpeedP };
            for (int i=0; i<4; i++)
            {
                var btn = Btns[i];
                var speed = i;
                btn.OnButtonClick += (b) => { if (!Expand) SetExpanded(true); else SwitchSpeed(speed+1); };
                btn.Position = new Vector2(289, 6);
                btn.InflateHitbox(5, 15);
                Add(btn);
            }

            TweenHook = 0;
        }

        public string LastClock = "";
        public int LastSpeed = -1;
        public override void Update(UpdateState state)
        {
            var min = VM.Context.Clock.Minutes;
            var hour = VM.Context.Clock.Hours;

            string suffix = (hour > 11) ? "PM" : "AM";
            hour %= 12;
            if (hour == 0) hour = 12;

            var text = hour.ToString() + ":" + min.ToString().PadLeft(2, '0') + " " + suffix;

            if (text != LastClock)
            {
                LastClock = text;
                TimeLabel.Caption = text;
                TimeLabelShadow.Caption = text;
            }

            MouseEvent.Region = OuterBg.GetBounds();

            var speed = RemapSpeed[VM.SpeedMultiplier];
            if (speed != LastSpeed)
            {
                if (speed == 4) InnerBg.Texture = Content.Get().CustomUI.Get("clockinbg_pause.png").Get(GameFacade.GraphicsDevice);
                else if (LastSpeed == 4) InnerBg.Texture = Content.Get().CustomUI.Get("clockinbg.png").Get(GameFacade.GraphicsDevice);

                for (int i=0; i<4; i++)
                {
                    Btns[i].Selected = (i+1 == speed);
                    if (i+1 == speed)
                    {
                        SendToFront(Btns[i]);
                    }
                    if (TweenHook == 0) Btns[i].Visible = Btns[i].Selected;
                }
                LastSpeed = speed;
            }
            base.Update(state);
        }

        public void SwitchSpeed(int speed)
        {
            switch (VM.SpeedMultiplier)
            {
                case 0:
                    switch (speed)
                    {
                        case 1:
                            HITVM.Get().PlaySoundEvent(UISounds.SpeedPTo1); break;
                        case 2:
                            HITVM.Get().PlaySoundEvent(UISounds.SpeedPTo2); break;
                        case 3:
                            HITVM.Get().PlaySoundEvent(UISounds.SpeedPTo3); break;
                    }
                    break;
                case 1:
                    switch (speed)
                    {
                        case 4:
                            HITVM.Get().PlaySoundEvent(UISounds.Speed1ToP); break;
                        case 2:
                            HITVM.Get().PlaySoundEvent(UISounds.Speed1To2); break;
                        case 3:
                            HITVM.Get().PlaySoundEvent(UISounds.Speed1To3); break;
                    }
                    break;
                case 3:
                    switch (speed)
                    {
                        case 4:
                            HITVM.Get().PlaySoundEvent(UISounds.Speed2ToP); break;
                        case 1:
                            HITVM.Get().PlaySoundEvent(UISounds.Speed2To1); break;
                        case 3:
                            HITVM.Get().PlaySoundEvent(UISounds.Speed2To3); break;
                    }
                    break;
                case 10:
                    switch (speed)
                    {
                        case 4:
                            HITVM.Get().PlaySoundEvent(UISounds.Speed3ToP); break;
                        case 1:
                            HITVM.Get().PlaySoundEvent(UISounds.Speed3To1); break;
                        case 2:
                            HITVM.Get().PlaySoundEvent(UISounds.Speed3To2); break;
                    }
                    break;
            }

            switch (speed)
            {
                case 4: VM.SpeedMultiplier = 0; break;
                case 1: VM.SpeedMultiplier = 1; break;
                case 2: VM.SpeedMultiplier = 3; break;
                case 3: VM.SpeedMultiplier = 10; break;
            }

            if (Expand) SetExpanded(false);
        }

        public void HandleMouseEvent(UIMouseEventType type, UpdateState state)
        {
            if (type == UIMouseEventType.MouseDown)
            {
                SetExpanded(!Expand);
            }
        }

        private float _TweenHook;
        public float TweenHook
        {
            set
            {
                Invalidate();
                _TweenHook = value;
                for (int i = 0; i < 4; i++) Btns[i].Visible = (value != 0) || i+1 == LastSpeed;
            }
            get
            {
                return _TweenHook;
            }
        }

        public void SetExpanded(bool expand)
        {
            var time = 0.3f;
            GameFacade.Screens.Tween.To(OuterBg, time, new Dictionary<string, float>() { { "X", (expand) ? 0f : 138f }, { "Width", (expand) ? 334f : 196f } }, TweenQuad.EaseOut);
            GameFacade.Screens.Tween.To(InnerBg, time, new Dictionary<string, float>() { { "X", (expand) ? 10f : 148f } }, TweenQuad.EaseOut);

            GameFacade.Screens.Tween.To(TimeLabelShadow, time, new Dictionary<string, float>() { { "X", (expand) ? 12f : 150f } }, TweenQuad.EaseOut);
            GameFacade.Screens.Tween.To(TimeLabel, time, new Dictionary<string, float>() { { "X", (expand) ? 10f : 148f } }, TweenQuad.EaseOut);
            GameFacade.Screens.Tween.To(this, time, new Dictionary<string, float>() { { "TweenHook", (expand) ? 1f : 0f } }, TweenQuad.EaseOut);

            for (int i=0; i<4; i++)
                GameFacade.Screens.Tween.To(Btns[i], time, new Dictionary<string, float>() { { "X", (expand) ? (151f+46*i) : 289f } }, TweenQuad.EaseOut);
            Expand = expand;
        }
    }
}
