using Vintagestory.API.Client;

namespace DoffAndDonAgain.Common {
  public static class Constants {
    public const string MOD_ID = "doffanddonagain";

    public const GlKeys DEFAULT_KEY = GlKeys.U;
    public const string DOFF_CODE = "doffarmor";
    public const string DON_CODE = "donarmor";
    public const string SWAP_CODE = "swaparmor";

    public static readonly string DoffHotkeyDescription = "Doff - Remove equipped armor and/or clothing".WithModPrefix();
    public static readonly string DonHotkeyDescription = "Don - Equip items from targeted Entity".WithModPrefix();
    public static readonly string SwapHotkeyDescription = "Swap - Exchange armor and/or clothing with targeted Entity".WithModPrefix();

    public static string WithModPrefix(this string key, string modId = MOD_ID) => $"{modId}:{key}";
  }
}
