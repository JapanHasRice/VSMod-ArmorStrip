using DoffAndDonAgain.Common;
using DoffAndDonAgain.Common.Network;
using Vintagestory.API.Client;

namespace DoffAndDonAgain.Client {
  public class DonInputHandler : ArmorManipulationInputHandler {
    public DonInputHandler(DoffAndDonSystem system) : base(system) {
      if (System.Config.EnableDon) {
        System.ClientAPI.Input.RegisterHotKey(Constants.DON_CODE, Constants.DON_DESC, Constants.DEFAULT_KEY, HotkeyType.CharacterControls);
        System.ClientAPI.Input.SetHotKeyHandler(Constants.DON_CODE, OnTryToDon);

        HandsRequired = System.Config.HandsNeededToDon;
        SaturationRequired = System.Config.SaturationCostPerDon;
      }
      else {
        System.ClientAPI.Input.RegisterHotKey(Constants.DON_CODE, Constants.DON_DISABLED_DESC, Constants.DEFAULT_KEY, HotkeyType.CharacterControls);
        System.ClientAPI.Input.SetHotKeyHandler(Constants.DON_CODE, OnTryToDonDisabled);
      }
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
      return IsTargetingArmorStand(out armorStandEntityId, out errorCode)
             && HasEnoughHandsFree(out errorCode)
             && HasEnoughSaturation(out errorCode);
    }

    private bool OnTryToDonDisabled(KeyCombination kc) {
      System.Error.TriggerFromClient(Constants.ERROR_DON_DISABLED);
      return true;
    }
  }
}