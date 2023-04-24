using System.Collections.Generic;
using DoffAndDonAgain.Common;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace DoffAndDonAgain.Server {
  public class TransferHandler {
    protected bool IsDoffToGroundEnabled { get; set; } = true;
    protected bool IsDoffToArmorStandEnabled { get; set; } = true;
    protected bool IsDropExcessWhenDoffingToStandEnabled { get; set; } = false;
    protected bool IsDonArmorEnabled { get; set; } = true;
    protected bool IsDonToolEnabled { get; set; } = true;
    protected bool ShouldDonToolOnlyToActiveHotbar { get; set; } = true;
    protected bool ShouldDonToolOnlyToHotbar { get; set; } = true;
    protected bool IsSwapEnabled { get; set; } = true;
    protected float SaturationRequired { get; set; } = 0f;

    public TransferHandler(DoffAndDonSystem doffAndDonSystem) {
      if (doffAndDonSystem.Side != EnumAppSide.Server) {
        throw new System.Exception($"Tried to create an instance of {nameof(TransferHandler)} Client-side or without a valid {nameof(ICoreAPI)} reference.");
      }
      LoadServerSettings(doffAndDonSystem.Api);

      doffAndDonSystem.OnServerReceivedDoffRequest += OnDoffRequest;
      doffAndDonSystem.OnServerReceivedDonRequest += OnDonRequest;
      doffAndDonSystem.OnServerReceivedSwapRequest += OnSwapRequest;

      doffAndDonSystem.OnAfterServerHandledRequest += OnAfterServerHandledRequest;
    }

    protected void LoadServerSettings(ICoreAPI api) {
      var worldConfig = api.World.Config;
      SaturationRequired = worldConfig.GetFloat("doffanddon-SaturationCost", 0f);
    }

    protected void OnDoffRequest(DoffAndDonEventArgs eventArgs) {
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

    protected void OnDonRequest(DoffAndDonEventArgs eventArgs) {
      eventArgs.ErrorCode = Constants.ERROR_UNDONNABLE; // Setting default error code
      TryDonFromEntity(eventArgs);
    }

    protected void OnSwapRequest(DoffAndDonEventArgs eventArgs) {
      eventArgs.ErrorCode = Constants.ERROR_COULD_NOT_SWAP;
      TryToSwapWithEntity(eventArgs);
    }

    protected void TryDoffToGround(DoffAndDonEventArgs eventArgs) {
      if (!IsDoffToGroundEnabled) {
        eventArgs.Successful = false;
        eventArgs.ErrorCode = Constants.ERROR_DOFF_GROUND_DISABLED;
        return;
      }

      DoffArmorToGround(eventArgs);
    }

    protected void TryDoffToEntity(DoffAndDonEventArgs eventArgs) {
      if (!IsDoffToArmorStandEnabled) {
        eventArgs.Successful = false;
        eventArgs.ErrorCode = Constants.ERROR_DOFF_ENTITY_DISABLED;
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

    protected void TryDonFromEntity(DoffAndDonEventArgs eventArgs) {
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

    protected void TryToSwapWithEntity(DoffAndDonEventArgs eventArgs) {
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

    protected void DoffArmorToGround(DoffAndDonEventArgs eventArgs) {
      foreach (var playerArmorSlot in eventArgs.ForPlayer.GetArmorSlots()) {
        DoffToGround(eventArgs, playerArmorSlot);
      }
    }

    protected void DoffToGround(DoffAndDonEventArgs eventArgs, ItemSlot slot) {
      var armor = slot.Itemstack?.Collectible as ItemWearable;
      bool armorDropped = eventArgs.ForPlayer.InventoryManager.DropItem(slot, true);
      eventArgs.Successful |= armorDropped;

      if (armorDropped) {
        eventArgs.DroppedArmor.Add(armor);
      }
    }

    protected void DoffToEntity(DoffAndDonEventArgs eventArgs, EntityAgent targetEntity) {
      Transfer(eventArgs, eventArgs.ForPlayer.GetArmorSlots(), targetEntity.GetArmorSlots(), dropExcessToGround: true);
      Transfer(eventArgs, eventArgs.ForPlayer.GetClothingSlots(), targetEntity.GetClothingSlots());
    }

    protected void DonFromEntity(DoffAndDonEventArgs eventArgs, EntityAgent targetEntity) {
      Transfer(eventArgs, targetEntity.GetArmorSlots(), eventArgs.ForPlayer.GetArmorSlots());
      Transfer(eventArgs, targetEntity.GetClothingSlots(), eventArgs.ForPlayer.GetClothingSlots());
      DonMiscFromEntity(eventArgs, targetEntity);
    }

    protected void Transfer(DoffAndDonEventArgs eventArgs, List<ItemSlot> fromSlots, List<ItemSlot> toSlots, bool dropExcessToGround = false) {
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

        if (dropExcessToGround && !itemMoved && IsDropExcessWhenDoffingToStandEnabled) {
          DoffToGround(eventArgs, sourceSlot);
        }
      }
    }

    protected void DonMiscFromEntity(DoffAndDonEventArgs eventArgs, EntityAgent targetEntity) {
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

    protected void Swap(DoffAndDonEventArgs eventArgs, List<ItemSlot> slots, List<ItemSlot> otherSlots) {
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

    protected void OnAfterServerHandledRequest(DoffAndDonEventArgs eventArgs) {
      if (eventArgs.Successful) {
        eventArgs.ForPlayer.Entity.GetBehavior<EntityBehaviorHunger>()?.ConsumeSaturation(SaturationRequired);
      }
    }
  }
}
