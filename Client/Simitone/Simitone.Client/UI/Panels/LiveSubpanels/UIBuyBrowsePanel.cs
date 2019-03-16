using FSO.Client.UI.Controls.Catalog;
using FSO.Content;
using FSO.Content.Interfaces;
using Microsoft.Xna.Framework;
using Simitone.Client.UI.Controls;
using Simitone.Client.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.Common.Rendering.Framework.Model;
using FSO.Client.UI.Framework;
using FSO.Client.UI.Controls;
using FSO.Client;
using Simitone.Client.UI.Model;
using FSO.Client.UI.Panels.LotControls;
using FSO.Client.UI.Model;
using FSO.LotView.Model;

namespace Simitone.Client.UI.Panels.LiveSubpanels
{
    public class UIBuyBrowsePanel : UISubpanel
    {
        public UITouchScroll CatContainer;
        public List<UICatalogElement> FullCategory;
        public IEnumerable<UICatalogElement> FilterCategory;
        public UICatalogMode Mode;
        public List<UICatButton> SelButtons = new List<UICatButton>();
        public List<UILabel> SelLabels = new List<UILabel>();
        public sbyte Category;
        public bool ChoosingSub;
        public int ItemID = -1;

        public static int[] RemapString = new int[]
        {
            0, //seat
            1, //surf
            4, //
            3,
            5,
            2,
            7,
            6
        };

        public static List<List<UICatalogSubcat>> Categories;
        public static string[][] CatIcons = new string[][]
        {
            new string[]
            {
                "seat_dine",
                "seat_loun",
                "seat_sofa",
                "seat_beds"
            },
            new string[]
            {
                "surf_count",
                "surf_tabl",
                "surf_endt",
                "surf_desk"
            },
            new string[]
            {
                "appl_stov",
                "appl_frig",
                "appl_smal",
                "appl_larg"
            },
            new string[]
            {
                "elec_ent",
                "elec_vide",
                "elec_audi",
                "elec_phon"
            },

            new string[]
            {
                "plum_toil",
                "plum_show",
                "plum_sink",
                "plum_hott"
            },

            new string[]
            {
                "deco_pain",
                "deco_scul",
                "deco_rugs",
                "deco_plan"
            },
            new string[]
            {
                "misc_recr",
                "misc_know",
                "misc_crea",
                "misc_ward",
                "misc_pets",
                "misc_pets",
                "misc_magi",
            },
            new string[]
            {
                "ligh_tabl",
                "ligh_stan",
                "ligh_wall",
                "ligh_hang"
            },
        };

        public static List<List<UICatalogSubcat>> BuildCategories = new List<List<UICatalogSubcat>>()
        {
            new List<UICatalogSubcat>() //architecture
            {
                new UICatalogSubcat() { MaskBit = 7+6, StrTable = 139, StrInd = 2}, //wall
                new UICatalogSubcat() { MaskBit = 7+8, StrTable = 139, StrInd = 3}, //wallpaper
                new UICatalogSubcat() { MaskBit = 7+9, StrTable = 139, StrInd = 7}, //floor
                new UICatalogSubcat() { MaskBit = 7+10, StrTable = 139, StrInd = 10}, //roof
            },
            new List<UICatalogSubcat>() //outdoors
            {
                new UICatalogSubcat() { MaskBit = 7+4, StrTable = 139, StrInd = 6}, //trees
                new UICatalogSubcat() { MaskBit = 7+11, StrTable = 139, StrInd = 0}, //terrain
                new UICatalogSubcat() { MaskBit = 7+7, StrTable = 139, StrInd = 1}, //water
            },
            new List<UICatalogSubcat>() //objects
            {
                new UICatalogSubcat() { MaskBit = 7+1, StrTable = 139, StrInd = 8}, //door
                new UICatalogSubcat() { MaskBit = 7+2, StrTable = 139, StrInd = 9}, //window
                new UICatalogSubcat() { MaskBit = 7+3, StrTable = 139, StrInd = 4}, //staircase
                new UICatalogSubcat() { MaskBit = 7+5, StrTable = 139, StrInd = 5}, //fireplaces
            },
        };

