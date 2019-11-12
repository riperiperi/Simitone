/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.Client.UI.Framework;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FSO.Common.Rendering.Framework.Model;
using FSO.Common.Rendering.Framework.IO;
using FSO.Common.Rendering.Framework;
using FSO.HIT;

using FSO.LotView;
using FSO.SimAntics;
using FSO.LotView.Components;
using Microsoft.Xna.Framework.Input;
using FSO.LotView.Model;
using FSO.SimAntics.Primitives;
using FSO.SimAntics.NetPlay.Model.Commands;
using FSO.SimAntics.Utils;
using FSO.Common;
using FSO.Client;
using FSO.Content;
using FSO.Client.Debug;
using Simitone.Client.UI.Screens;
using FSO.LotView.RC;
using Simitone.Client.UI.Panels.LotControls;
using FSO.Client.UI.Panels.LotControls;
using FSO.UI.Panels.LotControls;
using Simitone.Client.UI.Controls;

namespace Simitone.Client.UI.Panels
{
    /// <summary>
    /// Generates pie menus when the player clicks on objects.
    /// </summary>
    public class UILotControl : UIContainer, ILotControl
    {
        private UIMouseEventRef MouseEvt;
        public bool MouseIsOn;
        public I3DRotate Rotate { get { return World.State.Cameras.Camera3D; } }

        private UIPieMenu PieMenu;

        private bool ShowTooltip;
        private bool TipIsError;
        private Texture2D RMBCursor;

        public FSO.SimAntics.VM vm;
        public FSO.LotView.World World { get; set; }
        public VMEntity ActiveEntity { get; set; }
        public int Budget { get
            {
                return vm.TS1State.CurrentFamily?.Budget ?? int.MaxValue;
            }
        }
        public uint SelectedSimID
        {
            get
            {
                return (vm == null) ? 0 : vm.MyUID;
            }
        }
        public short ObjectHover;
        public bool InteractionsAvailable;
        public UIInteractionQueue Queue;

        public bool LiveMode = true;
        public bool PanelActive = false;
        public UILotControlTouchHelper Touch;
        public UIArchTouchHelper ArchTouch;

        public int WallsMode = 1;

        private int OldMX;
        private int OldMY;
        private bool FoundMe; //if false and avatar changes, center. Should center on join lot.

        public bool RMBScroll;
        private int RMBScrollX;
        private int RMBScrollY;

        //1 = near, 0.5 = med, 0.25 = far
        //"target" because we rescale the game target to fit this zoom level.
        public float TargetZoom = 1; 

        // NOTE: Blocking dialog system assumes that nothing goes wrong with data transmission (which it shouldn't, because we're using TCP)
        // and that the code actually blocks further dialogs from appearing while waiting for a response.
        // If we are to implement controlling multiple sims, this must be changed.
        private UIMobileDialog BlockingDialog;
        private UINeighborhoodSelectionPanel TS1NeighSelector;
        private ulong LastDialogID;

        private static uint GOTO_GUID = 0x000007C4;
        public VMEntity GotoObject;

        private Rectangle MouseCutRect = new Rectangle(-4, -4, 4, 4);
        private List<uint> CutRooms = new List<uint>();
        private HashSet<uint> LastCutRooms = new HashSet<uint>(); //final rooms, including those outside. used to detect dirty.
        public sbyte LastFloor = -1;
        public WorldRotation LastRotation = WorldRotation.TopLeft;
        private bool[] LastCuts; //cached roomcuts, to apply rect cut to.
        private int LastWallMode = -1; //invalidates last roomcuts
        private bool LastRectCutNotable = false; //set if the last rect cut made a noticable change to the cuts array. If true refresh regardless of new cut effect.

        public UIObjectHolder ObjectHolder;
        public UICustomLotControl CustomControl;
        public UIQueryPanel QueryPanel;
        public UIPickupPanel PickupPanel;

        /// <summary>
        /// Creates a new UILotControl instance.
        /// </summary>
        /// <param name="vm">A SimAntics VM instance.</param>
        /// <param name="World">A World instance.</param>
        public UILotControl(FSO.SimAntics.VM vm, FSO.LotView.World World)
        {
            this.vm = vm;
            this.World = World;
            
            MouseEvt = this.ListenForMouse(new Microsoft.Xna.Framework.Rectangle(0, 0,
                GlobalSettings.Default.GraphicsWidth, GlobalSettings.Default.GraphicsHeight), OnMouse);

            Queue = new UIInteractionQueue(ActiveEntity, vm);
            this.Add(Queue);

            ObjectHolder = new UIObjectHolder(vm, World, this);
            Touch = new UILotControlTouchHelper(this);
            Add(Touch);
            ArchTouch = new UIArchTouchHelper(this);
            Add(ArchTouch);
            SetupQuery();


            RMBCursor = GetTexture(0x24B00000001); //exploreanchor.bmp

            vm.OnDialog += vm_OnDialog;
            vm.OnBreakpoint += Vm_OnBreakpoint;
        }

