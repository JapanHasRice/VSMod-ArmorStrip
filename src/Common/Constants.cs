using Vintagestory.API.Client;

namespace DoffAndDonAgain.Common {
  public static class Constants {
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
