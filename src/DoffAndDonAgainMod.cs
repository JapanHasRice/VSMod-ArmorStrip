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
    private ICoreAPI Api;
    private INetworkChannel Channel;
    private int HandsNeededToDoff;
    private float SaturationCostPerDoff;
    private float SaturationCostPerDon;
    private bool DropArmorWhenDoffingToStand;
    public override void Start(ICoreAPI api) {
      base.Start(api);
      Api = api;

      LoadConfigs();
      SetupNetwork();
      SetupHotkeys();
    }

    private void LoadConfigs() {
      var config = DoffAndDonAgainConfig.LoadOrCreateDefault(Api);
      HandsNeededToDoff = config.HandsNeededToDoff;
      SaturationCostPerDoff = config.SaturationCostPerDoff;
      SaturationCostPerDon = config.SaturationCostPerDon;
      DropArmorWhenDoffingToStand = config.DropArmorWhenDoffingToStand;
    }

    private void SetupNetwork() {
      Channel = Api.Network.RegisterChannel(Constants.CHANNEL_NAME)
        .RegisterMessageType(typeof(DoffArmorPacket))
        .RegisterMessageType(typeof(DonArmorPacket))
        .RegisterMessageType(typeof(ArmorStandInventoryUpdatedPacket));

      if (Api.Side == EnumAppSide.Client) {
        ((IClientNetworkChannel)Channel)
          .SetMessageHandler<ArmorStandInventoryUpdatedPacket>((ArmorStandInventoryUpdatedPacket packet) => {
            MarkArmorStandDirty(GetEntityArmorStandById((Api as ICoreClientAPI).World.Player.Entity, packet.ArmorStandEntityId, 100, 100));
          });
      }
      else {
        ((IServerNetworkChannel)Channel)
          .SetMessageHandler<DoffArmorPacket>((IServerPlayer doffer, DoffArmorPacket packet) => {
            Doff(doffer, packet);
          })
          .SetMessageHandler<DonArmorPacket>((IServerPlayer donner, DonArmorPacket packet) => {
            Don(donner, packet);
          });
      }
    }

    private void SetupHotkeys() {
      if (Api.Side != EnumAppSide.Client) { return; }
      var inputAPI = ((ICoreClientAPI)Api).Input;

      inputAPI.RegisterHotKey(Constants.DOFF_CODE, Constants.DOFF_DESC, Constants.DEFAULT_KEY, HotkeyType.CharacterControls, ctrlPressed: true);
      inputAPI.SetHotKeyHandler(Constants.DOFF_CODE, (KeyCombination kc) => { return TryToDoff(Api as ICoreClientAPI); });

      inputAPI.RegisterHotKey(Constants.DON_CODE, Constants.DON_DESC, Constants.DEFAULT_KEY, HotkeyType.CharacterControls);
      inputAPI.SetHotKeyHandler(Constants.DON_CODE, (KeyCombination kc) => { return TryToDon(Api as ICoreClientAPI); });
    }

    private bool TryToDoff(ICoreClientAPI capi) {
      var doffer = capi.World.Player;
      if (HasEnoughHandsFree(doffer)) {
        var doffArmorPacket = new DoffArmorPacket(GetTargetedArmorStandEntity(doffer)?.EntityId);
        capi.Network.GetChannel(Constants.CHANNEL_NAME).SendPacket(doffArmorPacket);
        return true;
      }
      else {
        TriggerHandsError(doffer);
        return false;
      }
    }

    private bool TryToDon(ICoreClientAPI capi) {
      var donner = capi.World.Player;
      if (!HasEnoughHandsFree(donner)) {
        TriggerHandsError(donner);
        return false;
      }

      var armorStand = GetTargetedArmorStandEntity(donner);
      if (armorStand == null) {
        TriggerArmorStandTargetError(donner);
        return false;
      }

      var donArmorPacket = new DonArmorPacket(armorStand.EntityId);
      capi.Network.GetChannel(Constants.CHANNEL_NAME).SendPacket(donArmorPacket);
      return true;
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

    private void Don(IServerPlayer donner, DonArmorPacket packet) {
      Don(donner, GetEntityArmorStandById(donner.Entity, packet.ArmorStandEntityId));
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

      OnDoffWithoutDonner dropOrKeepItem = null;
      if (!DropArmorWhenDoffingToStand && armorStand != null) {
        dropOrKeepItem = (ItemSlot couldNotBeDonnedSlot) => {
          return false; // False so that a doff that fails this way does not count for saturation depletion.
        };
      }
      else {
        dropOrKeepItem = (ItemSlot couldNotBeDonnedSlot) => {
          return doffer.InventoryManager.DropItem(couldNotBeDonnedSlot, true);
        };
      }
      OnDonnedOneOrMore updateArmorStandRender = () => { BroadcastArmorStandUpdated(armorStand); };
      bool doffed = Doff(doffer: doffer.Entity,
                         donner: armorStand,
                         onDoffWithoutDonner: dropOrKeepItem,
                         onDonnedOneOrMore: updateArmorStandRender);

      if (doffed) { OnSuccessfulDoff(doffer); }
    }

    private void Don(IServerPlayer donner, EntityArmorStand armorStand) {
      if (!HasEnoughSaturation(donner, SaturationCostPerDon)) {
        TriggerSaturationError(donner);
        return;
      }

      OnDonnedOneOrMore updateArmorStandRender = () => { BroadcastArmorStandUpdated(armorStand); };
      bool donned = Doff(doffer: armorStand,
                         donner: donner.Entity,
                         onDoffWithoutDonner: null,
                         onDonnedOneOrMore: updateArmorStandRender);

      if (donned) { OnSuccessfulDon(donner); }
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
      // Do not use IInventory#GetBestSuitedSlot because it always returns a null slot for the character's gear inventory
      foreach (var slot in entityAgent.GearInventory) {
        if (slot.CanHold(sourceSlot)) {
          return slot.Empty ? slot : null;
        }
      }
      return null;
    }

    private void BroadcastArmorStandUpdated(EntityAgent armorStand) {
      if (armorStand == null) { return; }
      if (armorStand.World?.Side == EnumAppSide.Server && armorStand.GetType() == typeof(EntityArmorStand)) {
        armorStand.WatchedAttributes.MarkAllDirty();
        var sapi = armorStand.World.Api as ICoreServerAPI;
        sapi.World.RegisterCallback((IWorldAccessor world, BlockPos pos, float dt) => {
          sapi.Network.GetChannel(Constants.CHANNEL_NAME).BroadcastPacket(new ArmorStandInventoryUpdatedPacket(armorStand.EntityId));
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

    private void OnSuccessfulDon(IServerPlayer donner) {
      donner.Entity.GetBehavior<EntityBehaviorHunger>()?.ConsumeSaturation(SaturationCostPerDon);
    }

    private void TriggerHandsError(IClientPlayer player) {
      string errorCode;
      string errorDesc;
      if (HandsNeededToDoff == 2) {
        errorCode = Constants.ERROR_BOTH_HANDS;
        errorDesc = Constants.ERROR_BOTH_HANDS_DESC;
      }
      else {
        errorCode = Constants.ERROR_ONE_HAND;
        errorDesc = Constants.ERROR_ONE_HAND_DESC;
      }
      TriggerError(player, errorCode, errorDesc);
    }

    private void TriggerArmorStandTargetError(IClientPlayer player) {
      TriggerError(player, Constants.ERROR_MISSING_ARMOR_STAND_TARGET, Constants.ERROR_MISSING_ARMOR_STAND_TARGET_DESC);
    }

    private void TriggerSaturationError(IServerPlayer player) {
      TriggerError(player, Constants.ERROR_SATURATION, Constants.ERROR_SATURATION_DESC);
    }

    private void TriggerError(IPlayer player, string errorCode, string errorFallbackDesc) {
      string errorText = Lang.GetIfExists($"doffanddonagain:ingameerror-{errorCode}") ?? errorFallbackDesc;
      (player as IServerPlayer)?.SendIngameError(errorCode, errorText);
      (player?.Entity?.Api as ICoreClientAPI)?.TriggerIngameError(this, errorCode, errorText);
    }
  }

  public delegate void OnDonnedOneOrMore();

  // Return true to indicate a successful doffing.
  public delegate bool OnDoffWithoutDonner(ItemSlot couldNotBeDonnedSlot);
}
