using System.Collections.Generic;
using DoffAndDonAgain.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace DoffAndDonAgain.Client {
  public class ArmorManipulationInputHandler {
    protected DoffAndDonSystem System { get; }
    protected IClientPlayer Player => System.ClientAPI.World.Player;
    protected EntityPlayer PlayerEntity => Player.Entity;
    protected bool IsLeftHandEmpty => PlayerEntity.LeftHandItemSlot.Empty;
    protected bool IsRightHandEmpty => PlayerEntity.RightHandItemSlot.Empty;
    protected long? TargetedArmorStandEntityId => (Player.CurrentEntitySelection?.Entity as EntityArmorStand)?.EntityId;

    protected bool IsDoffToGroundEnabled { get; set; } = true;
    protected bool IsDoffToArmorStandEnabled { get; set; } = true;
    protected bool IsDonEnabled { get; set; } = true;
    protected bool IsSwapEnabled { get; set; } = true;
    protected Dictionary<EnumActionType, int> HandsRequired { get; set; } = new Dictionary<EnumActionType, int> {
      { EnumActionType.Doff, 0 },
      { EnumActionType.Don, 0 },
      { EnumActionType.Swap, 0 }
    };
    protected Dictionary<EnumActionType, float> SaturationRequired { get; set; } = new Dictionary<EnumActionType, float> {
      { EnumActionType.Doff, 0f },
      { EnumActionType.Don, 0f },
      { EnumActionType.Swap, 0f }
    };

    public ArmorManipulationInputHandler(DoffAndDonSystem system) {
      if (system.Side != EnumAppSide.Client) {
        system.Api.Logger.Warning("{0} is a client object instantiated on the server, ignoring.", nameof(ArmorManipulationInputHandler));
        return;
      }
      System = system;

      LoadServerSettings(system.Api);
      RegisterHotKeys(system.ClientAPI.Input, system.Event);

      system.Event.OnDoffKeyPressed += OnDoffKeyPressed;
      system.Event.OnDonKeyPressed += OnDonKeyPressed;
      system.Event.OnSwapKeyPressed += OnSwapKeyPressed;
    }

    protected void RegisterHotKeys(IInputAPI input, DoffAndDonEventApi eventApi) {
      input.RegisterHotKey(Constants.DOFF_CODE, Constants.DOFF_DESC, Constants.DEFAULT_KEY, HotkeyType.CharacterControls, ctrlPressed: true);
      input.SetHotKeyHandler(Constants.DOFF_CODE, eventApi.TriggerDoffKeyPressed);

      input.RegisterHotKey(Constants.DON_CODE, Constants.DON_DESC, Constants.DEFAULT_KEY, HotkeyType.CharacterControls);
      input.SetHotKeyHandler(Constants.DON_CODE, eventApi.TriggerDonKeyPressed);

      input.RegisterHotKey(Constants.SWAP_CODE, Constants.SWAP_DESC, Constants.DEFAULT_KEY, HotkeyType.CharacterControls, shiftPressed: true);
      input.SetHotKeyHandler(Constants.SWAP_CODE, eventApi.TriggerSwapKeyPressed);
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

    protected void LoadServerSettings(DoffAndDonServerConfig serverSettings) {
      IsDoffToGroundEnabled = serverSettings.EnableDoffToGround.Value;
      IsDoffToArmorStandEnabled = serverSettings.EnableDoffToArmorStand.Value;

      IsDonEnabled = serverSettings.EnableDon.Value;

      IsSwapEnabled = serverSettings.EnableSwap.Value;

      HandsRequired[EnumActionType.Doff] = serverSettings.HandsNeededToDoff.Value;
      HandsRequired[EnumActionType.Don] = serverSettings.HandsNeededToDon.Value;
      HandsRequired[EnumActionType.Swap] = serverSettings.HandsNeededToSwap.Value;

      SaturationRequired[EnumActionType.Doff] = serverSettings.SaturationCostPerDoff.Value;
      SaturationRequired[EnumActionType.Don] = serverSettings.SaturationCostPerDon.Value;
      SaturationRequired[EnumActionType.Swap] = serverSettings.SaturationCostPerSwap.Value;
    }

    protected void OnDoffKeyPressed(ref ArmorActionEventArgs eventArgs) {
      eventArgs.Successful = VerifyDoffEnabled(eventArgs)
                             && VerifyEnoughHandsFree(eventArgs)
                             && VerifyEnoughSaturation(eventArgs);
    }

    protected void OnDonKeyPressed(ref ArmorActionEventArgs eventArgs) {
      eventArgs.Successful = VerifyDonEnabled(eventArgs)
                             && VerifyTargetingArmorStand(eventArgs)
                             && VerifyEnoughHandsFree(eventArgs)
                             && VerifyEnoughSaturation(eventArgs);
    }

    protected void OnSwapKeyPressed(ref ArmorActionEventArgs eventArgs) {
      eventArgs.Successful = VerifySwapEnabled(eventArgs)
                             && VerifyTargetingArmorStand(eventArgs)
                             && VerifyEnoughHandsFree(eventArgs)
                             && VerifyEnoughSaturation(eventArgs);
    }

    protected bool VerifyDoffEnabled(ArmorActionEventArgs eventArgs) {
      eventArgs.ArmorStandEntityId = TargetedArmorStandEntityId;
      if (eventArgs.ArmorStandEntityId == null) {
        eventArgs.TargetType = EnumTargetType.Nothing;
        return VerifyDoffToGroundEnabled(eventArgs);
      }
      eventArgs.TargetType = EnumTargetType.ArmorStand;
      return VerifyDoffToArmorStandEnabled(eventArgs);
    }

    protected bool VerifyDoffToGroundEnabled(ArmorActionEventArgs eventArgs) {
      if (!IsDoffToGroundEnabled) {
        eventArgs.ErrorCode = Constants.ERROR_DOFF_GROUND_DISABLED;
        return false;
      }
      return true;
    }

    protected bool VerifyDoffToArmorStandEnabled(ArmorActionEventArgs eventArgs) {
      if (!IsDoffToArmorStandEnabled) {
        eventArgs.ErrorCode = Constants.ERROR_DOFF_STAND_DISABLED;
        return false;
      }
      return true;
    }

    protected bool VerifyDonEnabled(ArmorActionEventArgs eventArgs) {
      if (!IsDonEnabled) {
        eventArgs.ErrorCode = Constants.ERROR_DON_DISABLED;
        return false;
      }
      return true;
    }

    protected bool VerifySwapEnabled(ArmorActionEventArgs eventArgs) {
      if (!IsSwapEnabled) {
        eventArgs.ErrorCode = Constants.ERROR_SWAP_DISABLED;
        return false;
      }
      return true;
    }

    protected bool VerifyEnoughHandsFree(ArmorActionEventArgs eventArgs) {
      switch (HandsRequired[eventArgs.ActionType]) {
        case 2:
          return VerifyBothHandsFree(eventArgs);
        case 1:
          return VerifyOneHandFree(eventArgs);
        default:
          return LookMomNoHands(eventArgs);
      }
    }

    protected bool VerifyBothHandsFree(ArmorActionEventArgs eventArgs) {
      if (!IsLeftHandEmpty && IsRightHandEmpty) {
        eventArgs.ErrorCode = Constants.ERROR_BOTH_HANDS;
        return false;
      }
      return true;
    }

    protected bool VerifyOneHandFree(ArmorActionEventArgs eventArgs) {
      if (!IsRightHandEmpty || IsLeftHandEmpty) {
        eventArgs.ErrorCode = Constants.ERROR_ONE_HAND;
        return false;
      }
      return true;
    }

    protected bool LookMomNoHands(ArmorActionEventArgs eventArgs) => true;

    protected bool VerifyEnoughSaturation(ArmorActionEventArgs eventArgs) {
      // If satiety can't be read or is disabled for any reason, give the benefit of doubt and pass the check.
      var requiredSaturation = SaturationRequired[eventArgs.ActionType];
      var currentSaturation = PlayerEntity.GetBehavior<EntityBehaviorHunger>()?.Saturation ?? requiredSaturation;
      if (currentSaturation < requiredSaturation) {
        eventArgs.ErrorCode = Constants.ERROR_SATURATION;
        eventArgs.ErrorArgs = new string[] { requiredSaturation.ToString() };
        return false;
      }
      return true;
    }

    protected bool VerifyTargetingArmorStand(ArmorActionEventArgs eventArgs) {
      eventArgs.ArmorStandEntityId = TargetedArmorStandEntityId;
      if (eventArgs.ArmorStandEntityId == null) {
        eventArgs.ErrorCode = Constants.ERROR_MISSING_ARMOR_STAND_TARGET;
        return false;
      }
      return true;
    }
  }
}
