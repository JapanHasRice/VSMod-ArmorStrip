using Vintagestory.API.Client;

namespace DoffAndDonAgain.Common {
  public static class Constants {
    #region Code

    public const string MOD_ID = "doffanddonagain";
    public const GlKeys DEFAULT_KEY = GlKeys.U;
    public const string DOFF_CODE = "doffarmor";
    public const string DOFF_DESC = "Doff: Remove equipped armor and/or clothing";
    public const string DON_CODE = "donarmor";
    public const string DON_DESC = "Don: Equip items from targeted Entity";
    public const string SWAP_CODE = "swaparmor";
    public const string SWAP_DESC = "Swap: Exchange armor and/or clothing with targeted Entity";

    #endregion

    #region Errors

    public const string ERROR_BOTH_HANDS = "Need both hands free.";
    public const string ERROR_ONE_HAND = "Need at least 1 free hand.";
    public const string ERROR_SATURATION = "Not enough satiety, need at least {0}.";
    public const string ERROR_INVALID_ENTITY_TARGET = "Cannot {0} with {1}.";
    public const string ERROR_MUST_TARGET_ENTITY = "Need to be targeting something to {0}.";
    public const string ERROR_TARGET_LOST = "Server could not locate the targeted entity.";
    public const string ERROR_UNDOFFABLE = "Nothing to doff or the target cannot take the items.";
    public const string ERROR_UNDONNABLE = "Nothing to don or you do not have room.";
    public const string ERROR_COULD_NOT_SWAP = "Nothing to swap, or none of the items could be exchanged.";
    public const string ERROR_DON_DISABLED = "Donning is disabled by configuration.";
    public const string ERROR_SWAP_DISABLED = "Swapping is disabled by configuration.";
    public const string ERROR_DOFF_GROUND_DISABLED = "Doffing to the ground is disabled by configuration.";
    public const string ERROR_DOFF_ENTITY_DISABLED = "Doffing to an entity is disabled by configuration.";

    #endregion
  }
}
