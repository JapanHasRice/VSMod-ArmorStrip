using System.Collections.Generic;
using System.Linq;
using DoffAndDonAgain.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace DoffAndDonAgain.Client {
  public class InputHandler {
    protected DoffAndDonSystem DoffAndDonSystem { get; }
    protected string ModId => DoffAndDonSystem.Mod.Info.ModID;
    protected IClientPlayer Player => DoffAndDonSystem.ClientAPI.World.Player;
    protected EntityPlayer PlayerEntity => Player.Entity;
    protected bool IsLeftHandEmpty => PlayerEntity.LeftHandItemSlot.Empty;
    protected bool IsRightHandEmpty => PlayerEntity.RightHandItemSlot.Empty;
    protected EntityAgent TargetedEntityAgent => Player.CurrentEntitySelection?.Entity as EntityAgent;

    protected bool ShouldDoffArmorToGround { get; set; }
    protected bool ShouldDoffClothingToGround { get; set; }
    protected bool ShouldDoffArmorToEntities { get; set; }
    protected bool ShouldDoffClothingToEntities { get; set; }
    protected bool IsDoffToGroundEnabled { get; set; }
    protected bool IsDoffToEntityEnabled { get; set; }

    protected bool ShouldDonArmor { get; set; }
    protected bool ShouldDonClothing { get; set; }
    protected bool ShouldDonMisc { get; set; }
    protected bool IsDonEnabled { get; set; }
    protected EnumDonMiscBehavior DonMiscBehavior { get; set; }

    protected bool ShouldSwapArmor { get; set; }
    protected bool ShouldSwapClothing { get; set; }
    protected bool IsSwapEnabled { get; set; }

    protected Dictionary<int, ActionConsumable<DoffAndDonEventArgs>> HandsRequiredDictionary { get; set; } = new Dictionary<int, ActionConsumable<DoffAndDonEventArgs>>();
    protected int HandsNeeded { get; set; }
    protected float SaturationCost { get; set; }

    public InputHandler(DoffAndDonSystem doffAndDonSystem) {
      if (doffAndDonSystem.Side != EnumAppSide.Client) {
        throw new System.Exception($"Tried to create an instance of {nameof(InputHandler)} Server-side or without a valid {nameof(ICoreAPI)} reference.");
      }
      DoffAndDonSystem = doffAndDonSystem;

      HandsRequiredDictionary.Add(0, LookMomNoHands);
      HandsRequiredDictionary.Add(1, VerifyOneHandFree);
      HandsRequiredDictionary.Add(2, VerifyBothHandsFree);
      LoadSettings(doffAndDonSystem.Api);
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

    protected void LoadSettings(ICoreAPI api) {
      var worldConfig = api.World.Config;
      var clientSettings = api.ModLoader.GetModSystem<ConfigSystem>()?.ClientSettings ?? new ClientSettings();

      HandsNeeded = System.Math.Max(worldConfig.GetInt("doffanddon-HandsNeeded", 2), clientSettings.HandsNeeded.Value);
      if (clientSettings.SaturationCost.Value > worldConfig.GetFloat("doffanddon-SaturationCost", 0f)) {
        SaturationCost = clientSettings.SaturationCost.Value;
      }

      ShouldDoffArmorToGround = worldConfig.GetBool("doffanddon-DoffArmorToGround", true) && clientSettings.DoffArmorToGround.Value;
      ShouldDoffClothingToGround = worldConfig.GetBool("doffanddon-DoffClothingToGround", false) && clientSettings.DoffClothingToGround.Value;
      IsDoffToGroundEnabled = ShouldDoffArmorToGround || ShouldDoffClothingToGround;

      ShouldDoffArmorToEntities = worldConfig.GetBool("doffanddon-DoffArmorToEntities", true) && clientSettings.DoffArmorToEntities.Value;
      ShouldDoffClothingToEntities = worldConfig.GetBool("doffanddon-DoffClothingToEntities", true) && clientSettings.DoffClothingToEntities.Value;
      IsDoffToEntityEnabled = ShouldDoffArmorToEntities || ShouldDoffClothingToEntities;

      ShouldDonArmor = worldConfig.GetBool("doffanddon-DonArmorFromEntities", true) && clientSettings.DonArmorFromEntities.Value;
      ShouldDonClothing = worldConfig.GetBool("doffanddon-DonClothingFromEntities", true) && clientSettings.DonClothingFromEntities.Value;
      ShouldDonMisc = worldConfig.GetBool("doffanddon-DonMiscFromEntities", true) && clientSettings.DonMiscFromEntities.Value;
      IsDonEnabled = ShouldDonArmor || ShouldDonClothing || ShouldDonMisc;

      if (clientSettings.DonMiscOnlyToActiveHotbar.Value) {
        DonMiscBehavior = EnumDonMiscBehavior.ActiveSlotOnly;
      }
      else if (clientSettings.DonMiscOnlyToHotbar.Value) {
        DonMiscBehavior = EnumDonMiscBehavior.Hotbar;
      }
      else {
        DonMiscBehavior = EnumDonMiscBehavior.Anywhere;
      }

      ShouldSwapArmor = worldConfig.GetBool("doffanddon-SwapArmorWithEntities", true) && clientSettings.SwapArmorWithEntities.Value;
      ShouldSwapClothing = worldConfig.GetBool("doffanddon-SwapClothingWithEntities", true) && clientSettings.SwapClothingWithEntities.Value;
      IsSwapEnabled = ShouldSwapArmor || ShouldSwapClothing;
    }

    protected void OnDoffKeyPressed(DoffAndDonEventArgs eventArgs) {
      eventArgs.Successful = VerifyDoffEnabled(eventArgs)
                             && VerifyEnoughHandsFree(eventArgs);
      if (eventArgs.Successful) {
        SetClientDoffSlotIds(eventArgs);
        eventArgs.SaturationCost = SaturationCost;
      }
    }

    protected void OnDonKeyPressed(DoffAndDonEventArgs eventArgs) {
      eventArgs.Successful = VerifyDonEnabled(eventArgs)
                             && VerifyTargetEntityIsValid(eventArgs)
                             && VerifyEnoughHandsFree(eventArgs);
      if (eventArgs.Successful) {
        SetClientDonSlotIds(eventArgs);
        eventArgs.SaturationCost = SaturationCost;
      }
    }

    protected void OnSwapKeyPressed(DoffAndDonEventArgs eventArgs) {
      eventArgs.Successful = VerifySwapEnabled(eventArgs)
                             && VerifyTargetEntityIsValid(eventArgs)
                             && VerifyEnoughHandsFree(eventArgs);
      if (eventArgs.Successful) {
        SetClientSwapSlotIds(eventArgs);
        eventArgs.SaturationCost = SaturationCost;
      }
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
        eventArgs.ErrorCode = Constants.ERROR_DOFF_ENTITY_DISABLED;
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
      if (!HandsRequiredDictionary.TryGetValue(HandsNeeded, out var VerifyMethod)) {
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
        eventArgs.ErrorArgs = new string[] { Lang.GetMatching(ModId + ":" + eventArgs.ActionType.ToString()) };
        return false;
      }

      eventArgs.TargetEntityAgentId = TargetedEntityAgent.EntityId;
      if (!TargetedEntityAgent.CanBeTargetedFor(eventArgs.ActionType)) {
        eventArgs.ErrorCode = Constants.ERROR_INVALID_ENTITY_TARGET;
        eventArgs.ErrorArgs = new string[] { Lang.GetMatching(ModId + ":" + eventArgs.ActionType.ToString()), TargetedEntityAgent.GetName() };
        return false;
      }

      return true;
    }

    protected void SetClientDoffSlotIds(DoffAndDonEventArgs eventArgs) {
      if ((eventArgs.TargetType == EnumTargetType.Nothing && ShouldDoffArmorToGround)
          || (eventArgs.TargetType == EnumTargetType.EntityAgent && ShouldDoffArmorToEntities)) {
        eventArgs.ClientArmorSlotIds = PlayerEntity.GetArmorSlots().Where(slot => !slot.Empty).Select(slot => slot.Inventory.GetSlotId(slot)).ToArray();
      }
      if ((eventArgs.TargetType == EnumTargetType.Nothing && ShouldDoffClothingToGround)
          || (eventArgs.TargetType == EnumTargetType.EntityAgent && ShouldDoffClothingToEntities)) {
        eventArgs.ClientClothingSlotIds = PlayerEntity.GetClothingSlots().Where(slot => !slot.Empty).Select(slot => slot.Inventory.GetSlotId(slot)).ToArray();
      }
    }

    protected void SetClientDonSlotIds(DoffAndDonEventArgs eventArgs) {
      if (ShouldDonArmor) {
        eventArgs.ClientArmorSlotIds = PlayerEntity.GetArmorSlots().Where(slot => slot.Empty).Select(slot => slot.Inventory.GetSlotId(slot)).ToArray();
      }
      if (ShouldDonClothing) {
        eventArgs.ClientClothingSlotIds = PlayerEntity.GetClothingSlots().Where(slot => slot.Empty).Select(slot => slot.Inventory.GetSlotId(slot)).ToArray();
      }
      eventArgs.ClientDonMiscBehavior = DonMiscBehavior;
    }

    protected void SetClientSwapSlotIds(DoffAndDonEventArgs eventArgs) {
      if (ShouldSwapArmor) {
        eventArgs.ClientArmorSlotIds = PlayerEntity.GetArmorSlots().Select(slot => slot.Inventory.GetSlotId(slot)).ToArray();
      }
      if (ShouldSwapClothing) {
        eventArgs.ClientClothingSlotIds = PlayerEntity.GetClothingSlots().Select(slot => slot.Inventory.GetSlotId(slot)).ToArray();
      }
    }
  }
}
