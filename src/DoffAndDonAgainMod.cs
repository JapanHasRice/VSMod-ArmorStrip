using DoffAndDonAgain.Config;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace DoffAndDonAgain {
  public class DoffAndDonAgainMod : ModSystem {
    private int HandsNeededToDoff;
    private float SaturationCostPerDoff;
    public override void Start(ICoreAPI api) {
      base.Start(api);

      var config = DoffAndDonAgainConfig.Load(api);
      HandsNeededToDoff = config.HandsNeededToDoff;
      SaturationCostPerDoff = config.SaturationCostPerDoff;

      api.Network.RegisterChannel(Constants.DOFF_CHANNEL_NAME)
        .RegisterMessageType(typeof(DoffArmorPacket))
        .RegisterMessageType(typeof(ArmorStandInventoryUpdatedPacket));
    }

    public override void StartClientSide(ICoreClientAPI capi) {
      base.StartClientSide(capi);

      capi.Input.RegisterHotKey(Constants.DOFF_CODE, Constants.DOFF_DESC, Constants.DEFAULT_DOFF_KEY, HotkeyType.CharacterControls);
      capi.Input.SetHotKeyHandler(Constants.DOFF_CODE, (KeyCombination kc) => { return TryToDoff(capi); });

      capi.Network.GetChannel(Constants.DOFF_CHANNEL_NAME).SetMessageHandler<ArmorStandInventoryUpdatedPacket>((ArmorStandInventoryUpdatedPacket packet) => {
        MarkArmorStandDirty(GetEntityArmorStandById(capi.World.Player.Entity, packet.ArmorStandEntityId, 100, 100));
      });
    }

    public override void StartServerSide(ICoreServerAPI sapi) {
      base.StartServerSide(sapi);

      sapi.Network.GetChannel(Constants.DOFF_CHANNEL_NAME).SetMessageHandler<DoffArmorPacket>((IServerPlayer doffer, DoffArmorPacket packet) => {
        Doff(doffer, packet);
      });
    }

    private bool TryToDoff(ICoreClientAPI capi) {
      var doffer = capi.World.Player;
      if (HasEnoughHandsFree(doffer)) {
        var doffArmorPacket = new DoffArmorPacket(GetTargetedArmorStandEntity(doffer)?.EntityId);
        capi.Network.GetChannel(Constants.DOFF_CHANNEL_NAME).SendPacket(doffArmorPacket);
        return true;
      }
      else {
        TriggerHandsError(capi);
        return false;
      }
    }

    private bool HasEnoughSaturation(IServerPlayer player, float neededSaturation) {
      return player.Entity.GetBehavior<EntityBehaviorHunger>()?.Saturation >= neededSaturation;
    }

    private bool HasEnoughHandsFree(IPlayer player) {
      int freeHands = player.Entity.RightHandItemSlot.Empty ? 1 : 0;
      freeHands += player.Entity.LeftHandItemSlot.Empty ? 1 : 0;
      return freeHands >= HandsNeededToDoff;
    }

    private EntityArmorStand GetTargetedArmorStandEntity(IClientPlayer player) {
      return player.CurrentEntitySelection?.Entity as EntityArmorStand;
    }

    private void Doff(IServerPlayer doffer, DoffArmorPacket packet) {
      Doff(doffer, GetEntityArmorStandById(doffer.Entity, packet.ArmorStandEntityId));
    }

    private EntityArmorStand GetEntityArmorStandById(EntityPlayer aroundPlayer, long? armorStandEntityId, float horRange = 10, float vertRange = 10) {
      return armorStandEntityId == null ? null : aroundPlayer.World.GetNearestEntity(aroundPlayer.Pos.AsBlockPos.ToVec3d(), horRange, vertRange, (Entity entity) => {
        return entity.EntityId == armorStandEntityId;
      }) as EntityArmorStand;
    }

    private void Doff(IServerPlayer doffer, EntityArmorStand armorStand) {
      if (!HasEnoughSaturation(doffer, SaturationCostPerDoff)) {
        TriggerSaturationError(doffer);
        return;
      }

      OnDoffWithoutDonner dropItem = (ItemSlot couldNotBeDonnedSlot) => {
        return doffer.InventoryManager.DropItem(couldNotBeDonnedSlot, true);
      };
      OnDonnedOneOrMore updateArmorStandRender = () => { BroadcastArmorStandUpdated(armorStand); };
      bool doffed = Doff(doffer: doffer.Entity,
                         donner: armorStand,
                         onDoffWithoutDonner: dropItem,
                         onDonnedOneOrMore: updateArmorStandRender);

      if (doffed) { OnSuccessfulDoff(doffer); }
    }

    private bool Doff(EntityAgent doffer, EntityAgent donner = null, OnDoffWithoutDonner onDoffWithoutDonner = null, OnDonnedOneOrMore onDonnedOneOrMore = null) {
      if (doffer == null) { return false; }
      bool doffed = false;

      if (donner == null) {
        foreach (var slot in doffer.GetFilledArmorSlots()) {
          if (slot.Empty) { continue; }
          doffed = onDoffWithoutDonner?.Invoke(slot) ?? true || doffed;
        }
      }
      else {
        bool donnerDonned = false;
        foreach (var slot in doffer.GetFilledArmorSlots()) {
          if (slot.Empty) { continue; }
          doffed = true;

          ItemSlot sinkSlot = GetAvailableSlotOn(donner, slot);
          if (sinkSlot != null && slot.TryPutInto(doffer.World, sinkSlot) > 0) {
            donnerDonned = true;
            sinkSlot.MarkDirty();
          }
          else {
            doffed = onDoffWithoutDonner?.Invoke(slot) ?? true || doffed;
          }
        }
        if (donnerDonned) {
          onDonnedOneOrMore?.Invoke();
        }
      }
      return doffed;
    }

    private ItemSlot GetAvailableSlotOnArmorStand(EntityArmorStand armorStand, ItemSlot sourceSlot) {
      WeightedSlot sinkSlot = armorStand.GearInventory.GetBestSuitedSlot(sourceSlot);
      return sinkSlot.weight > 0 ? sinkSlot.slot : null;
    }

    private ItemSlot GetAvailableSlotOn(EntityAgent entityAgent, ItemSlot sourceSlot) {
      return entityAgent.GearInventory.GetBestSuitedSlot(sourceSlot).slot;
    }

    private void BroadcastArmorStandUpdated(EntityAgent armorStand) {
      if (armorStand == null) { return; }
      if (armorStand.World?.Side == EnumAppSide.Server && armorStand.GetType() == typeof(EntityArmorStand)) {
        armorStand.WatchedAttributes.MarkAllDirty();
        var sapi = armorStand.World.Api as ICoreServerAPI;
        sapi.World.RegisterCallback((IWorldAccessor world, BlockPos pos, float dt) => {
          sapi.Network.GetChannel(Constants.DOFF_CHANNEL_NAME).BroadcastPacket(new ArmorStandInventoryUpdatedPacket(armorStand.EntityId));
        }, armorStand.Pos.AsBlockPos, 500);
      }
    }

    private void MarkArmorStandDirty(EntityArmorStand armorStand) {
      if (armorStand?.IsRendered ?? false) {
        armorStand.OnEntityLoaded();
      }
    }

    private void OnSuccessfulDoff(IServerPlayer doffer) {
      doffer.Entity.GetBehavior<EntityBehaviorHunger>()?.ConsumeSaturation(SaturationCostPerDoff);
    }

    private void TriggerHandsError(ICoreClientAPI capi) {
      string errorCode;
      string errorDesc;
      if (HandsNeededToDoff == 2) {
        errorCode = Constants.DOFF_ERROR_BOTH_HANDS;
        errorDesc = Constants.DOFF_ERROR_BOTH_HANDS_DESC;
      }
      else {
        errorCode = Constants.DOFF_ERROR_ONE_HAND;
        errorDesc = Constants.DOFF_ERROR_ONE_HAND_DESC;
      }
      capi.TriggerIngameError(this, errorCode, Lang.GetIfExists($"doffanddonagain:ingameerror-{errorCode}") ?? errorDesc);
    }

    private void TriggerSaturationError(IServerPlayer player) {
      player.SendIngameError(Constants.DOFF_ERROR_SATURATION, Lang.GetIfExists($"doffanddonagain:ingameerror-{Constants.DOFF_ERROR_SATURATION}") ?? Constants.DOFF_ERROR_SATURATION_DESC);
    }
  }

  public delegate void OnDonnedOneOrMore();

  // Return true to indicate a successful doffing.
  public delegate bool OnDoffWithoutDonner(ItemSlot couldNotBeDonnedSlot);
}
