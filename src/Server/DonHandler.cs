using DoffAndDonAgain.Common;
using DoffAndDonAgain.Common.Network;
using DoffAndDonAgain.Utility;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace DoffAndDonAgain.Server {
  public class DonHandler : OneWayArmorTransfer {
    protected bool ShouldDonTool;
    protected bool ShouldDonOnlyToActiveHotbar;
    protected bool ShouldDonOnlyToHotbar;

    public DonHandler(DoffAndDonSystem system) {
      if (system.Side != EnumAppSide.Server) {
        system.Api.Logger.Warning("{0} is a server object instantiated on the client, ignoring.", nameof(DonHandler));
        return;
      }
      System = system;

      System.ServerChannel.SetMessageHandler<DonArmorPacket>(OnDonPacket);

      ShouldDonTool = System.Config.EnableToolDonning;
      ShouldDonOnlyToActiveHotbar = System.Config.DonToolOnlyToActiveHotbar;
      ShouldDonOnlyToHotbar = System.Config.DonToolOnlyToHotbar;
    }

    private void OnDonPacket(IServerPlayer donner, DonArmorPacket packet) {
      bool donned = false;
      var armorStand = donner?.Entity.GetEntityArmorStandById(packet.ArmorStandEntityId);
      if (armorStand == null) {
        System.Error.TriggerFromServer(Constants.ERROR_TARGET_LOST, donner);
      }
      else {
        if (System.Config.EnableDon) {
          donned = DonFromArmorStand(donner, armorStand);
          OnDonCompleted(donner, donned);
        }
        else {
          System.Error.TriggerFromServer(Constants.ERROR_DON_DISABLED, donner);
        }
      }
    }

    private bool DonFromArmorStand(IServerPlayer donner, EntityArmorStand armorStand) {
      bool armorWasTransferred = TransferArmor(initiatingPlayer: donner,
                                               doffer: armorStand,
                                               donner: donner.Entity,
                                               onDoffWithoutDonner: KeepUndonnableOnDoff,
                                               onDonnedOneOrMore: () => { System.ArmorStandRerenderHandler?.UpdateRender(armorStand); });
      bool toolWasTransferred = ShouldDonTool && TransferTool(donner,
                                                              armorStand,
                                                              donOnlyToActiveHotbar: ShouldDonOnlyToActiveHotbar,
                                                              donOnlyToHotbar: ShouldDonOnlyToHotbar);

      return armorWasTransferred || toolWasTransferred;
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
  }
}
