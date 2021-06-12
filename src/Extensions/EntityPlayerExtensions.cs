using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace DoffAndDonAgain {
  public static class EntityPlayerExtensions {
    public static List<ItemSlot> GetFilledArmorSlots(this EntityPlayer playerEntity) {
      List<ItemSlot> filledArmorSlots = new List<ItemSlot>();
      foreach (ItemSlot slot in playerEntity.GearInventory) {
        var wearable = slot.Itemstack?.Item as ItemWearable;
        if (wearable?.IsArmor ?? false) {
          filledArmorSlots.Add(slot);
        }
      }
      return filledArmorSlots;
    }
  }
}