        public static string[][] BuildIcons = new string[][]
        {
            new string[]
            {
                "build_wall",
                "build_walp",
                "build_flor",
                "build_roof",
            },
            new string[]
            {
                "build_tree",
                "build_terr",
                "build_watr",
            },
            new string[]
            {
                "build_door",
                "build_wind",
                "build_stai",
                "build_fire"
            },
        };

        public static Dictionary<UICatalogMode, List<UICatalogSubcat>> DTCategories = new Dictionary<UICatalogMode, List<UICatalogSubcat>>()
        {
            {
                UICatalogMode.Downtown,
                new List<UICatalogSubcat>()
                {
                    new UICatalogSubcat() { MaskBit = 0, StrTable = 150, StrInd = 16}, //food
                    new UICatalogSubcat() { MaskBit = 1, StrTable = 150, StrInd = 17}, //shops
                    new UICatalogSubcat() { MaskBit = 2, StrTable = 150, StrInd = 18}, //outside
                    new UICatalogSubcat() { MaskBit = 3, StrTable = 150, StrInd = 19}, //street
                }
            },

            {
                UICatalogMode.Community,
                new List<UICatalogSubcat>()
                {
                    new UICatalogSubcat() { MaskBit = 0, StrTable = 150, StrInd = 32}, //food
                    new UICatalogSubcat() { MaskBit = 1, StrTable = 150, StrInd = 33}, //shops
                    new UICatalogSubcat() { MaskBit = 2, StrTable = 150, StrInd = 34}, //outside
                    new UICatalogSubcat() { MaskBit = 3, StrTable = 150, StrInd = 35}, //street
                }
            },

            {
                UICatalogMode.Vacation,
                new List<UICatalogSubcat>()
                {
                    new UICatalogSubcat() { MaskBit = 0, StrTable = 150, StrInd = 24}, //lodging
                    new UICatalogSubcat() { MaskBit = 1, StrTable = 150, StrInd = 25}, //shops
                    new UICatalogSubcat() { MaskBit = 2, StrTable = 150, StrInd = 26}, //recreation
                    new UICatalogSubcat() { MaskBit = 3, StrTable = 150, StrInd = 27}, //ameneties
                }
            },

            {
                UICatalogMode.Studiotown,
                new List<UICatalogSubcat>()
                {
                    new UICatalogSubcat() { MaskBit = 0, StrTable = 150, StrInd = 40}, //food
                    new UICatalogSubcat() { MaskBit = 1, StrTable = 150, StrInd = 41}, //shops
                    new UICatalogSubcat() { MaskBit = 2, StrTable = 150, StrInd = 42}, //studio
                    new UICatalogSubcat() { MaskBit = 3, StrTable = 150, StrInd = 43}, //spa
                }
            },

            {
                UICatalogMode.Magictown,
                new List<UICatalogSubcat>()
                {
                    new UICatalogSubcat() { MaskBit = 0, StrTable = 150, StrInd = 16}, //food
                    new UICatalogSubcat() { MaskBit = 1, StrTable = 150, StrInd = 17}, //shops
                    new UICatalogSubcat() { MaskBit = 2, StrTable = 150, StrInd = 44}, //magico
                    new UICatalogSubcat() { MaskBit = 3, StrTable = 150, StrInd = 18}, //outside
                }
            },
        };

