using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;

namespace Global.API
{
    public class GlobalModApi
    {
        public static bool IsRunningGlobal()
        {
            return false;
        }

        public static void GetAllBlocksOfType(List<IMyCubeBlock> list, IMyCubeGrid grid, MyObjectBuilderType type)
        {
            list.AddRange(grid.GetFatBlocks<IMyCubeBlock>().Where(block => block.BlockDefinition.TypeId == type));
        }

        public static void GetAllBlocksOfTypeId(List<IMyCubeBlock> list, long gridId, MyObjectBuilderType type)
        {
            var grid = MyAPIGateway.Entities.GetEntityById(gridId) as IMyCubeGrid;
            if (grid == null) return;
            list.AddRange(grid.GetFatBlocks<IMyCubeBlock>().Where(block => block.BlockDefinition.TypeId == type));
        }
    }
}