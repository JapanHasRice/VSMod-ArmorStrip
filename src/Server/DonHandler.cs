using DoffAndDonAgain.Common;
using DoffAndDonAgain.Common.Network;
using DoffAndDonAgain.Utility;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace DoffAndDonAgain.Server {
  public class DonHandler : OneWayArmorTransfer {
    protected bool ShouldDonArmor { get; set; } = true;
    protected bool ShouldDonTool { get; set; } = true;
    protected bool ShouldDonToolOnlyToActiveHotbar { get; set; } = true;
    protected bool ShouldDonToolOnlyToHotbar { get; set; } = true;
    protected float SaturationCostPerDon { get; set; } = 0f;

    public DonHandler(DoffAndDonSystem system) : base(system) {
      System.ServerChannel.SetMessageHandler<DonArmorPacket>(OnDonPacket);
    }

    protected override void LoadServerSettings(DoffAndDonServerConfig serverSettings) {
      ShouldDonArmor = serverSettings.EnableDon.Value;

      ShouldDonTool = serverSettings.EnableToolDonning.Value;
      ShouldDonToolOnlyToActiveHotbar = serverSettings.DonToolOnlyToActiveHotbar.Value;
      ShouldDonToolOnlyToHotbar = serverSettings.DonToolOnlyToHotbar.Value;

      SaturationCostPerDon = serverSettings.SaturationCostPerDon.Value;
    }

    private void OnDonPacket(IServerPlayer donner, DonArmorPacket packet) {
      bool donned = false;
      var armorStand = donner?.Entity.GetEntityArmorStandById(packet.ArmorStandEntityId);
      if (armorStand == null) {
        System.Error.TriggerFromServer(Constants.ERROR_TARGET_LOST, donner);
      }
      else {
        if (ShouldDonArmor) {
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
                                               onDoffWithoutDonner: KeepUndonnableOnDoff);
      bool toolWasTransferred = ShouldDonTool && TransferTool(donner,
                                                              armorStand,
                                                              donOnlyToActiveHotbar: ShouldDonToolOnlyToActiveHotbar,
                                                              donOnlyToHotbar: ShouldDonToolOnlyToHotbar);

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
      donner.Entity.ConsumeSaturation(SaturationCostPerDon);
    }
  }
}