        public void SetupQuery()
        {
            /*UIContainer parent = null;
            if (QueryPanel?.Parent?.Parent != null)
            {
                parent = QueryPanel.Parent;
            }*/

            QueryPanel = new UIQueryPanel(World);
            QueryPanel.X = 0;
            QueryPanel.Y = -114;

            PickupPanel = new UIPickupPanel();
            PickupPanel.OnResponse += (resp) =>
            {
                if (resp) ObjectHolder.SellBack(null);
                else ObjectHolder.Cancel();
            };
        }

        public override void GameResized()
        {
            base.GameResized();
            MouseEvt.Region.Width = GlobalSettings.Default.GraphicsWidth;
            MouseEvt.Region.Height = GlobalSettings.Default.GraphicsHeight;

            //SetupQuery();
        }


        private void Vm_OnBreakpoint(VMEntity entity)
        {
            if (IDEHook.IDE != null) IDEHook.IDE.IDEBreakpointHit(vm, entity);
        }

        void vm_OnDialog(FSO.SimAntics.Model.VMDialogInfo info)
        {
            if (info != null && ((info.DialogID == LastDialogID && info.DialogID != 0 && info.Block))) return;
            //return if same dialog as before, or not ours
            if ((info == null || info.Block) && BlockingDialog != null)
            {
                //cancel current dialog because it's no longer valid
                UIScreen.RemoveDialog(BlockingDialog);
                LastDialogID = 0;
                BlockingDialog = null;
            }
            if (info == null) return; //return if we're just clearing a dialog.

            var options = new UIAlertOptions
            {
                Title = info.Title,
                Message = info.Message,
                Width = 325 + (int)(info.Message.Length / 3.5f),
                Alignment = TextAlignment.Left,
                TextSize = 12
            };

            var b0Event = (info.Block) ? new ButtonClickDelegate(DialogButton0) : null;
            var b1Event = (info.Block) ? new ButtonClickDelegate(DialogButton1) : null;
            var b2Event = (info.Block) ? new ButtonClickDelegate(DialogButton2) : null;

            VMDialogType type = (info.Operand == null) ? VMDialogType.Message : info.Operand.Type;

            switch (type)
            {
                default:
                case VMDialogType.Message:
                    options.Buttons = new UIAlertButton[] { new UIAlertButton(UIAlertButtonType.OK, b0Event, info.Yes) };
                    break;
                case VMDialogType.YesNo:
                    options.Buttons = new UIAlertButton[]
                    {
                        new UIAlertButton(UIAlertButtonType.Yes, b0Event, info.Yes),
                        new UIAlertButton(UIAlertButtonType.No, b1Event, info.No),
                    };
                    break;
                case VMDialogType.YesNoCancel:
                    options.Buttons = new UIAlertButton[]
                    {
                        new UIAlertButton(UIAlertButtonType.Yes, b0Event, info.Yes),
                        new UIAlertButton(UIAlertButtonType.No, b1Event, info.No),
                        new UIAlertButton(UIAlertButtonType.Cancel, b2Event, info.Cancel),
                    };
                    break;
                case VMDialogType.TextEntry:
                    options.Buttons = new UIAlertButton[] { new UIAlertButton(UIAlertButtonType.OK, b0Event, info.Yes) };
                    options.TextEntry = true;
                    break;
                case VMDialogType.NumericEntry:
                    if (!vm.TS1) goto case VMDialogType.TextEntry;
                    else goto case VMDialogType.TS1Neighborhood;
                case VMDialogType.TS1Vacation:
                case VMDialogType.TS1Neighborhood:
                case VMDialogType.TS1StudioTown:
                case VMDialogType.TS1Magictown:
                    TS1NeighSelector = new UINeighborhoodSelectionPanel((ushort)VMDialogPrivateStrings.TypeToNeighID[type]);
                    Parent.Add(TS1NeighSelector);
                    ((TS1GameScreen)Parent).Bg.Visible = true;
                    ((TS1GameScreen)Parent).LotControl.Visible = false;
                    TS1NeighSelector.OnHouseSelect += HouseSelected;
                    return;
                case VMDialogType.TS1PhoneBook:
                    var phone = new UICallNeighborAlert(((VMAvatar)info.Caller).GetPersonData(FSO.SimAntics.Model.VMPersonDataVariable.NeighborId), vm);
                    BlockingDialog = phone;
                    UIScreen.GlobalShowDialog(phone, true);
                    phone.OnResult += (result) =>
                    {
                        vm.SendCommand(new VMNetDialogResponseCmd
                        {
                            ActorUID = info.Caller.PersistID,
                            ResponseCode = (byte)((result > 0) ? 1 : 0),
                            ResponseText = result.ToString()
                        });
                        BlockingDialog = null;
                    };
                    return;

                case VMDialogType.TS1PetChoice:
                case VMDialogType.TS1Clothes:
                    var ts1categories = new string[] { "b", "f", "s", "l", "w", "h" };
                    var pet = type == VMDialogType.TS1PetChoice;
                    var stackObj = info.Caller.Thread.Stack.Last().StackObject;
                    
                    var skin = new UISelectSkinAlert(pet?null:(info.Caller as VMAvatar), pet?((stackObj as VMAvatar).IsCat?"cat":"dog"):ts1categories[info.Caller.Thread.TempRegisters[0]], vm);
                    BlockingDialog = skin;
                    UIScreen.GlobalShowDialog(skin, true);
                    skin.OnResult += (result) =>
                    {
                        vm.SendCommand(new VMNetDialogResponseCmd
                        {
                            ActorUID = info.Caller.PersistID,
                            ResponseCode = (byte)((result > -1)?1:0),
                            ResponseText = result.ToString()
                        });
                        BlockingDialog = null;
                    };
                    return;
            }

            var alert = new UIMobileAlert(options);

            UIScreen.GlobalShowDialog(alert, true);

            if (info.Block)
            {
                BlockingDialog = alert;
                LastDialogID = info.DialogID;
            }

            var entity = info.Icon;
            if (entity is VMGameObject)
            {
                var objects = entity.MultitileGroup.Objects;
                ObjectComponent[] objComps = new ObjectComponent[objects.Count];
                for (int i = 0; i < objects.Count; i++)
                {
                    objComps[i] = (ObjectComponent)objects[i].WorldUI;
                }
                var thumb = World.GetObjectThumb(objComps, entity.MultitileGroup.GetBasePositions(), GameFacade.GraphicsDevice);
                alert.SetIcon(thumb, 256, 256);
            }
        }

