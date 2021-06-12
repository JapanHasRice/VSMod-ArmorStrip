using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace ShakeItDoff {
  public static class EntityPlayerExtensions {
    public static ItemSlot[] GetFilledArmorSlots(this EntityPlayer playerEntity) {
      ItemSlotCharacter[] filledArmorSlots = new ItemSlotCharacter[0];
      foreach (ItemSlotCharacter slot in playerEntity.GearInventory) {
        var wearable = slot.Itemstack?.Item as ItemWearable;
        if (wearable?.IsArmor ?? false) {
          filledArmorSlots.Append(slot);
        }
      }
      return filledArmorSlots;
    }
  }
}
