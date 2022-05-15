using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRageMath;
using VRage.Game.ModAPI;
using Sandbox.ModAPI;
using Global.API;
using Sandbox.Common.ObjectBuilders;
using IMyUpgradeModule = Sandbox.ModAPI.Ingame.IMyUpgradeModule;

namespace JumpDriveInhibitor
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Beacon), false,
        "FSDInhibitor",
        "FSDInhibitor_Enhanced",
        "FSDInhibitor_Proficient",
        "FSDInhibitor_Elite"
    )]
    public class JumpDriveInhibitorBlock : MyGameLogicComponent
    {
        private VRage.ObjectBuilders.MyObjectBuilder_EntityBase _objectBuilder;
        private Sandbox.ModAPI.Ingame.IMyBeacon _beacon;
        private IMyEntity _parentGrid;
        private System.IO.TextWriter logger = null;

        private bool _logicEnabled = false;

        public override void Close()
        {
        }

        public override void Init(VRage.ObjectBuilders.MyObjectBuilder_EntityBase objectBuilder)
        {
            _objectBuilder = objectBuilder;

            _beacon = Entity as Sandbox.ModAPI.Ingame.IMyBeacon;

            if (_beacon == null || !MyAPIGateway.Multiplayer.IsServer) return;

            _logicEnabled = true;
            NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
        }

        private readonly List<IMyCubeBlock> jumpDriveCache = new List<IMyCubeBlock>();

        public override void UpdateAfterSimulation10()
        {
            try
            {
                if (!_logicEnabled || _beacon == null || !_beacon.Enabled || !_beacon.IsWorking ||
                    !_beacon.IsFunctional) return;

                var sphere = new BoundingSphereD(_beacon.GetPosition(), _beacon.Radius);
                var l = MyAPIGateway.Entities.GetEntitiesInSphere(ref sphere);

                var parentGrid = _beacon.CubeGrid;
                if (_parentGrid == null)
                    _parentGrid = MyAPIGateway.Entities.GetEntityById(parentGrid.EntityId);

                if (parentGrid == null) return;

                foreach (var grid in l.OfType<IMyCubeGrid>())
                {
                    GlobalModApi.GetAllBlocksOfType(jumpDriveCache, grid, typeof(MyObjectBuilder_UpgradeModule));

                    foreach (var b in jumpDriveCache)
                    {
                        if (!b.IsFunctional || !b.IsWorking) continue;
                        var jumpDrive = b as IMyUpgradeModule;
                        if (jumpDrive == null || !jumpDrive.Enabled) continue;

                        var slim = b.SlimBlock;
                        var damage = grid.GridSizeEnum.Equals(MyCubeSize.Large) ? 0.5f : 0.05f;
                        slim.DecreaseMountLevel(damage, null, true);
                        slim.ApplyAccumulatedDamage();

                        jumpDrive.Enabled = false;
                    }

                    jumpDriveCache.Clear();
                }
            }
            catch (Exception)
            {
                MyAPIGateway.Utilities.ShowMessage("Jumpdrive Inhibitor", "An error happened in the mod");
            }
        }

        public override VRage.ObjectBuilders.MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return _objectBuilder;
        }
    }
}