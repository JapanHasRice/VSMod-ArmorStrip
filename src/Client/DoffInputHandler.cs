using DoffAndDonAgain.Common;
using DoffAndDonAgain.Common.Network;
using Vintagestory.API.Client;

namespace DoffAndDonAgain.Client {
  public class DoffInputHandler : ArmorManipulationInputHandler {
    public DoffInputHandler(DoffAndDonSystem system) : base(system) {
      System.ClientAPI.Input.RegisterHotKey(Constants.DOFF_CODE, Constants.DOFF_DESC, Constants.DEFAULT_KEY, HotkeyType.CharacterControls, ctrlPressed: true);
      System.ClientAPI.Input.SetHotKeyHandler(Constants.DOFF_CODE, OnTryToDoff);

      HandsRequired = System.Config.HandsNeededToDoff;
      SaturationRequired = System.Config.SaturationCostPerDoff;
    }

    private bool OnTryToDoff(KeyCombination kc) {
      string errorCode;
      if (CanPlayerDoff(out errorCode)) {
        SendDoffRequest(GetTargetedArmorStandEntity()?.EntityId);
      }
      else {
        System.Error.TriggerFromClient(errorCode);
      }
      return true;
    }

    private void SendDoffRequest(long? armorStandEntityId = null) {
      System.ClientChannel.SendPacket(new DoffArmorPacket(armorStandEntityId));
    }

    private bool CanPlayerDoff(out string errorCode) {
      return HasEnoughHandsFree(out errorCode) && HasEnoughSaturation(out errorCode);
    }
  }
}
