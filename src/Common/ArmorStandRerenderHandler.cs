using DoffAndDonAgain.Common.Network;
using DoffAndDonAgain.Utility;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace DoffAndDonAgain.Common {
  public class ArmorStandRerenderHandler {
    private DoffAndDonSystem System { get; }

    public ArmorStandRerenderHandler(DoffAndDonSystem system) {
      System = system;
      if (System.Side == EnumAppSide.Client) {
        System.ClientChannel.SetMessageHandler<ArmorStandInventoryUpdatedPacket>(OnArmorStandInventoryUpdatedPacket);
      }
    }

    private void OnArmorStandInventoryUpdatedPacket(ArmorStandInventoryUpdatedPacket packet) {
      var playerEntity = System.ClientAPI.World.Player.Entity;
      var armorStand = playerEntity?.GetEntityArmorStandById(packet.ArmorStandEntityId, 100, 100);
      UpdateRender(armorStand);
    }

    public void UpdateRender(EntityArmorStand armorStand) {
      if (armorStand == null) { return; }
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
