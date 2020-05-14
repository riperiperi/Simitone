using FSO.Client;
using FSO.Client.UI.Framework;
using FSO.Common;
using FSO.Common.Utils;
using FSO.SimAntics;
using FSO.SimAntics.Engine.TSOTransaction;
using FSO.SimAntics.NetPlay;
using FSO.SimAntics.NetPlay.Drivers;
using FSO.SimAntics.NetPlay.Model;
using Simitone.Client.UI.Panels.WorldUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.Common.Rendering.Framework.Model;
using FSO.Common.Rendering.Framework.Camera;
using Microsoft.Xna.Framework;
using FSO.LotView.RC;
using FSO.LotView.Utils;
using FSO.LotView.Model;
using FSO.Content;
using FSO.Vitaboy;
using FSO.Content.TS1;
using Simitone.Client.UI.Panels.CAS;
using Simitone.Client.UI.Controls;
using FSO.SimAntics.Model;
using FSO.Files.Formats.IFF.Chunks;
using Simitone.Client.UI.Panels;
using FSO.Client.UI.Controls;
using Simitone.Client.Utils;
using FSO.SimAntics.Utils;
using FSO.SimAntics.Model.TS1Platform;
using FSO.LotView;

namespace Simitone.Client.UI.Screens
{
    public class TS1CASScreen : UIScreen
    {
        private FSO.LotView.World World;
        public FSO.SimAntics.VM vm { get; set; }
        public VMNetDriver Driver;
        public BasicCamera Cam;
        public bool Initialized;
        public VMAvatar[] HeadAvatars;
        public VMAvatar[] BodyAvatars;
        public List<string> ActiveHeads;
        public List<string> ActiveBodies;
        public List<string> ActiveHeadTex;
        public List<string> ActiveHandgroupTex;
        public List<string> ActiveBodyTex;

        public static int NeighTypeFrom = 4;
        private bool Dead;

        private float _FamilySimInterp = -2;
        public float FamilySimInterp
        {
            set
            {
                CameraInterp(value);
                CASPanel.Position = new Vector2((Cam == null)?10:((ScreenWidth - 500) / 2), 10 - (282 * (1-value)));
                FamilyPanel.ShowI = 1-Math.Abs(value);
                FamiliesPanel.TitleI = 1 - Math.Abs(value+1);

                _FamilySimInterp = value;
            }
            get
            {
                return _FamilySimInterp;
            }
        }

        public float HeadPosition = -9f;
        public float BodyPosition = -9f;

        public float HeadSpeed = 0f;
        public float BodySpeed = 0f;
        public float XLast = -1f;

        public int HeadPositionLast = 0;
        public int BodyPositionLast = 0;

        public string CurrentCode = "ma";
        public string CurrentSkin = "lgt";
        private bool CurrentChild;

        private int? MoveInFamily;

        private UICASMode Mode = UICASMode.FamilyEdit;

        public List<CASFamilyMember> WIPFamily = new List<CASFamilyMember>();
        public List<VMAvatar> RepresentFamily = new List<VMAvatar>();

        public UISimCASPanel CASPanel;
        public UIFamilyCASPanel FamilyPanel;
        public UIFamiliesCASPanel FamiliesPanel;
        public UITwoStateButton BackButton;
        public UITwoStateButton AcceptButton;

        public Vector3[] ModePositions = new Vector3[]
        {
            new Vector3(177.3843f, 150.92333f, 3.25105f),
            new Vector3(157.3843f, 28.92333f, 23.25105f),
            new Vector3(119.5611f, 6.122346f, 104.0364f),
            new Vector3(114.0793f, 10f, 64.67827f)
        };

        public Vector3[] ModeTargets = new Vector3[]
        {
            new Vector3(177.3843f-7f, 130.92333f, 3.25105f+5f),
            new Vector3(150.3057f, 26.01005f, 29.6858f),
            new Vector3(111.7678f, 3.936443f, 98.164f),
            new Vector3(104.5736f, 6.896059f, 64.59684f)
        };

        public Vector3[] Mode2D = new Vector3[]
        {
            new Vector3(104, 0, 57),
            new Vector3(104, 0, 57),
            new Vector3(103.5611f, 0, 92.0364f),
            new Vector3(84.0793f+6, 0, 64f-6)
        };

