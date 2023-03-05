using System.Collections.Generic;
using DoffAndDonAgain.Common;
using DoffAndDonAgain.Utility;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace DoffAndDonAgain.Server {
  public class ArmorTransferHandler {
    protected bool IsDoffToGroundEnabled { get; set; } = true;
    protected bool IsDoffToArmorStandEnabled { get; set; } = true;
    protected bool IsDropExcessWhenDoffingToStandEnabled { get; set; } = false;
    protected bool IsDonArmorEnabled { get; set; } = true;
    protected bool IsDonToolEnabled { get; set; } = true;
    protected bool ShouldDonToolOnlyToActiveHotbar { get; set; } = true;
    protected bool ShouldDonToolOnlyToHotbar { get; set; } = true;
    protected bool IsSwapEnabled { get; set; } = true;
    protected Dictionary<EnumActionType, float> SaturationRequired { get; set; } = new Dictionary<EnumActionType, float> {
      { EnumActionType.Doff, 0f },
      { EnumActionType.Don, 0f },
      { EnumActionType.Swap, 0f }
    };

    public ArmorTransferHandler(DoffAndDonSystem system) {
      if (system.Side != EnumAppSide.Server) {
        system.Api.Logger.Warning("{0} is a server object instantiated on the client, ignoring.", nameof(ArmorTransferHandler));
        return;
      }
      LoadServerSettings(system.Api);

      system.Event.OnServerReceivedDoffRequest += OnDoffRequest;
      system.Event.OnServerReceivedDonRequest += OnDonRequest;
      system.Event.OnServerReceivedSwapRequest += OnSwapRequest;

      system.Event.OnAfterServerHandledRequest += OnAfterServerHandledRequest;
    }

    protected void LoadServerSettings(ICoreAPI api) {
      var configSystem = api.ModLoader.GetModSystem<DoffAndDonConfigurationSystem>();
      if (configSystem == null) {
        api.Logger.Error("[{0}] {1} was not loaded. Using defaults.", nameof(ArmorTransferHandler), nameof(DoffAndDonConfigurationSystem));
        LoadServerSettings(new DoffAndDonServerConfig());
        return;
      }

      LoadServerSettings(configSystem.ServerSettings);
    }

    protected void LoadServerSettings(DoffAndDonServerConfig serverSettings) {
      IsDoffToGroundEnabled = serverSettings.EnableDoffToGround.Value;
      IsDoffToArmorStandEnabled = serverSettings.EnableDoffToArmorStand.Value;
      IsDropExcessWhenDoffingToStandEnabled = serverSettings.DropArmorWhenDoffingToStand.Value;

      IsDonArmorEnabled = serverSettings.EnableDon.Value;

      IsDonToolEnabled = serverSettings.EnableToolDonning.Value;
      ShouldDonToolOnlyToActiveHotbar = serverSettings.DonToolOnlyToActiveHotbar.Value;
      ShouldDonToolOnlyToHotbar = serverSettings.DonToolOnlyToHotbar.Value;

      IsSwapEnabled = serverSettings.EnableSwap.Value;

      SaturationRequired[EnumActionType.Doff] = serverSettings.SaturationCostPerDoff.Value;
      SaturationRequired[EnumActionType.Don] = serverSettings.SaturationCostPerDon.Value;
      SaturationRequired[EnumActionType.Swap] = serverSettings.SaturationCostPerSwap.Value;
    }

    private void OnDoffRequest(ref ArmorActionEventArgs eventArgs) {
      eventArgs.ErrorCode = Constants.ERROR_UNDOFFABLE; // Setting default error code
      switch (eventArgs.TargetType) {
        case EnumTargetType.Nothing:
          TryDoffToGround(ref eventArgs);
          break;
        case EnumTargetType.ArmorStand:
          TryDoffToArmorStand(ref eventArgs);
          break;
      }
    }

    private void TryDoffToGround(ref ArmorActionEventArgs eventArgs) {
      if (!IsDoffToGroundEnabled) {
        eventArgs.Successful = false;
        eventArgs.ErrorCode = Constants.ERROR_DOFF_GROUND_DISABLED;
        return;
      }

      DoffToGround(ref eventArgs);
    }

    private void DoffToGround(ref ArmorActionEventArgs eventArgs) {
      foreach (var playerArmorSlot in eventArgs.ForPlayer.Entity.GetArmorSlots()) {
        DoffToGround(ref eventArgs, playerArmorSlot);
      }
    }

    private void DoffToGround(ref ArmorActionEventArgs eventArgs, ItemSlot playerArmorSlot) {
      var armor = playerArmorSlot.Itemstack?.Collectible as ItemWearable;
      bool armorDropped = eventArgs.ForPlayer.InventoryManager.DropItem(playerArmorSlot, true);
      eventArgs.Successful |= armorDropped;

      if (armorDropped) {
        eventArgs.DroppedArmor.Add(armor);
      }
    }

    private void TryDoffToArmorStand(ref ArmorActionEventArgs eventArgs) {
      if (!IsDoffToArmorStandEnabled) {
        eventArgs.Successful = false;
        eventArgs.ErrorCode = Constants.ERROR_DOFF_STAND_DISABLED;
        return;
      }

      if (eventArgs.ArmorStandEntityId == null) {
        eventArgs.Successful = false;
        eventArgs.ErrorCode = Constants.ERROR_MISSING_ARMOR_STAND_TARGET;
        return;
      }

      EntityArmorStand armorStand = eventArgs.ForPlayer?.Entity.GetEntityArmorStandById(eventArgs.ArmorStandEntityId);
      if (armorStand == null) {
        eventArgs.Successful = false;
        eventArgs.ErrorCode = Constants.ERROR_TARGET_LOST;
        return;
      }

      DoffToArmorStand(ref eventArgs, armorStand);
    }

    private void DoffToArmorStand(ref ArmorActionEventArgs eventArgs, EntityArmorStand armorStand) {
      TransferArmor(ref eventArgs, eventArgs.ForPlayer.Entity, armorStand);
    }

    private void OnDonRequest(ref ArmorActionEventArgs eventArgs) {
      eventArgs.ErrorCode = Constants.ERROR_UNDONNABLE; // Setting default error code
      TryDonFromArmorStand(ref eventArgs);
    }

    private void TryDonFromArmorStand(ref ArmorActionEventArgs eventArgs) {
      if (!IsDonArmorEnabled) {
        eventArgs.Successful = false;
        eventArgs.ErrorCode = Constants.ERROR_DON_DISABLED;
        return;
      }

      if (eventArgs.ArmorStandEntityId == null) {
        eventArgs.Successful = false;
        eventArgs.ErrorCode = Constants.ERROR_MISSING_ARMOR_STAND_TARGET;
        return;
      }

      EntityArmorStand armorStand = eventArgs.ForPlayer?.Entity.GetEntityArmorStandById(eventArgs.ArmorStandEntityId);
      if (armorStand == null) {
        eventArgs.Successful = false;
        eventArgs.ErrorCode = Constants.ERROR_TARGET_LOST;
      }

      DonFromArmorStand(ref eventArgs, armorStand);
    }

    private void DonFromArmorStand(ref ArmorActionEventArgs eventArgs, EntityArmorStand armorStand) {
      TransferArmor(ref eventArgs, armorStand, eventArgs.ForPlayer.Entity);
      DonTool(ref eventArgs, armorStand);
    }

    private void DonTool(ref ArmorActionEventArgs eventArgs, EntityArmorStand armorStand) {
      if (!IsDonToolEnabled) { return; }

      ItemSlot sinkSlot;
      if (ShouldDonToolOnlyToActiveHotbar) {
        sinkSlot = eventArgs.ForPlayer.InventoryManager.ActiveHotbarSlot;
      }
      else if (ShouldDonToolOnlyToHotbar) {
        sinkSlot = eventArgs.ForPlayer.InventoryManager.GetBestSuitedHotbarSlot(null, armorStand.RightHandItemSlot);
      }
      else {
        // skipSlots is an empty list instead of null due to a crash when in creative mode
        sinkSlot = eventArgs.ForPlayer.InventoryManager.GetBestSuitedSlot(armorStand.RightHandItemSlot, onlyPlayerInventory: true, skipSlots: new List<ItemSlot>());
      }

      if (sinkSlot != null) {
        eventArgs.Successful |= armorStand.RightHandItemSlot.TryPutInto(eventArgs.ForPlayer.Entity.World, sinkSlot) > 0;
      }
    }

    private void TransferArmor(ref ArmorActionEventArgs eventArgs, EntityAgent doffingEntity, EntityAgent donningEntity) {
      var doffingArmorSlots = doffingEntity.GetArmorSlots();
      var donningArmorSlots = donningEntity.GetArmorSlots();

      for (var i = 0; i < doffingArmorSlots.Count; i++) {
        TransferArmor(ref eventArgs, doffingArmorSlots[i], donningArmorSlots[i]);
      }
    }

    private void TransferArmor(ref ArmorActionEventArgs eventArgs, ItemSlot sourceSlot, ItemSlot sinkSlot) {
      var armorMoved = sourceSlot.TryPutInto(eventArgs.ForPlayer.Entity.World, sinkSlot) > 0;
      eventArgs.Successful |= armorMoved;

      if (armorMoved) {
        eventArgs.MovedArmor.Add(sinkSlot.Itemstack.Collectible as ItemWearable);
      }
      else if (IsDropExcessWhenDoffingToStandEnabled && eventArgs.DoffExcessToGround) {
        DoffToGround(ref eventArgs, sourceSlot);
      }
    }

    private void OnSwapRequest(ref ArmorActionEventArgs eventArgs) {
      eventArgs.ErrorCode = Constants.ERROR_COULD_NOT_SWAP;
      TryToSwapArmorWithStand(ref eventArgs);
    }

    private void TryToSwapArmorWithStand(ref ArmorActionEventArgs eventArgs) {
      if (!IsSwapEnabled) {
        eventArgs.Successful = false;
        eventArgs.ErrorCode = Constants.ERROR_SWAP_DISABLED;
        return;
      }

      if (eventArgs.ArmorStandEntityId == null) {
        eventArgs.Successful = false;
        eventArgs.ErrorCode = Constants.ERROR_MISSING_ARMOR_STAND_TARGET;
        return;
      }

      EntityArmorStand armorStand = eventArgs.ForPlayer.Entity.GetEntityArmorStandById(eventArgs.ArmorStandEntityId);
      if (armorStand == null) {
        eventArgs.Successful = false;
        eventArgs.ErrorCode = Constants.ERROR_TARGET_LOST;
        return;
      }

      SwapArmorWithStand(ref eventArgs, armorStand);
    }

    private void SwapArmorWithStand(ref ArmorActionEventArgs eventArgs, EntityArmorStand armorStand) {
      var playerArmor = eventArgs.ForPlayer.Entity.GetArmorSlots();
      var standArmor = armorStand.GetArmorSlots();

      for (int i = 0; i < playerArmor.Count; i++) {
        if (playerArmor[i].Empty && standArmor[i].Empty) {
          // TryFlipWith would return true, even though nothing is exchanged.
          continue;
        }

        bool swapped = playerArmor[i].TryFlipWith(standArmor[i]);
        eventArgs.Successful |= swapped;
        if (swapped) {
          if (playerArmor[i].Itemstack?.Collectible is ItemWearable wearable) {
            eventArgs.MovedArmor.Add(wearable);
          }
          if (standArmor[i].Itemstack?.Collectible is ItemWearable otherWearable) {
            eventArgs.MovedArmor.Add(otherWearable);
          }
        }
      }
    }

    private void OnAfterServerHandledRequest(ArmorActionEventArgs eventArgs) {
      if (eventArgs.Successful) {
        eventArgs.ForPlayer.Entity.ConsumeSaturation(SaturationRequired[eventArgs.ActionType]);
      }
    }
  }
}
