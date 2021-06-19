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
#region Variables and Properties
    private ICoreAPI Api;
    private INetworkChannel Channel;
    private int HandsNeededToDoff;
    private float SaturationCostPerDoff;
    private float SaturationCostPerDon;
    private bool DropArmorWhenDoffingToStand;

    private ICoreClientAPI ClientAPI {
      get { return Api as ICoreClientAPI; }
    }

    private ICoreServerAPI ServerAPI {
      get { return Api as ICoreServerAPI; }
    }

    private IClientNetworkChannel ClientChannel {
      get { return Channel as IClientNetworkChannel; }
    }

    private IServerNetworkChannel ServerChannel {
      get { return Channel as IServerNetworkChannel; }
    }
#endregion

#region Delegates

    public delegate void OnDonnedOneOrMore();

    // Return true to indicate a successful doffing.
    public delegate bool OnDoffWithoutDonner(ItemSlot couldNotBeDonnedSlot);

#endregion

#region Mod initialization
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

      ClientChannel
        ?.SetMessageHandler<ArmorStandInventoryUpdatedPacket>(OnArmorStandInventoryUpdated);

      ServerChannel
        ?.SetMessageHandler<DoffArmorPacket>(OnDoff)
        ?.SetMessageHandler<DonArmorPacket>(OnDon);
    }

    private void SetupHotkeys() {
      if (Api.Side != EnumAppSide.Client) { return; }

      ClientAPI.Input.RegisterHotKey(Constants.DOFF_CODE, Constants.DOFF_DESC, Constants.DEFAULT_KEY, HotkeyType.CharacterControls, ctrlPressed: true);
      ClientAPI.Input.SetHotKeyHandler(Constants.DOFF_CODE, OnTryToDoff);

      ClientAPI.Input.RegisterHotKey(Constants.DON_CODE, Constants.DON_DESC, Constants.DEFAULT_KEY, HotkeyType.CharacterControls);
      ClientAPI.Input.SetHotKeyHandler(Constants.DON_CODE, OnTryToDon);
    }
#endregion

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

    private ItemSlot GetAvailableSlotOn(EntityAgent entityAgent, ItemSlot sourceSlot) {
      // Do not use IInventory#GetBestSuitedSlot because it always returns a null slot for the character's gear inventory
      foreach (var slot in entityAgent.GearInventory) {
        if (slot.CanHold(sourceSlot)) {
          return slot.Empty ? slot : null;
        }
      }
      return null;
    }

    private EntityArmorStand GetEntityArmorStandById(EntityPlayer aroundPlayer, long? armorStandEntityId, float horRange = 10, float vertRange = 10) {
      return armorStandEntityId == null ? null : aroundPlayer.World.GetNearestEntity(aroundPlayer.Pos.AsBlockPos.ToVec3d(), horRange, vertRange, (Entity entity) => {
        return entity.EntityId == armorStandEntityId;
      }) as EntityArmorStand;
    }

    private EntityArmorStand GetTargetedArmorStandEntity(IClientPlayer player) {
      return player.CurrentEntitySelection?.Entity as EntityArmorStand;
    }

    private bool HasEnoughSaturation(IServerPlayer player, float neededSaturation) {
      return player.Entity.GetBehavior<EntityBehaviorHunger>()?.Saturation >= neededSaturation;
    }

    private bool HasEnoughHandsFree(IPlayer player) {
      int freeHands = player.Entity.RightHandItemSlot.Empty ? 1 : 0;
      freeHands += player.Entity.LeftHandItemSlot.Empty ? 1 : 0;
      return freeHands >= HandsNeededToDoff;
    }

    private void MarkArmorStandDirty(EntityArmorStand armorStand) {
      if (armorStand?.IsRendered ?? false) {
        armorStand.OnEntityLoaded(); // Should figure out if this has unwanted side effects or if there's a proper, more direct way of solving
      }
    }

    private void OnArmorStandInventoryUpdated(ArmorStandInventoryUpdatedPacket packet) {
      if (Api.Side != EnumAppSide.Client) { return; }
      var armorStand = GetEntityArmorStandById((Api as ICoreClientAPI).World.Player.Entity, packet.ArmorStandEntityId, 100, 100);
      MarkArmorStandDirty(armorStand);
    }

    private void OnDoff(IServerPlayer doffer, DoffArmorPacket packet) {
      Doff(doffer, GetEntityArmorStandById(doffer.Entity, packet.ArmorStandEntityId));
    }

    private void OnDon(IServerPlayer donner, DonArmorPacket packet) {
      Don(donner, GetEntityArmorStandById(donner.Entity, packet.ArmorStandEntityId));
    }

    private void OnSuccessfulDoff(IServerPlayer doffer) {
      doffer.Entity.GetBehavior<EntityBehaviorHunger>()?.ConsumeSaturation(SaturationCostPerDoff);
    }

    private void OnSuccessfulDon(IServerPlayer donner) {
      donner.Entity.GetBehavior<EntityBehaviorHunger>()?.ConsumeSaturation(SaturationCostPerDon);
    }

    private bool OnTryToDoff(KeyCombination kc) {
      var doffer = ClientAPI.World.Player;

      if (!HasEnoughHandsFree(doffer)) {
        TriggerHandsError(doffer);
        return false;
      }

      SendDoffRequest(doffer);
      return true;
    }

    private bool OnTryToDon(KeyCombination kc) {
      var donner = ClientAPI.World.Player;
      if (!HasEnoughHandsFree(donner)) {
        TriggerHandsError(donner);
        return false;
      }

      var armorStand = GetTargetedArmorStandEntity(donner);
      if (armorStand == null) {
        TriggerArmorStandTargetError(donner);
        return false;
      }

      SendDonRequest(donner, armorStand);
      return true;
    }

    private void SendDoffRequest(IClientPlayer doffer) {
      var doffArmorPacket = new DoffArmorPacket(GetTargetedArmorStandEntity(doffer)?.EntityId);
      ClientChannel.SendPacket(doffArmorPacket);
    }

    private void SendDonRequest(IClientPlayer donner, EntityArmorStand armorStand) {
      var donArmorPacket = new DonArmorPacket(armorStand.EntityId);
      ClientChannel.SendPacket(donArmorPacket);
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
}
