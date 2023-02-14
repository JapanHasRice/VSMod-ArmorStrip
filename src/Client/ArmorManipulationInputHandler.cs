using DoffAndDonAgain.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace DoffAndDonAgain.Client {
  public abstract class ArmorManipulationInputHandler {
    protected DoffAndDonSystem System { get; }
    protected IClientPlayer Player => System.ClientAPI.World.Player;
    protected EntityPlayer PlayerEntity => Player.Entity;
    protected float SaturationRequired { get; set; } = 0f;
    private int _handsRequired;
    public int HandsRequired {
      get => _handsRequired;
      protected set {
        _handsRequired = value;
        switch (HandsRequired) {
          case 2:
            HasEnoughHandsFree = HasBothHandsFree;
            break;
          case 1:
            HasEnoughHandsFree = HasOneHandFree;
            break;
          case 0:
            HasEnoughHandsFree = LookMomNoHands;
            break;
          default:
            System.Api.Logger.Warning("Attempted to set 'HandsRequired' to an invalid value '{0}', valid values are [0, 1, 2]. Defaulting to 0.", value);
            _handsRequired = 0;
            HasEnoughHandsFree = LookMomNoHands;
            break;
        }
      }
    }

    protected delegate bool AvailableHandsCheck(out string errorCode);

    protected AvailableHandsCheck HasEnoughHandsFree;

    public ArmorManipulationInputHandler(DoffAndDonSystem system) {
      if (system.Side != EnumAppSide.Client) {
        system.Api.Logger.Warning("{0} is a client object instantiated on the server, ignoring.", nameof(ArmorManipulationInputHandler));
        return;
      }
      System = system;
      HandsRequired = 0;
      LoadServerSettings(system.Api);
    }

    protected void LoadServerSettings(ICoreAPI api) {
      var configSystem = api.ModLoader.GetModSystem<DoffAndDonConfigurationSystem>();
      if (configSystem == null) {
        api.Logger.Error("[{0}] {1} was not loaded. Using defaults.", nameof(ArmorManipulationInputHandler), nameof(DoffAndDonConfigurationSystem));
        LoadServerSettings(new DoffAndDonServerConfig());
        return;
      }

      configSystem.ServerSettingsReceived += LoadServerSettings;
      if (configSystem.ServerSettings != null) {
        LoadServerSettings(configSystem.ServerSettings);
      }
    }

    protected abstract void LoadServerSettings(DoffAndDonServerConfig serverSettings);

    protected EntityArmorStand GetTargetedArmorStandEntity() => Player.CurrentEntitySelection?.Entity as EntityArmorStand;

    protected bool HasBothHandsFree(out string errorCode) {
      errorCode = null;
      bool bothHandsAreFree = IsLeftHandEmpty() && IsRightHandEmpty();
      if (!bothHandsAreFree) {
        errorCode = Constants.ERROR_BOTH_HANDS;
      }
      return bothHandsAreFree;
    }

    protected bool HasOneHandFree(out string errorCode) {
      errorCode = null;
      bool oneHandIsFree = IsRightHandEmpty() || IsLeftHandEmpty();
      if (!oneHandIsFree) {
        errorCode = Constants.ERROR_ONE_HAND;
      }
      return oneHandIsFree;
    }

    protected bool LookMomNoHands(out string errorCode) {
      errorCode = null;
      return true;
    }

    private bool IsLeftHandEmpty() => PlayerEntity.LeftHandItemSlot.Empty;
    private bool IsRightHandEmpty() => PlayerEntity.RightHandItemSlot.Empty;

    protected bool HasEnoughSaturation(out string errorCode) {
      errorCode = null;
      var currentSaturation = PlayerEntity.WatchedAttributes.GetTreeAttribute("hunger")?.TryGetFloat("currentsaturation");
      // If satiety can't be read or is disabled for any reason, give the benefit of doubt and pass the check.
      bool enoughSaturation = currentSaturation == null ? true : currentSaturation >= SaturationRequired;
      if (!enoughSaturation) {
        errorCode = System.Error.GetErrorText(Constants.ERROR_SATURATION, SaturationRequired);
      }
      return enoughSaturation;
    }

    protected bool IsTargetingArmorStand(out long armorStandEntityId, out string errorCode) {
      errorCode = null;
      armorStandEntityId = GetTargetedArmorStandEntity()?.EntityId ?? -1;
      if (armorStandEntityId == -1) {
        errorCode = Constants.ERROR_MISSING_ARMOR_STAND_TARGET;
        return false;
      }
      return true;
    }
  }
}