        public static Dictionary<UICatalogMode, List<string>> DTIcons = new Dictionary<UICatalogMode, List<string>>()
        {
            {
                UICatalogMode.Downtown,
                new List<string>()
                {
                    "dt_food", //food
                    "dt_shop", //shops
                    "dt_out", //outside
                    "dt_street", //street
                }
            },

            {
                UICatalogMode.Community,
                new List<string>()
                {
                    "dt_food", //food
                    "dt_shop", //shops
                    "dt_out", //outside
                    "dt_street", //street
                }
            },

            {
                UICatalogMode.Vacation,
                new List<string>()
                {
                    "vac_lodg", //food
                    "dt_shop", //shops
                    "vac_recr", //recreation
                    "vac_amen", //amenities
                }
            },

            {
                UICatalogMode.Studiotown,
                new List<string>()
                {
                    "dt_food", //food
                    "dt_shop", //shops
                    "st_studio", //studio
                    "st_spa", //spa
                }
            },

            {
                UICatalogMode.Magictown,
                new List<string>()
                {
                    "dt_food", //food
                    "dt_shop", //shops
                    "misc_magi", //magic
                    "dt_out", //outside
                }
            },
        };

        static UIBuyBrowsePanel()
        {
            Categories = new List<List<UICatalogSubcat>>();

            for (int i=0; i<8; i++)
            {
                //init buy categories.
                var cat = new List<UICatalogSubcat>();
                var modi = RemapString[i];
                for (int j=0; j<4; j++)
                {
                    cat.Add(new UICatalogSubcat()
                    {
                        StrTable = 200 + modi,
                        MaskBit = j,
                        StrInd = j
                    });
                }

                if (i == 6)
                {
                    cat.Add(new UICatalogSubcat()
                    {
                        StrTable = 210,
                        MaskBit = 5,
                        StrInd = 3, //pets
                    });

                    cat.Add(new UICatalogSubcat()
                    {
                        StrTable = 210,
                        MaskBit = 6,
                        StrInd = 4, //magic
                    });
                }

                cat.Add(new UICatalogSubcat()
                {
                    StrTable = 210,
                    MaskBit = 7,
                    StrInd = 1, //other
                });

                cat.Add(new UICatalogSubcat()
                {
                    StrTable = 210,
                    MaskBit = 8,
                    StrInd = 2, //all
                });
                Categories.Add(cat);
            }
        }

        public bool HoldingEvents;

        public UIBuyBrowsePanel(TS1GameScreen screen, sbyte category, UICatalogMode mode) : base(screen) {
            CatContainer = new UITouchScroll(() => FilterCategory?.Count() ?? 0, CatalogElemProvider);
            CatContainer.ItemWidth = 90;
            CatContainer.DrawBounds = false;
            CatContainer.Margin = 15;
            CatContainer.SetScroll(-15);
            CatContainer.Size = new Vector2(775, 128);
            Category = category;

            Add(CatContainer);
            Mode = mode;
            GameResized();

            InitCategory(category, false);

            screen.LotControl.ObjectHolder.OnPickup += ObjectHolder_OnPickup;
            screen.LotControl.ObjectHolder.OnPutDown += ObjectHolder_OnPutDown;
            screen.LotControl.ObjectHolder.OnDelete += ObjectHolder_OnDelete;
            HoldingEvents = true;
        }

        private void ObjectHolder_OnDelete(UIObjectSelection holding, UpdateState state)
        {
            Game.Frontend.MainPanel.SetSubpanelPickup(1f);
            Game.LotControl.QueryPanel.Active = false;
            ItemID = -1;
        }

        private void ObjectHolder_OnPutDown(UIObjectSelection holding, UpdateState state)
        {
            Game.LotControl.QueryPanel.Active = false;
            Game.Frontend.MainPanel.SetSubpanelPickup(1f);
            if (ItemID != -1)
            {
                if (!holding.IsBought && (state.ShiftDown))
                {
                    //place another
                    var prevDir = holding.Dir;
                    Selected(ItemID);
                    Game.LotControl.QueryPanel.SetShown(false);
                    Game.LotControl.ObjectHolder.Holding.Dir = prevDir;
                }
                else
                {
                    ItemID = -1;
                }
            }
        }

        private void ObjectHolder_OnPickup(UIObjectSelection holding, UpdateState state)
        {
            Game.LotControl.PickupPanel.SetInfo(Game.LotControl.vm, holding.Group.BaseObject);
            Game.Frontend.MainPanel.SetSubpanelPickup(0f);
        }

