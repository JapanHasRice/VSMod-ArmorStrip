using DoffAndDonAgain.Common.Network;
using DoffAndDonAgain.Utility;
using Vintagestory.API.Common;

namespace DoffAndDonAgain.Client {
  public class ArmorStandRerenderHandler {
    private DoffAndDonSystem System { get; }

    public ArmorStandRerenderHandler(DoffAndDonSystem system) {
      if (system.Side != EnumAppSide.Client) {
        system.Api.Logger.Warning("{0} is a client object instantiated on the server, ignoring.", nameof(ArmorStandRerenderHandler));
        return;
      }
      System = system;
      System.ClientChannel.SetMessageHandler<ArmorStandInventoryUpdatedPacket>(OnArmorStandInventoryUpdatedPacket);
    }

    private void OnArmorStandInventoryUpdatedPacket(ArmorStandInventoryUpdatedPacket packet) {
      var playerEntity = System.ClientAPI.World.Player.Entity;
      var armorStand = playerEntity?.GetEntityArmorStandById(packet.ArmorStandEntityId, 100, 100);
      armorStand?.UpdateRender();
    }
  }
}
