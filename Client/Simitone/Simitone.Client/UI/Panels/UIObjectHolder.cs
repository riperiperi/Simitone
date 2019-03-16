/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.LotView;
using FSO.SimAntics;
using FSO.Common.Rendering.Framework.Model;
using Microsoft.Xna.Framework;
using FSO.LotView.Components;
using FSO.SimAntics.Entities;
using FSO.LotView.Model;
using FSO.Client.UI.Model;
using FSO.HIT;
using FSO.SimAntics.Model;
using Microsoft.Xna.Framework.Input;
using FSO.Client.UI.Framework;
using FSO.SimAntics.NetPlay.Model.Commands;
using FSO.Common;
using FSO.Client.UI.Controls;
using FSO.Common.Rendering.Framework;
using FSO.Content;
using FSO.Client;

namespace Simitone.Client.UI.Panels
{
    public class UIObjectHolder //controls the object holder interface
    {
        public VM vm;
        public World World;
        public UILotControl ParentControl;

        public Direction Rotation;
        public int MouseDownX;
        public int MouseDownY;
        private bool MouseIsDown;
        private bool MouseWasDown;
        private bool MouseClicked;

        private int OldMX;
        private int OldMY;
        private UpdateState LastState; //state for access from Sellback and friends.
        public bool DirChanged;
        public bool ShowTooltip;
        public bool Roommate = true;
        public bool UseNet = false;

        public event HolderEventHandler OnPickup;
        public event HolderEventHandler OnDelete;
        public event HolderEventHandler OnPutDown;

        public UIObjectSelection Holding;

        public UIObjectHolder(VM vm, World World, UILotControl parent)
        {
            this.vm = vm;
            this.World = World;
            ParentControl = parent;
        }

        public void SetSelected(VMMultitileGroup Group)
        {
            if (Holding != null) ClearSelected();
            Holding = new UIObjectSelection();
            Holding.Group = Group;
            if (!UseNet) Holding.Group.ExecuteEntryPoint(12, vm.Context); //User Pickup
            Holding.PreviousTile = Holding.Group.BaseObject.Position;
            Holding.Dir = Group.Objects[0].Direction;
            Holding.OriginalDir = Holding.Dir;
            VMEntity[] CursorTiles = new VMEntity[Group.Objects.Count];
            for (int i = 0; i < Group.Objects.Count; i++)
            {
                var target = Group.Objects[i];
                target.SetRoom(65535);
                if (target is VMGameObject) ((ObjectComponent)target.WorldUI).ForceDynamic = true;
                CursorTiles[i] = vm.Context.CreateObjectInstance(0x00000437, new LotTilePos(target.Position), FSO.LotView.Model.Direction.NORTH, true).Objects[0];
                CursorTiles[i].SetPosition(target.Position, Direction.NORTH, vm.Context);
                ((ObjectComponent)CursorTiles[i].WorldUI).ForceDynamic = true;
            }
            Holding.TilePosOffset = new Vector2(0, 0);
            Holding.CursorTiles = CursorTiles;

            uint guid;
            var bobj = Group.BaseObject;
            guid = bobj.Object.OBJ.GUID;
            if (bobj.MasterDefinition != null) guid = bobj.MasterDefinition.GUID;
            var catalogItem = Content.Get().WorldCatalog.GetItemByGUID(guid);
            if (catalogItem != null)
            {
                var price = (int)catalogItem.Value.Price;
                var dcPercent = VMBuildableAreaInfo.GetDiscountFor(catalogItem.Value, vm);
                var finalPrice = (price * (100 - dcPercent)) / 100;
                Holding.Price = finalPrice;
            }
        }

        public void MoveSelected(Vector2 pos, sbyte level)
        {
            Holding.TilePos = pos;
            Holding.Level = level;

            //first, eject the object from any slots
            for (int i = 0; i < Holding.Group.Objects.Count; i++)
            {
                var obj = Holding.Group.Objects[i];
                if (obj.Container != null)
                {
                    obj.Container.ClearSlot(obj.ContainerSlot);
                }
            }

            //rotate through to try all configurations
            var dir = Holding.Dir;
            VMPlacementError status = VMPlacementError.Success;
            if (!Holding.IsBought && !vm.PlatformState.CanPlaceNewUserObject(vm)) status = VMPlacementError.TooManyObjectsOnTheLot;
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    status = Holding.Group.ChangePosition(LotTilePos.FromBigTile((short)pos.X, (short)pos.Y, World.State.Level), dir, vm.Context, VMPlaceRequestFlags.UserPlacement).Status;
                    if (status != VMPlacementError.MustBeAgainstWall) break;
                    dir = (Direction)((((int)dir << 6) & 255) | ((int)dir >> 2));
                }
                if (Holding.Dir != dir) Holding.Dir = dir;
            }

