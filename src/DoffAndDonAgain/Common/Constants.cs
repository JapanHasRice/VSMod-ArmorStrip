using Vintagestory.API.Client;

namespace DoffAndDonAgain.Common {
  public static class Constants {
    #region Code

    public const string MOD_ID = "doffanddonagain";
    public const GlKeys DEFAULT_KEY = GlKeys.U;
    public const string DOFF_CODE = "doffarmor";
    public const string DON_CODE = "donarmor";
    public const string SWAP_CODE = "swaparmor";

    #endregion

    public const string DOFF_DESC = "Doff - Remove equipped armor and/or clothing";
    public const string DON_DESC = "Don - Equip items from targeted Entity";
    public const string SWAP_DESC = "Swap - Exchange armor and/or clothing with targeted Entity";


    #region Errors

    public const string DOFF = "Doff";
    public const string DOFF_GROUND = "Doff to the ground";
    public const string DOFF_ENTITY = "Doff to an entity";
    public const string DON = "Don";
    public const string SWAP = "Swap";
    public const string ERROR_INVALID_ENTITY_TARGET = "Cannot {0} with {1}.";
    public const string ERROR_MUST_TARGET_ENTITY = "Need to be targeting something to {0}.";
    public const string ERROR_DISABLED = "{0} is disabled by configuration";


    public const string ERROR_TARGET_LOST = "Server could not locate the targeted entity.";

    public const string ERROR_UNDOFFABLE = "Nothing to doff or the target cannot take the items.";
    public const string ERROR_UNDONNABLE = "Nothing to don or you do not have room.";
    public const string ERROR_UNSWAPPABLE = "Nothing to swap, or none of the items could be exchanged.";

    public const string ERROR_BOTH_HANDS = "Need both hands free.";
    public const string ERROR_ONE_HAND = "Need at least 1 free hand.";

    #endregion

    public static string WithModPrefix(this string key, string modId = MOD_ID) => $"{modId}:{key}";
  }
}
