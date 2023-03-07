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
    private bool IsLeftHandEmpty => PlayerEntity.LeftHandItemSlot.Empty;
    private bool IsRightHandEmpty => PlayerEntity.RightHandItemSlot.Empty;
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
      input.RegisterHotKey(Constants.DON_CODE, Constants.DON_DESC, Constants.DEFAULT_KEY, HotkeyType.CharacterControls);
      input.SetHotKeyHandler(Constants.DON_CODE, eventApi.TriggerDonKeyPressed);

      input.RegisterHotKey(Constants.DOFF_CODE, Constants.DOFF_DESC, Constants.DEFAULT_KEY, HotkeyType.CharacterControls, ctrlPressed: true);
      input.SetHotKeyHandler(Constants.DOFF_CODE, eventApi.TriggerDoffKeyPressed);

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

    private void OnDoffKeyPressed(ref ArmorActionEventArgs eventArgs) {
      _ = VerifyDoffEnabled(ref eventArgs)
          && VerifyEnoughHandsFree(ref eventArgs)
          && VerifyEnoughSaturation(ref eventArgs);
    }

    private void OnDonKeyPressed(ref ArmorActionEventArgs eventArgs) {
      _ = VerifyDonEnabled(ref eventArgs)
          && VerifyTargetingArmorStand(ref eventArgs)
          && VerifyEnoughHandsFree(ref eventArgs)
          && VerifyEnoughSaturation(ref eventArgs);
    }

    private void OnSwapKeyPressed(ref ArmorActionEventArgs eventArgs) {
      _ = VerifySwapEnabled(ref eventArgs)
          && VerifyTargetingArmorStand(ref eventArgs)
          && VerifyEnoughHandsFree(ref eventArgs)
          && VerifyEnoughSaturation(ref eventArgs);
    }

    private bool VerifyDoffEnabled(ref ArmorActionEventArgs eventArgs) {
      eventArgs.ArmorStandEntityId = TargetedArmorStandEntityId;
      if (eventArgs.ArmorStandEntityId == null) {
        eventArgs.TargetType = EnumTargetType.Nothing;
        return VerifyDoffToGroundEnabled(ref eventArgs);
      }
      eventArgs.TargetType = EnumTargetType.ArmorStand;
      return VerifyDoffToArmorStandEnabled(ref eventArgs);
    }

    private bool VerifyDoffToGroundEnabled(ref ArmorActionEventArgs eventArgs) {
      eventArgs.Successful = IsDoffToArmorStandEnabled;
      if (!IsDoffToGroundEnabled) {
        eventArgs.ErrorCode = Constants.ERROR_DOFF_GROUND_DISABLED;
      }
      return eventArgs.Successful;
    }

    private bool VerifyDoffToArmorStandEnabled(ref ArmorActionEventArgs eventArgs) {
      eventArgs.Successful = IsDoffToArmorStandEnabled;
      if (!IsDoffToArmorStandEnabled) {
        eventArgs.ErrorCode = Constants.ERROR_DOFF_STAND_DISABLED;
      }
      return eventArgs.Successful;
    }

    private bool VerifyDonEnabled(ref ArmorActionEventArgs eventArgs) {
      eventArgs.Successful = IsDonEnabled;
      if (!IsDonEnabled) {
        eventArgs.ErrorCode = Constants.ERROR_DON_DISABLED;
      }
      return eventArgs.Successful;
    }

    private bool VerifySwapEnabled(ref ArmorActionEventArgs eventArgs) {
      eventArgs.Successful = IsSwapEnabled;
      if (!IsSwapEnabled) {
        eventArgs.ErrorCode = Constants.ERROR_SWAP_DISABLED;
      }
      return eventArgs.Successful;
    }

    protected bool VerifyEnoughHandsFree(ref ArmorActionEventArgs eventArgs) {
      switch (HandsRequired[eventArgs.ActionType]) {
        case 2:
          return VerifyBothHandsFree(ref eventArgs);
        case 1:
          return VerifyOneHandFree(ref eventArgs);
        default:
          return LookMomNoHands(ref eventArgs);
      }
    }

    protected bool VerifyBothHandsFree(ref ArmorActionEventArgs eventArgs) {
      eventArgs.Successful = IsLeftHandEmpty && IsRightHandEmpty;
      if (!eventArgs.Successful) {
        eventArgs.ErrorCode = Constants.ERROR_BOTH_HANDS;
      }
      return eventArgs.Successful;
    }

    protected bool VerifyOneHandFree(ref ArmorActionEventArgs eventArgs) {
      eventArgs.Successful = IsRightHandEmpty || IsLeftHandEmpty;
      if (!eventArgs.Successful) {
        eventArgs.ErrorCode = Constants.ERROR_ONE_HAND;
      }
      return eventArgs.Successful;
    }

    protected bool LookMomNoHands(ref ArmorActionEventArgs eventArgs) {
      eventArgs.Successful = true;
      return eventArgs.Successful;
    }

    protected bool VerifyEnoughSaturation(ref ArmorActionEventArgs eventArgs) {
      var currentSaturation = PlayerEntity.WatchedAttributes.GetTreeAttribute("hunger")?.TryGetFloat("currentsaturation");
      // If satiety can't be read or is disabled for any reason, give the benefit of doubt and pass the check.
      eventArgs.Successful = currentSaturation == null ? true : currentSaturation >= SaturationRequired[eventArgs.ActionType];
      if (!eventArgs.Successful) {
        eventArgs.ErrorCode = Constants.ERROR_SATURATION;
        eventArgs.ErrorArgs = new object[] { SaturationRequired[eventArgs.ActionType] };
      }
      return eventArgs.Successful;
    }

    protected bool VerifyTargetingArmorStand(ref ArmorActionEventArgs eventArgs) {
      eventArgs.ArmorStandEntityId = TargetedArmorStandEntityId;
      eventArgs.Successful = eventArgs.ArmorStandEntityId != null;
      if (!eventArgs.Successful) {
        eventArgs.ErrorCode = Constants.ERROR_MISSING_ARMOR_STAND_TARGET;
      }
      return eventArgs.Successful;
    }
  }
}
