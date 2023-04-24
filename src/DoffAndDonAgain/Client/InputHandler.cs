using System.Collections.Generic;
using DoffAndDonAgain.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace DoffAndDonAgain.Client {
  public class InputHandler {
    protected DoffAndDonSystem System { get; }
    protected IClientPlayer Player => System.ClientAPI.World.Player;
    protected EntityPlayer PlayerEntity => Player.Entity;
    protected bool IsLeftHandEmpty => PlayerEntity.LeftHandItemSlot.Empty;
    protected bool IsRightHandEmpty => PlayerEntity.RightHandItemSlot.Empty;
    protected EntityAgent TargetedEntityAgent => Player.CurrentEntitySelection?.Entity as EntityAgent;

    protected bool IsDoffToGroundEnabled { get; set; } = true;
    protected bool IsDoffToEntityEnabled { get; set; } = true;
    protected bool IsDonEnabled { get; set; } = true;
    protected bool IsSwapEnabled { get; set; } = true;
    protected Dictionary<int, ActionConsumable<DoffAndDonEventArgs>> HandsRequiredDictionary { get; set; } = new Dictionary<int, ActionConsumable<DoffAndDonEventArgs>>();
    protected int HandsRequired = 2;

    public InputHandler(DoffAndDonSystem doffAndDonSystem) {
      if (doffAndDonSystem.Side != EnumAppSide.Client) {
        throw new System.Exception($"Tried to create an instance of {nameof(InputHandler)} Server-side or without a valid {nameof(ICoreAPI)} reference.");
      }
      System = doffAndDonSystem;

      HandsRequiredDictionary.Add(0, LookMomNoHands);
      HandsRequiredDictionary.Add(1, VerifyOneHandFree);
      HandsRequiredDictionary.Add(2, VerifyBothHandsFree);
      LoadServerSettings(doffAndDonSystem.Api);
      RegisterHotKeys(doffAndDonSystem);

      doffAndDonSystem.OnDoffKeyPressed += OnDoffKeyPressed;
      doffAndDonSystem.OnDonKeyPressed += OnDonKeyPressed;
      doffAndDonSystem.OnSwapKeyPressed += OnSwapKeyPressed;
    }

    protected void RegisterHotKeys(DoffAndDonSystem doffAndDonSystem) {
      var input = doffAndDonSystem.ClientAPI.Input;
      input.RegisterHotKey(Constants.DOFF_CODE, Constants.DOFF_DESC, Constants.DEFAULT_KEY, HotkeyType.CharacterControls, ctrlPressed: true);
      input.SetHotKeyHandler(Constants.DOFF_CODE, doffAndDonSystem.TriggerDoffKeyPressed);

      input.RegisterHotKey(Constants.DON_CODE, Constants.DON_DESC, Constants.DEFAULT_KEY, HotkeyType.CharacterControls);
      input.SetHotKeyHandler(Constants.DON_CODE, doffAndDonSystem.TriggerDonKeyPressed);

      input.RegisterHotKey(Constants.SWAP_CODE, Constants.SWAP_DESC, Constants.DEFAULT_KEY, HotkeyType.CharacterControls, shiftPressed: true);
      input.SetHotKeyHandler(Constants.SWAP_CODE, doffAndDonSystem.TriggerSwapKeyPressed);
    }

    protected void LoadServerSettings(ICoreAPI api) {
      var worldConfig = api.World.Config;
      HandsRequired = worldConfig.GetInt("doffanddon-HandsNeeded", 2);
    }

    protected void OnDoffKeyPressed(DoffAndDonEventArgs eventArgs) {
      eventArgs.Successful = VerifyDoffEnabled(eventArgs)
                             && VerifyEnoughHandsFree(eventArgs);
    }

    protected void OnDonKeyPressed(DoffAndDonEventArgs eventArgs) {
      eventArgs.Successful = VerifyDonEnabled(eventArgs)
                             && VerifyTargetEntityIsValid(eventArgs)
                             && VerifyEnoughHandsFree(eventArgs);
    }

    protected void OnSwapKeyPressed(DoffAndDonEventArgs eventArgs) {
      eventArgs.Successful = VerifySwapEnabled(eventArgs)
                             && VerifyTargetEntityIsValid(eventArgs)
                             && VerifyEnoughHandsFree(eventArgs);
    }

    protected bool VerifyDoffEnabled(DoffAndDonEventArgs eventArgs) {
      eventArgs.TargetEntityAgentId = TargetedEntityAgent?.EntityId;
      if (eventArgs.TargetEntityAgentId == null) {
        eventArgs.TargetType = EnumTargetType.Nothing;
        return VerifyDoffToGroundEnabled(eventArgs);
      }

      eventArgs.TargetType = EnumTargetType.EntityAgent;
      return VerifyDoffToEntityEnabled(eventArgs) && VerifyTargetEntityIsValid(eventArgs);
    }

    protected bool VerifyDoffToGroundEnabled(DoffAndDonEventArgs eventArgs) {
      if (!IsDoffToGroundEnabled) {
        eventArgs.ErrorCode = Constants.ERROR_DOFF_GROUND_DISABLED;
        return false;
      }
      return true;
    }

    protected bool VerifyDoffToEntityEnabled(DoffAndDonEventArgs eventArgs) {
      if (!IsDoffToEntityEnabled) {
        eventArgs.ErrorCode = Constants.ERROR_DOFF_STAND_DISABLED;
        return false;
      }
      return true;
    }

    protected bool VerifyDonEnabled(DoffAndDonEventArgs eventArgs) {
      if (!IsDonEnabled) {
        eventArgs.ErrorCode = Constants.ERROR_DON_DISABLED;
        return false;
      }
      return true;
    }

    protected bool VerifySwapEnabled(DoffAndDonEventArgs eventArgs) {
      if (!IsSwapEnabled) {
        eventArgs.ErrorCode = Constants.ERROR_SWAP_DISABLED;
        return false;
      }
      return true;
    }

    protected bool VerifyEnoughHandsFree(DoffAndDonEventArgs eventArgs) {
      if (!HandsRequiredDictionary.TryGetValue(HandsRequired, out var VerifyMethod)) {
        VerifyMethod = VerifyBothHandsFree;
      }
      return VerifyMethod(eventArgs);
    }

    protected bool VerifyBothHandsFree(DoffAndDonEventArgs eventArgs) {
      if (IsLeftHandEmpty && IsRightHandEmpty) {
        return true;
      }

      eventArgs.ErrorCode = Constants.ERROR_BOTH_HANDS;
      return false;
    }

    protected bool VerifyOneHandFree(DoffAndDonEventArgs eventArgs) {
      if (IsRightHandEmpty || IsLeftHandEmpty) {
        return true;
      }

      eventArgs.ErrorCode = Constants.ERROR_ONE_HAND;
      return false;
    }

    protected bool LookMomNoHands(DoffAndDonEventArgs eventArgs) => true;

    protected bool VerifyTargetEntityIsValid(DoffAndDonEventArgs eventArgs) {
      if (TargetedEntityAgent == null) {
        eventArgs.ErrorCode = Constants.ERROR_MUST_TARGET_ENTITY;
        eventArgs.ErrorArgs = new string[] { eventArgs.ActionType.ToString() };
        return false;
      }

      eventArgs.TargetEntityAgentId = TargetedEntityAgent.EntityId;
      if (!TargetedEntityAgent.CanBeTargetedFor(eventArgs.ActionType)) {
        eventArgs.ErrorCode = Constants.ERROR_INVALID_ENTITY_TARGET;
        eventArgs.ErrorArgs = new string[] { eventArgs.ActionType.ToString(), TargetedEntityAgent.GetName() };
        return false;
      }

      return true;
    }
  }
}
