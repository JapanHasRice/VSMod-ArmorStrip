using DoffAndDonAgain.Common;
using DoffAndDonAgain.Common.Network;
using DoffAndDonAgain.Utility;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace DoffAndDonAgain.Server {
  public class DoffHandler : OneWayArmorTransfer {
    protected bool ShouldDoffToGround { get; set; } = true;
    protected bool ShouldDoffToArmorStand { get; set; } = true;
    protected float SaturationCostPerDoff { get; set; } = 0f;

    private OnDoffWithoutDonner dropOrKeepArmorWhenDoffingToStand;

    public DoffHandler(DoffAndDonSystem system) : base(system) {
      System.ServerChannel.SetMessageHandler<DoffArmorPacket>(OnDoffPacket);
    }

    protected override void LoadServerSettings(DoffAndDonServerConfig serverSettings) {
      if (serverSettings.DropArmorWhenDoffingToStand.Value) {
        dropOrKeepArmorWhenDoffingToStand = DropUndonnableOnDoff;
      }
      else {
        dropOrKeepArmorWhenDoffingToStand = KeepUndonnableOnDoff;
      }

      ShouldDoffToGround = serverSettings.EnableDoffToGround.Value;
      ShouldDoffToArmorStand = serverSettings.EnableDoffToArmorStand.Value;
      SaturationCostPerDoff = serverSettings.SaturationCostPerDoff.Value;
    }

    private void OnDoffPacket(IServerPlayer doffer, DoffArmorPacket packet) {
      bool doffed = false;
      var armorStand = doffer?.Entity.GetEntityArmorStandById(packet.ArmorStandEntityId);
      // TODO: Why would you do this?! You know better...
      if (armorStand == null) {
        if (packet.ArmorStandEntityId == null) {
          if (ShouldDoffToGround) {
            doffed = DoffToGround(doffer);
            OnDoffCompleted(doffer, doffed);
          }
          else {
            System.Error.TriggerFromServer(Constants.ERROR_DOFF_GROUND_DISABLED, doffer);
          }
        }
        else {
          // Armor stand entity id was provided, but was not found nearby the player for some reason.
          // It may have been picked up or destroyed between client action and server handling.
          System.Error.TriggerFromServer(Constants.ERROR_TARGET_LOST, doffer);
        }
      }
      else {
        if (ShouldDoffToArmorStand) {
          doffed = DoffToArmorStand(doffer, armorStand);
          OnDoffCompleted(doffer, doffed);
        }
        else {
          System.Error.TriggerFromServer(Constants.ERROR_DOFF_STAND_DISABLED, doffer);
        }
      }
    }

    private bool DoffToGround(IServerPlayer doffer) {
      return TransferArmor(initiatingPlayer: doffer,
                           doffer: doffer.Entity,
                           onDoffWithoutDonner: DropUndonnableOnDoff);
    }

    private bool DoffToArmorStand(IServerPlayer doffer, EntityArmorStand armorStand) {
      return TransferArmor(initiatingPlayer: doffer,
                           doffer: doffer.Entity,
                           donner: armorStand,
                           onDoffWithoutDonner: dropOrKeepArmorWhenDoffingToStand);
    }

    private void OnDoffCompleted(IServerPlayer doffer, bool successful) {
      if (successful) {
        OnSuccessfulDoff(doffer);
      }
      else {
        System.Error.TriggerFromServer(Constants.ERROR_UNDOFFABLE, doffer);
      }
    }

    private void OnSuccessfulDoff(IServerPlayer doffer) {
      doffer.Entity.ConsumeSaturation(SaturationCostPerDoff);
    }
  }
}
