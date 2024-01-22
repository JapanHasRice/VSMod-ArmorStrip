using Vintagestory.API.Common;

namespace DoffAndDonAgain.Common {
  public static class WorldConfig {
    // GENERAL
    public const string WorldConfigCategoryGeneral = Constants.MOD_ID + "General";
    public static readonly WorldConfigurationAttribute AllowArmorStandArmor = new WorldConfigurationAttribute {
      DataType = EnumDataType.Bool,
      Category = WorldConfigCategoryGeneral,
      Code = Constants.MOD_ID + nameof(AllowArmorStandArmor),
      Default = true.ToString()
    };

    public static readonly WorldConfigurationAttribute AllowArmorStandHands = new WorldConfigurationAttribute {
      DataType = EnumDataType.Bool,
      Category = WorldConfigCategoryGeneral,
      Code = Constants.MOD_ID + nameof(AllowArmorStandHands),
      Default = true.ToString()
    };

    public static readonly WorldConfigurationAttribute SaturationCost = new WorldConfigurationAttribute {
      DataType = EnumDataType.IntInput,
      Category = WorldConfigCategoryGeneral,
      Code = Constants.MOD_ID + nameof(SaturationCost),
      Default = 0.ToString(),
      OnCustomizeScreen = false
    };

    public static readonly WorldConfigurationAttribute HandsNeeded = new WorldConfigurationAttribute {
      DataType = EnumDataType.IntRange,
      Category = WorldConfigCategoryGeneral,
      Code = Constants.MOD_ID + nameof(HandsNeeded),
      Default = 2.ToString(),
      Min = 0,
      Max = 2,
      OnCustomizeScreen = false
    };

    // MANNEQUINS
    public const string WorldConfigCategoryMannequins = Constants.MOD_ID + "Mannequins";
    public static readonly WorldConfigurationAttribute AllowMannequinArmor = new WorldConfigurationAttribute {
      DataType = EnumDataType.Bool,
      Category = WorldConfigCategoryMannequins,
      Code = Constants.MOD_ID + nameof(AllowMannequinArmor),
      Default = false.ToString()
    };

    public static readonly WorldConfigurationAttribute AllowMannequinClothing = new WorldConfigurationAttribute {
      DataType = EnumDataType.Bool,
      Category = WorldConfigCategoryMannequins,
      Code = Constants.MOD_ID + nameof(AllowMannequinClothing),
      Default = true.ToString()
    };

    public static readonly WorldConfigurationAttribute AllowMannequinHands = new WorldConfigurationAttribute {
      DataType = EnumDataType.Bool,
      Category = WorldConfigCategoryMannequins,
      Code = Constants.MOD_ID + nameof(AllowMannequinHands),
      Default = false.ToString()
    };

    public static readonly WorldConfigurationAttribute AllowMannequinBackpack = new WorldConfigurationAttribute {
      DataType = EnumDataType.Bool,
      Category = WorldConfigCategoryMannequins,
      Code = Constants.MOD_ID + nameof(AllowMannequinBackpack),
      Default = false.ToString()
    };

    // DOFF
    public const string WorldConfigCategoryDoff = Constants.MOD_ID + "Doff";
    public static readonly WorldConfigurationAttribute DoffArmorToEntities = new WorldConfigurationAttribute {
      DataType = EnumDataType.Bool,
      Category = WorldConfigCategoryDoff,
      Code = Constants.MOD_ID + nameof(DoffArmorToEntities),
      Default = true.ToString()
    };

    public static readonly WorldConfigurationAttribute DoffArmorToGround = new WorldConfigurationAttribute {
      DataType = EnumDataType.Bool,
      Category = WorldConfigCategoryDoff,
      Code = Constants.MOD_ID + nameof(DoffArmorToGround),
      Default = true.ToString()
    };

    public static readonly WorldConfigurationAttribute DropUnplaceableArmor = new WorldConfigurationAttribute {
      DataType = EnumDataType.Bool,
      Category = WorldConfigCategoryDoff,
      Code = Constants.MOD_ID + nameof(DropUnplaceableArmor),
      Default = false.ToString()
    };

    public static readonly WorldConfigurationAttribute DoffClothingToEntities = new WorldConfigurationAttribute {
      DataType = EnumDataType.Bool,
      Category = WorldConfigCategoryDoff,
      Code = Constants.MOD_ID + nameof(DoffClothingToEntities),
      Default = true.ToString()
    };

