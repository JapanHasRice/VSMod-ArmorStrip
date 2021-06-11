using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace ArmorStrip {
  public class ArmorStripMod : ModSystem {
    private const GlKeys DEFAULT_STRIP_KEY = GlKeys.U;
    private const string STRIP_CODE = "droparmor";
    private const string STRIP_DESC = "Remove all armor";
    private const string STRIP_ERROR_HANDS = "needbothhandsfree";
    private const string STRIP_ERROR_HANDS_DESC = "Need both hands free.";
    public const string STRIP_CHANNEL_NAME = "armorstrip";
    public override void Start(ICoreAPI api) {
      base.Start(api);

      api.Network.RegisterChannel(STRIP_CHANNEL_NAME)
        .RegisterMessageType(typeof(DropAllArmorPacket));
    }

    public override void StartClientSide(ICoreClientAPI capi) {
      base.StartClientSide(capi);

      capi.Input.RegisterHotKey(STRIP_CODE, STRIP_DESC, DEFAULT_STRIP_KEY, HotkeyType.CharacterControls);
      capi.Input.SetHotKeyHandler(STRIP_CODE, (KeyCombination kc) => { return TryToStrip(capi); });
    }

    public override void StartServerSide(ICoreServerAPI sapi) {
      base.StartServerSide(sapi);

      sapi.Network.GetChannel(STRIP_CHANNEL_NAME).SetMessageHandler<DropAllArmorPacket>((IServerPlayer stripper, DropAllArmorPacket packet) => { Strip(stripper); });
    }

    private bool TryToStrip(ICoreClientAPI capi) {
      if (HasBothHandsEmpty(capi.World.Player)) {
        capi.Network.GetChannel(STRIP_CHANNEL_NAME).SendPacket(new DropAllArmorPacket());
        return true;
      }
      else {
        capi.TriggerIngameError(this, STRIP_ERROR_HANDS, Lang.GetIfExists($"armorstrip:ingameerror-{STRIP_ERROR_HANDS}") ?? STRIP_ERROR_HANDS_DESC);
        return false;
      }
    }

    private bool HasBothHandsEmpty(IPlayer stripper) {
      return stripper.Entity.RightHandItemSlot.Empty && stripper.Entity.LeftHandItemSlot.Empty;
    }

    private void Strip(IServerPlayer stripper) {
      foreach (var slot in stripper.Entity.GetArmorSlots().Values) {
        if (!(slot?.Empty ?? true)) { stripper.InventoryManager.DropItem(slot, true); }
      }
    }
  }
}
