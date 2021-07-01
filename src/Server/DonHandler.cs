using DoffAndDonAgain.Common;
using DoffAndDonAgain.Common.Network;
using DoffAndDonAgain.Utility;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace DoffAndDonAgain.Server {
  public class DonHandler : OneWayArmorTransfer {
    private DoffAndDonSystem System { get; }
    public DonHandler(DoffAndDonSystem system) {
      if (system.Side != EnumAppSide.Server) {
        system.Api.Logger.Warning("{0} is a server object instantiated on the client, ignoring.", nameof(DonHandler));
        return;
      }
      System = system;

      if (System.Config.EnableDon) {
        System.ServerChannel.SetMessageHandler<DonArmorPacket>(OnDonPacket);
      }
      else {
        System.ServerChannel.SetMessageHandler<DonArmorPacket>(OnDonPacketDonDisabled);
      }
    }

    private void OnDonPacket(IServerPlayer donner, DonArmorPacket packet) {
      bool donned = false;
      var armorStand = donner?.Entity.GetEntityArmorStandById(packet.ArmorStandEntityId);
      if (armorStand == null) {
        System.Error.TriggerFromServer(Constants.ERROR_TARGET_LOST, donner);
        return;
      }
      else {
        donned = DonFromArmorStand(donner, armorStand);
      }
      OnDonCompleted(donner, donned);
    }

    private bool DonFromArmorStand(IServerPlayer donner, EntityArmorStand armorStand) {
      return TransferArmor(initiatingPlayer: donner,
                           doffer: armorStand,
                           donner: donner.Entity,
                           onDoffWithoutDonner: KeepUndonnableOnDoff,
                           onDonnedOneOrMore: () => { System.ArmorStandRerenderHandler?.UpdateRender(armorStand); });
    }

    private void OnDonCompleted(IServerPlayer donner, bool successful) {
      if (successful) {
        OnSuccessfulDon(donner);
      }
      else {
        System.Error.TriggerFromServer(Constants.ERROR_UNDONNABLE, donner);
      }
    }

    private void OnSuccessfulDon(IServerPlayer donner) {
      donner.Entity.ConsumeSaturation(System.Config.SaturationCostPerDon);
    }

    private void OnDonPacketDonDisabled(IServerPlayer wouldBeDonner, DonArmorPacket packet) {
      System.Error.TriggerFromServer(Constants.ERROR_DON_DISABLED, wouldBeDonner);
    }
  }
}
