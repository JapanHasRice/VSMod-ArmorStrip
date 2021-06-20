using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace DoffAndDonAgain {
  public static class EntityAgentExtensions {
    // TODO: make this better. Might require API additions though
    private const int BEGIN_ARMOR_INDEX = 12;
    private const int END_ARMOR_INDEX = 14;
    public static List<ItemSlot> GetFilledArmorSlots(this EntityAgent entityAgent) {
      List<ItemSlot> filledArmorSlots = new List<ItemSlot>();
      foreach (ItemSlot slot in entityAgent.GearInventory) {
        var wearable = slot.Itemstack?.Item as ItemWearable;
        if (wearable?.IsArmor ?? false) {
          filledArmorSlots.Add(slot);
        }
      }
      return filledArmorSlots;
    }

    public static List<ItemSlot> GetArmorSlots(this EntityAgent entityAgent) {
      List<ItemSlot> armorSlots = new List<ItemSlot>();
      if (entityAgent == null) { return null; }
      for (int i = BEGIN_ARMOR_INDEX; i <= END_ARMOR_INDEX; i++) {
        armorSlots.Add(entityAgent.GearInventory[i]);
      }
      return armorSlots;
    }
  }
}
