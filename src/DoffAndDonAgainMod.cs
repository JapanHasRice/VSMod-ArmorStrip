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
    private AvailableHandsCheck HasEnoughHandsFree;
    private AvailableHandsError TriggerHandsError;
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
    public delegate bool OnDoffWithoutDonner(IServerPlayer player, ItemSlot couldNotBeDonnedSlot);

    private delegate bool AvailableHandsCheck(IPlayer player);

    private delegate void AvailableHandsError(IPlayer player);

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

      switch (config.HandsNeededToDoff) {
        case 2:
          HasEnoughHandsFree = HasBothHandsFree;
          TriggerHandsError = TriggerBothHandsError;
          break;
        case 1:
          HasEnoughHandsFree = HasOneHandFree;
          TriggerHandsError = TriggerOneHandError;
          break;
        case 0:
        default:
          HasEnoughHandsFree = LookMomNoHands;
          TriggerHandsError = TriggerNoHandsError;
          break;
      }

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

    private void BroadcastArmorStandUpdated(EntityArmorStand armorStand) {
      if (armorStand == null || armorStand.World?.Side != EnumAppSide.Server) { return; }

      armorStand.WatchedAttributes.MarkAllDirty();
      ServerAPI.World.RegisterCallback((IWorldAccessor world, BlockPos pos, float dt) => {
        ServerChannel.BroadcastPacket(new ArmorStandInventoryUpdatedPacket(armorStand.EntityId));
      }, armorStand.Pos.AsBlockPos, 500);
    }

    private void Doff(IServerPlayer doffer, EntityArmorStand armorStand) {
      OnDoffWithoutDonner dropOrKeepItem = null;
      if (!DropArmorWhenDoffingToStand && armorStand != null) {
        dropOrKeepItem = KeepUndonnableOnDoff;
      }
      else {
        dropOrKeepItem = DropUndonnableOnDoff;
      }
      OnDonnedOneOrMore updateArmorStandRender = () => { BroadcastArmorStandUpdated(armorStand); };
      bool doffed = Doff(initiatingPlayer: doffer,
                         doffer: doffer.Entity,
                         donner: armorStand,
                         onDoffWithoutDonner: dropOrKeepItem,
                         onDonnedOneOrMore: updateArmorStandRender);

      if (doffed) { OnSuccessfulDoff(doffer); }
    }

    private bool Doff(IServerPlayer initiatingPlayer, EntityAgent doffer, EntityAgent donner = null, OnDoffWithoutDonner onDoffWithoutDonner = null, OnDonnedOneOrMore onDonnedOneOrMore = null) {
      if (doffer == null) { return false; }
      bool doffed = false;

      if (donner == null) {
        foreach (var slot in doffer.GetFilledArmorSlots()) {
          if (slot.Empty) { continue; }
          doffed = onDoffWithoutDonner?.Invoke(initiatingPlayer, slot) ?? true || doffed;
        }
      }
      else {
        bool donnerDonned = false;
        foreach (var slot in doffer.GetFilledArmorSlots()) {
          if (slot.Empty) { continue; }

          ItemSlot sinkSlot = GetAvailableSlotOn(donner, slot);
          if (sinkSlot != null && slot.TryPutInto(doffer.World, sinkSlot) > 0) {
            donnerDonned = true;
            sinkSlot.MarkDirty();
            doffed = true;
          }
          else {
            doffed = onDoffWithoutDonner?.Invoke(initiatingPlayer, slot) ?? true || doffed;
          }
        }
        if (donnerDonned) {
          onDonnedOneOrMore?.Invoke();
        }
      }
      return doffed;
    }

    private void Don(IServerPlayer donner, EntityArmorStand armorStand) {
      OnDonnedOneOrMore updateArmorStandRender = () => { BroadcastArmorStandUpdated(armorStand); };
      bool donned = Doff(initiatingPlayer: donner,
                         doffer: armorStand,
                         donner: donner.Entity,
                         onDoffWithoutDonner: KeepUndonnableOnDoff,
                         onDonnedOneOrMore: updateArmorStandRender);

      if (donned) { OnSuccessfulDon(donner); }
    }

    private bool DropUndonnableOnDoff(IServerPlayer doffer, ItemSlot couldNotBeDonnedSlot) {
      if (doffer == null) return false;
      return doffer.InventoryManager.DropItem(couldNotBeDonnedSlot, true);
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

    private bool HasBothHandsFree(IPlayer player) {
      if (player == null) { return true; }
      return player.Entity.LeftHandItemSlot.Empty && player.Entity.RightHandItemSlot.Empty;
    }

    private bool HasEnoughSaturation(IPlayer player, float neededSaturation) {
      var currentSaturation = player.Entity.WatchedAttributes.GetTreeAttribute("hunger")?.TryGetFloat("currentsaturation");
      if (currentSaturation == null) { return true; }
      return currentSaturation >= neededSaturation;
    }

    private bool HasOneHandFree(IPlayer player) {
      if (player == null) { return true; }
      return player.Entity.RightHandItemSlot.Empty || player.Entity.LeftHandItemSlot.Empty;
    }

    private bool LookMomNoHands(IPlayer player) {
      return true;
    }

    private void MarkArmorStandDirty(EntityArmorStand armorStand) {
      if (armorStand?.IsRendered ?? false) {
        armorStand.OnEntityLoaded(); // TODO: figure out if this has unwanted side effects or if there's a proper, more direct way of solving
      }
    }

    private bool KeepUndonnableOnDoff(IServerPlayer doffer, ItemSlot couldNotBeDonnedSlot) {
      return false; // False so that a doff that fails this way does not count for saturation depletion.
    }

    private void OnArmorStandInventoryUpdated(ArmorStandInventoryUpdatedPacket packet) {
      if (Api.Side != EnumAppSide.Client) { return; }
      var armorStand = GetEntityArmorStandById(ClientAPI.World.Player.Entity, packet.ArmorStandEntityId, 100, 100);
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

      if (!HasEnoughSaturation(doffer, SaturationCostPerDoff)) {
        TriggerSaturationError(doffer);
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

      if (!HasEnoughSaturation(donner, SaturationCostPerDon)) {
        TriggerSaturationError(donner);
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

    private void TriggerArmorStandTargetError(IClientPlayer player) {
      TriggerError(player, Constants.ERROR_MISSING_ARMOR_STAND_TARGET, Constants.ERROR_MISSING_ARMOR_STAND_TARGET_DESC);
    }

    private void TriggerBothHandsError(IPlayer player) {
      TriggerError(player, Constants.ERROR_BOTH_HANDS, Constants.ERROR_BOTH_HANDS_DESC);
    }

    private void TriggerNoHandsError(IPlayer player) {
      return;
    }

    private void TriggerOneHandError(IPlayer player) {
      TriggerError(player, Constants.ERROR_ONE_HAND, Constants.ERROR_ONE_HAND_DESC);
    }

    private void TriggerSaturationError(IPlayer player) {
      TriggerError(player, Constants.ERROR_SATURATION, Constants.ERROR_SATURATION_DESC);
    }

    private void TriggerError(IPlayer player, string errorCode, string errorFallbackDesc) {
      string errorText = Lang.GetIfExists($"doffanddonagain:ingameerror-{errorCode}") ?? errorFallbackDesc;
      (player as IServerPlayer)?.SendIngameError(errorCode, errorText);
      ClientAPI?.TriggerIngameError(this, errorCode, errorText);
    }
  }
}