        private void HouseSelected(int house)
        {
            if (ActiveEntity == null || TS1NeighSelector == null) return;
            vm.SendCommand(new VMNetDialogResponseCmd
            {
                ActorUID = ActiveEntity.PersistID,
                ResponseCode = (byte)((house > 0) ? 1 : 0),
                ResponseText = house.ToString()
            });
            Parent.Remove(TS1NeighSelector);
            TS1NeighSelector = null;
        }

        private void DialogButton0(UIElement button) { DialogResponse(0); }
        private void DialogButton1(UIElement button) { DialogResponse(1); }
        private void DialogButton2(UIElement button) { DialogResponse(2); }

        private void DialogResponse(byte code)
        {
            if (BlockingDialog == null || !(BlockingDialog is UIMobileAlert)) return;
            BlockingDialog.Close();
            var ma = (UIMobileAlert)BlockingDialog;
            LastDialogID = 0;
            vm.SendCommand(new VMNetDialogResponseCmd
            {
                ResponseCode = code,
                ResponseText = (ma.ResponseText == null) ? "" : ma.ResponseText
            });
            BlockingDialog = null;
        }

        private void OnMouse(UIMouseEventType type, UpdateState state)
        {
            if (!vm.Ready) return;

            if (type == UIMouseEventType.MouseOver)
            {
                if (QueryPanel.Mode == 1) QueryPanel.SetShown(false);
                MouseIsOn = true;
            }
            else if (type == UIMouseEventType.MouseOut)
            {
                MouseIsOn = false;
                GameFacade.Cursor.SetCursor(CursorType.Normal);
                Tooltip = null;
            }
            else if (type == UIMouseEventType.MouseDown)
            {
                if (!FSOEnvironment.SoftwareKeyboard)
                {
                    if (!LiveMode)
                    {
                        if (CustomControl != null) CustomControl.MouseDown(state);
                        else ObjectHolder.MouseDown(state);
                        return;
                    }
                }
                Touch.MiceDown.Add(state.CurrentMouseID);
            }
            else if (type == UIMouseEventType.MouseUp)
            {
                Touch.MiceDown.Remove(state.CurrentMouseID);
                if (!FSOEnvironment.SoftwareKeyboard)
                {
                    if (!LiveMode)
                    {
                        if (CustomControl != null) CustomControl.MouseUp(state);
                        else ObjectHolder.MouseUp(state);
                        return;
                    }
                }
                state.UIState.TooltipProperties.Show = false;
                state.UIState.TooltipProperties.Opacity = 0;
                ShowTooltip = false;
                TipIsError = false;
            }
        }

        public void SimulateMD(UpdateState state)
        {
            if (CustomControl != null) CustomControl.MouseDown(state);
            else ObjectHolder.MouseDown(state);
        }

        public void SimulateMU(UpdateState state)
        {
            if (CustomControl != null) CustomControl.MouseUp(state);
            else ObjectHolder.MouseUp(state);
        }

        public void AddModifier(UILotControlModifiers mod)
        {
            if (CustomControl != null)
                CustomControl.Modifiers |= mod;
        }

        public void RemoveModifier(UILotControlModifiers mod)
        {
            if (CustomControl != null)
                CustomControl.Modifiers &= ~mod;
        }