        public Vector3[] SinTransitions = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(12f, 0, 0),
            new Vector3(-6f, 0, 0),
            new Vector3()
        };

        public void CameraInterp(float value)
        {
            if (value < -2) value = -2;
            var prev = (int)(value + 2);
            var next = (int)(Math.Ceiling(value) + 2);
            if (next > 3) next = 3;

            if (Cam == null)
            {
                //2d
                var pos1 = Mode2D[prev];
                var pos2 = Mode2D[next];

                if (World != null) World.State.PreciseZoom = 1 + Math.Min(0, value * 0.5f);
                value = (float)DirectionUtils.PosMod(value, 1.0);
                var campos = Vector3.Lerp(pos1, pos2, value);
                campos += (float)Math.Sin(value * Math.PI) * SinTransitions[prev];



                if (World != null)
                    World.State.CenterTile = new Vector2(campos.X, campos.Z) / 3f;
            }
            else
            {

                var pos1 = ModePositions[prev];
                var pos2 = ModePositions[next];
                var targ1 = ModeTargets[prev] - pos1;
                var targ2 = ModeTargets[next] - pos2;

                value = (float)DirectionUtils.PosMod(value, 1.0);
                var camvec = Vector3.Lerp(targ1, targ2, value);
                var campos = Vector3.Lerp(pos1, pos2, value);

                campos += (float)Math.Sin(value * Math.PI) * SinTransitions[prev];

                Cam.Position = campos;
                Cam.Target = campos + camvec;
            }
        }

        private string MissingFallback(string x, IEnumerable<string> texnames)
        {
            return "x";
        }

