using System.Collections.Generic;
using DoffAndDonAgain.Common;
using DoffAndDonAgain.Common.Network;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace DoffAndDonAgain.Utility {
  public static class GameExtensions {

    #region EntityArmorStand
    public static void UpdateRender(this EntityArmorStand armorStand) {
      if (armorStand.World.Side == EnumAppSide.Server) {
        armorStand.WatchedAttributes.MarkAllDirty();
        armorStand.World.RegisterCallback(armorStand.RenderUpdateCallback, armorStand.Pos.AsBlockPos, 500);
      }
      else if (armorStand.IsRendered) {
        armorStand.OnEntityLoaded();
      }
    }

    private static void RenderUpdateCallback(this EntityArmorStand armorStand, IWorldAccessor world, BlockPos pos, float dt) {
      // TODO: having to get my channel here probably means this should be inside the rerender handler instead?
      (world?.Api as ICoreServerAPI)?.Network.GetChannel(Constants.MOD_ID).BroadcastPacket(new ArmorStandInventoryUpdatedPacket(armorStand.EntityId));
    }

    #endregion

    #region EntityAgent

    // Nullsafe
    // Returns null if the armorstand cannot be located.
    public static EntityArmorStand GetEntityArmorStandById(this EntityAgent aroundEntity, long? armorStandEntityId, float horRange = 10, float vertRange = 10) {
      if (armorStandEntityId == null || aroundEntity == null) {
        return null;
      }
      else {
        ActionConsumable<Entity> matchesArmorStandId = (Entity entity) => { return entity?.EntityId == armorStandEntityId; };
        return aroundEntity.World.GetNearestEntity(aroundEntity.Pos.AsBlockPos.ToVec3d(), horRange, vertRange, matchesArmorStandId) as EntityArmorStand;
      }
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
