using System.Collections.Generic;
using DoffAndDonAgain.Common;
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

    protected void OnDoffRequest(ArmorActionEventArgs eventArgs) {
      eventArgs.ErrorCode = Constants.ERROR_UNDOFFABLE; // Setting default error code
      switch (eventArgs.TargetType) {
        case EnumTargetType.Nothing:
          TryDoffToGround(eventArgs);
          break;
        case EnumTargetType.EntityAgent:
          TryDoffToEntity(eventArgs);
          break;
      }
    }

    protected void OnDonRequest(ArmorActionEventArgs eventArgs) {
      eventArgs.ErrorCode = Constants.ERROR_UNDONNABLE; // Setting default error code
      TryDonFromEntity(eventArgs);
    }

    protected void OnSwapRequest(ArmorActionEventArgs eventArgs) {
      eventArgs.ErrorCode = Constants.ERROR_COULD_NOT_SWAP;
      TryToSwapWithEntity(eventArgs);
    }

    protected void TryDoffToGround(ArmorActionEventArgs eventArgs) {
      if (!IsDoffToGroundEnabled) {
        eventArgs.Successful = false;
        eventArgs.ErrorCode = Constants.ERROR_DOFF_GROUND_DISABLED;
        return;
      }

      DoffArmorToGround(eventArgs);
    }

    protected void TryDoffToEntity(ArmorActionEventArgs eventArgs) {
      if (!IsDoffToArmorStandEnabled) {
        eventArgs.Successful = false;
        eventArgs.ErrorCode = Constants.ERROR_DOFF_STAND_DISABLED;
        return;
      }

      if (eventArgs.TargetEntityAgentId == null) {
        eventArgs.Successful = false;
        eventArgs.ErrorCode = Constants.ERROR_MUST_TARGET_ENTITY;
        return;
      }

      EntityAgent targetEntity = eventArgs.ForPlayer?.Entity.World.GetEntityById((long)eventArgs.TargetEntityAgentId) as EntityAgent;
      if (targetEntity == null) {
        eventArgs.Successful = false;
        eventArgs.ErrorCode = Constants.ERROR_TARGET_LOST;
        return;
      }

      DoffToEntity(eventArgs, targetEntity);
    }

    protected void TryDonFromEntity(ArmorActionEventArgs eventArgs) {
      if (!IsDonArmorEnabled) {
        eventArgs.Successful = false;
        eventArgs.ErrorCode = Constants.ERROR_DON_DISABLED;
        return;
      }

      if (eventArgs.TargetEntityAgentId == null) {
        eventArgs.Successful = false;
        eventArgs.ErrorCode = Constants.ERROR_MUST_TARGET_ENTITY;
        eventArgs.ErrorArgs = new string[] { eventArgs.ActionType.ToString() };
        return;
      }

      EntityAgent targetEntity = eventArgs.ForPlayer?.Entity.World.GetEntityById((long)eventArgs.TargetEntityAgentId) as EntityAgent;
      if (targetEntity == null) {
        eventArgs.Successful = false;
        eventArgs.ErrorCode = Constants.ERROR_TARGET_LOST;
      }

      DonFromEntity(eventArgs, targetEntity);
    }

    protected void TryToSwapWithEntity(ArmorActionEventArgs eventArgs) {
      if (!IsSwapEnabled) {
        eventArgs.Successful = false;
        eventArgs.ErrorCode = Constants.ERROR_SWAP_DISABLED;
        return;
      }

      if (eventArgs.TargetEntityAgentId == null) {
        eventArgs.Successful = false;
        eventArgs.ErrorCode = Constants.ERROR_MUST_TARGET_ENTITY;
        eventArgs.ErrorArgs = new string[] { eventArgs.ActionType.ToString() };
        return;
      }

      EntityAgent targetEntity = eventArgs.ForPlayer?.Entity.World.GetEntityById((long)eventArgs.TargetEntityAgentId) as EntityAgent;
      if (targetEntity == null) {
        eventArgs.Successful = false;
        eventArgs.ErrorCode = Constants.ERROR_TARGET_LOST;
        return;
      }

      Swap(eventArgs, eventArgs.ForPlayer.GetArmorSlots(), targetEntity.GetArmorSlots());
      Swap(eventArgs, eventArgs.ForPlayer.GetClothingSlots(), targetEntity.GetClothingSlots());
    }

    protected void DoffArmorToGround(ArmorActionEventArgs eventArgs) {
      foreach (var playerArmorSlot in eventArgs.ForPlayer.GetArmorSlots()) {
        DoffToGround(eventArgs, playerArmorSlot);
      }
    }

    protected void DoffToGround(ArmorActionEventArgs eventArgs, ItemSlot slot) {
      var armor = slot.Itemstack?.Collectible as ItemWearable;
      bool armorDropped = eventArgs.ForPlayer.InventoryManager.DropItem(slot, true);
      eventArgs.Successful |= armorDropped;

      if (armorDropped) {
        eventArgs.DroppedArmor.Add(armor);
      }
    }

    protected void DoffToEntity(ArmorActionEventArgs eventArgs, EntityAgent targetEntity) {
      Transfer(eventArgs, eventArgs.ForPlayer.GetArmorSlots(), targetEntity.GetArmorSlots(), dropExcessToGround: true);
      Transfer(eventArgs, eventArgs.ForPlayer.GetClothingSlots(), targetEntity.GetClothingSlots());
    }

    protected void DonFromEntity(ArmorActionEventArgs eventArgs, EntityAgent targetEntity) {
      Transfer(eventArgs, targetEntity.GetArmorSlots(), eventArgs.ForPlayer.GetArmorSlots());
      Transfer(eventArgs, targetEntity.GetClothingSlots(), eventArgs.ForPlayer.GetClothingSlots());
      DonMiscFromEntity(eventArgs, targetEntity);
    }

    protected void Transfer(ArmorActionEventArgs eventArgs, List<ItemSlot> fromSlots, List<ItemSlot> toSlots, bool dropExcessToGround = false) {
      foreach (var sourceSlot in fromSlots) {
        if (sourceSlot.Empty) {
          continue;
        }

        bool itemMoved = false;
        foreach (var sinkSlot in toSlots) {
          itemMoved = sourceSlot.TryPutInto(eventArgs.ForPlayer.Entity.World, sinkSlot) > 0;
          eventArgs.Successful |= itemMoved;

          if (itemMoved) {
            if (sinkSlot.Itemstack.Collectible is ItemWearable wearable) {
              eventArgs.MovedArmor.Add(wearable);
            }
            break;
          }
        }

        if (dropExcessToGround && !itemMoved && IsDropExcessWhenDoffingToStandEnabled && eventArgs.DoffExcessToGround) {
          DoffToGround(eventArgs, sourceSlot);
        }
      }
    }

    protected void DonMiscFromEntity(ArmorActionEventArgs eventArgs, EntityAgent targetEntity) {
      if (!IsDonToolEnabled) { return; }

      foreach (var sourceSlot in targetEntity.GetMiscDonFromSlots()) {
        ItemSlot sinkSlot;
        if (ShouldDonToolOnlyToActiveHotbar) {
          sinkSlot = eventArgs.ForPlayer.InventoryManager.ActiveHotbarSlot;
        }
        else if (ShouldDonToolOnlyToHotbar) {
          sinkSlot = eventArgs.ForPlayer.InventoryManager.GetBestSuitedHotbarSlot(null, sourceSlot);
        }
        else {
          // skipSlots is an empty list instead of null due to a crash when in creative mode
          sinkSlot = eventArgs.ForPlayer.InventoryManager.GetBestSuitedSlot(sourceSlot, onlyPlayerInventory: true, skipSlots: new List<ItemSlot>());
        }

        if (sinkSlot != null) {
          eventArgs.Successful |= sourceSlot.TryPutInto(eventArgs.ForPlayer.Entity.World, sinkSlot) > 0;
        }
      }
    }

    protected void Swap(ArmorActionEventArgs eventArgs, List<ItemSlot> slots, List<ItemSlot> otherSlots) {
      foreach (var slot in slots) {
        bool swapped = false;
        foreach (var otherSlot in otherSlots) {
          if (slot.Empty && otherSlot.Empty) {
            // TryFlipWith would return true, even though nothing is exchanged.
            continue;
          }
          swapped = slot.TryFlipWith(otherSlot);
          eventArgs.Successful |= swapped;
          if (swapped) {
            if (slot.Itemstack?.Collectible is ItemWearable wearable) {
              eventArgs.MovedArmor.Add(wearable);
            }
            if (otherSlot.Itemstack?.Collectible is ItemWearable otherWearable) {
              eventArgs.MovedArmor.Add(otherWearable);
            }
            break;
          }
        }
      }
    }

    protected void OnAfterServerHandledRequest(ArmorActionEventArgs eventArgs) {
      if (eventArgs.Successful) {
        eventArgs.ForPlayer.Entity.GetBehavior<EntityBehaviorHunger>()?.ConsumeSaturation(SaturationRequired[eventArgs.ActionType]);
      }
    }
  }
}
