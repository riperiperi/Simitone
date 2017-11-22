
using FSO.Client;
using FSO.Client.Debug;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Client.UI.Model;
using FSO.Common;
using FSO.Common.Rendering.Framework;
using FSO.Common.Utils;
using FSO.Content;
using FSO.Files.Formats.IFF.Chunks;
using FSO.HIT;
using FSO.LotView;
using FSO.SimAntics;
using FSO.SimAntics.Engine.TSOTransaction;
using FSO.SimAntics.Marshals;
using FSO.SimAntics.NetPlay;
using FSO.SimAntics.NetPlay.Drivers;
using FSO.SimAntics.NetPlay.Model;
using FSO.SimAntics.NetPlay.Model.Commands;
using FSO.SimAntics.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Simitone.Client.UI.Panels;
using Simitone.Client.UI.Panels.WorldUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simitone.Client.UI.Screens
{
    public class TS1GameScreen : FSO.Client.UI.Framework.GameScreen
    {
        public UIContainer WindowContainer;
        public bool Downtown;

        public UILotControl LotControl { get; set; } 
        public UISimitoneFrontend Frontend { get; set; }
        private FSO.LotView.World World;
        public FSO.SimAntics.VM vm { get; set; }
        public VMNetDriver Driver;
        public UISimitoneBg Bg;
        public uint VisualBudget { get; set; }

        //for TS1 hybrid mode
        public UINeighborhoodSelectionPanel TS1NeighPanel;
        public FAMI ActiveFamily;

        public bool InLot
        {
            get
            {
                return (vm != null);
            }
        }

        private int m_ZoomLevel;
        public int ZoomLevel
        {
            get
            {
                return m_ZoomLevel;
            }
            set
            {
                value = Math.Max(1, Math.Min(3, value));

                if (value < 4)
                {
                    if (vm == null)
                    {

                    }
                    else
                    {
                        var targ = (WorldZoom)(4 - value); //near is 3 for some reason... will probably revise
                        //HITVM.Get().PlaySoundEvent(UIMusic.None);
                        LotControl.Visible = true;
                        Bg.Visible = false;
                        World.Visible = true;
                        //ucp.SetMode(UIUCP.UCPMode.LotMode);
                        if (m_ZoomLevel != value) vm.Context.World.InitiateSmoothZoom(targ);
                        vm.Context.World.State.Zoom = targ;
                        m_ZoomLevel = value;
                    }
                }
                else //cityrenderer! we'll need to recreate this if it doesn't exist...
                {
                    if (m_ZoomLevel < 4)
                    { //coming from lot view... snap zoom % to 0 or 1
                        if (World != null)
                        {
                            LotControl.Visible = false;
                        }
                    }
                    m_ZoomLevel = value;
                }
            }
        }

        private int _Rotation = 0;
        public int Rotation
        {
            get
            {
                return _Rotation;
            }
            set
            {
                _Rotation = value;
                World.State.CenterTile = World.EstTileAtPosWithScroll(new Vector2(ScreenWidth / 2, ScreenHeight / 2));
                if (World != null)
                {
                    switch (_Rotation)
                    {
                        case 0:
                            World.State.Rotation = WorldRotation.TopLeft; break;
                        case 1:
                            World.State.Rotation = WorldRotation.TopRight; break;
                        case 2:
                            World.State.Rotation = WorldRotation.BottomRight; break;
                        case 3:
                            World.State.Rotation = WorldRotation.BottomLeft; break;
                    }
                }
                World.RestoreTerrainToCenterTile();
            }
        }

        public sbyte Level
        {
            get
            {
                if (World == null) return 1;
                else return World.State.Level;
            }
            set
            {
                if (World != null)
                {
                    World.State.Level = value;
                }
            }
        }

        public sbyte Stories
        {
            get
            {
                if (World == null) return 2;
                return World.Stories;
            }
        }

        public VMAvatar SelectedAvatar
        {
            get
            {
                return vm.GetAvatarByPersist(vm.MyUID);
            }
        }

        public TS1GameScreen() : base()
        {
            Bg = new UISimitoneBg();
            Bg.Position = (new Vector2(ScreenWidth, ScreenHeight)) / 2;
            Add(Bg);

            WindowContainer = new UIContainer();
            Add(WindowContainer);

            if (Content.Get().TS1)
            {
                NeighSelection();
            }
        }

        public void NeighSelection()
        {
            TS1NeighPanel = new UINeighborhoodSelectionPanel(4);
            var switcher = new UINeighbourhoodSwitcher(TS1NeighPanel, 4);
            TS1NeighPanel.OnHouseSelect += (house) =>
            {
                ActiveFamily = Content.Get().Neighborhood.GetFamilyForHouse((short)house);
                InitializeLot(Path.Combine(FSOEnvironment.UserDir, "UserData/Houses/House" + house.ToString().PadLeft(2, '0') + ".iff"), false);// "UserData/Houses/House21.iff"
                Remove(TS1NeighPanel);
                Remove(switcher);
            };
            Add(TS1NeighPanel);
            Add(switcher);
        }

        public override void GameResized()
        {
            base.GameResized();
            Bg.Position = (new Vector2(ScreenWidth, ScreenHeight)) / 2;
            World?.GameResized();
        }

        public void Initialize(string propertyName, bool external)
        {
            GameFacade.CurrentCityName = propertyName;
            ZoomLevel = 1; //screen always starts at near zoom
            InitializeLot(propertyName, external);
        }

        private int SwitchLot = -1;

        public void ChangeSpeedTo(int speed)
        {
            //0 speed is 0x
            //1 speed is 1x
            //2 speed is 3x
            //3 speed is 10x

            if (vm == null) return;
            if (vm.SpeedMultiplier == -1) return;

            switch (vm.SpeedMultiplier)
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
                        case 0:
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
                        case 0:
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
                        case 0:
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
                case 0: vm.SpeedMultiplier = 0; break;
                case 1: vm.SpeedMultiplier = 1; break;
                case 2: vm.SpeedMultiplier = 3; break;
                case 3: vm.SpeedMultiplier = 10; break;
            }
        }

        public override void Update(FSO.Common.Rendering.Framework.Model.UpdateState state)
        {
            GameFacade.Game.IsFixedTimeStep = (vm == null || vm.Ready);

            Visible = (World?.State as FSO.LotView.RC.WorldStateRC)?.CameraMode != true;
            GameFacade.Game.IsMouseVisible = Visible;

            base.Update(state);
            if (state.NewKeys.Contains(Keys.NumPad1)) ChangeSpeedTo(1);
            if (state.NewKeys.Contains(Keys.NumPad2)) ChangeSpeedTo(2);
            if (state.NewKeys.Contains(Keys.NumPad3)) ChangeSpeedTo(3);
            if (state.NewKeys.Contains(Keys.P)) ChangeSpeedTo(0);

            if (World != null)
            {
                //stub smooth zoom?
            }

            if (SwitchLot > 0)
            {
                if (!Downtown) SavedLot = vm.Save();
                if (SwitchLot == ActiveFamily.HouseNumber && SavedLot != null)
                {
                    Downtown = false;
                    InitializeLot(SavedLot);
                    SavedLot = null;
                }
                else
                {
                    Downtown = true;
                    InitializeLot(Path.Combine(Content.Get().TS1BasePath, "UserData/Houses/House" + SwitchLot.ToString().PadLeft(2, '0') + ".iff"), false);
                }
                SwitchLot = -1;
            }
            //vm.Context.Clock.Hours = 12;
            if (vm != null) vm.Update();

            //SaveHouseButton_OnButtonClick(null);
        }

        public override void PreDraw(UISpriteBatch batch)
        {
            base.PreDraw(batch);
            vm?.PreDraw();
        }

        public void CleanupLastWorld()
        {
            if (vm == null) return;

            //clear our cache too, if the setting lets us do that
            TimedReferenceController.Clear();
            TimedReferenceController.Clear();
            VM.ClearAssembled();

            if (ZoomLevel < 4) ZoomLevel = 5;
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
            //LotControl.Dispose();
            this.Remove(LotControl);
            this.Remove(Frontend);
            vm.SuppressBHAVChanges();
            vm = null;
            World = null;
            Driver = null;
            LotControl = null;
        }

        private VMMarshal SavedLot;

        public void InitializeLot()
        {
            CleanupLastWorld();

            if (FSOEnvironment.Enable3D)
            {
                World = new FSO.LotView.RC.WorldRC(GameFacade.GraphicsDevice);
            } else
            {
                World = new FSO.LotView.World(GameFacade.GraphicsDevice);
            }

            World.Opacity = 1;
            GameFacade.Scenes.Add(World);

            var globalLink = new VMTS1GlobalLinkStub();
            Driver = new VMServerDriver(globalLink);

            vm = new VM(new VMContext(World), Driver, new UIHeadlineRendererProvider());
            vm.ListenBHAVChanges();
            vm.Init();

            LotControl = new UILotControl(vm, World);
            this.AddAt(0, LotControl);

            if (m_ZoomLevel > 3)
            {
                World.Visible = false;
                LotControl.Visible = false;
            }

            ZoomLevel = Math.Max(ZoomLevel, 4);

            if (IDEHook.IDE != null) IDEHook.IDE.StartIDE(vm);

            vm.OnFullRefresh += VMRefreshed;
            //vm.OnEODMessage += LotControl.EODs.OnEODMessage;
            vm.OnRequestLotSwitch += VMLotSwitch;
            vm.OnGenericVMEvent += Vm_OnGenericVMEvent;
        }

        public void InitializeLot(VMMarshal marshal)
        {
            InitializeLot();
            vm.MyUID = 1;
            vm.Load(marshal);

            vm.ActivateFamily(ActiveFamily);

            var settings = GlobalSettings.Default;
            var myClient = new VMNetClient
            {
                PersistID = 1,
                RemoteIP = "local",
                AvatarState = new VMNetAvatarPersistState()
                {
                    Name = settings.LastUser ?? "",
                    DefaultSuits = new VMAvatarDefaultSuits(settings.DebugGender),
                    BodyOutfit = settings.DebugBody,
                    HeadOutfit = settings.DebugHead,
                    PersistID = 1,
                    SkinTone = (byte)settings.DebugSkin,
                    Gender = (short)(settings.DebugGender ? 1 : 0),
                    Permissions = FSO.SimAntics.Model.TSOPlatform.VMTSOAvatarPermissions.Admin,
                    Budget = 1000000
                }

            };

            var server = (VMServerDriver)Driver;
            server.ConnectClient(myClient);

            GameFacade.Cursor.SetCursor(CursorType.Normal);
            ZoomLevel = 1;

            Frontend = new UISimitoneFrontend(this);
            this.Add(Frontend);
        }

        public void InitializeLot(string lotName, bool external)
        {
            if (lotName == "") return;
            InitializeLot();
            
            if (!external)
            {
                if (!Downtown && ActiveFamily != null)
                {
                    ActiveFamily.SelectWholeFamily();
                    vm.ActivateFamily(ActiveFamily);
                }
                BlueprintReset(lotName);

                vm.TSOState.Size = (10) | (3 << 8);
                vm.Context.UpdateTSOBuildableArea();
                vm.MyUID = 1;
                var settings = GlobalSettings.Default;
                var myClient = new VMNetClient
                {
                    PersistID = 1,
                    RemoteIP = "local",
                    AvatarState = new VMNetAvatarPersistState()
                    {
                        Name = settings.LastUser ?? "",
                        DefaultSuits = new VMAvatarDefaultSuits(settings.DebugGender),
                        BodyOutfit = settings.DebugBody,
                        HeadOutfit = settings.DebugHead,
                        PersistID = 1,
                        SkinTone = (byte)settings.DebugSkin,
                        Gender = (short)(settings.DebugGender ? 1 : 0),
                        Permissions = FSO.SimAntics.Model.TSOPlatform.VMTSOAvatarPermissions.Admin,
                        Budget = 1000000
                    }

                };

                var server = (VMServerDriver)Driver;
                server.ConnectClient(myClient);

                GameFacade.Cursor.SetCursor(CursorType.Normal);
                ZoomLevel = 1;
            }

            Frontend = new UISimitoneFrontend(this);
            this.Add(Frontend);
        }

        public void BlueprintReset(string path)
        {
            string filename = Path.GetFileName(path);
            try
            {
                using (var file = new BinaryReader(File.OpenRead(Path.Combine(FSOEnvironment.UserDir, "LocalHouse/") + filename.Substring(0, filename.Length - 4) + ".fsov")))
                {
                    var marshal = new FSO.SimAntics.Marshals.VMMarshal();
                    marshal.Deserialize(file);
                    //vm.SendCommand(new VMStateSyncCmd()
                    //{
                    //    State = marshal
                    //});

                    vm.Load(marshal);
                    vm.Reset();
                }
            }
            catch (Exception)
            {
                var floorClip = Rectangle.Empty;
                var offset = new Point();
                var targetSize = 0;

                var isIff = path.EndsWith(".iff");
                short jobLevel = -1;
                if (isIff) jobLevel = short.Parse(path.Substring(path.Length - 6, 2));
                vm.SendCommand(new VMBlueprintRestoreCmd
                {
                    JobLevel = jobLevel,
                    XMLData = File.ReadAllBytes(path),
                    IffData = isIff,

                    FloorClipX = floorClip.X,
                    FloorClipY = floorClip.Y,
                    FloorClipWidth = floorClip.Width,
                    FloorClipHeight = floorClip.Height,
                    OffsetX = offset.X,
                    OffsetY = offset.Y,
                    TargetSize = targetSize
                });
            }

            vm.Tick();

            if (ActiveFamily == null)
            {
                vm.SetGlobalValue(32, 1);
                vm.SpeedMultiplier = -1;
            }
        }


        private void Vm_OnGenericVMEvent(VMEventType type, object data)
        {
            //hmm...
        }

        private void VMLotSwitch(uint lotId)
        {
            vm.SpeedMultiplier = 0;
            if ((short)lotId == -1)
            {
                lotId = (uint)ActiveFamily.HouseNumber;
            }
            SwitchLot = (int)lotId;
        }

        private void VMRefreshed()
        {
            if (vm == null) return;
            LotControl.ActiveEntity = null;
            LotControl.RefreshCut();
        }

        private void SaveHouseButton_OnButtonClick(UIElement button)
        {
            if (vm == null) return;

            var exporter = new VMWorldExporter();
            Directory.CreateDirectory(Path.Combine(FSOEnvironment.UserDir, "Blueprints/cas.xml"));
            exporter.SaveHouse(vm, Path.Combine(FSOEnvironment.UserDir, "Blueprints/cas.xml"));
            var marshal = vm.Save();
            Directory.CreateDirectory(Path.Combine(FSOEnvironment.UserDir, "LocalHouse/"));
            using (var output = new FileStream(Path.Combine(FSOEnvironment.UserDir, "LocalHouse/cas.fsov"), FileMode.Create))
            {
                marshal.SerializeInto(new BinaryWriter(output));
            }
        }

        private UIMobileAlert CloseAlert;
        public override bool CloseAttempt()
        {
            GameThread.NextUpdate(x =>
            {
                if (CloseAlert == null)
                {
                    var canSave = vm != null;
                    CloseAlert = new UIMobileAlert(new FSO.Client.UI.Controls.UIAlertOptions
                    {
                        Title = GameFacade.Strings.GetString("153", "1"), //quit?
                        Message = GameFacade.Strings.GetString("153", canSave?"6":"2"), //are you sure (2), save before quitting (3)
                        Buttons = 
                        canSave?
                        UIAlertButton.YesNoCancel(
                            (b) => { Save(); GameFacade.Game.Exit(); },
                            (b) => { GameFacade.Game.Exit(); },
                            (b) => { CloseAlert.Close(); CloseAlert = null; }
                            )
                        :
                        UIAlertButton.YesNo(
                            (b) => { GameFacade.Game.Exit(); },
                            (b) => { CloseAlert.Close(); CloseAlert = null; }
                            )
                    });
                    GlobalShowDialog(CloseAlert, true);
                }
            });
            return false;
        }

        public void ReturnToNeighbourhood()
        {
            if (CloseAlert == null)
            {
                CloseAlert = new UIMobileAlert(new FSO.Client.UI.Controls.UIAlertOptions
                {
                    Title = GameFacade.Strings.GetString("153", "3"), //save
                    Message = GameFacade.Strings.GetString("153", "4"), //Do you want to save the game?
                    Buttons =
                    UIAlertButton.YesNoCancel(
                        (b) => { Save(); ExitLot(); CloseAlert.Close(); CloseAlert = null; },
                        (b) => { ExitLot(); CloseAlert.Close(); CloseAlert = null; },
                        (b) => { CloseAlert.Close(); CloseAlert = null; }
                        )
                });
                GlobalShowDialog(CloseAlert, true);
            }
        }

        public void Save()
        {

        }

        public void ExitLot()
        {
            CleanupLastWorld();
            NeighSelection();
        }
    }
}