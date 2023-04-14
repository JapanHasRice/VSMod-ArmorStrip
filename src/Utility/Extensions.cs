using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

namespace DoffAndDonAgain.Utility {
  public static class GameExtensions {

    #region EntityAgent

    // Nullsafe
    // Returns null if the armorstand cannot be located.
    public static EntityArmorStand GetEntityArmorStandById(this EntityAgent aroundEntity, long? armorStandEntityId, float horRange = 10, float vertRange = 10) {
      if (armorStandEntityId == null || aroundEntity == null) {
        return null;
      }

      ActionConsumable<Entity> matchesArmorStandId = (Entity entity) => { return entity?.EntityId == armorStandEntityId; };
      return aroundEntity.World.GetNearestEntity(aroundEntity.Pos.AsBlockPos.ToVec3d(), horRange, vertRange, matchesArmorStandId) as EntityArmorStand;
    }

    public static void ConsumeSaturation(this EntityAgent player, float amount) {
      player.GetBehavior<EntityBehaviorHunger>()?.ConsumeSaturation(amount);
    }

    // TODO: make this better. Might require API additions though
    private const int BEGIN_ARMOR_INDEX = 12;
    private const int END_ARMOR_INDEX = 14;

    public static List<ItemSlot> GetArmorSlots(this EntityAgent entityAgent) {
      List<ItemSlot> armorSlots = new List<ItemSlot>();
      if (entityAgent == null) { return null; }
      for (int i = BEGIN_ARMOR_INDEX; i <= END_ARMOR_INDEX; i++) {
        armorSlots.Add(entityAgent.GearInventory[i]);
      }
      return armorSlots;
    }

    #endregion
  }
}