        private void RemoveEvents()
        {
            if (HoldingEvents)
            {
                HoldingEvents = false;
                Game.LotControl.ObjectHolder.OnPickup -= ObjectHolder_OnPickup;
                Game.LotControl.ObjectHolder.OnPutDown -= ObjectHolder_OnPutDown;
                Game.LotControl.ObjectHolder.OnDelete -= ObjectHolder_OnDelete;
                Game.LotControl.QueryPanel.Active = false;
                Game.Frontend.MainPanel.SetSubpanelPickup(1f);

                if (Game.LotControl.CustomControl != null)
                {
                    Game.LotControl.CustomControl.Release();
                    Game.LotControl.CustomControl = null;
                }

                if (Game.LotControl.ObjectHolder.Holding != null)
                {
                    //delete object that hasn't been placed yet
                    //TODO: all holding objects should obviously just be ghosts.
                    //Holder.Holding.Group.Delete(vm.Context);
                    Game.LotControl.ObjectHolder.ClearSelected();
                }
            }
        }

        public override void Kill()
        {
            RemoveEvents();

            base.Kill();
        }

        public void Selected(int itemID)
        {
            var holder = Game.LotControl.ObjectHolder;
            var control = Game.LotControl;
            holder.ClearSelected();
            var item = FilterCategory.ElementAt(itemID);

            //todo: check if over budget?

            // if (OldSelection != -1) Catalog.SetActive(OldSelection, false);
            //Catalog.SetActive(selection, true);

            if (control.CustomControl != null)
            {
                control.CustomControl.Release();
                control.CustomControl = null;
            }

            if (item.Special != null)
            {
                var res = item.Special.Res;
                var resID = item.Special.ResID;
                if (res != null && res.GetName(resID) != "")
                {
                    Game.LotControl.QueryPanel.SetInfo(res.GetThumb(resID), res.GetName(resID), res.GetDescription(resID), res.GetPrice(resID), res.DoDispose());
                    Game.LotControl.QueryPanel.Mode = 1;
                    //QueryPanel.Tab = 0;
                    Game.LotControl.QueryPanel.Active = true;
                    Game.LotControl.QueryPanel.SetShown(true);
                }
                control.CustomControl = (UICustomLotControl)Activator.CreateInstance(item.Special.Control, control.vm, control.World, control, item.Special.Parameters);
            }
            else
            {
                var BuyItem = control.vm.Context.CreateObjectInstance(item.Item.GUID, LotTilePos.OUT_OF_WORLD, Direction.NORTH, holder.UseNet);
                if (BuyItem.Objects.Count != 0)
                {
                    Game.LotControl.QueryPanel.SetInfo(Game.LotControl.vm, BuyItem.Objects[0], false);
                    Game.LotControl.QueryPanel.Mode = 1;
                    //QueryPanel.Tab = 0;
                    Game.LotControl.QueryPanel.Active = true;
                    Game.LotControl.QueryPanel.SetShown(true);
                    holder.SetSelected(BuyItem);
                }
            }

            ItemID = itemID;
        }

        public override void Removed()
        {
            RemoveEvents();
            base.Removed();
            //Catalog.UICatalogItem.ClearIconCache(); //might want to be careful here...
        }

        public void Reset()
        {
            GameFacade.Screens.Tween.To(CatContainer, 0.5f, new Dictionary<string, float>() { { "Opacity", 0f } }, TweenQuad.EaseOut);
            InitCategory(Category, false);
        }

