using DoffAndDonAgain.Common;
using DoffAndDonAgain.Common.Network;
using DoffAndDonAgain.Utility;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace DoffAndDonAgain.Server {
  public class DoffHandler : OneWayArmorTransfer {
    private DoffAndDonSystem System { get; }
    private OnDoffWithoutDonner dropOrKeepArmorWhenDoffingToStand;

    public DoffHandler(DoffAndDonSystem system) {
      if (system.Side != EnumAppSide.Server) {
        system.Api.Logger.Warning("{0} is a server object instantiated on the client, ignoring.", nameof(DoffHandler));
        return;
      }
      System = system;
      System.ServerChannel.SetMessageHandler<DoffArmorPacket>(OnDoffPacket);

      if (System.Config.DropArmorWhenDoffingToStand) {
        dropOrKeepArmorWhenDoffingToStand = DropUndonnableOnDoff;
      }
      else {
        dropOrKeepArmorWhenDoffingToStand = KeepUndonnableOnDoff;
      }
    }

    private void OnDoffPacket(IServerPlayer doffer, DoffArmorPacket packet) {
      bool doffed = false;
      var armorStand = doffer?.Entity.GetEntityArmorStandById(packet.ArmorStandEntityId);
      if (armorStand == null) {
        if (packet.ArmorStandEntityId == null) {
          if (System.Config.EnableDoffToGround) {
            doffed = DoffToGround(doffer);
          }
          else {
            System.Error.TriggerFromServer(Constants.ERROR_DOFF_GROUND_DISABLED, doffer);
          }
        }
        else {
          // Armor stand entity id was provided, but was not found nearby the player for some reason.
          // It may have been picked up or destroyed between client action and server handling.
          System.Error.TriggerFromServer(Constants.ERROR_TARGET_LOST, doffer);
          return;
        }
      }
      else {
        if (System.Config.EnableDoffToArmorStand) {
          doffed = DoffToArmorStand(doffer, armorStand);
        }
        else {
          System.Error.TriggerFromServer(Constants.ERROR_DOFF_STAND_DISABLED, doffer);
          return;
        }
      }
      OnDoffCompleted(doffer, doffed);
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
                           onDoffWithoutDonner: dropOrKeepArmorWhenDoffingToStand,
                           onDonnedOneOrMore: () => { System.ArmorStandRerenderHandler?.UpdateRender(armorStand); });
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
      doffer.Entity.ConsumeSaturation(System.Config.SaturationCostPerDoff);
    }
  }
}
