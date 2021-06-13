using Vintagestory.API.Client;

namespace DoffAndDonAgain.Config {
  public static class Constants {
    public const string FILENAME = "DoffAndDonAgainConfig.json";
    public const float DEFAULT_DOFF_COST = 20;
    public const float MIN_DOFF_COST = 0;
    public readonly static string SaturationCostPerDoffDescription = $"Satiety required to quickly remove all of your armor. [Default: {DEFAULT_DOFF_COST}, Min: {MIN_DOFF_COST}]";
    public const float DEFAULT_DON_COST = DEFAULT_DOFF_COST;
    public const float MIN_DON_COST = MIN_DOFF_COST;
    public readonly static string SaturationCostPerDonDescription = $"Satiety required to quickly put on armor from an armor stand. [Default: {DEFAULT_DON_COST}, Min: {MIN_DON_COST}]";
    public const int DEFAULT_HANDS_FREE = 2;
    public const int MIN_HANDS_FREE = 0;
    public const int MAX_HANDS_FREE = 2;
    public readonly static string HandsNeededToDoffDescription = $"Number of available (empty) hands needed to quickly remove all of your armor. [Default: {DEFAULT_HANDS_FREE}, Min: {MIN_HANDS_FREE}, Max: {MAX_HANDS_FREE}]";
    public const bool DEFAULT_DROP_ON_STAND_DOFF = false;
    public readonly static string DropArmorWhenDoffingToStandDescription = $"If enabled, when doffing to an armor stand, any armor that cannot be placed on the stand is dropped to the ground as if the player had doffed without an armor stand. [Default: {DEFAULT_DROP_ON_STAND_DOFF}]";
  }
}

namespace DoffAndDonAgain {
  public static class Constants {
    public const string CHANNEL_NAME = "doffanddonagain";
    public const GlKeys DEFAULT_KEY = GlKeys.U;
    public const string DOFF_CODE = "doffarmor";
    public const string DOFF_DESC = "Doff: Remove all armor";
    public const string DON_CODE = "donarmor";
    public const string DON_DESC = "Don: Equip empty armor slots from Armor Stand";

    public const string ERROR_BOTH_HANDS = "needbothhandsfree";
    public const string ERROR_BOTH_HANDS_DESC = "Need both hands free.";
    public const string ERROR_ONE_HAND = "needonefreehand";
    public const string ERROR_ONE_HAND_DESC = "Need at least 1 free hand.";
    public const string ERROR_SATURATION = "toohungry";
    public const string ERROR_SATURATION_DESC = "Not enough energy to doff/don.";
    public const string ERROR_MISSING_ARMOR_STAND_TARGET = "musttargetarmorstand";
    public const string ERROR_MISSING_ARMOR_STAND_TARGET_DESC = "Need to be targeting an armor stand.";
  }
}