        public void InitCategory(sbyte category, bool build)
        {
            //start by populating with entries from the catalog
            if (!build) ((UIMainPanel)Parent)?.Switcher?.MainButton?.RestoreImage();
            var catalog = Content.Get().WorldCatalog;

            var items = catalog.GetItemsByCategory(category);
            
            FullCategory = items.Select(x => new UICatalogElement()
            {
                Item = x,
                CalcPrice = (int)x.Price
            }).ToList();

            //pull from other categories

            if (category == 15) AddWallpapers();
            if (category == 14 || category == 16) AddFloors(category);
            if (category == 17) AddRoofs();
            if (category == 18) AddTerrainTools();

            FullCategory = FullCategory.OrderBy(x => (int)x.Item.Price).ToList();
            if (category == 13) AddWallStyles();
            //if we're not build mode, init the subcategory selection
            if (build) return;

            foreach (var btn in SelButtons) Remove(btn);
            foreach (var label in SelLabels) Remove(label);
            SelButtons.Clear();
            SelLabels.Clear();

            ChoosingSub = true;

            List<UICatalogSubcat> cats;
            if (Mode == UICatalogMode.Build) cats = BuildCategories[category];
            else if (Mode != UICatalogMode.Normal)
            {
                cats = DTCategories[Mode];
                if (cats.Count == 4) //haven't added other or all subcats yet
                {
                    cats.Add(new UICatalogSubcat()
                    {
                        StrTable = 210,
                        MaskBit = 7,
                        StrInd = 1, //other
                    });

                    cats.Add(new UICatalogSubcat()
                    {
                        StrTable = 210,
                        MaskBit = 8,
                        StrInd = 2, //all
                    });
                }
            }
            else
            {
                cats = Categories[category];
            }

            var boff = CatContainer.Size.X/(cats.Count + 0.5f) / 2f;

            for (int i=0; i<cats.Count; i++)
            {
                var cat = cats[i];
                var str = GameFacade.Strings.GetString(cat.StrTable.ToString(), cat.StrInd.ToString());

                var label = new UILabel();
                label.Caption = str;
                label.Alignment = TextAlignment.Middle | TextAlignment.Center;
                label.Wrapped = true;
                label.Position = new Vector2(boff * (1.5f+i*2) - (120/2), 106);
                label.Size = new Vector2(120, 1);
                label.CaptionStyle = label.CaptionStyle.Clone();
                label.CaptionStyle.Size = 12;
                label.CaptionStyle.Color = UIStyle.Current.Text;
                SelLabels.Add(label);
                Add(label);

                var name = "";
                if (Mode == UICatalogMode.Build) {
                    name = BuildIcons[category][i];
                }
                else if (Mode != UICatalogMode.Normal) {
                    if (cat.MaskBit == 7) name = "other";
                    else if (cat.MaskBit == 8) name = "all";
                    else name = DTIcons[Mode][cat.MaskBit];
                }
                else
                {
                    if (cat.MaskBit == 7) name = "other";
                    else if (cat.MaskBit == 8) name = "all";
                    else name = CatIcons[category][cat.MaskBit];
                }

                cat.IconName = name; 

                var subbutton = new UICatButton(Content.Get().CustomUI.Get("cat_"+name+".png").Get(GameFacade.GraphicsDevice));
                subbutton.OnButtonClick += (btn) => { InitSubcategory(cat); };
                subbutton.Position = new Vector2(boff * (1.5f + i * 2) - (65 / 2), 16);
                subbutton.Disabled = SubcatIsEmpty(cat);
                SelButtons.Add(subbutton);
                Add(subbutton);
            }

            //InitSubcategory(0);
        }

        private void AddFloors(sbyte category)
        {
            var res = new UICatalogFloorResProvider();

            var floors = Content.Get().WorldFloors.List();

            for (int i = 0; i < floors.Count; i++)
            {
                var floor = (FloorReference)floors[i];

                if ((category == 14) != (floor.ID >= 65534)) continue;
                FullCategory.Insert(0, new UICatalogElement
                {
                    Item = new ObjectCatalogItem()
                    {
                        Name = floor.Name,
                        Category = category,
                        Price = (uint)floor.Price,
                    },
                    Special = new UISpecialCatalogElement
                    {
                        Control = typeof(UIFloorPainter),
                        ResID = floor.ID,
                        Res = res,
                        Parameters = new List<int> { (int)floor.ID } //pattern
                    }
                });
            }
        }

