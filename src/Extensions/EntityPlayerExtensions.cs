using System.Collections.Generic;
using Vintagestory.API.Common;

namespace ShakeItDoff {
  public static class EntityPlayerExtensions {
    public static Dictionary<string, ItemSlot> GetArmorSlots(this EntityPlayer playerEntity) {
      Dictionary<string, ItemSlot> armorSlots = new Dictionary<string, ItemSlot>();
      foreach (EnumCharacterDressType type in EnumCharacterDressTypeExtensions.ArmorDressTypes) {
        armorSlots[type.ToString().ToLowerInvariant()] = null;
      }
      foreach (ItemSlotCharacter slot in playerEntity.GearInventory) {
        string dressType = (slot.Itemstack?.Collectible?.Attributes["clothescategory"]?.AsString() ?? "").ToLowerInvariant();
        if (armorSlots.ContainsKey(dressType)) {
          armorSlots[dressType] = slot;
        }
      }
      return armorSlots;
    }
  }
}
