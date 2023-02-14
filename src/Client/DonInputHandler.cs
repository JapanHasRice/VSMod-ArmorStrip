using DoffAndDonAgain.Common;
using DoffAndDonAgain.Common.Network;
using Vintagestory.API.Client;

namespace DoffAndDonAgain.Client {
  public class DonInputHandler : ArmorManipulationInputHandler {
    protected bool DonEnabled { get; set; } = true;
    public DonInputHandler(DoffAndDonSystem system) : base(system) {
      System.ClientAPI.Input.RegisterHotKey(Constants.DON_CODE, Constants.DON_DESC, Constants.DEFAULT_KEY, HotkeyType.CharacterControls);
      System.ClientAPI.Input.SetHotKeyHandler(Constants.DON_CODE, OnTryToDon);
    }

    protected override void LoadServerSettings(DoffAndDonServerConfig serverSettings) {
      HandsRequired = serverSettings.HandsNeededToDon.Value;
      SaturationRequired = serverSettings.SaturationCostPerDon.Value;
      DonEnabled = serverSettings.EnableDon.Value;
    }

    private bool OnTryToDon(KeyCombination kc) {
      string errorCode;
      long armorStandEntityId;
      if (CanPlayerDon(out armorStandEntityId, out errorCode)) {
        SendDonRequest(armorStandEntityId);
      }
      else {
        System.Error.TriggerFromClient(errorCode);
      }
      return true;
    }

    private void SendDonRequest(long armorStandEntityId) {
      System.ClientChannel.SendPacket(new DonArmorPacket(armorStandEntityId));
    }

    private bool CanPlayerDon(out long armorStandEntityId, out string errorCode) {
      armorStandEntityId = -1;
      return IsDonEnabled(out errorCode)
             && IsTargetingArmorStand(out armorStandEntityId, out errorCode)
             && HasEnoughHandsFree(out errorCode)
             && HasEnoughSaturation(out errorCode);
    }

    private bool IsDonEnabled(out string errorCode) {
      errorCode = null;
      if (DonEnabled) {
        return true;
      }
      else {
        errorCode = System.Error.GetErrorText(Constants.ERROR_DON_DISABLED);
        return false;
      }
    }
  }
}
