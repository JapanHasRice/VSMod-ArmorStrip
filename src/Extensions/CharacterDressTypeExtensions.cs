using System.Collections.Generic;
using Vintagestory.API.Common;

namespace ShakeItDoff {
  public static class EnumCharacterDressTypeExtensions {
    public static List<EnumCharacterDressType> ArmorDressTypes {
      get { return armorDressTypes; }
    }

    private static List<EnumCharacterDressType> armorDressTypes = new List<EnumCharacterDressType>(
      new EnumCharacterDressType[] { EnumCharacterDressType.ArmorHead, EnumCharacterDressType.ArmorBody, EnumCharacterDressType.ArmorLegs }
    );

    public static bool IsArmor(this EnumCharacterDressType dressType) {
      switch (dressType) {
        case EnumCharacterDressType.ArmorHead:
        case EnumCharacterDressType.ArmorBody:
        case EnumCharacterDressType.ArmorLegs:
          return true;
        default:
          return false;
      }
    }
  }
}
