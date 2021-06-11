using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace ArmorStrip {
  public class ArmorStripMod : ModSystem {
    private const GlKeys DEFAULT_STRIP_KEY = GlKeys.Y;
    private const string STRIP_CODE = "droparmor";
    private const string STRIP_DESC = "Remove all armor";
    public const string STRIP_CHANNEL_NAME = "armorstrip";
    public override void Start(ICoreAPI api) {
      base.Start(api);

      api.Network.RegisterChannel(STRIP_CHANNEL_NAME)
        .RegisterMessageType(typeof(DropAllArmorPacket));
    }

    public override void StartClientSide(ICoreClientAPI capi) {
      base.StartClientSide(capi);

      capi.Input.RegisterHotKey(STRIP_CODE, STRIP_DESC, DEFAULT_STRIP_KEY, HotkeyType.CharacterControls);
      capi.Input.SetHotKeyHandler(STRIP_CODE, (KeyCombination kc) => {
        capi.Network.GetChannel(STRIP_CHANNEL_NAME).SendPacket(new DropAllArmorPacket());
        return true;
      });
    }

    public override void StartServerSide(ICoreServerAPI sapi) {
      base.StartServerSide(sapi);

      sapi.Network.GetChannel(STRIP_CHANNEL_NAME).SetMessageHandler<DropAllArmorPacket>((IServerPlayer stripper, DropAllArmorPacket packet) => {
        foreach (var slot in stripper.Entity.GetArmorSlots().Values) {
          if (!(slot?.Empty ?? true)) { stripper.InventoryManager.DropItem(slot, true); }
        }
      });
    }
  }
}
