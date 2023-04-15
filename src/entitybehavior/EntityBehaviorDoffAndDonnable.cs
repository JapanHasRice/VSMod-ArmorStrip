using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace DoffAndDonAgain {
  public class EntityBehaviorDoffAndDonnable : EntityBehavior {
    protected int[] armorSlotIds;
    protected int[] clothingSlotIds;
    protected int[] miscSlotIds;

    public List<ItemSlot> ArmorSlots;
    public List<ItemSlot> ClothingSlots;
    public List<ItemSlot> MiscSlots;

    public EntityBehaviorDoffAndDonnable(Entity entity) : base(entity) { }

    public override string PropertyName() => "doffanddonnable";

    public override void Initialize(EntityProperties properties, JsonObject attributes) {
      base.Initialize(properties, attributes);

      armorSlotIds = attributes[nameof(armorSlotIds)].AsObject<int[]>(new int[0]);
      clothingSlotIds = attributes[nameof(clothingSlotIds)].AsObject<int[]>(new int[0]);
      miscSlotIds = attributes[nameof(miscSlotIds)].AsObject<int[]>(new int[0]);
      InitializeInventory();
    }

    private void InitializeInventory() {
      var entityInventory = (entity as EntityAgent)?.GearInventory;
      if (entityInventory == null) {
        return;
      }

      ArmorSlots = new List<ItemSlot>();
      for (int a = 0; a < armorSlotIds.Length; a++) {
        var slot = entityInventory[armorSlotIds[a]];
        if (slot == null) {
          continue;
        }
        ArmorSlots.Add(slot);
      }

      ClothingSlots = new List<ItemSlot>();
      for (int c = 0; c < clothingSlotIds.Length; c++) {
        var slot = entityInventory[clothingSlotIds[c]];
        if (slot == null) {
          continue;
        }
        ClothingSlots.Add(slot);
      }

      MiscSlots = new List<ItemSlot>();
      for (int m = 0; m < miscSlotIds.Length; m++) {
        var slot = entityInventory[miscSlotIds[m]];
        if (slot == null) {
          continue;
        }
        MiscSlots.Add(slot);
      }
    }

    public override void OnEntityLoaded() {
      base.OnEntityLoaded();
      InitializeInventory();
    }

    public bool CanBeTargetedFor(EnumActionType actionType) {
      return true;
    }
  }

  public static class DoffAndDonnableExtensions {
    public static List<ItemSlot> GetArmorSlots(this IServerPlayer player) {
      return player?.Entity?.GetArmorSlots();
    }

    public static List<ItemSlot> GetArmorSlots(this Entity entity) {
      return entity?.GetBehavior<EntityBehaviorDoffAndDonnable>()?.ArmorSlots;
    }

    public static List<ItemSlot> GetMiscSlots(this Entity entity) {
      return entity?.GetBehavior<EntityBehaviorDoffAndDonnable>()?.MiscSlots;
    }

    public static bool CanBeTargetedFor(this EntityAgent engityAgent, EnumActionType actionType) {
      var result = engityAgent?.GetBehavior<EntityBehaviorDoffAndDonnable>()?.CanBeTargetedFor(actionType) ?? false;
      return result;
    }
  }
}
