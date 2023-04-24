using System.Collections.Generic;
using DoffAndDonAgain.Common;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace DoffAndDonAgain {
  public class EntityBehaviorDoffAndDonnable : EntityBehavior {
    protected int[] armorSlotIds;
    protected int[] clothingSlotIds;
    protected int[] miscDonFromSlotIds;

    public List<ItemSlot> ArmorSlots = new List<ItemSlot>(3);
    public List<ItemSlot> ClothingSlots = new List<ItemSlot>(12);
    public List<ItemSlot> MiscDonFromSlots = new List<ItemSlot>(5);

    public EntityBehaviorDoffAndDonnable(Entity entity) : base(entity) { }

    public override string PropertyName() => "doffanddonnable";

    public override void Initialize(EntityProperties properties, JsonObject attributes) {
      base.Initialize(properties, attributes);

      try {
        armorSlotIds = attributes[nameof(armorSlotIds)].AsArray<int>(new int[0]);
        clothingSlotIds = attributes[nameof(clothingSlotIds)].AsArray<int>(new int[0]);
        miscDonFromSlotIds = attributes[nameof(miscDonFromSlotIds)].AsArray<int>(new int[0]);
      }
      catch (System.Exception e) {
        entity.World.Logger.Error("DoffAndDonAgain: Error parsing {0} behavior for {1}. Doff/Don/Swap may not work correctly with these entities. {2}", PropertyName(), entity.Code, e);
      }
      InitializeInventory();
    }

    private void InitializeInventory() {
      var entityInventory = (entity as EntityAgent)?.GearInventory;
      if (entityInventory == null) {
        return;
      }

      ArmorSlots = new List<ItemSlot>();
      for (int a = 0; a < armorSlotIds?.Length; a++) {
        int inventoryIndex = armorSlotIds[a];
        if (inventoryIndex < 0 || inventoryIndex >= entityInventory.Count) {
          continue;
        }
        var slot = entityInventory[inventoryIndex];
        if (slot == null) {
          continue;
        }
        ArmorSlots.Add(slot);
      }

      ClothingSlots = new List<ItemSlot>();
      for (int c = 0; c < clothingSlotIds?.Length; c++) {
        int inventoryIndex = clothingSlotIds[c];
        if (inventoryIndex < 0 || inventoryIndex >= entityInventory.Count) {
          continue;
        }
        var slot = entityInventory[inventoryIndex];
        if (slot == null) {
          continue;
        }
        ClothingSlots.Add(slot);
      }

      MiscDonFromSlots = new List<ItemSlot>();
      for (int m = 0; m < miscDonFromSlotIds?.Length; m++) {
        int inventoryIndex = miscDonFromSlotIds[m];
        if (inventoryIndex < 0 || inventoryIndex >= entityInventory.Count) {
          continue;
        }
        var slot = entityInventory[inventoryIndex];
        if (slot == null) {
          continue;
        }
        MiscDonFromSlots.Add(slot);
      }
    }

    public override void OnEntityLoaded() {
      base.OnEntityLoaded();
      InitializeInventory();
    }

    public bool CanBeTargetedFor(EnumActionType actionType) {
      if (entity is EntityPlayer) {
        return false;
      }

      int wearableCount = ArmorSlots.Count + ClothingSlots.Count;
      switch (actionType) {
        case EnumActionType.Don:
          return wearableCount + MiscDonFromSlots.Count > 0;
        default:
          return wearableCount > 0;
      }
    }
  }

  public static class DoffAndDonnableExtensions {
    public static bool CanBeTargetedFor(this EntityAgent engityAgent, EnumActionType actionType) {
      var result = engityAgent?.GetBehavior<EntityBehaviorDoffAndDonnable>()?.CanBeTargetedFor(actionType) ?? false;
      return result;
    }

    public static List<ItemSlot> GetArmorSlots(this IServerPlayer player) {
      return player?.Entity?.GetArmorSlots();
    }

    public static List<ItemSlot> GetClothingSlots(this IServerPlayer player) {
      return player?.Entity?.GetClothingSlots();
    }

    public static List<ItemSlot> GetMiscDonFromSlots(this IServerPlayer player) {
      return player?.Entity?.GetMiscDonFromSlots();
    }

    public static List<ItemSlot> GetArmorSlots(this Entity entity) {
      return entity?.GetBehavior<EntityBehaviorDoffAndDonnable>()?.ArmorSlots;
    }

    public static List<ItemSlot> GetClothingSlots(this Entity entity) {
      return entity?.GetBehavior<EntityBehaviorDoffAndDonnable>()?.ClothingSlots;
    }

    public static List<ItemSlot> GetMiscDonFromSlots(this Entity entity) {
      return entity?.GetBehavior<EntityBehaviorDoffAndDonnable>()?.MiscDonFromSlots;
    }
  }
}