        public void ShowPieMenu(Point pt, UpdateState state)
        {
            if (!LiveMode)
            {
                /*
                if (CustomControl != null) CustomControl.MouseDown(state);
                else ObjectHolder.MouseDown(state);
                */
                if (FSOEnvironment.SoftwareKeyboard && ObjectHolder.Holding == null)
                {
                    ObjectHolder.MouseDown(state);
                }
                return;
            }
            if (PieMenu == null && ActiveEntity != null)
            {
                VMEntity obj;
                //get new pie menu, make new pie menu panel for it
                var tilePos = World.EstTileAtPosWithScroll(new Vector2(pt.X, pt.Y) / FSOEnvironment.DPIScaleFactor);

                LotTilePos targetPos = LotTilePos.FromBigTile((short)tilePos.X, (short)tilePos.Y, World.State.Level);
                if (vm.Context.SolidToAvatars(targetPos).Solid) targetPos = LotTilePos.OUT_OF_WORLD;

                GotoObject.SetPosition(targetPos, Direction.NORTH, vm.Context);

                var newHover = World.GetObjectIDAtScreenPos(pt.X,
                    pt.Y,
                    GameFacade.GraphicsDevice);

                ObjectHover = newHover;

                bool objSelected = ObjectHover > 0;
                if (objSelected || (GotoObject.Position != LotTilePos.OUT_OF_WORLD && ObjectHover <= 0))
                {
                    if (objSelected)
                    {
                        obj = vm.GetObjectById(ObjectHover);
                    }
                    else
                    {
                        obj = GotoObject;
                    }
                    if (obj is VMAvatar && state.CtrlDown)
                    {
                        //debug switch to avatar
                        vm.SendCommand(new VMNetChangeControlCmd()
                        {
                            TargetID = obj.ObjectID
                        });
                    }
                    else if (obj != null)
                    {
                        obj = obj.MultitileGroup.GetInteractionGroupLeader(obj);
                        if (obj is VMGameObject && ((VMGameObject)obj).Disabled > 0)
                        {
                            var flags = ((VMGameObject)obj).Disabled;

                            if ((flags & VMGameObjectDisableFlags.ForSale) > 0)
                            {
                                //for sale
                                var retailPrice = obj.MultitileGroup.Price; //wrong... should get this from catalog
                                var salePrice = obj.MultitileGroup.SalePrice;
                                ShowErrorTooltip(state, 22, true, "$" + retailPrice.ToString("##,#0"), "$" + salePrice.ToString("##,#0"));
                            }
                            else if ((flags & VMGameObjectDisableFlags.LotCategoryWrong) > 0)
                                ShowErrorTooltip(state, 21, true); //category wrong
                            else if ((flags & VMGameObjectDisableFlags.TransactionIncomplete) > 0)
                                ShowErrorTooltip(state, 27, true); //transaction not yet complete
                            else if ((flags & VMGameObjectDisableFlags.ObjectLimitExceeded) > 0)
                                ShowErrorTooltip(state, 24, true); //object is temporarily disabled... todo: something more helpful
                            else if ((flags & VMGameObjectDisableFlags.PendingRoommateDeletion) > 0)
                                ShowErrorTooltip(state, 16, true); //pending roommate deletion
                        }
                        else
                        {
                            var menu = obj.GetPieMenu(vm, ActiveEntity, false, true);
                            if (menu.Count != 0)
                            {
                                HITVM.Get().PlaySoundEvent(UISounds.PieMenuAppear);
                                PieMenu = new UIPieMenu(menu, obj, ActiveEntity, this);
                                this.Add(PieMenu);
                                PieMenu.X = state.MouseState.X / FSOEnvironment.DPIScaleFactor;
                                PieMenu.Y = state.MouseState.Y / FSOEnvironment.DPIScaleFactor;
                                PieMenu.UpdateHeadPosition(state.MouseState.X, state.MouseState.Y);
                            }
                        }
                    }

                }
                else
                {
                    ShowErrorTooltip(state, 0, true);
                }
            }
            else
            {
                if (PieMenu != null) PieMenu.RemoveSimScene();
                this.Remove(PieMenu);
                PieMenu = null;
            }
        }

        private void ShowErrorTooltip(UpdateState state, uint id, bool playSound, params string[] args)
        {
            if (playSound) HITVM.Get().PlaySoundEvent(UISounds.Error);
            state.UIState.TooltipProperties.Show = true;
            state.UIState.TooltipProperties.Color = Color.Black;
            state.UIState.TooltipProperties.Opacity = 1;
            state.UIState.TooltipProperties.Position = new Vector2(state.MouseState.X,
                state.MouseState.Y);
            state.UIState.Tooltip = GameFacade.Strings.GetString("159", id.ToString(), args);
            state.UIState.TooltipProperties.UpdateDead = false;
            ShowTooltip = true;
            TipIsError = true;
        }

