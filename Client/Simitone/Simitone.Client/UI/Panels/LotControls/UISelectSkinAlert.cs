using FSO.Client;
using FSO.Client.UI.Framework;
using FSO.Common;
using FSO.Common.Rendering.Framework;
using FSO.Common.Rendering.Framework.Camera;
using FSO.Common.Rendering.Framework.Model;
using FSO.Common.Utils;
using FSO.Content;
using FSO.Content.TS1;
using FSO.LotView.Model;
using FSO.SimAntics;
using FSO.SimAntics.Engine.TSOTransaction;
using FSO.SimAntics.NetPlay;
using FSO.SimAntics.NetPlay.Drivers;
using FSO.SimAntics.NetPlay.Model;
using FSO.Vitaboy;
using Microsoft.Xna.Framework;
using Simitone.Client.UI.Controls;
using Simitone.Client.UI.Panels.WorldUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simitone.Client.UI.Panels.LotControls
{
    public class UISelectSkinAlert : UIMobileDialog
    {
        public UISelectSkinPanel NPanel;
        /*
        public short SelectedNeighbour
        {
            get
            {
                return NPanel.SelectedNeighbour;
            }
        }*/

        public event Action<short> OnResult;

        public UISelectSkinAlert(VMAvatar target, string type, VM vm)
        {
            var pet = type == "cat" || type == "dog";
            Caption = pet ? "Adopt a Pet" : "Please Select Outfit";
            SetHeight(600);
            NPanel = new UISelectSkinPanel(target, type, vm);
            NPanel.Position = new Microsoft.Xna.Framework.Vector2((Width - 1030) / 2, 110);
            NPanel.OnResult += (res) => { OnResult?.Invoke(res); Close(); };
            Add(NPanel);
        }

        public override void GameResized()
        {
            base.GameResized();
            NPanel.Position = new Microsoft.Xna.Framework.Vector2((Width - 1030) / 2, 110);
        }
    }

    public class UISelectSkinPanel : UIContainer{
        public FSO.SimAntics.VM vm { get; set; }
        public BasicCamera Camera;

        public uint BaseGUID = 0x7FD96B54; //all of our templates should be of this kind.
        public string CurrentCode;
        public string CurrentSkin; //
        
        public VMAvatar[] BodyAvatars;
        public List<string> ActiveBodies;
        public List<string> ActiveHandgroupTex;
        public List<string> ActiveBodyTex;

        public float HeadPosition = -9f;
        public float BodyPosition = -9f;

        public float HeadSpeed = 0f;
        public float BodySpeed = 0f;
        public float XLast = -1f;

        public int HeadPositionLast = 0;
        public int BodyPositionLast = 0;
        public bool Pet;

        public _3DTargetScene Scene;
        public string CollectionType = "b";

        public event Action<short> OnResult;
        public UIBigButton OKButton;
        public UIBigButton CancelButton;

        public short ResultInd
        {
            get
            {
                var i = (int)DirectionUtils.PosMod(Math.Round(BodyPosition + 8), ActiveBodies.Count);
                return (short)i;
            }
        }

        public string ResultBody
        {
            get
            {
                var i = (int)DirectionUtils.PosMod(Math.Round(BodyPosition + 8), ActiveBodies.Count);
                return ActiveBodies[i] + ",BODY=" + ActiveBodyTex[i];
            }
        }

        public UISelectSkinPanel(VMAvatar target, string type, VM vm) {
            this.vm = vm;
            if (target != null)
            {
                BaseGUID = target.Object.OBJ.GUID;
                CollectionType = type;
                var bodyStrings = target.Object.Resource.Get<FSO.Files.Formats.IFF.Chunks.STR>(target.Object.OBJ.BodyStringID);
                type = bodyStrings.GetString(1).Substring(4);
                type = type.Substring(0, type.IndexOf('_'));

                CurrentSkin = bodyStrings.GetString(14);
            } else
            {
                Pet = true;
                if (type == "cat") BaseGUID = 0x7BEA0977;
                else if (type == "dog") BaseGUID = 0x4A70DF92;
            }
            Camera = new BasicCamera(GameFacade.GraphicsDevice, new Vector3(5, 1, 0), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            Camera.NearPlane = 0.001f;
            Scene = new _3DTargetScene(GameFacade.GraphicsDevice, Camera, new Point(1030, 500), GlobalSettings.Default.AntiAlias?8:0);
            Scene.Initialize(GameFacade.Scenes);

            InitializeLot();
            PopulateSimType(type);

            OKButton = new UIBigButton(true);
            OKButton.Caption = GameFacade.Strings.GetString("142", "0");
            OKButton.Position = new Microsoft.Xna.Framework.Vector2((515 + 300)-137, 370);
            OKButton.OnButtonClick += (btn) => { OnResult?.Invoke(ResultInd); };
            OKButton.Width = 275;
            Add(OKButton);

            CancelButton = new UIBigButton(true);
            CancelButton.Caption = GameFacade.Strings.GetString("142", "1");
            CancelButton.Position = new Microsoft.Xna.Framework.Vector2((515 - 300) - 137, 370);
            CancelButton.OnButtonClick += (btn) => { OnResult?.Invoke(-1); };
            CancelButton.Width = 275;
            Add(CancelButton);
        }

        public override void PreDraw(UISpriteBatch batch)
        {
            base.PreDraw(batch);
            Camera.Position = new Vector3(10f, 4f, 0);
            Camera.Target = new Vector3(4f, Pet?0f:0.8f, 0);
            GameFacade.GraphicsDevice.RasterizerState = Microsoft.Xna.Framework.Graphics.RasterizerState.CullCounterClockwise;
            Scene.Draw(GameFacade.GraphicsDevice);
            GameFacade.GraphicsDevice.RasterizerState = Microsoft.Xna.Framework.Graphics.RasterizerState.CullNone;
        }

        public override void Draw(UISpriteBatch batch)
        {
            base.Draw(batch);
            DrawLocalTexture(batch, Scene.Target, new Vector2(0, 0));
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);
            UpdateCarousel(state);


            for (int i = 0; i < 18; i++)
            {
                var relPos = BodyPosition + i - 9;
                relPos = (float)DirectionUtils.PosMod(relPos, 18);
                if (relPos > 9) relPos += 6;
                var angle = (relPos / 24) * (Math.PI * 2);
                
                var pos = new Vector3(0 + 4.5f * (float)Math.Cos(angle), 0, 0 + 4.5f * (float)Math.Sin(angle));
                BodyAvatars[i].Avatar.RotationY = -(float)angle + (float)Math.PI / 2;
                var ang2 = (float)Math.Abs(DirectionUtils.PosMod(angle + Math.PI, Math.PI*2) - Math.PI);
                //if (ang2 > Math.PI * 2) ang2 = 255;
                var ava = BodyAvatars[i].Avatar;
                ava.AmbientLight = Vector4.One * Math.Max(0, Math.Min(1, 2-ang2));
                if (i != DirectionUtils.PosMod(-Math.Round(BodyPosition + 9), 18)) ava.AmbientLight *= 0.5f;
                else ava.AmbientLight *= 1.1f;
                ava.Position = pos;
                ava.Scale = Vector3.One * (Pet ? 1 : 1 / 2f);
            }
        }

        private void PopulateSimType(string simtype) // cat / dog / (ma/fa/mc/fc/uc) + (fit/fat/skn/chd)
        {
            CurrentCode = simtype;
            var col = Content.Get().BCFGlobal.CollectionsByName[CollectionType];
            var bodies = col.ClothesByAvatarType[simtype];

            var tex = (TS1AvatarTextureProvider)Content.Get().AvatarTextures;
            var texnames = tex.GetAllNames();
            ActiveBodies = bodies;
            
            ActiveBodyTex = bodies.Select(x => RemoveExt(texnames.FirstOrDefault(y => y.StartsWith(ExtractID(x, CurrentSkin))))).ToList();
            ActiveHandgroupTex = bodies.Select(x => (RemoveExt(texnames.FirstOrDefault(y => y == "huao" + FindHG(x))) ?? "huao" + CurrentSkin).Substring(4)).ToList();

            for (int i = 0; i < ActiveBodies.Count; i++)
            {
                if (ActiveBodyTex[i] == null)
                {
                    ActiveBodyTex.RemoveAt(i);
                    ActiveHandgroupTex.RemoveAt(i);
                    ActiveBodies.RemoveAt(i--);
                }
            }
            
            BodyPositionLast = 0;
            PopulateReal();
        }

        private string FindHG(string item)
        {
            var ind = item.IndexOf('_');
            if (ind != -1) item = item.Substring(ind);
            return item;
        }

        private string RemoveExt(string item)
        {
            if (item == null) return null;
            var ind = item.LastIndexOf('.');
            if (ind != -1) return item.Substring(0, ind);
            return item;
        }

        private string ExtractID(string item, string skncol)
        {
            var ind = item.IndexOf('_');
            if (ind != -1) item = item.Substring(0, ind);
            return item + skncol;
        }

        private string InsertSkinColor(string name, string skncol)
        {
            var ind = name.IndexOf('_');
            if (ind != -1) name = name.Insert(ind, skncol);
            return name;
        }

        private void UpdateCarousel(UpdateState state)
        {
            var frac = 60f / FSOEnvironment.RefreshRate;
            var minSpeed = (Math.PI / 240f) * frac;
            var mult = (float)Math.Pow(0.95, frac);

            var moving = 0;

            if (state.MouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                var pos = GlobalPoint(state.MouseState.Position.ToVector2());
                if (XLast == -1)
                {
                    if (pos.Y > -100)
                        XLast = state.MouseState.X;
                }
                else
                {
                    if (pos.Y > 0 && pos.Y < 375)
                    {
                        BodySpeed = ((XLast - state.MouseState.X) / 200f) * frac;
                        moving = 1;
                    }
                    XLast = state.MouseState.X;
                }
            }
            else
            {
                XLast = -1;
            }

            BodySpeed = BodySpeed * mult;
            if (Math.Abs(BodySpeed) < minSpeed && moving != 1)
            {
                var targInt = (int)Math.Round(BodyPosition);
                BodySpeed = (float)Math.Max(-minSpeed, Math.Min(minSpeed, targInt - BodyPosition));
            }

            HeadSpeed = HeadSpeed * mult;
            if (Math.Abs(HeadSpeed) < minSpeed && moving != 2)
            {
                var targInt = (int)Math.Round(HeadPosition);
                HeadSpeed = (float)Math.Max(-minSpeed, Math.Min(minSpeed, targInt - HeadPosition));
            }

            HeadPosition += HeadSpeed;
            BodyPosition += BodySpeed;

            var room = 0;// vm.Context.GetRoomAt(LotTilePos.FromBigTile(28, 21, 1));
            foreach (var body in BodyAvatars) body.SetRoom(65534);

            var curbody = (int)BodyPosition;
            if (curbody != BodyPositionLast)
            {
                FSO.HIT.HITVM.Get().PlaySoundEvent(FSO.Client.UI.Model.UISounds.Click);
                int replacePos = (int)DirectionUtils.PosMod((-BodyPositionLast), 18);
                int increment = 1;
                if (curbody < BodyPositionLast) //start adding after last position's compliment position    
                {
                    increment = -1;
                    replacePos = (int)DirectionUtils.PosMod((-BodyPositionLast), 18);
                }
                var total = Math.Abs(curbody - BodyPositionLast);
                for (int i = 0; i < total; i++)
                {
                    if (increment == -1)
                    {
                        SetBody(BodyAvatars[replacePos], BodyPositionLast - 1);
                    }
                    else
                    {
                        SetBody(BodyAvatars[replacePos], BodyPositionLast + 17);
                    }
                    BodyPositionLast += increment;
                    replacePos = ((replacePos - increment) + 18) % 18;

                }
                BodyPositionLast = curbody;
            }
        }

        private void SetBody(VMAvatar body, int i)
        {
            i = (int)DirectionUtils.PosMod(i, ActiveBodies.Count);
            var oft = new Outfit() { TS1AppearanceID = ActiveBodies[i] + ".apr", TS1TextureID = ActiveBodyTex[i] };
            
            if (!Pet)
            {
                var code = CurrentCode[0];
                if (CurrentCode[1] != 'a') code = 'u';
                var hg = ActiveHandgroupTex[i];
                oft.LiteralHandgroup = new HandGroup()
                {
                    TS1HandSet = true,
                    LightSkin = new HandSet()
                    {
                        LeftHand = new Hand()
                        {
                            Idle = new Gesture() { Name = "h" + code + "lo.apr", TexName = "huao" + hg },
                            Pointing = new Gesture() { Name = "h" + code + "lp.apr", TexName = "huap" + hg },
                            Fist = new Gesture() { Name = "h" + code + "lc.apr", TexName = "huac" + hg }
                        },
                        RightHand = new Hand()
                        {
                            Idle = new Gesture() { Name = "h" + code + "ro.apr", TexName = "huao" + hg },
                            Pointing = new Gesture() { Name = "h" + code + "rp.apr", TexName = "huap" + hg },
                            Fist = new Gesture() { Name = "h" + code + "rc.apr", TexName = "huac" + hg }
                        }
                    }
                };
            }

            body.BodyOutfit = new FSO.SimAntics.Model.VMOutfitReference(oft);
        }

        private void PopulateReal()
        {
            int i = 0;
            foreach (var body in BodyAvatars)
            {
                SetBody(body, 17 - i);
                i++;
            }
        }

        #region Lot Content Stuff
        public void CleanupLastWorld()
        {
            if (vm == null) return;

            foreach (var body in BodyAvatars)
            {
                body.Delete(true, vm.Context);
            }
        }

        public void InitializeLot()
        {
            BodyAvatars = new VMAvatar[18];
            for (int i = 0; i < 18; i++)
            {
                BodyAvatars[i] = (VMAvatar)vm.Context.CreateObjectInstance(BaseGUID, LotTilePos.OUT_OF_WORLD, Direction.NORTH, true).BaseObject;
                BodyAvatars[i].SetValue(FSO.SimAntics.Model.VMStackObjectVariable.Hidden, 1);
                Scene.Add(BodyAvatars[i].Avatar);
            }
        }
        #endregion
    }
}
