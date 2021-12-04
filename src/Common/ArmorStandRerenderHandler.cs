using System;
using DoffAndDonAgain.Common.Network;
using DoffAndDonAgain.Utility;
using ProperVersion;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace DoffAndDonAgain.Common {
  public class ArmorStandRerenderHandler {
    private DoffAndDonSystem System { get; }
    private bool shouldSkipRerender;

    private static readonly SemVer armorStandRenderFixVersion = SemVer.Parse("1.15.0-rc.4");

    public ArmorStandRerenderHandler(DoffAndDonSystem system) {
      System = system;
      if (System.Side == EnumAppSide.Client) {
        System.ClientChannel.SetMessageHandler<ArmorStandInventoryUpdatedPacket>(OnArmorStandInventoryUpdatedPacket);
      }
      var gameVersion = SemVer.Parse(GameVersion.OverallVersion);
      shouldSkipRerender = gameVersion >= armorStandRenderFixVersion;
    }

    private void OnArmorStandInventoryUpdatedPacket(ArmorStandInventoryUpdatedPacket packet) {
      var playerEntity = System.ClientAPI.World.Player.Entity;
      var armorStand = playerEntity?.GetEntityArmorStandById(packet.ArmorStandEntityId, 100, 100);
      UpdateRender(armorStand);
    }

    public void UpdateRender(EntityArmorStand armorStand) {
      if (shouldSkipRerender || armorStand == null) { return; }
      if (armorStand.World.Side == EnumAppSide.Server) {
        armorStand.WatchedAttributes.MarkAllDirty();
        Action<float> broadcastCallback = (float timePassed) => { System.ServerChannel.BroadcastPacket(new ArmorStandInventoryUpdatedPacket(armorStand.EntityId)); };
        armorStand.World.RegisterCallback(broadcastCallback, 500);
      }
      else if (armorStand.IsRendered) {
        armorStand.OnEntityLoaded();
      }
    }
  }
}
