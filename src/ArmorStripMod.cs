using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace ArmorStrip {
  public class ArmorStripMod : ModSystem {
    private const GlKeys DEFAULT_STRIP_KEY = GlKeys.U;
    private const string STRIP_CODE = "droparmor";
    private const string STRIP_DESC = "Remove all armor";
    private const string STRIP_ERROR_HANDS = "needbothhandsfree";
    private const string STRIP_ERROR_HANDS_DESC = "Need both hands free.";
    public const string STRIP_CHANNEL_NAME = "armorstrip";
    public override void Start(ICoreAPI api) {
      base.Start(api);

      api.Network.RegisterChannel(STRIP_CHANNEL_NAME)
        .RegisterMessageType(typeof(DropAllArmorPacket));
    }

    public override void StartClientSide(ICoreClientAPI capi) {
      base.StartClientSide(capi);

      capi.Input.RegisterHotKey(STRIP_CODE, STRIP_DESC, DEFAULT_STRIP_KEY, HotkeyType.CharacterControls);
      capi.Input.SetHotKeyHandler(STRIP_CODE, (KeyCombination kc) => { return TryToStrip(capi); });
    }

    public override void StartServerSide(ICoreServerAPI sapi) {
      base.StartServerSide(sapi);

      sapi.Network.GetChannel(STRIP_CHANNEL_NAME).SetMessageHandler<DropAllArmorPacket>((IServerPlayer stripper, DropAllArmorPacket packet) => { Strip(stripper, packet); });
    }

    private bool TryToStrip(ICoreClientAPI capi) {
      var stripper = capi.World.Player;
      if (HasBothHandsEmpty(stripper)) {
        var dropArmorPacket = new DropAllArmorPacket();
        var armorStand = GetTargetedArmorStandEntity(stripper);
        dropArmorPacket.ArmorStandEntityId = armorStand?.EntityId;
        capi.Network.GetChannel(STRIP_CHANNEL_NAME).SendPacket(dropArmorPacket);
        Strip(stripper, armorStand);
        return true;
      }
      else {
        capi.TriggerIngameError(this, STRIP_ERROR_HANDS, Lang.GetIfExists($"armorstrip:ingameerror-{STRIP_ERROR_HANDS}") ?? STRIP_ERROR_HANDS_DESC);
        return false;
      }
    }

    private bool HasBothHandsEmpty(IPlayer stripper) {
      return stripper.Entity.RightHandItemSlot.Empty && stripper.Entity.LeftHandItemSlot.Empty;
    }

    private EntityArmorStand GetTargetedArmorStandEntity(IClientPlayer player) {
      return player.CurrentEntitySelection?.Entity as EntityArmorStand;
    }

    private void Strip(IServerPlayer stripper, DropAllArmorPacket packet) {
      EntityArmorStand armorStand = stripper.Entity.World.GetNearestEntity(stripper.Entity.Pos.AsBlockPos.ToVec3d(), 10, 10, (Entity entity) => {
        return entity.EntityId == packet.ArmorStandEntityId;
      }) as EntityArmorStand;
      Strip(stripper, armorStand);
    }

    private void Strip(IPlayer stripper, EntityArmorStand armorStand) {
      bool gaveToArmorStand = false;
      foreach (var slot in stripper.Entity.GetArmorSlots().Values) {
        if (!(slot?.Empty ?? true)) {
          var sinkSlot = armorStand?.GearInventory?.GetBestSuitedSlot(slot);
          if (sinkSlot?.slot != null && sinkSlot.weight > 0) {
            if (slot.TryPutInto(stripper.Entity.World, sinkSlot.slot) > 0) {
              gaveToArmorStand = true;
              sinkSlot.slot.MarkDirty();
            }
            gaveToArmorStand = true;
          }
          else {
            stripper.InventoryManager.DropItem(slot, true);
          }
          slot.MarkDirty();
        }
      }
      if (gaveToArmorStand) {
        armorStand.WatchedAttributes.MarkAllDirty();
      }
    }
  }
}