        private void AddWallpapers()
        {
            var res = new UICatalogWallpaperResProvider();

            var walls = Content.Get().WorldWalls.List();

            for (int i = 0; i < walls.Count; i++)
            {
                var wall = (WallReference)walls[i];
                FullCategory.Insert(0, new UICatalogElement
                {
                    Item = new ObjectCatalogItem()
                    {
                        Name = wall.Name,
                        Category = 15,
                        Price = (uint)wall.Price,
                    },
                    Special = new UISpecialCatalogElement
                    {
                        Control = typeof(UIWallPainter),
                        ResID = wall.ID,
                        Res = res,
                        Parameters = new List<int> { (int)wall.ID } //pattern
                    }
                });
            }
        }

        private void AddRoofs()
        {
            var res = new UICatalogRoofResProvider();

            var total = Content.Get().WorldRoofs.Count;

            for (int i = 0; i < total; i++)
            {
                sbyte category = 17;
                FullCategory.Insert(0, new UICatalogElement
                {
                    Item = new ObjectCatalogItem()
                    {
                        Name = "",
                        Category = category,
                        Price = 0,
                    },
                    Special = new UISpecialCatalogElement
                    {
                        Control = typeof(UIRoofer),
                        ResID = (uint)i,
                        Res = res,
                        Parameters = new List<int> { i } //pattern
                    }
                });
            }
        }

        public static short[] WallStyleIDs =
        {
            0x1, //wall
            0x2, //picket fence
            0xD, //iron fence
            0xC, //privacy fence
            0xE //banisters
        };

        public static short[] WallStylePatterns =
        {
            0, //wall
            248, //picket fence
            250, //iron fence
            249, //privacy fence
            251, //banisters
        };

        private void AddWallStyles()
        {
            var res = new UICatalogWallResProvider();

            for (int i = 0; i < WallStyleIDs.Length; i++)
            {
                var walls = Content.Get().WorldWalls;
                var style = walls.GetWallStyle((ulong)WallStyleIDs[i]);
                FullCategory.Insert(i, new UICatalogElement
                {
                    Item = new ObjectCatalogItem()
                    {
                        Name = style.Name,
                        Category = 13,
                        Price = (uint)style.Price,
                    },
                    Special = new UISpecialCatalogElement
                    {
                        Control = typeof(UIWallPlacer),
                        ResID = (ulong)WallStyleIDs[i],
                        Res = res,
                        Parameters = new List<int> { WallStylePatterns[i], WallStyleIDs[i] } //pattern, style
                    }
                });
            }
        }

        private void AddTerrainTools()
        {
            var res = new UICatalogWallResProvider();

            FullCategory.Insert(0, new UICatalogElement
            {
                Item = new ObjectCatalogItem()
                {
                    Name = "Raise/Lower Terrain",
                    Category = 18,
                    Price = 1,
                },
                Special = new UISpecialCatalogElement
                {
                    Control = typeof(UITerrainRaiser),
                    ResID = 0,
                    Res = new UICatalogTerrainResProvider(),
                    Parameters = new List<int> { }
                }
            });

            FullCategory.Insert(0, new UICatalogElement
            {
                Item = new ObjectCatalogItem()
                {
                    Name = "Flatten Terrain",
                    Category = 18,
                    Price = 1,
                },
                Special = new UISpecialCatalogElement
                {
                    Control = typeof(UITerrainFlatten),
                    ResID = 1,
                    Res = new UICatalogTerrainResProvider(),
                    Parameters = new List<int> { }
                }
            });

            FullCategory.Insert(0, new UICatalogElement
            {
                Item = new ObjectCatalogItem()
                {
                    Name = "Grass Tool",
                    Category = 18,
                    Price = 1,
                },
                Special = new UISpecialCatalogElement
                {
                    Control = typeof(UIGrassPaint),
                    ResID = 2,
                    Res = new UICatalogTerrainResProvider(),
                    Parameters = new List<int> { }
                }
            });
        }