    public static readonly WorldConfigurationAttribute DoffClothingToGround = new WorldConfigurationAttribute {
      DataType = EnumDataType.Bool,
      Category = WorldConfigCategoryDoff,
      Code = Constants.MOD_ID + nameof(DoffClothingToGround),
      Default = false.ToString()
    };

    public static readonly WorldConfigurationAttribute DropUnplaceableClothing = new WorldConfigurationAttribute {
      DataType = EnumDataType.Bool,
      Category = WorldConfigCategoryDoff,
      Code = Constants.MOD_ID + nameof(DropUnplaceableClothing),
      Default = false.ToString()
    };

    // DON
    public const string WorldConfigCategoryDon = Constants.MOD_ID + "Don";
    public static readonly WorldConfigurationAttribute DonArmorFromEntities = new WorldConfigurationAttribute {
      DataType = EnumDataType.Bool,
      Category = WorldConfigCategoryDon,
      Code = Constants.MOD_ID + nameof(DonArmorFromEntities),
      Default = true.ToString()
    };

    public static readonly WorldConfigurationAttribute DonClothingFromEntities = new WorldConfigurationAttribute {
      DataType = EnumDataType.Bool,
      Category = WorldConfigCategoryDon,
      Code = Constants.MOD_ID + nameof(DonClothingFromEntities),
      Default = true.ToString()
    };

    public static readonly WorldConfigurationAttribute DonMiscFromEntities = new WorldConfigurationAttribute {
      DataType = EnumDataType.Bool,
      Category = WorldConfigCategoryDon,
      Code = Constants.MOD_ID + nameof(DonMiscFromEntities),
      Default = true.ToString()
    };

    public const string WorldConfigCategorySwap = Constants.MOD_ID + "Swap";
    public static readonly WorldConfigurationAttribute SwapArmorWithEntities = new WorldConfigurationAttribute {
      DataType = EnumDataType.Bool,
      Category = WorldConfigCategorySwap,
      Code = Constants.MOD_ID + nameof(SwapArmorWithEntities),
      Default = true.ToString()
    };

    public static readonly WorldConfigurationAttribute SwapClothingWithEntities = new WorldConfigurationAttribute {
      DataType = EnumDataType.Bool,
      Category = WorldConfigCategorySwap,
      Code = Constants.MOD_ID + nameof(SwapClothingWithEntities),
      Default = true.ToString()
    };
  }

  public static class WorldConfigExtensions {
    public static bool AsBool(this WorldConfigurationAttribute attribute, ICoreAPI api) {
      switch (attribute.DataType) {
        case EnumDataType.Bool:
          return api.World.Config.GetBool(attribute.Code, (bool)attribute.TypedDefault);
        default:
          LogError(attribute, api, typeof(bool));
          return default(bool);
      }
    }

    public static int AsInt(this WorldConfigurationAttribute attribute, ICoreAPI api) {
      switch (attribute.DataType) {
        case EnumDataType.IntInput:
        case EnumDataType.IntRange:
          return api.World.Config.GetInt(attribute.Code, (int)attribute.TypedDefault);
        default:
          LogError(attribute, api, typeof(int));
          return default(int);
      }
    }

    public static double AsDouble(this WorldConfigurationAttribute attribute, ICoreAPI api) {
      switch (attribute.DataType) {
        case EnumDataType.DoubleInput:
          return api.World.Config.GetDecimal(attribute.Code, (float)attribute.TypedDefault);
        default:
          LogError(attribute, api, typeof(double));
          return default(double);
      }
    }

    public static float AsFloat(this WorldConfigurationAttribute attribute, ICoreAPI api) {
      return (float)attribute.AsDouble(api);
    }

    public static string AsString(this WorldConfigurationAttribute attribute, ICoreAPI api) {
      switch (attribute.DataType) {
        case EnumDataType.String:
        case EnumDataType.DropDown:
          return api.World.Config.GetString(attribute.Code, (string)attribute.TypedDefault);
        default:
          LogError(attribute, api, typeof(string));
          return default(string);
      }
    }

    private static void LogError(WorldConfigurationAttribute attribute, ICoreAPI api, System.Type requestedType) {
      api?.Logger.Error("{0} - Cannot retrieve {1} as a {2}, it is defined as a {3}.", Constants.MOD_ID, attribute.Code, requestedType, attribute.DataType);
    }
  }
}
