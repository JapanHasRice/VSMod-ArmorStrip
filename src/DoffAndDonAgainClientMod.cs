using DoffAndDonAgain.Config;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace DoffAndDonAgain {
  public class DoffAndDonAgainClientMod : DoffAndDonAgainMod {
    #region Variables and Properties
    protected ICoreClientAPI ClientAPI;
    protected IClientNetworkChannel ClientChannel;
    protected AvailableHandsCheck HasEnoughHandsFree;
    protected AvailableHandsError TriggerHandsError;

    protected IClientPlayer Player {
      get { return ClientAPI.World.Player; }
    }

    protected EntityPlayer PlayerEntity {
      get { return Player.Entity; }
    }

    #endregion

    #region Delegates
    protected delegate bool AvailableHandsCheck();
    protected delegate void AvailableHandsError();

    #endregion

    #region Initialization
    public override EnumAppSide ForSide() {
      return EnumAppSide.Client;
    }

    public override void Start(ICoreAPI api) {
      base.Start(api);
      ClientAPI = api as ICoreClientAPI;
      ClientChannel = Channel as IClientNetworkChannel;

      LoadClientConfigs();
      SetupClientNetwork();
      SetupHotkeys();
    }

    private void LoadClientConfigs() {
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
    }

    private void SetupClientNetwork() {
      ClientChannel
        .SetMessageHandler<ArmorStandInventoryUpdatedPacket>(OnArmorStandInventoryUpdated);
    }

    protected void SetupHotkeys() {
      ClientAPI.Input.RegisterHotKey(Constants.DOFF_CODE, Constants.DOFF_DESC, Constants.DEFAULT_KEY, HotkeyType.CharacterControls, ctrlPressed: true);
      ClientAPI.Input.SetHotKeyHandler(Constants.DOFF_CODE, OnTryToDoff);

      ClientAPI.Input.RegisterHotKey(Constants.DON_CODE, Constants.DON_DESC, Constants.DEFAULT_KEY, HotkeyType.CharacterControls);
      ClientAPI.Input.SetHotKeyHandler(Constants.DON_CODE, OnTryToDon);

      ClientAPI.Input.RegisterHotKey(Constants.SWAP_CODE, Constants.SWAP_DESC, Constants.DEFAULT_KEY, HotkeyType.CharacterControls, shiftPressed: true);
      ClientAPI.Input.SetHotKeyHandler(Constants.SWAP_CODE, OnTryToSwap);
    }

    #endregion

    protected EntityArmorStand GetTargetedArmorStandEntity() {
      return Player.CurrentEntitySelection?.Entity as EntityArmorStand;
    }

    protected bool HasBothHandsFree() {
      return PlayerEntity.LeftHandItemSlot.Empty && PlayerEntity.RightHandItemSlot.Empty;
    }

    protected bool HasEnoughSaturation(float neededSaturation) {
      var currentSaturation = PlayerEntity.WatchedAttributes.GetTreeAttribute("hunger")?.TryGetFloat("currentsaturation");
      if (currentSaturation == null) { return true; } // If satiety can't be read, give player benefit of doubt.
      return currentSaturation >= neededSaturation;
    }

    protected bool HasOneHandFree() {
      return PlayerEntity.RightHandItemSlot.Empty || PlayerEntity.LeftHandItemSlot.Empty;
    }

    protected bool LookMomNoHands() {
      return true;
    }

    protected void MarkArmorStandDirty(EntityArmorStand armorStand) {
      if (armorStand?.IsRendered ?? false) {
        armorStand.OnEntityLoaded(); // TODO: figure out if this has unwanted side effects or if there's a proper, more direct way of solving
      }
    }

    protected void OnArmorStandInventoryUpdated(ArmorStandInventoryUpdatedPacket packet) {
      var armorStand = GetEntityArmorStandById(PlayerEntity, packet.ArmorStandEntityId, 100, 100);
      MarkArmorStandDirty(armorStand);
    }

    protected bool OnTryToDoff(KeyCombination kc) {
      if (!HasEnoughHandsFree()) {
        TriggerHandsError();
        return false;
      }

      if (!HasEnoughSaturation(SaturationCostPerDoff)) {
        TriggerSaturationError();
        return false;
      }

      if (PlayerEntity.GetFilledArmorSlots().Count == 0) {
        TriggerNotWearingArmorError();
        return false;
      }

      SendDoffRequest();
      return true;
    }

    protected bool OnTryToDon(KeyCombination kc) {
      if (!HasEnoughHandsFree()) {
        TriggerHandsError();
        return false;
      }

      var armorStand = GetTargetedArmorStandEntity();
      if (armorStand == null) {
        TriggerArmorStandTargetError();
        return false;
      }

      if (!HasEnoughSaturation(SaturationCostPerDon)) {
        TriggerSaturationError();
        return false;
      }

      if (armorStand.GetFilledArmorSlots().Count == 0) {
        TriggerEmptyArmorStandError();
        return false;
      }

      SendDonRequest(armorStand);
      return true;
    }

    protected bool OnTryToSwap(KeyCombination kc) {
      if (!HasEnoughHandsFree()) {
        TriggerHandsError();
        return false;
      }

      var armorStand = GetTargetedArmorStandEntity();
      if (armorStand == null) {
        TriggerArmorStandTargetError();
        return false;
      }

      if (!HasEnoughSaturation(SaturationCostPerSwap)) {
        TriggerSaturationError();
        return false;
      }

      if (PlayerEntity.GetFilledArmorSlots().Count == 0 && armorStand.GetFilledArmorSlots().Count == 0) {
        TriggerNothingToSwapError();
        return false;
      }

      SendSwapRequest(armorStand);
      return true;
    }

    protected void SendDoffRequest() {
      var doffArmorPacket = new DoffArmorPacket(GetTargetedArmorStandEntity()?.EntityId);
      ClientChannel.SendPacket(doffArmorPacket);
    }

    protected void SendDonRequest(EntityArmorStand armorStand) {
      var donArmorPacket = new DonArmorPacket(armorStand.EntityId);
      ClientChannel.SendPacket(donArmorPacket);
    }

    protected void SendSwapRequest(EntityArmorStand armorStand) {
      var swapArmorPacket = new SwapArmorPacket(armorStand.EntityId);
      ClientChannel.SendPacket(swapArmorPacket);
    }

    protected void TriggerArmorStandTargetError() {
      TriggerError(Constants.ERROR_MISSING_ARMOR_STAND_TARGET, Constants.ERROR_MISSING_ARMOR_STAND_TARGET_DESC);
    }

    protected void TriggerNothingToSwapError() {
      TriggerError(Constants.ERROR_BIRTHDAY_SUIT_PARTY, Constants.ERROR_BIRTHDAY_SUIT_PARTY_DESC);
    }

    protected void TriggerBothHandsError() {
      TriggerError(Constants.ERROR_BOTH_HANDS, Constants.ERROR_BOTH_HANDS_DESC);
    }

    protected void TriggerEmptyArmorStandError() {
      TriggerError(Constants.ERROR_NOTHING_TO_DON, Constants.ERROR_NOTHING_TO_DON_DESC);
    }

    protected void TriggerError(string errorCode, string errorFallbackText) {
      ClientAPI.TriggerIngameError(this, errorCode, GetErrorText(errorCode, errorFallbackText));
    }

    protected void TriggerNoHandsError() {
      return;
    }

    protected void TriggerNotWearingArmorError() {
      TriggerError(Constants.ERROR_NOTHING_TO_DOFF, Constants.ERROR_NOTHING_TO_DOFF_DESC);
    }

    protected void TriggerOneHandError() {
      TriggerError(Constants.ERROR_ONE_HAND, Constants.ERROR_ONE_HAND_DESC);
    }

    protected void TriggerSaturationError() {
      TriggerError(Constants.ERROR_SATURATION, Constants.ERROR_SATURATION_DESC);
    }
  }
}