        public void ClosePie()
        {
            if (PieMenu != null)
            {
                PieMenu.RemoveSimScene();
                Queue.PieMenuClickPos = PieMenu.Position;
                this.Remove(PieMenu);
                PieMenu = null;
            }
        }

        public override Rectangle GetBounds()
        {
            return new Rectangle(0, 0, GlobalSettings.Default.GraphicsWidth, GlobalSettings.Default.GraphicsHeight);
        }

        public void LiveModeUpdate(UpdateState state, bool scrolled)
        {
            if (MouseIsOn && !RMBScroll && ActiveEntity != null && !FSOEnvironment.SoftwareKeyboard)
            {

                if (state.MouseState.X != OldMX || state.MouseState.Y != OldMY)
                {
                    OldMX = state.MouseState.X;
                    OldMY = state.MouseState.Y;
                    var newHover = World.GetObjectIDAtScreenPos(state.MouseState.X,
                        state.MouseState.Y,
                        GameFacade.GraphicsDevice);

                    if (ObjectHover != newHover)
                    {
                        ObjectHover = newHover;
                        if (ObjectHover > 0)
                        {
                            var obj = vm.GetObjectById(ObjectHover);
                            if (obj != null)
                            {
                                var menu = obj.GetPieMenu(vm, ActiveEntity, false, true);
                                InteractionsAvailable = (menu.Count > 0);
                            }
                        }
                    }

                    if (!TipIsError) ShowTooltip = false;
                    if (ObjectHover > 0)
                    {
                        var obj = vm.GetObjectById(ObjectHover);
                        if (!TipIsError && obj != null)
                        {
                            if (obj is VMAvatar)
                            {
                                state.UIState.TooltipProperties.Show = true;
                                state.UIState.TooltipProperties.Color = Color.Black;
                                state.UIState.TooltipProperties.Opacity = 1;
                                state.UIState.TooltipProperties.Position = new Vector2(state.MouseState.X,
                                    state.MouseState.Y);
                                state.UIState.Tooltip = GetAvatarString(obj as VMAvatar);
                                state.UIState.TooltipProperties.UpdateDead = false;
                                ShowTooltip = true;
                            }
                            else if (((VMGameObject)obj).Disabled > 0)
                            {
                                var flags = ((VMGameObject)obj).Disabled;
                                if ((flags & VMGameObjectDisableFlags.ForSale) > 0)
                                {
                                    //for sale
                                    //try to get catalog price
                                    var guid = obj.MasterDefinition?.GUID ?? obj.Object.OBJ.GUID;
                                    var item = Content.Get().WorldCatalog.GetItemByGUID(guid);

                                    var retailPrice = (int?)(item?.Price) ?? obj.MultitileGroup.Price;
                                    var salePrice = obj.MultitileGroup.SalePrice;
                                    ShowErrorTooltip(state, 22, false, "$" + retailPrice.ToString("##,#0"), "$" + salePrice.ToString("##,#0"));
                                    TipIsError = false;
                                }
                            }

                        }
                    }
                    if (!ShowTooltip)
                    {
                        state.UIState.TooltipProperties.Show = false;
                        state.UIState.TooltipProperties.Opacity = 0;
                    }
                }
            }
            else
            {
                ObjectHover = 0;
            }

            if (!scrolled)
            { //set cursor depending on interaction availability
                CursorType cursor;

                if (PieMenu == null && MouseIsOn)
                {
                    if (ObjectHover == 0)
                    {
                        cursor = CursorType.LiveNothing;
                    }
                    else
                    {
                        if (InteractionsAvailable)
                        {
                            if (vm.GetObjectById(ObjectHover) is VMAvatar) cursor = CursorType.LivePerson;
                            else cursor = CursorType.LiveObjectAvail;
                        }
                        else
                        {
                            cursor = CursorType.LiveObjectUnavail;
                        }
                    }
                }
                else
                {

                    cursor = CursorType.Normal;
                }

                CursorManager.INSTANCE.SetCursor(cursor);
            }

        }

        private string GetAvatarString(VMAvatar ava)
        {
            return ava.ToString();
        }

        public void RefreshCut()
        {
            LastFloor = -1;
            LastWallMode = -1;

            if (vm.Context.Blueprint != null && LastCuts != null)
            {
                vm.Context.Blueprint.Cutaway = LastCuts;
                vm.Context.Blueprint.Changes.SetFlag(BlueprintGlobalChanges.WALL_CUT_CHANGED);
            }

            //MouseCutRect = new Rectangle(0,0,0,0);
        }

        public void SetTargetZoom(WorldZoom zoom)
        {
            switch (zoom)
            {
                case WorldZoom.Near:
                    TargetZoom = 1f; break;
                case WorldZoom.Medium:
                    TargetZoom = 0.5f; break;
                case WorldZoom.Far:
                    TargetZoom = 0.25f; break;
            }
            LastZoom = World.State.Zoom;
        }

