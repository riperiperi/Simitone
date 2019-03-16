using FSO.Client;
using FSO.LotView;
using FSO.LotView.Model;
using FSO.SimAntics;
using FSO.SimAntics.Engine.TSOTransaction;
using FSO.SimAntics.Model;
using FSO.SimAntics.NetPlay.Drivers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simitone.Client.Utils
{
    public static class PersonGeneratorHelper
    {
        private static VM TempVM;
        private static void InitVM()
        {
            var world = new World(GameFacade.GraphicsDevice);
            world.Initialize(GameFacade.Scenes);
            var context = new VMContext(world);

            TempVM = new VM(context, new VMServerDriver(new VMTSOGlobalLinkStub()), new VMNullHeadlineProvider());
            TempVM.Init();

            var blueprint = new Blueprint(3, 3);
            world.InitBlueprint(blueprint);
            context.Blueprint = blueprint;
            context.Architecture = new VMArchitecture(3, 3, blueprint, TempVM.Context);
            blueprint.Terrain = new FSO.LotView.Components.TerrainComponent(new Rectangle(0, 0, 3, 3), blueprint);
            TempVM.Tick();
        }

        public static short[] PreparePersonDataFromObject(uint guid)
        {
            if (TempVM == null) InitVM();

            var obj = TempVM.Context.CreateObjectInstance(guid, LotTilePos.OUT_OF_WORLD, Direction.NORTH)?.BaseObject as VMAvatar;

            if (obj == null) return new short[88];
            var result = obj.GetPersonDataClone();
            obj.Delete(true, TempVM.Context);

            return result.Take(88).ToArray();
        }
    }
}
