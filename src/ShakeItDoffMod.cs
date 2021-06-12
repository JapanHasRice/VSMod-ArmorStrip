using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace ShakeItDoff {
  public class ShakeItDoffMod : ModSystem {
    public const string DOFF_CHANNEL_NAME = "shakeitdoff";
    private const GlKeys DEFAULT_DOFF_KEY = GlKeys.U;
    private const string DOFF_CODE = "doffarmor";
    private const string DOFF_DESC = "Doff: Remove all armor";
    private const string DOFF_ERROR_HANDS = "needbothhandsfree";
    private const string DOFF_ERROR_HANDS_DESC = "Need both hands free.";
    public override void Start(ICoreAPI api) {
      base.Start(api);

      api.Network.RegisterChannel(DOFF_CHANNEL_NAME)
        .RegisterMessageType(typeof(DoffArmorPacket))
        .RegisterMessageType(typeof(ArmorStandInventoryUpdatedPacket));
    }

    public override void StartClientSide(ICoreClientAPI capi) {
      base.StartClientSide(capi);

      capi.Input.RegisterHotKey(DOFF_CODE, DOFF_DESC, DEFAULT_DOFF_KEY, HotkeyType.CharacterControls);
      capi.Input.SetHotKeyHandler(DOFF_CODE, (KeyCombination kc) => { return TryToDoff(capi); });

      capi.Network.GetChannel(DOFF_CHANNEL_NAME).SetMessageHandler<ArmorStandInventoryUpdatedPacket>((ArmorStandInventoryUpdatedPacket packet) => {
        MarkArmorStandDirty(GetEntityArmorStandById(capi.World.Player.Entity, packet.ArmorStandEntityId, 100, 100));
      });
    }

    public override void StartServerSide(ICoreServerAPI sapi) {
      base.StartServerSide(sapi);

      sapi.Network.GetChannel(DOFF_CHANNEL_NAME).SetMessageHandler<DoffArmorPacket>((IServerPlayer doffer, DoffArmorPacket packet) => { Doff(doffer, packet); });
    }

    private bool TryToDoff(ICoreClientAPI capi) {
      var doffer = capi.World.Player;
      if (HasBothHandsEmpty(doffer)) {
        var doffArmorPacket = new DoffArmorPacket(GetTargetedArmorStandEntity(doffer)?.EntityId);
        capi.Network.GetChannel(DOFF_CHANNEL_NAME).SendPacket(doffArmorPacket);
        return true;
      }
      else {
        capi.TriggerIngameError(this, DOFF_ERROR_HANDS, Lang.GetIfExists($"shakeitdoff:ingameerror-{DOFF_ERROR_HANDS}") ?? DOFF_ERROR_HANDS_DESC);
        return false;
      }
    }

    private bool HasBothHandsEmpty(IPlayer doffer) {
      return doffer.Entity.RightHandItemSlot.Empty && doffer.Entity.LeftHandItemSlot.Empty;
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
      bool gaveToArmorStand = false;
      bool isTargetingArmorStand = armorStand != null;
      foreach (var slot in doffer.Entity.GetFilledArmorSlots()) {
        if (slot.Empty) { continue; } // just in case
        if (!isTargetingArmorStand) {
          doffer.InventoryManager.DropItem(slot, true);
          continue;
        }

        ItemSlot sinkSlot = GetAvailableSlotOnArmorStand(armorStand, slot);
        if (sinkSlot != null && slot.TryPutInto(doffer.Entity.World, sinkSlot) > 0) {
          gaveToArmorStand = true;
          sinkSlot.MarkDirty();
        }
        else {
          doffer.InventoryManager.DropItem(slot, true);
        }
        slot.MarkDirty();
      }
      if (gaveToArmorStand) {
        armorStand.WatchedAttributes.MarkAllDirty();
        BroadcastArmorStandUpdated(armorStand.World.Api as ICoreServerAPI, armorStand);
      }
    }

    private ItemSlot GetAvailableSlotOnArmorStand(EntityArmorStand armorStand, ItemSlot sourceSlot) {
      WeightedSlot sinkSlot = armorStand.GearInventory.GetBestSuitedSlot(sourceSlot);
      return sinkSlot.weight > 0 ? sinkSlot.slot : null;
    }

    private void BroadcastArmorStandUpdated(ICoreServerAPI sapi, EntityArmorStand armorStand) {
      sapi.World.RegisterCallback((IWorldAccessor world, BlockPos pos, float dt) => {
        (world.Api as ICoreServerAPI).Network.GetChannel(DOFF_CHANNEL_NAME).BroadcastPacket(new ArmorStandInventoryUpdatedPacket(armorStand.EntityId));
      }, armorStand.Pos.AsBlockPos, 500);
      sapi.Network.GetChannel(DOFF_CHANNEL_NAME).BroadcastPacket(new ArmorStandInventoryUpdatedPacket(armorStand.EntityId));
    }

    private void MarkArmorStandDirty(EntityArmorStand armorStand) {
      if (armorStand?.IsRendered ?? false) {
        armorStand.OnEntityLoaded();
      }
    }
  }
}