        private void PopulateSimType(string simtype)
        {
            CurrentCode = simtype;
            var heads = Content.Get().BCFGlobal.CollectionsByName["c"].ClothesByAvatarType[simtype];
            if (simtype[1] == 'c') simtype += "chd";
            var bodies = Content.Get().BCFGlobal.CollectionsByName["b"].GeneralAvatarType(simtype);

            var tex = (TS1AvatarTextureProvider)Content.Get().AvatarTextures;
            var texnames = tex.GetAllNames();
            ActiveHeads = heads;
            ActiveBodies = bodies;
            
            ActiveHeadTex = heads.Select(x => RemoveExt(texnames.FirstOrDefault(y => y.StartsWith(ExtractID(x, CurrentSkin))))).ToList();
            ActiveBodyTex = bodies.Select(x => RemoveExt(
                texnames.FirstOrDefault(y => y.StartsWith(ExtractID(x, CurrentSkin)))
                ?? texnames.FirstOrDefault(y => y.StartsWith(ExtractID(x, ""))) ?? MissingFallback(x, texnames)
                )).ToList();
            ActiveHandgroupTex = ActiveBodyTex.Select(x => (RemoveExt(texnames.FirstOrDefault(y => RemoveExt(y) == "huao"+FindHG(x))) ?? "huao"+ CurrentSkin).Substring(4)).ToList();

            for (int i=0; i<ActiveHeads.Count; i++)
            {
                if (ActiveHeadTex[i] == null)
                {
                    ActiveHeadTex.RemoveAt(i);
                    ActiveHeads.RemoveAt(i--);
                }
            }

            for (int i = 0; i < ActiveBodies.Count; i++)
            {
                if (ActiveBodyTex[i] == null)
                {
                    ActiveBodyTex.RemoveAt(i);
                    ActiveHandgroupTex.RemoveAt(i);
                    ActiveBodies.RemoveAt(i--);
                }
            }

            HeadPositionLast = 0;
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

            if (state.MouseStates.Count(x => x.MouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed) > 0)
            {
                if (XLast == -1)
                {
                    if (state.MouseState.Y > 282)
                        XLast = state.MouseState.X;
                }
                else
                {
                    if (state.MouseState.Y / (float)UIScreen.Current.ScreenHeight > 0.625f)
                    {
                        BodySpeed = ((XLast - state.MouseState.X) / 200f) * frac;
                        moving = 1;
                    }
                    else
                    {
                        HeadSpeed = ((XLast - state.MouseState.X) / 200f) * frac;
                        moving = 2;
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

            var room = vm.Context.GetRoomAt(LotTilePos.FromBigTile(28, 21, 1));
            foreach (var body in BodyAvatars) body.SetRoom(room);
            foreach (var head in HeadAvatars) head.SetRoom(65534);

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
                for (int i=0; i<total; i++)
                {
                    if (increment == -1)
                    {
                        SetBody(BodyAvatars[replacePos], BodyPositionLast-1);
                    } else
                    {
                        SetBody(BodyAvatars[replacePos], BodyPositionLast + 17);
                    }
                    BodyPositionLast += increment;
                    replacePos = ((replacePos - increment) + 18) % 18;

                }
                BodyPositionLast = curbody;
            }

            var curhead = (int)HeadPosition;
            if (curhead != HeadPositionLast)
            {
                FSO.HIT.HITVM.Get().PlaySoundEvent(FSO.Client.UI.Model.UISounds.Click);
                int replacePos = (int)DirectionUtils.PosMod(-HeadPositionLast, 18);
                int increment = 1;
                if (curhead < HeadPositionLast) //start adding after last position's compliment position    
                {
                    increment = -1;
                    replacePos = (int)DirectionUtils.PosMod((-HeadPositionLast), 18);
                }
                var total = Math.Abs(curhead - HeadPositionLast);
                for (int i = 0; i < total; i++)
                {
                    if (increment == -1)
                    {
                        SetHead(HeadAvatars[replacePos], HeadPositionLast - 1);
                    }
                    else
                    {
                        SetHead(HeadAvatars[replacePos], HeadPositionLast + 17);
                    }
                    HeadPositionLast += increment;
                    replacePos = ((replacePos - increment) + 18) % 18;

                }
                HeadPositionLast = curhead;
            }
        }

        private void SetBody(VMAvatar body, int i)
        {
            i = (int)DirectionUtils.PosMod(i, ActiveBodies.Count);
            var code = CurrentCode[0];
            if (CurrentCode[1] != 'a') code = 'u';
            var oft = new Outfit() { TS1AppearanceID = ActiveBodies[i] + ".apr", TS1TextureID = ActiveBodyTex[i] };
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

            body.BodyOutfit = new FSO.SimAntics.Model.VMOutfitReference(oft);
        }

        private void SetHead(VMAvatar head, int i)
        {
            i = (int)DirectionUtils.PosMod(i, ActiveHeads.Count);
            head.HeadOutfit = new FSO.SimAntics.Model.VMOutfitReference(new Outfit() { TS1AppearanceID = ActiveHeads[i] + ".apr", TS1TextureID = ActiveHeadTex[i] });
        }

        private void PopulateReal()
        {
            var child = CurrentCode[1] == 'c';
            //var animSource = HeadAvatars[0].Avatar.Skeleton;

            int i = 0;
            foreach (var head in HeadAvatars)
            {
                SetHead(head, 17-i);
                //head.Avatar.Skeleton = animSource;
                if (child != CurrentChild)
                {
                    head.Avatar.Skeleton = Content.Get().AvatarSkeletons.Get(child ? "child.skel" : "adult.skel");
                    head.Avatar.ReloadSkeleton();
                }
                i++;
            }
            i = 0;
            foreach (var body in BodyAvatars)
            {
                SetBody(body, 17-i);
                //body.Avatar.Skeleton = animSource;
                if (child != CurrentChild)
                {
                    body.Avatar.Skeleton = Content.Get().AvatarSkeletons.Get(child ? "child.skel" : "adult.skel");
                    body.Avatar.ReloadSkeleton();
                }
                i++;
            }
            CurrentChild = child;
        }

        public TS1CASScreen()
        {
            var ui = Content.Get().CustomUI;
            var gd = GameFacade.GraphicsDevice;

            CASPanel = new UISimCASPanel();
            CASPanel.OnCollectionChange += CASPanel_OnCollectionChange;
            CASPanel.Position = new Vector2(0, -400);
            CASPanel.OnRandom += CASPanel_OnRandom;
            Add(CASPanel);

            FamilyPanel = new UIFamilyCASPanel(RepresentFamily);
            FamilyPanel.ModifySim = ModifySim;
            Add(FamilyPanel);

            FamiliesPanel = new UIFamiliesCASPanel();
            FamiliesPanel.OnNewFamily += () => { SetMode(UICASMode.FamilyEdit); };
            Add(FamiliesPanel);

            BackButton = new UITwoStateButton(ui.Get("btn_back.png").Get(gd));
            BackButton.Position = new Vector2(25, ScreenHeight - 140);
            Add(BackButton);

            AcceptButton = new UITwoStateButton(ui.Get("btn_accept.png").Get(gd));
            AcceptButton.Position = new Vector2(ScreenWidth-140, ScreenHeight - 140);
            Add(AcceptButton);

            BackButton.OnButtonClick += GoBack;
            AcceptButton.OnButtonClick += Accept;
        }

        private int EditIndex = -1;

        public void ModifySim(bool delete, int index)
        {
            if (index == -1)
            {
                PrepareEdit(index);
                SetMode(UICASMode.SimEdit);
            } else
            {
                if (delete)
                {
                    var fam = RepresentFamily[index];
                    fam.Delete(true, vm.Context);
                    RepresentFamily.RemoveAt(index);
                    WIPFamily.RemoveAt(index);

                    foreach (var fam2 in RepresentFamily)
                    {
                        fam2.SetPosition(LotTilePos.OUT_OF_WORLD, Direction.NORTH, vm.Context);
                    }
                    for (int i=0; i<RepresentFamily.Count; i++)
                    {
                        SetFamilyMember(i);
                    }
                    FamilyPanel.Reset();
                } else
                {
                    //prepare sim edit mode with old sim's parameters
                    PrepareEdit(index);
                    SetMode(UICASMode.SimEdit);
                }
            }
        }

        private void PrepareEdit(int i)
        {
            EditIndex = i;
            if (i > -1)
            {
                var sim = WIPFamily[i];

                //load this family member's traits into the editor
                CASPanel.FirstNameTextBox.CurrentText = sim.Name;
                CASPanel.BioEdit.CurrentText = sim.Bio;
                switch (sim.Gender)
                {
                    case 0: CurrentCode = "ma"; break;
                    case 1: CurrentCode = "fa"; break;
                    case 2: CurrentCode = "mc"; break;
                    case 3: CurrentCode = "fc"; break;
                }
                for (int j = 0; j < 5; j++)
                {
                    CASPanel.Personalities[j].Points = sim.Personality[j] / 100;
                }
                CurrentSkin = sim.SkinColor;

                PopulateSimType(CurrentCode);

                //find index for the sims body and head

                BodyPosition = ActiveBodies.IndexOf(sim.Body) - 8;
                HeadPosition = ActiveHeads.IndexOf(sim.Head) - 8;
            } else
            {
                CASPanel.FirstNameTextBox.CurrentText = "";
                CASPanel.BioEdit.CurrentText = "";
                CurrentCode = "ma";
                CurrentSkin = "lgt";
                for (int j = 0; j < 5; j++)
                {
                    CASPanel.Personalities[j].Points = 0;
                }
                PopulateSimType(CurrentCode);
                BodyPosition = - 8;
                HeadPosition = - 8;
            }
            CASPanel.AType = CurrentCode;
            CASPanel.SkinType = CurrentSkin;
            CASPanel.UpdateTotalPoints();
            CASPanel.UpdateType();
        }

        public UIMobileAlert ConfirmDialog;

        private void Accept(UIElement button)
        {
            switch (Mode)
            {
                case UICASMode.SimEdit:
                    //add or replace the sim in the family
                    AcceptMember();
                    break;
                case UICASMode.FamilyEdit:
                    //add or replace the family in the neighbourhood
                    //need to generate an actual FAMI and save it for this
                    if (ConfirmDialog == null)
                    {
                        ConfirmDialog = new UIMobileAlert(new UIAlertOptions()
                        {
                            Title = GameFacade.Strings.GetString("129", "13"),
                            Message = GameFacade.Strings.GetString("129", "14"),
                            Buttons = UIAlertButton.YesNo(
                                (ybtn) => { ConfirmDialog.Close(); Accept(ybtn); ConfirmDialog = null; },
                                (nbtn) => { ConfirmDialog.Close(); ConfirmDialog = null; }
                            )
                        });
                        UIScreen.GlobalShowDialog(ConfirmDialog, true);
                        return;
                    } else
                    {
                        SaveFamily();
                    }
                    break;
                case UICASMode.FamilySelect:
                    //accept button here is move in. notify the neighbourhood screen that we're moving in now.
                    MoveInFamily = FamiliesPanel.Families[FamiliesPanel.Selection].ChunkID;
                    break;
            }
            SetMode((UICASMode)(((int)Mode) - 1));
        }

        private void GoBack(UIElement button)
        {
            if (Mode == UICASMode.FamilyEdit)
            {
                if (ConfirmDialog == null)
                {
                    ConfirmDialog = new UIMobileAlert(new UIAlertOptions()
                    {
                        Title = GameFacade.Strings.GetString("129", "7"),
                        Message = GameFacade.Strings.GetString("129", "8"),
                        Buttons = UIAlertButton.YesNo(
                            (ybtn) => { ConfirmDialog.Close(); GoBack(ybtn); ConfirmDialog = null; },
                            (nbtn) => { ConfirmDialog.Close(); ConfirmDialog = null; }
                        )
                    });
                    UIScreen.GlobalShowDialog(ConfirmDialog, true);
                    return;
                }
                else
                {
                    ClearFamily();
                }
            }
            SetMode((UICASMode)(((int)Mode) - 1));
        }

        public void SetMode(UICASMode mode)
        {
            if (mode == UICASMode.ToNeighborhood)
            {
                //todo: animate into this
                
                Dead = true;
                var dialog = new UITransDialog("normal", () => {
                    CleanupLastWorld();
                    if (MoveInFamily == null)
                        GameController.EnterGameMode("", false);
                    else
                        GameController.EnterGameMode("!"+((NeighTypeFrom == 7)?'m':'n')+MoveInFamily.Value.ToString(), false);
                });
                return;
            } else if (mode == UICASMode.FamilyEdit)
            {
                FamilyPanel.Reset();
            }

            FamiliesPanel.SetSelection(-1);
            if (mode == UICASMode.FamilySelect) AcceptButton.Texture = Content.Get().CustomUI.Get("btn_movein.png").Get(GameFacade.GraphicsDevice);
            else AcceptButton.Texture = Content.Get().CustomUI.Get("btn_accept.png").Get(GameFacade.GraphicsDevice);

            GameFacade.Screens.Tween.To(this, 1f, new Dictionary<string, float> { { "FamilySimInterp", (int)mode-1 } }, TweenQuad.EaseInOut);
            Mode = mode;
        }

        private void CASPanel_OnRandom()
        {
            var rand = new Random();
            HeadPosition = rand.Next(ActiveHeads.Count);
            BodyPosition = rand.Next(ActiveBodies.Count);
            PopulateReal();
            HeadPositionLast = 0;
            BodyPositionLast = 0;
        }

        private void CASPanel_OnCollectionChange()
        {
            if (CurrentSkin == CASPanel.SkinType && CurrentCode == CASPanel.AType) return;
            CurrentSkin = CASPanel.SkinType;
            if (vm == null) return;
            PopulateSimType(CASPanel.AType);
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);
            ModePositions[3].Y = 11;
            ModeTargets[3].Y = 7.896059f;
            if (Dead) return;
            if (vm == null) InitializeLot();
            vm.Update();
            if (World != null && !Initialized)
            {
                World.State.DisableSmoothRotation = true;
                if (GraphicsModeControl.Mode == GlobalGraphicsMode.Full3D)
                {
                    World.State.SetCameraType(World, FSO.LotView.Utils.Camera.CameraControllerType.FirstPerson, 0);
                    var fp = World.State.Cameras.CameraFirstPerson;
                    Cam = fp.Camera;
                    fp.FixedCam = true;
                }
                SetMode(UICASMode.FamilySelect);
                SetFamilies();
                Initialized = true;
                //FamilySimInterp = FamilySimInterp;
            }

            if (World.State.PreciseZoom != 1) World.State.PreciseZoom = World.State.PreciseZoom;
            switch (Mode)
            {
                case UICASMode.FamilySelect:
                    if (World.State.Level != 2)
                    {
                        World.State.Level = 2;
                        World.State.DrawRoofs = true;
                        vm.Context.Blueprint.Cutaway = new bool[vm.Context.Blueprint.Cutaway.Length];
                        vm.Context.Blueprint.Changes.SetFlag(BlueprintGlobalChanges.WALL_CUT_CHANGED);
                    }
                    break;
                default:
                    if (World.State.Level != 1 && Cam == null)
                    {
                        World.State.Level = 1;
                        World.State.DrawRoofs = false;
                        vm.Context.Blueprint.Cutaway = VMArchitectureTools.GenerateRoomCut(vm.Context.Architecture, World.State.Level, World.State.CutRotation, 
                            new HashSet<uint>(vm.Context.RoomInfo.Where(x => x.Room.IsOutside == false).Select(x => (uint)x.Room.RoomID)));
                        vm.Context.Blueprint.Changes.SetFlag(BlueprintGlobalChanges.WALL_CUT_CHANGED);
                    }
                    break;
            }

            vm.Context.Clock.Minutes = 0;
            vm.Context.Clock.Hours = 12;

            var disableAccept = false;
            switch (Mode)
            {
                case UICASMode.SimEdit:
                    if (CASPanel.FirstNameTextBox.CurrentText.Length == 0) disableAccept = true;
                    break;
                case UICASMode.FamilySelect:
                    if (FamiliesPanel.Selection == -1) disableAccept = true;
                    break;
                case UICASMode.FamilyEdit:
                    if (WIPFamily.Count == 0) disableAccept = true;
                    break;
            }

            AcceptButton.Disabled = disableAccept;
            //AcceptButton.ForceState = disableAccept ? 0 : -1;
            //AcceptButton.Opacity = disableAccept ? 0.5f : 1;

            if (Mode == UICASMode.SimEdit)
                UpdateCarousel(state);

            for (int i=0; i<18; i++)
            {
                var relPos = HeadPosition + i - 9;
                relPos = (float)DirectionUtils.PosMod(relPos, 18);
                if (relPos > 9) relPos += 6;
                var angle = (relPos / 24) * (Math.PI * 2);
                var pos = new Vector3(28.5f + 4.5f*(float)Math.Cos(angle), 21.5f+ 4.5f * (float)Math.Sin(angle), 0);
                HeadAvatars[i].RadianDirection = (float)angle + (float)Math.PI/2;
                HeadAvatars[i].VisualPosition = pos;
            }

            for (int i = 0; i < 18; i++)
            {
                var relPos = BodyPosition + i - 9;
                relPos = (float)DirectionUtils.PosMod(relPos, 18);
                if (relPos > 9) relPos += 6;
                var angle = (relPos / 24) * (Math.PI * 2);
                var pos = new Vector3(28.5f + 4.5f * (float)Math.Cos(angle), 21.5f + 4.5f * (float)Math.Sin(angle), 0);
                BodyAvatars[i].RadianDirection = (float)angle + (float)Math.PI / 2;
                BodyAvatars[i].VisualPosition = pos;
            }

            foreach (var fam in RepresentFamily)
            {
                for (int i = 0; i < 16; i++)
                {
                    fam.SetMotiveData((VMMotive)i, 100);
                }
                var q = new List<FSO.SimAntics.Engine.VMQueuedAction>(fam.Thread.Queue);
                foreach (var action in q)
                    fam.Thread.CancelAction(action.UID);
            }
        }

        public void SetFamilies()
        {
            //get all families that don't have a house from neighbourhood, and populate the list
            //i think house number -1 is townies, so only select 0
            var all = Content.Get().Neighborhood.MainResource.List<FAMI>();
            var families = all.Where(x => (x.Unknown & 16) > 0 && x.HouseNumber == 0);
            FamiliesPanel.UpdateFamilies(families.ToList(), vm);
        }

        public void SetFamilyMember(int index)
        {
            if (RepresentFamily.Count <= index)
            {
                var member = (VMAvatar)vm.Context.CreateObjectInstance(0x7FD96B54, LotTilePos.OUT_OF_WORLD
                    
                    , Direction.EAST, false).BaseObject;

                RepresentFamily.Add(member);
            }

            var fam = RepresentFamily[index];

            fam.SetPosition(LotTilePos.FromBigTile(34, 31, 1) +
                    new LotTilePos((short)((((index + 1) / 2) % 2) * 8), (short)(((index % 2) * 2 - 1) * ((index + 1) / 2) * 10), 0), Direction.EAST, vm.Context, VMPlaceRequestFlags.AllowIntersection);
            var data = WIPFamily[index];
            //set this person's body and head

            fam.Name = data.Name;
            fam.Avatar.Skeleton = Content.Get().AvatarSkeletons.Get((data.Gender>1) ? "child.skel" : "adult.skel").Clone();
            fam.Avatar.BaseSkeleton = fam.Avatar.Skeleton.Clone();
            fam.Avatar.ReloadSkeleton();

            fam.SetPersonData(VMPersonDataVariable.PersonsAge, (short)((data.Gender > 1) ? 12 : 21));
            fam.InitBodyData(vm.Context);

            var oft = new Outfit() { TS1AppearanceID = data.Body + ".apr", TS1TextureID = data.BodyTex };
            var code = (data.Gender > 1) ? "u" : ((data.Gender == 0) ? "m" : "f");
            var hg = data.HandgroupTex;
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

            fam.BodyOutfit = new FSO.SimAntics.Model.VMOutfitReference(oft);
            fam.HeadOutfit = new FSO.SimAntics.Model.VMOutfitReference(new Outfit() { TS1AppearanceID = data.Head+".apr", TS1TextureID = data.HeadTex });
        }

        private SimTemplateCreateInfo CASToNeighGen(CASFamilyMember x)
        {
            var code = ((x.Gender & 1) == 0) ? "m" : "f";
            code += (x.Gender > 1) ? "c" : "a";
            var ind = x.Body.IndexOf("_");
            var bodyType = x.Body.Substring(ind - 3, 3);
            code += bodyType;
            var info = new SimTemplateCreateInfo(code, x.SkinColor);
            info.Name = x.Name;
            info.Bio = x.Bio;
            info.PersonalityPoints = x.Personality;

            info.BodyStringReplace[1] = x.Body + ",BODY=" + x.BodyTex;
            info.BodyStringReplace[2] = x.Head + ",HEAD-HEAD=" + x.HeadTex;

            var hand = (x.Gender > 1) ? "u" : ((x.Gender == 0) ? "m" : "f");
            info.BodyStringReplace[17] = "H" + hand + "LO,HAND=" + "huao" + x.HandgroupTex;
            info.BodyStringReplace[18] = "H" + hand + "RO,HAND=" + "huao" + x.HandgroupTex;
            info.BodyStringReplace[19] = "H" + hand + "LP,HAND=" + "huao" + x.HandgroupTex;
            info.BodyStringReplace[20] = "H" + hand + "RP,HAND=" + "huao" + x.HandgroupTex;
            info.BodyStringReplace[21] = "H" + hand + "LO,HAND=" + "huao" + x.HandgroupTex;
            info.BodyStringReplace[22] = "H" + hand + "RC,HAND=" + "huao" + x.HandgroupTex;
            return info;
        }

        public void ClearFamily()
        {
            var count = WIPFamily.Count;
            FamilyPanel.SecondName.CurrentText = "";
            for (int i = count-1; i >= 0; i--)
                ModifySim(true, i);
        }

        public void SaveFamily()
        {
            SimitoneNeighbourGenerator.CreateFamily(FamilyPanel.SecondName.CurrentText, WIPFamily.Count, WIPFamily.Select(CASToNeighGen).ToArray());
            SetFamilies();
            ClearFamily();
        }

        public void AcceptMember()
        {
            var mem = BuildMember();
            if (EditIndex == -1)
            {
                WIPFamily.Add(mem);
                SetFamilyMember(WIPFamily.Count - 1);
            } else
            {
                WIPFamily[EditIndex] = mem;
                SetFamilyMember(EditIndex);
            }
        }

        public CASFamilyMember BuildMember()
        {
            //build the object out of the contents of various menus
            var i = (int)DirectionUtils.PosMod(Math.Round(BodyPosition+8), ActiveBodies.Count);
            var j = (int)DirectionUtils.PosMod(Math.Round(HeadPosition+8), ActiveHeads.Count);
            var sim = new CASFamilyMember()
            {
                Name = CASPanel.FirstNameTextBox.CurrentText,
                Bio = CASPanel.BioEdit.CurrentText,
                Body = ActiveBodies[i],
                BodyTex = ActiveBodyTex[i],
                HandgroupTex = ActiveHandgroupTex[i],
                Head = ActiveHeads[j],
                HeadTex = ActiveHeadTex[j],
                Gender = (short)(((CurrentCode[0] == 'm') ? 0 : 1) | ((CurrentCode[1] == 'c') ? 2 : 0)),
                Personality = CASPanel.Personalities.Select(x => (short)(x.Points * 100)).ToArray(),
                SkinColor = CurrentSkin
            };
            return sim;
        }

        public override void GameResized()
        {
            base.GameResized();
            World?.GameResized();
            BackButton.Position = new Vector2(25, ScreenHeight - 140);
            AcceptButton.Position = new Vector2(ScreenWidth - 140, ScreenHeight - 140);
            FamilySimInterp = FamilySimInterp;
        }

        public override void PreDraw(UISpriteBatch batch)
        {
            base.PreDraw(batch);
            vm?.PreDraw();
        }

        public override void Draw(UISpriteBatch batch)
        {
            base.Draw(batch);
        }

        public void CleanupLastWorld()
        {
            if (vm == null) return;

            //clear our cache too, if the setting lets us do that
            TimedReferenceController.Clear();
            TimedReferenceController.Clear();

            vm.Context.Ambience.Kill();
            foreach (var ent in vm.Entities)
            { //stop object sounds
                var threads = ent.SoundThreads;
                for (int i = 0; i < threads.Count; i++)
                {
                    threads[i].Sound.RemoveOwner(ent.ObjectID);
                }
                threads.Clear();
            }
            vm.CloseNet(VMCloseNetReason.LeaveLot);
            GameFacade.Scenes.Remove(World);
            World.Dispose();
            vm.SuppressBHAVChanges();
            vm = null;
            World = null;
            Driver = null;
        }

        public void InitializeLot()
        {
            CleanupLastWorld();
            
            World = new FSO.LotView.World(GameFacade.GraphicsDevice);

            World.Opacity = 1;
            GameFacade.Scenes.Add(World);

            var globalLink = new VMTS1GlobalLinkStub();
            Driver = new VMServerDriver(globalLink);

            vm = new VM(new VMContext(World), Driver, new UIHeadlineRendererProvider());
            vm.ListenBHAVChanges();
            vm.Init();

            using (var file = new BinaryReader(File.OpenRead(Path.Combine(FSOEnvironment.ContentDir, "cas.fsov"))))
            {
                var marshal = new FSO.SimAntics.Marshals.VMMarshal();
                marshal.Deserialize(file);
                marshal.PlatformState = new VMTS1LotState();
                vm.Load(marshal);
                vm.Reset();
            }
            vm.Tick();

            vm.Context.Clock.Hours = 12;
            vm.MyUID = uint.MaxValue;
            var settings = GlobalSettings.Default;
            var myClient = new VMNetClient
            {
                PersistID = uint.MaxValue,
                RemoteIP = "local",
                AvatarState = new VMNetAvatarPersistState()
                {
                    Name = settings.LastUser ?? "",
                    DefaultSuits = new VMAvatarDefaultSuits(settings.DebugGender),
                    BodyOutfit = settings.DebugBody,
                    HeadOutfit = settings.DebugHead,
                    PersistID = uint.MaxValue,
                    SkinTone = (byte)settings.DebugSkin,
                    Gender = (short)(settings.DebugGender ? 1 : 0),
                    Permissions = FSO.SimAntics.Model.TSOPlatform.VMTSOAvatarPermissions.Admin,
                    Budget = 1000000
                }

            };

            var server = (VMServerDriver)Driver;
            server.ConnectClient(myClient);

            HeadAvatars = new VMAvatar[18];
            for (int i=0; i<18; i++)
            {
                HeadAvatars[i] = (VMAvatar)vm.Context.CreateObjectInstance(0x7FD96B54, LotTilePos.OUT_OF_WORLD, Direction.NORTH, true).BaseObject;
            }

            BodyAvatars = new VMAvatar[18];
            for (int i = 0; i < 18; i++)
            {
                BodyAvatars[i] = (VMAvatar)vm.Context.CreateObjectInstance(0x7FD96B54, LotTilePos.OUT_OF_WORLD, Direction.NORTH, true).BaseObject;
            }

            PopulateSimType("ma");
        }
    }

    public class CASFamilyMember
    {
        public string Name;
        public string SkinColor;
        public short Gender; //adult 0,1... child 2,3

        public string Head;
        public string HeadTex;
        public string Body;
        public string BodyTex;
        public string HandgroupTex;

        public short[] Personality = new short[5];
        public string Bio;
        public uint RefGUID; //for family editing. TODO.
        public int ReplaceIndex; //for editing existing sims
    }

    public enum UICASMode : int
    {
        ToNeighborhood = -1,
        FamilySelect = 0,
        FamilyEdit = 1,
        SimEdit = 2,
    }
}
