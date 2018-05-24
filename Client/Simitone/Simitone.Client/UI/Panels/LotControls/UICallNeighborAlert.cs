using FSO.Client.UI.Framework;
using FSO.Content;
using FSO.LotView.Model;
using FSO.SimAntics;
using FSO.SimAntics.Model;
using Simitone.Client.UI.Controls;
using Simitone.Client.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simitone.Client.UI.Panels.LotControls
{
    public class UICallNeighborAlert : UIMobileDialog
    {
        public UICallNeighborPanel NPanel;
        public short SelectedNeighbour
        {
            get
            {
                return NPanel.SelectedNeighbour;
            }
        }
        
        public event Action<int> OnResult;

        public UICallNeighborAlert(short callerNID, VM vm)
        {
            Caption = "Call Neighbour";
            SetHeight(490);
            NPanel = new UICallNeighborPanel(callerNID, vm);
            NPanel.Position = new Microsoft.Xna.Framework.Vector2((Width - 1030) / 2, 110);
            NPanel.OnResult += (res) => { OnResult?.Invoke(res); Close(); };
            Add(NPanel);
        }
    }

    public class UICallNeighborPanel : UIContainer
    {
        public Dictionary<short, List<short>> NeighborsByFamilyID = new Dictionary<short, List<short>>();
        public UITouchStringList FamilyList;
        public UITouchStringList NeighbourList;
        public UIAvatarSelectButton Icon;
        public UIBigButton CallButton;
        public int SelectedFamily = -1;
        public short SelectedNeighbour = -1;
        public VM VM;

        public event Action<int> OnResult;

        public UICallNeighborPanel(short callerNID, VM vm)
        {
            VM = vm;
            var nb = Content.Get().Neighborhood;
            var neigh = nb.GetNeighborByID(callerNID);
            var rels = neigh.Relationships.Keys;
            //var rels = nb.Neighbors.NeighbourByID.Keys;

            foreach (var to in rels)
            {
                var tn = nb.GetNeighborByID((short)to);
                var family = tn.PersonData?.ElementAt((int)VMPersonDataVariable.TS1FamilyNumber) ?? 0;
                var gender = tn.PersonData?.ElementAt((int)VMPersonDataVariable.Gender) ?? 0; //can't call pets
                if (family != 0 && gender < 2)
                {
                    List<short> famList = null;
                    if (!NeighborsByFamilyID.TryGetValue(family, out famList))
                    {
                        famList = new List<short>();
                        NeighborsByFamilyID[family] = famList;
                    }

                    famList.Add((short)to);
                }
            }

            FamilyList = new UITouchStringList();
            FamilyList.Size = new Microsoft.Xna.Framework.Vector2(320, 350);
            FamilyList.BackingList = NeighborsByFamilyID.Select(x => nb.GetFamilyString((ushort)x.Key).GetString(0)).ToList();
            FamilyList.Refresh();
            FamilyList.OnSelectionChange += FamilyList_OnSelectionChange;
            Add(FamilyList);

            NeighbourList = new UITouchStringList();
            NeighbourList.Size = new Microsoft.Xna.Framework.Vector2(320, 350);
            NeighbourList.Position = new Microsoft.Xna.Framework.Vector2(370, 0);
            NeighbourList.OnSelectionChange += NeighbourList_OnSelectionChange;
            Add(NeighbourList);

            var cancelButton = new UIBigButton(false);
            cancelButton.Caption = "Cancel";
            cancelButton.Position = new Microsoft.Xna.Framework.Vector2(370 + 385, 135);
            cancelButton.OnButtonClick += (btn) => { OnResult?.Invoke(-1); };
            cancelButton.Width = 275;
            Add(cancelButton);

            CallButton = new UIBigButton(true);
            CallButton.Caption = "Call";
            CallButton.Position = new Microsoft.Xna.Framework.Vector2(370 + 385, 255);
            CallButton.OnButtonClick += (btn) => { OnResult?.Invoke(SelectedNeighbour); };
            CallButton.Width = 275;
            Add(CallButton);

            NeighbourList_OnSelectionChange((NeighborsByFamilyID.Count==0)?-2:-1);

            OnResult += (res) => { CallButton.Disabled = true; cancelButton.Disabled = true; };
        }

        private void NeighbourList_OnSelectionChange(int obj)
        {
            if (Icon != null) { Remove(Icon); Icon = null; }
            if (obj == -1)
            {
                SelectedNeighbour = -1;
                CallButton.Disabled = true;
            } else
            {
                SelectedNeighbour = NeighborsByFamilyID.ElementAt(SelectedFamily).Value[obj];
                CallButton.Disabled = false;

                var guid = Content.Get().Neighborhood.GetNeighborByID(SelectedNeighbour).GUID;
                var temp = VM.Context.CreateObjectInstance(guid, LotTilePos.OUT_OF_WORLD, Direction.NORTH, true);
                Icon = new UIAvatarSelectButton(UIIconCache.GetObject(temp.BaseObject));
                Icon.Position = new Microsoft.Xna.Framework.Vector2(892, 60);
                Add(Icon);
                temp.Delete(VM.Context);
            }
        }

        private void FamilyList_OnSelectionChange(int obj)
        {
            var nb = Content.Get().Neighborhood;
            //populate the rightmost list with the selected family
            NeighbourList.BackingList.Clear();
            if (obj != -1)
            {
                var people = NeighborsByFamilyID.ElementAt(obj).Value;
                NeighbourList.BackingList = 
                    people.Select(x => {
                        var guid = nb.GetNeighborByID(x).GUID;
                        var gobj = Content.Get().WorldObjects.Get(guid);
                        if (gobj == null) return "Unknown";
                        return gobj.Resource.Get<FSO.Files.Formats.IFF.Chunks.CTSS>(gobj.OBJ.CatalogStringsID)?.GetString(0) ?? "Unknown";
                        }
                    ).ToList();
            }
            NeighbourList.Refresh();
            NeighbourList_OnSelectionChange(-1);
            SelectedFamily = obj;
        }
    }
}
