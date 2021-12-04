using Vintagestory.API.Client;

namespace DoffAndDonAgain.Common {
  public static class Constants {

    #region Config
    public const string FILENAME = "DoffAndDonAgainConfig.json";
    public const float DEFAULT_DOFF_COST = 20;
    public const float MIN_SATURATION_COST = 0;
    public readonly static string SaturationCostPerDoffDescription = $"Satiety required to doff. [Default: {DEFAULT_DOFF_COST}, Min: {MIN_SATURATION_COST}]";
    public const float DEFAULT_DON_COST = DEFAULT_DOFF_COST;
    public readonly static string SaturationCostPerDonDescription = $"Satiety required to don armor from an armor stand. [Default: {DEFAULT_DON_COST}, Min: {MIN_SATURATION_COST}]";
    public const float DEFAULT_SWAP_COST = (DEFAULT_DOFF_COST + DEFAULT_DON_COST) * 0.6f;
    public readonly static string SaturationCostPerSwapDescription = $"Satiety required to swap armor with an armor stand. [Default: {DEFAULT_SWAP_COST}, Min: {MIN_SATURATION_COST}]";
    public const int DEFAULT_HANDS_FREE = 2;
    public const int MIN_HANDS_FREE = 0;
    public const int MAX_HANDS_FREE = 2;
    public readonly static string HandsNeededToDoffDescription = $"Number of available (empty) hands needed to doff. [Default: {DEFAULT_HANDS_FREE}, Min: {MIN_HANDS_FREE}, Max: {MAX_HANDS_FREE}]";
    public readonly static string HandsNeededToDonDescription = $"Number of available (empty) hands needed to don. [Default: {DEFAULT_HANDS_FREE}, Min: {MIN_HANDS_FREE}, Max: {MAX_HANDS_FREE}]";
    public readonly static string HandsNeededToSwapDescription = $"Number of available (empty) hands needed to swap. [Default: {DEFAULT_HANDS_FREE}, Min: {MIN_HANDS_FREE}, Max: {MAX_HANDS_FREE}]";
    public const bool DEFAULT_DROP_ON_STAND_DOFF = false;
    public readonly static string DropArmorWhenDoffingToStandDescription = $"If enabled, when doffing to an armor stand, any armor that cannot be placed on the stand is dropped to the ground as if the player had doffed without an armor stand. [Default: {DEFAULT_DROP_ON_STAND_DOFF}]";
    public const bool DEFAULT_ENABLE_DON = true;
    public readonly static string EnableDonDescription = $"If enabled, player can quickly equip armor from an armor stand they are currently targeting. [Default: {DEFAULT_ENABLE_DON}]";
    public const bool DEFAULT_ENABLE_SWAP = true;
    public readonly static string EnableSwapDescription = $"If enabled, player can quickly swap armor with an armor stand they are currently targeting. [Default: {DEFAULT_ENABLE_SWAP}]";
    public const bool DEFAULT_ENABLE_GROUND_DOFF = true;
    public readonly static string EnableDoffToGroundDescription = $"If enabled, player can quickly unequip armor and throw it on the ground. [Default: {DEFAULT_ENABLE_GROUND_DOFF}]";
    public const bool DEFAULT_ENABLE_STAND_DOFF = true;
    public readonly static string EnableDoffToArmorStandDescription = $"If enabled, player can quickly unequip armor, placing it on the currently targeted armor stand. [Default: {DEFAULT_ENABLE_STAND_DOFF}]";
    public const bool DEFAULT_ENABLE_TOOL_DONNING = true;
    public readonly static string EnableToolDonningDescription = $"If enabled, when targeted armor stand has an equipped tool, player will attempt to take the tool into their inventory. [Default: {DEFAULT_ENABLE_TOOL_DONNING}]\nSee additional options below for rules on how the tool can be placed in the player's inventory, and be mindful of the empty hands setting.";
    public const bool DEFAULT_DON_TOOL_ACTIVE_HOTBAR_ONLY = true;
    public readonly static string DonToolOnlyToActiveHotbarDescription = $"If enabled, tools donned from armor stands will only be placed in the currently active hotbar slot. [Default: {DEFAULT_DON_TOOL_ACTIVE_HOTBAR_ONLY}]";
    public const bool DEFAULT_DON_TOOL_HOTBAR_ONLY = true;
    public readonly static string DonToolOnlyToHotbarDescription = $"If enabled, tools donned from armor stands will only be placed in an available hotbar slot. [Default: {DEFAULT_DON_TOOL_HOTBAR_ONLY}]";

    #endregion

    #region Code

    public const string MOD_ID = "doffanddonagain";
    public const GlKeys DEFAULT_KEY = GlKeys.U;
    public const string DOFF_CODE = "doffarmor";
    public const string DOFF_DESC = "Doff: Remove all armor";
    public const string DON_CODE = "donarmor";
    public const string DON_DESC = "Don: Equip empty armor slots from Armor Stand";
    public const string SWAP_CODE = "swaparmor";
    public const string SWAP_DESC = "Swap: Exchange all armor with targeted Armor Stand";

    #endregion

    #region Errors

    public const string ERROR_BOTH_HANDS = "Need both hands free.";
    public const string ERROR_ONE_HAND = "Need at least 1 free hand.";
    public const string ERROR_SATURATION = "Not enough satiety, need at least {0}.";
    public const string ERROR_MISSING_ARMOR_STAND_TARGET = "Need to be targeting an armor stand.";
    public const string ERROR_TARGET_LOST = "Server could not locate the targeted armor stand.";
    public const string ERROR_UNDOFFABLE = "Nothing to doff or the armor stand does not have room.";
    public const string ERROR_UNDONNABLE = "Nothing to don or you do not have room.";
    public const string ERROR_COULD_NOT_SWAP = "Nothing to swap, or none of the armor could be exchanged.";
    public const string ERROR_DON_DISABLED = "Donning armor is disabled by configuration.";
    public const string ERROR_SWAP_DISABLED = "Swapping armor is disabled by configuration.";
    public const string ERROR_DOFF_GROUND_DISABLED = "Doffing armor to the ground is disabled by configuration.";
    public const string ERROR_DOFF_STAND_DISABLED = "Doffing armor to an armor stand is disabled by configuration.";

    #endregion
  }
}