            if (status != VMPlacementError.Success)
            {
                Holding.Group.ChangePosition(LotTilePos.OUT_OF_WORLD, Holding.Dir, vm.Context, VMPlaceRequestFlags.UserPlacement);

                Holding.Group.SetVisualPosition(new Vector3(pos,
                (((Holding.Group.Objects[0].GetValue(VMStackObjectVariable.AllowedHeightFlags) & 1) == 1) ? 0 : 4f / 5f) + (World.State.Level - 1) * 2.95f),
                //^ if we can't be placed on the floor, default to table height.
                Holding.Dir, vm.Context);
            }

            for (int i = 0; i < Holding.Group.Objects.Count; i++)
            {
                var target = Holding.Group.Objects[i];
                var tpos = target.VisualPosition;
                tpos.Z = (World.State.Level - 1) * 2.95f;
                Holding.CursorTiles[i].MultitileGroup.SetVisualPosition(tpos, Holding.Dir, vm.Context);
            }
            Holding.CanPlace = status;
        }

        public void ClearSelected()
        {
            //TODO: selected items are only spooky ghosts of the items themselves.
            //      ...so that they dont cause serverside desyncs
            //      and so that clearing selections doesnt delete already placed objects.
            if (Holding != null)
            {
                //try to copy back visible flags from holding copy
                if (Holding.RealEnt != null && UseNet)
                {
                    RecursiveUnhide(Holding.Group.BaseObject, Holding.RealEnt);
                }

                if (UseNet || !Holding.IsBought)
                {
                    RecursiveDelete(vm.Context, Holding.Group.BaseObject);
                } else
                {
                    if (Holding.RealEnt == null) Holding.RealEnt = Holding.Group.BaseObject;
                    Holding.RealEnt.SetPosition(Holding.PreviousTile, Holding.Dir, vm.Context);
                    Holding.RealEnt.MultitileGroup.ExecuteEntryPoint(11, vm.Context); //User Placement
                }

                for (int i = 0; i < Holding.CursorTiles.Length; i++)
                {
                    Holding.CursorTiles[i].Delete(true, vm.Context);
                    ((ObjectComponent)Holding.CursorTiles[i].WorldUI).ForceDynamic = false;
                }
            }
            Holding = null;
            vm.Tick();
        }

        private void RecursiveDelete(VMContext context, VMEntity real)
        {
            var rgrp = real.MultitileGroup;
            for (int i = 0; i < rgrp.Objects.Count; i++)
            {
                var slots = rgrp.Objects[i].TotalSlots();
                var objs = new List<VMEntity>();
                for (int j = 0; j < slots; j++)
                {
                    var slot = rgrp.Objects[i].GetSlot(j);
                    if (slot != null)
                    {
                        objs.Add(slot);
                    }

                }
                foreach (var obj in objs) RecursiveDelete(context, obj);
            }
            rgrp.Delete(context);
        }

        public void MouseDown(UpdateState state)
        {
            MouseIsDown = true;
            MouseDownX = state.MouseState.X;
            MouseDownY = state.MouseState.Y;
            if (Holding != null)
            {
                Rotation = Holding.Dir;
                DirChanged = false;
            }
        }

        public void MouseUp(UpdateState state)
        {
            MouseIsDown = false;
            if (Holding != null && Holding.Clicked)
            {
                if (Holding.CanPlace == VMPlacementError.Success)
                {
                    HITVM.Get().PlaySoundEvent((Holding.IsBought) ? UISounds.ObjectMovePlace : UISounds.ObjectPlace);
                    //ExecuteEntryPoint(11); //User Placement
                    var putDown = Holding;
                    var pos = Holding.Group.BaseObject.Position;
                    if (Holding.IsBought)
                    {
                        if (UseNet)
                        {
                            vm.SendCommand(new VMNetMoveObjectCmd
                            {
                                ObjectID = Holding.MoveTarget,
                                dir = Holding.Dir,
                                level = pos.Level,
                                x = pos.x,
                                y = pos.y
                            });
                        }
                        else
                        {
                            //otherwise we're kind of already here?
                            Holding.PreviousTile = new LotTilePos(pos.x, pos.y, pos.Level);
                        }
                    }
                    else if (Holding.InventoryPID > 0)
                    {
                        vm.SendCommand(new VMNetPlaceInventoryCmd
                        {
                            ObjectPID = Holding.InventoryPID,
                            dir = Holding.Dir,
                            level = pos.Level,
                            x = pos.x,
                            y = pos.y
                        });
                    }
                    else
                    {
                        var GUID = (Holding.Group.MultiTile) ? Holding.Group.BaseObject.MasterDefinition.GUID : Holding.Group.BaseObject.Object.OBJ.GUID;
                        if (UseNet || ParentControl.ActiveEntity != null)
                        {
                            vm.SendCommand(new VMNetBuyObjectCmd
                            {
                                GUID = GUID,
                                dir = Holding.Dir,
                                level = pos.Level,
                                x = pos.x,
                                y = pos.y
                            });
                        } else
                        {
                            Holding.MoveTarget = Holding.Group.BaseObject.ObjectID;
                            Holding.PreviousTile = new LotTilePos(pos.x, pos.y, pos.Level);
                        }
                    }
                    ClearSelected();
                    if (OnPutDown != null) OnPutDown(putDown, state); //call this after so that buy mode etc can produce more.
                }
                else
                {

                }
            }

            state.UIState.TooltipProperties.Show = false;
            state.UIState.TooltipProperties.Opacity = 0;
            ShowTooltip = false;
        }

        private void ExecuteEntryPoint(int num)
        {
            for (int i = 0; i < Holding.Group.Objects.Count; i++) Holding.Group.Objects[i].ExecuteEntryPoint(num, vm.Context, true);
        }

        public void SellBack(UIElement button)
        {
            if (Holding == null || !Roommate) return;
            if (Holding.IsBought)
            {
                if (Holding.CanDelete)
                {
                    vm.SendCommand(new VMNetDeleteObjectCmd
                    {
                        ObjectID = Holding.MoveTarget,
                        CleanupAll = true
                    });
                    HITVM.Get().PlaySoundEvent(UISounds.MoneyBack);
                }
                else
                {
                    ShowErrorAtMouse(LastState, VMPlacementError.CannotDeleteObject);
                    return;
                }
            }
            OnDelete?.Invoke(Holding, null); //TODO: cleanup callbacks which don't need updatestate into another delegate.
            ClearSelected();
        }

        public void Cancel()
        {
            Holding.Dir = Holding.OriginalDir;
            OnDelete?.Invoke(Holding, null);
            ClearSelected();
        }

        public void Update(UpdateState state, bool scrolled)
        {
            UseNet = true;
            LastState = state;
            if (ShowTooltip) state.UIState.TooltipProperties.UpdateDead = false;
            MouseClicked = (MouseIsDown && (!MouseWasDown));

            CursorType cur = CursorType.SimsMove;
            if (Holding != null)
            {
                if (Roommate) cur = CursorType.SimsPlace;
                if (state.KeyboardState.IsKeyDown(Keys.Delete))
                {
                    SellBack(null);
                }
                else if (state.KeyboardState.IsKeyDown(Keys.Escape))
                {
                    Cancel();
                }
            }
            if (Holding != null && Roommate)
            {
                if (MouseClicked) Holding.Clicked = true;
                var mpos = FSOEnvironment.SoftwareKeyboard ? new Point(UIScreen.Current.ScreenWidth / 2, UIScreen.Current.ScreenHeight / 2) : state.MouseState.Position;
                if (MouseIsDown && Holding.Clicked)
                {
                    bool updatePos = MouseClicked;
                    int xDiff = mpos.X - MouseDownX;
                    int yDiff = mpos.Y - MouseDownY;
                    cur = CursorType.SimsRotate;
                    if (Math.Sqrt(xDiff * xDiff + yDiff * yDiff) > 64 && !FSOEnvironment.SoftwareKeyboard)
                    {
                        var from = World.EstTileAtPosWithScroll(new Vector2(MouseDownX, MouseDownY));
                        var target = World.EstTileAtPosWithScroll(mpos.ToVector2());

                        var vec = target - from;
                        var dir = Math.Atan2(vec.Y, vec.X);
                        dir += Math.PI / 2;
                        if (dir < 0) dir += Math.PI * 2;
                        var newDir = (Direction)(1 << (((int)Math.Round(dir / (Math.PI / 2)) % 4) * 2));

                        if (newDir != Holding.Dir || MouseClicked)
                        {
                            updatePos = true;
                            HITVM.Get().PlaySoundEvent(UISounds.ObjectRotate);
                            Holding.Dir = newDir;
                            DirChanged = true;
                        }
                    }
                    if (updatePos)
                    {
                        MoveSelected(Holding.TilePos, Holding.Level);
                        if (!Holding.IsBought && Holding.CanPlace == VMPlacementError.Success &&
                            ParentControl.ActiveEntity != null && ParentControl.Budget < Holding.Price)
                            Holding.CanPlace = VMPlacementError.InsufficientFunds;
                        if (Holding.CanPlace != VMPlacementError.Success)
                        {
                            state.UIState.TooltipProperties.Show = true;
                            state.UIState.TooltipProperties.Color = Color.Black;
                            state.UIState.TooltipProperties.Opacity = 1;
                            state.UIState.TooltipProperties.Position = new Vector2(MouseDownX,
                                MouseDownY);
                            state.UIState.Tooltip = GameFacade.Strings.GetString("137", ((int)Holding.CanPlace).ToString()); //"kPErr" + Holding.CanPlace.ToString()
                                //+ ((Holding.CanPlace == VMPlacementError.CannotPlaceComputerOnEndTable) ? "," : ""));
                            // comma added to curcumvent problem with language file. We should probably just index these with numbers?
                            state.UIState.TooltipProperties.UpdateDead = false;
                            ShowTooltip = true;
                            HITVM.Get().PlaySoundEvent(UISounds.Error);
                        }
                        else
                        {
                            state.UIState.TooltipProperties.Show = false;
                            state.UIState.TooltipProperties.Opacity = 0;
                            ShowTooltip = false;
                        }
                    }
                }
                else
                {
                    var tilePos = World.EstTileAtPosWithScroll(new Vector2(mpos.X, mpos.Y) / FSOEnvironment.DPIScaleFactor) + Holding.TilePosOffset;
                    MoveSelected(tilePos, 1);
                }
            }
            else if (MouseClicked)
            {
                //not holding an object, but one can be selected
                var newHover = World.GetObjectIDAtScreenPos(state.MouseState.X, state.MouseState.Y, GameFacade.GraphicsDevice);
                if (MouseClicked && (newHover != 0) && (vm.GetObjectById(newHover) is VMGameObject))
                {
                    var objGroup = vm.GetObjectById(newHover).MultitileGroup;
                    var objBasePos = objGroup.BaseObject.Position;
                    var success = (Roommate || objGroup.SalePrice > -1) ? objGroup.BaseObject.IsUserMovable(vm.Context, false) : VMPlacementError.ObjectNotOwnedByYou;
                    if (GameFacade.EnableMod) success = VMPlacementError.Success;
                    if (objBasePos.Level != World.State.Level) success = VMPlacementError.CantEffectFirstLevelFromSecondLevel;
                    if (success == VMPlacementError.Success)
                    {
                        var ghostGroup = (UseNet) ? vm.Context.GhostCopyGroup(objGroup) : objGroup ;
                        ghostGroup.ChangePosition(objGroup.BaseObject.Position, objGroup.BaseObject.Direction, vm.Context, VMPlaceRequestFlags.Default);
                        var canDelete = GameFacade.EnableMod || (objGroup.BaseObject.IsUserMovable(vm.Context, true)) == VMPlacementError.Success;
                        SetSelected(ghostGroup);

                        Holding.RealEnt = objGroup.BaseObject;
                        if (UseNet) RecursiveHide(Holding.RealEnt);
                        Holding.CanDelete = canDelete;
                        Holding.MoveTarget = newHover;
                        var estBase = state.MouseState.Position; //FSOEnvironment.SoftwareKeyboard ? new Point(UIScreen.Current.ScreenWidth / 2, UIScreen.Current.ScreenHeight / 2) : state.MouseState.Position;
                        Holding.TilePosOffset = new Vector2(objBasePos.x / 16f, objBasePos.y / 16f) - World.EstTileAtPosWithScroll(new Vector2(estBase.X, estBase.Y) / FSOEnvironment.DPIScaleFactor);
                        if (OnPickup != null) OnPickup(Holding, state);
                        //ExecuteEntryPoint(12); //User Pickup
                        if (FSOEnvironment.SoftwareKeyboard) MouseIsDown = false;
                    }
                    else
                    {
                        ShowErrorAtMouse(state, success);
                    }
                }
            }

            if (ParentControl.MouseIsOn && !ParentControl.RMBScroll)
            {
                GameFacade.Cursor.SetCursor(cur);
            }

            MouseWasDown = MouseIsDown;
        }

        private void RecursiveHide(VMEntity ent)
        {
            ent.MultitileGroup.Objects.ForEach((x) => {
                x.SetValue(VMStackObjectVariable.Hidden, 1);

                var slots = x.TotalSlots();
                for (int i=0; i<slots; i++)
                {
                    var slot = x.GetSlot(i);
                    if (slot != null && slot.GetValue(VMStackObjectVariable.Hidden) == 0) RecursiveHide(slot);
                }
                });
        }

        private void RecursiveUnhide(VMEntity fake, VMEntity real)
        {
            var rgrp = real.MultitileGroup;
            for (int i = 0; i < rgrp.Objects.Count; i++)
            {
                rgrp.Objects[i].SetValue(VMStackObjectVariable.Hidden, fake.MultitileGroup.Objects[i].GetValue(VMStackObjectVariable.Hidden));
                var slots = rgrp.Objects[i].TotalSlots();
                for (int j = 0; j < slots; j++)
                {
                    var slot = rgrp.Objects[i].GetSlot(j);
                    var slot2 = fake.GetSlot(j);
                    if (slot != null && slot2 != null && slot.GetValue(VMStackObjectVariable.Hidden) != slot2.GetValue(VMStackObjectVariable.Hidden)) 
                        RecursiveUnhide(slot2, slot);
                }
            }
        }

        private void ShowErrorAtMouse(UpdateState state, VMPlacementError error)
        {
            if (Holding == null) return;
            state.UIState.TooltipProperties.Show = true;
            state.UIState.TooltipProperties.Color = Color.Black;
            state.UIState.TooltipProperties.Opacity = 1;
            state.UIState.TooltipProperties.Position = new Vector2(MouseDownX,
                MouseDownY);
            state.UIState.Tooltip = GameFacade.Strings.GetString("137", ((int)Holding.CanPlace).ToString());
            state.UIState.TooltipProperties.UpdateDead = false;
            ShowTooltip = true;
            HITVM.Get().PlaySoundEvent(UISounds.Error);
        }

        public void RotateObject(int notches)
        {
            Direction newDir;
            if (notches > 0)
            {
                var dir = (int)Holding.Dir << notches;
                if (dir > 255) dir >>= 8;
                newDir = (Direction)dir;
            } else
            {
                var dir = ((int)Holding.Dir << 8) >> (-notches);
                if (dir > 255) dir >>= 8;
                newDir = (Direction)dir;
            }
            HITVM.Get().PlaySoundEvent(UISounds.ObjectRotate);
            Holding.Dir = newDir;
            DirChanged = true;
        }

        public delegate void HolderEventHandler(UIObjectSelection holding, UpdateState state);
    }

    public class UIObjectSelection
    {
        public short MoveTarget = 0;

        public VMMultitileGroup Group;
        public VMEntity[] CursorTiles;
        public LotTilePos PreviousTile;
        public Direction OriginalDir = Direction.NORTH;
        public Direction Dir = Direction.NORTH;
        public Vector2 TilePos;
        public Vector2 TilePosOffset;
        public bool Clicked;
        public VMPlacementError CanPlace;
        public sbyte Level;
        public int Price;
        public uint InventoryPID = 0;
        public bool CanDelete;
        public VMEntity RealEnt;

        public bool IsBought
        {
            get
            {
                return (MoveTarget != 0);
            }
        }
    }
}