        public override void Draw(UISpriteBatch batch)
        {
            //DrawLocalTexture(batch, World.State.Light.LightMap, new Rectangle(0,0, World.State.Light.LightMap.Width/3, World.State.Light.LightMap.Height/2), new Vector2());
            if (RMBScroll)
            {
                DrawLocalTexture(batch, RMBCursor, new Vector2(RMBScrollX - RMBCursor.Width / 2, RMBScrollY - RMBCursor.Height / 2));
            }
            base.Draw(batch);
        }

        private WorldZoom LastZoom;
        public override void Update(UpdateState state)
        {
            base.Update(state);

            if (!vm.Ready || vm.Context.Architecture == null) return;

            //handling smooth scaled zoom
            var camType = World.State.Cameras.ActiveType;
            Touch._3D = camType != FSO.LotView.Utils.Camera.CameraControllerType._2D;
            if (World.State.Cameras.ActiveType == FSO.LotView.Utils.Camera.CameraControllerType._3D)
            {
                if (World.BackbufferScale != 1) World.BackbufferScale = 1;
                var s3d = World.State.Cameras.Camera3D;
                if (TargetZoom < -0.25f)
                {
                    TargetZoom -= (TargetZoom - 0.25f) * (1f - (float)Math.Pow(0.975f, 60f / FSOEnvironment.RefreshRate));
                }
                s3d.Zoom3D += ((9.75f - (TargetZoom - 0.25f) * 5.7f) - s3d.Zoom3D) / 10;
            }
            else if (World.State.Cameras.ActiveType == FSO.LotView.Utils.Camera.CameraControllerType._2D)
            {
                if (World.State.Zoom != LastZoom)
                {
                    //zoom has been changed by something else. inherit the value
                    SetTargetZoom(World.State.Zoom);
                    LastZoom = World.State.Zoom;
                }

                float BaseScale;
                WorldZoom targetZoom;
                if (TargetZoom < 0.5f)
                {
                    targetZoom = WorldZoom.Far;
                    BaseScale = 0.25f;
                }
                else if (TargetZoom < 1f)
                {
                    targetZoom = WorldZoom.Medium;
                    BaseScale = 0.5f;
                }
                else
                {
                    targetZoom = WorldZoom.Near;
                    BaseScale = 1f;
                }
                World.BackbufferScale = TargetZoom / BaseScale;
                if (World.State.Zoom != targetZoom) World.State.Zoom = targetZoom;
                WorldConfig.Current.SmoothZoom = false;
            }
            
            if (ActiveEntity == null || ActiveEntity.Dead || ActiveEntity.PersistID != SelectedSimID)
            {
                ActiveEntity = vm.Entities.FirstOrDefault(x => x is VMAvatar && x.PersistID == SelectedSimID); //try and hook onto a sim if we have none selected.
                //if (ActiveEntity == null) ActiveEntity = vm.Entities.FirstOrDefault(x => x is VMAvatar);

                if (!FoundMe && ActiveEntity != null)
                {
                    vm.Context.World.State.CenterTile = new Vector2(ActiveEntity.VisualPosition.X, ActiveEntity.VisualPosition.Y);
                    vm.Context.World.State.ScrollAnchor = null;
                    FoundMe = true;
                }
                Queue.QueueOwner = ActiveEntity;
            }

            if (GotoObject == null) GotoObject = vm.Context.CreateObjectInstance(GOTO_GUID, LotTilePos.OUT_OF_WORLD, Direction.NORTH, true).Objects[0];


            //update plumbbob
            var plumb = Content.Get().RCMeshes.Get("arrow.fsom");
            foreach (VMAvatar avatar in vm.Context.ObjectQueries.Avatars)
            {
                if (avatar.Avatar == null) continue;
                var isActive = (avatar == ActiveEntity);
                if ((avatar.Avatar.HeadObject == plumb) != isActive)
                {
                    avatar.Avatar.HeadObject = (avatar == ActiveEntity) ? plumb : null;
                    avatar.Avatar.HeadObjectSpeedyVel = 0.2f;
                }
                avatar.Avatar.HeadObjectRotation += 3f / FSOEnvironment.RefreshRate;
                if (isActive)
                {
                    avatar.Avatar.HeadObjectRotation += avatar.Avatar.HeadObjectSpeedyVel;
                    avatar.Avatar.HeadObjectSpeedyVel *= 0.98f;
                } else if (avatar.GetValue(FSO.SimAntics.Model.VMStackObjectVariable.Category) == 87)
                {
                    avatar.Avatar.HeadObject = Content.Get().RCMeshes.Get("star.fsom");
                }
            }
            /*
            if (ActiveEntity != null && BlockingDialog != null)
            {
                //are we still waiting on a blocking dialog? if not, cancel.
                if (ActiveEntity.Thread != null && (ActiveEntity.Thread.BlockingState == null || !(ActiveEntity.Thread.BlockingState is VMDialogResult)))
                {
                    BlockingDialog.Close();
                    LastDialogID = 0;
                    BlockingDialog = null;
                }
            }*/

            if (Visible)
            {
                if (ShowTooltip) state.UIState.TooltipProperties.UpdateDead = false;

                bool scrolled = false;
                if (RMBScroll)
                {
                    World.State.ScrollAnchor = null;
                    Vector2 scrollBy = new Vector2();
                    if (state.TouchMode)
                    {
                        scrollBy = new Vector2(RMBScrollX - state.MouseState.X, RMBScrollY - state.MouseState.Y);
                        RMBScrollX = state.MouseState.X;
                        RMBScrollY = state.MouseState.Y;
                        scrollBy /= 128f;
                        scrollBy /= FSOEnvironment.DPIScaleFactor;
                    }
                    else
                    {
                        scrollBy = new Vector2(state.MouseState.X - RMBScrollX, state.MouseState.Y - RMBScrollY);
                        scrollBy *= 0.0005f;

                        var angle = (Math.Atan2(state.MouseState.X - RMBScrollX, (RMBScrollY - state.MouseState.Y) * 2) / Math.PI) * 4;
                        angle += 8;
                        angle %= 8;

                        CursorType type = CursorType.ArrowUp;
                        switch ((int)Math.Round(angle))
                        {
                            case 0: type = CursorType.ArrowUp; break;
                            case 1: type = CursorType.ArrowUpRight; break;
                            case 2: type = CursorType.ArrowRight; break;
                            case 3: type = CursorType.ArrowDownRight; break;
                            case 4: type = CursorType.ArrowDown; break;
                            case 5: type = CursorType.ArrowDownLeft; break;
                            case 6: type = CursorType.ArrowLeft; break;
                            case 7: type = CursorType.ArrowUpLeft; break;
                        }
                        GameFacade.Cursor.SetCursor(type);
                    }
                    World.Scroll(scrollBy * (60f / FSOEnvironment.RefreshRate));
                    scrolled = true;
                }
                if (MouseIsOn)
                {
                    if (state.MouseState.RightButton == ButtonState.Pressed)
                    {
                        if (RMBScroll == false)
                        {
                            RMBScroll = true;
                            RMBScrollX = state.MouseState.X;
                            RMBScrollY = state.MouseState.Y;
                        }
                    }
                    else
                    {
                        if (!scrolled && GlobalSettings.Default.EdgeScroll && !state.TouchMode) scrolled = World.TestScroll(state);
                    }
                }

                if (state.MouseState.RightButton != ButtonState.Pressed)
                {
                    if (RMBScroll) GameFacade.Cursor.SetCursor(CursorType.Normal);
                    RMBScroll = false;
                }

                if (!LiveMode && PieMenu != null)
                {
                    PieMenu.RemoveSimScene();
                    this.Remove(PieMenu);
                    PieMenu = null;
                }

                if (state.NewKeys.Contains(Keys.F11))
                {
                    var utils = new FSO.SimAntics.Test.CollisionTestUtils();
                    utils.VerifyAllCollision(vm);
                }

                if (state.NewKeys.Contains(Keys.F8))
                {
                    UIMobileAlert alert = null;
                    alert = new UIMobileAlert(new UIAlertOptions()
                    {
                        Title = "Debug Lot Thumbnail",
                        Message = "Arch Value: "+VMArchitectureStats.GetArchValue(vm.Context.Architecture),
                        Buttons = UIAlertButton.Ok((btn) => UIScreen.RemoveDialog(alert))
                    });
                    Texture2D roofless = null;
                    var thumb = World.GetLotThumb(GameFacade.GraphicsDevice, (tex) => roofless = FSO.Common.Utils.TextureUtils.Decimate(tex, GameFacade.GraphicsDevice, 2, false));
                    thumb = FSO.Common.Utils.TextureUtils.Decimate(thumb, GameFacade.GraphicsDevice, 2, false);
                    alert.SetIcon(thumb, thumb.Width, thumb.Height);
                    UIScreen.GlobalShowDialog(alert, true);
                }
                if (LiveMode) LiveModeUpdate(state, scrolled);
                else if (CustomControl != null)
                {
                    if (FSOEnvironment.SoftwareKeyboard) CustomControl.MousePosition = new Point(UIScreen.Current.ScreenWidth / 2, UIScreen.Current.ScreenHeight / 2);
                    else
                    {
                        CustomControl.Modifiers = 0;
                        if (state.CtrlDown) CustomControl.Modifiers |= UILotControlModifiers.CTRL;
                        if (state.ShiftDown) CustomControl.Modifiers |= UILotControlModifiers.SHIFT;
                        CustomControl.MousePosition = state.MouseState.Position;
                    }
                    CustomControl.Update(state, scrolled);
                }
                else ObjectHolder.Update(state, scrolled);

                //set cutaway around mouse
                UpdateCutaway(state);

                if (RMBScrollX == int.MinValue) Dummy(); //cannon fodder for mono AOT compilation: never called but gives these constructors a meaning in life
            }
        }