        public override void GameResized()
        {
            base.GameResized();
            CatContainer.Size = new Vector2(Size.X, 128);
            if (ChoosingSub) Reset();
        }

        public override void Update(UpdateState state)
        {
            Invalidate();
            var first = SelButtons.FirstOrDefault();
            if (first != null && first.Opacity == 0)
            {
                foreach (var btn in SelButtons) Remove(btn);
                foreach (var label in SelLabels) Remove(label);
                SelButtons.Clear();
                SelLabels.Clear();
            }
            base.Update(state);
        }

        public UITSContainer CatalogElemProvider(int index)
        {
            var elem = new Catalog.UICatalogItem(FilterCategory.ElementAt(index), this);
            return elem;
        }

        public byte GetSubsort(ObjectCatalogItem item)
        {
            switch (Mode)
            {
                case UICatalogMode.Downtown:
                    return item.DowntownSort;
                case UICatalogMode.Community:
                    return item.CommunitySort;
                case UICatalogMode.Vacation:
                    return item.VacationSort;
                case UICatalogMode.Studiotown:
                    return item.StudiotownSort;
                case UICatalogMode.Magictown:
                    return item.MagictownSort;
                default:
                    return item.Subsort;
            }
        }

        private bool SubcatIsEmpty(UICatalogSubcat cat)
        {
            var index = cat.MaskBit;
            if (Mode == UICatalogMode.Build)
            {
                return FullCategory.FirstOrDefault() == null;
            }
            else if (index == 8)
            {
                return !FullCategory.Any(x => (GetSubsort(x.Item)) > 0);
            }
            else
            {
                var mask = 1 << index;
                return !FullCategory.Any(x => (GetSubsort(x.Item) & mask) > 0);
            }
        }

        public void InitSubcategory(UICatalogSubcat cat)
        {
            var index = cat.MaskBit;
            if (!ChoosingSub) return;
            ChoosingSub = false;
            ((UIMainPanel)Parent).Switcher.Close();
            ((UIMainPanel)Parent).Switcher.MainButton.ReplaceImage(Content.Get().CustomUI.Get("cat_" + cat.IconName + ".png").Get(GameFacade.GraphicsDevice));

            foreach (var btn in SelButtons)
            {
                GameFacade.Screens.Tween.To(btn, 0.5f, new Dictionary<string, float>() { { "Opacity", 0f } }, TweenQuad.EaseOut);
            }
            foreach (var label in SelLabels)
            {
                GameFacade.Screens.Tween.To(label, 0.5f, new Dictionary<string, float>() { { "Opacity", 0f } }, TweenQuad.EaseOut);
            }

            CatContainer.Opacity = 0f;
            GameFacade.Screens.Tween.To(CatContainer, 0.5f, new Dictionary<string, float>() { { "Opacity", 1f } }, TweenQuad.EaseOut);

            if (Mode == UICatalogMode.Build)
            {
                InitCategory((sbyte)index, true);
                FilterCategory = FullCategory;
            }
            else if (index == 8)
            {
                FilterCategory = FullCategory.Where(x => (GetSubsort(x.Item)) > 0);
            }
            else
            {
                var mask = 1 << index;
                FilterCategory = FullCategory.Where(x => (GetSubsort(x.Item) & mask) > 0);
            }
            CatContainer.Reset();
        }
    }

    public enum UICatalogMode
    {
        Normal,
        Downtown,
        Community,
        Vacation,
        Studiotown,
        Magictown,
        Build
    }

    public class UICatalogElement
    {
        public ObjectCatalogItem Item;
        public int CalcPrice;
        public UISpecialCatalogElement Special;
    }

    public class UISpecialCatalogElement
    {
        public Type Control;
        public ulong ResID;
        public UICatalogResProvider Res;
        public List<int> Parameters;
    }

    public class UICatalogSubcat
    {
        public int StrTable;
        public int StrInd;
        public int MaskBit;
        public string IconName;
    }
}