        private void Dummy()
        {
            CustomControl = new UIWallPlacer(vm, World, this, new List<int>());
            CustomControl = new UIFloorPainter(vm, World, this, new List<int>());
            CustomControl = new UIWallPainter(vm, World, this, new List<int>());
            CustomControl = new UIGrassPaint(vm, World, this, new List<int>());
            CustomControl = new UIRoofer(vm, World, this, new List<int>());
            CustomControl = new UITerrainFlatten(vm, World, this, new List<int>());
            CustomControl = new UITerrainRaiser(vm, World, this, new List<int>());
        }

        private void UpdateCutaway(UpdateState state)
        {
            if (vm.Context.Blueprint != null)
            {
                World.State.DynamicCutaway = (WallsMode == 1);
                //first we need to cycle the rooms that are being cutaway. Keep this up even if we're in all-cut mode.
                var mouseTilePos = World.EstTileAtPosWithScroll(new Vector2(state.MouseState.X, state.MouseState.Y) / FSOEnvironment.DPIScaleFactor);
                var roomHover = vm.Context.GetRoomAt(LotTilePos.FromBigTile((short)(mouseTilePos.X), (short)(mouseTilePos.Y), World.State.Level));
                var outside = (vm.Context.RoomInfo[roomHover].Room.IsOutside);
                if (!outside && !CutRooms.Contains(roomHover))
                    CutRooms.Add(roomHover); //outside hover should not persist like with other rooms.
                while (CutRooms.Count > 3) CutRooms.Remove(CutRooms.ElementAt(0));

                if (LastWallMode != WallsMode)
                {
                    if (WallsMode == 0) //walls down
                    {
                        LastCuts = new bool[vm.Context.Architecture.Width * vm.Context.Architecture.Height];
                        vm.Context.Blueprint.Cutaway = LastCuts;
                        vm.Context.Blueprint.Changes.SetFlag(BlueprintGlobalChanges.WALL_CUT_CHANGED);
                        for (int i = 0; i < LastCuts.Length; i++) LastCuts[i] = true;
                    }
                    else if (WallsMode == 1)
                    {
                        MouseCutRect = new Rectangle();
                        LastCutRooms = new HashSet<uint>() { uint.MaxValue }; //must regenerate cuts
                    }
                    else //walls up or roof
                    {
                        LastCuts = new bool[vm.Context.Architecture.Width * vm.Context.Architecture.Height];
                        vm.Context.Blueprint.Cutaway = LastCuts;
                        vm.Context.Blueprint.Changes.SetFlag(BlueprintGlobalChanges.WALL_CUT_CHANGED);
                    }
                    LastWallMode = WallsMode;
                }

                if (WallsMode == 1)
                {
                    HashSet<uint> finalRooms;
                    int recut = 0;
                    if (FSOEnvironment.SoftwareKeyboard)
                    {
                        finalRooms = new HashSet<uint>();
                        foreach (var room in vm.Context.RoomInfo)
                        {
                            if (!room.Room.IsOutside && room.Room.Floor == World.State.Level-1) finalRooms.Add(room.Room.RoomID);
                        }
                    }
                    else
                    {
                        if (RMBScroll || !MouseIsOn) return;
                        finalRooms = new HashSet<uint>(CutRooms);
                        var newCut = new Rectangle((int)(mouseTilePos.X - 2.5), (int)(mouseTilePos.Y - 2.5), 5, 5);
                        newCut.X -= VMArchitectureTools.CutCheckDir[(int)World.State.CutRotation][0] * 2;
                        newCut.Y -= VMArchitectureTools.CutCheckDir[(int)World.State.CutRotation][1] * 2;
                        if (newCut != MouseCutRect)
                        {
                            MouseCutRect = newCut;
                            recut = 1;
                        }
                    }

                    if (LastFloor != World.State.Level || LastRotation != World.State.CutRotation || !finalRooms.SetEquals(LastCutRooms))
                    {
                        LastCuts = VMArchitectureTools.GenerateRoomCut(vm.Context.Architecture, World.State.Level, World.State.CutRotation, finalRooms);
                        recut = 2;
                        LastFloor = World.State.Level;
                        LastRotation = World.State.CutRotation;
                    }
                    LastCutRooms = finalRooms;

                    if (recut > 0)
                    {
                        var finalCut = new bool[LastCuts.Length];
                        Array.Copy(LastCuts, finalCut, LastCuts.Length);
                        var notableChange = VMArchitectureTools.ApplyCutRectangle(vm.Context.Architecture, World.State.Level, finalCut, MouseCutRect);
                        if (recut > 1 || notableChange || LastRectCutNotable)
                        {
                            vm.Context.Blueprint.Cutaway = finalCut;
                            vm.Context.Blueprint.Changes.SetFlag(BlueprintGlobalChanges.WALL_CUT_CHANGED);
                        }
                        LastRectCutNotable = notableChange;
                    }
                }
            }
        }
    }
}
