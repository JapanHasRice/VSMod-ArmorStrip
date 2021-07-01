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
      if (CanPlayerDoff(out long? armorStandEntityId, out string errorCode)) {
        SendDoffRequest(armorStandEntityId);
      }
      else {
        System.Error.TriggerFromClient(errorCode);
      }
      return true;
    }

    private void SendDoffRequest(long? armorStandEntityId = null) {
      System.ClientChannel.SendPacket(new DoffArmorPacket(armorStandEntityId));
    }

    private bool CanPlayerDoff(out long? armorStandEntityId, out string errorCode) {
      return IsDoffEnabled(out armorStandEntityId, out errorCode)
             && HasEnoughHandsFree(out errorCode)
             && HasEnoughSaturation(out errorCode);
    }

    private bool IsDoffEnabled(out long? armorStandEntityId, out string errorCode) {
      armorStandEntityId = GetTargetedArmorStandEntity()?.EntityId;
      return armorStandEntityId == null ? IsDoffToGroundEnabled(out errorCode) : IsDoffToArmorStandEnabled(out errorCode);
    }

    private bool IsDoffToGroundEnabled(out string errorCode) {
      errorCode = null;
      if (System.Config.EnableDoffToGround) {
        return true;
      }
      else {
        errorCode = System.Error.GetErrorText(Constants.ERROR_DOFF_GROUND_DISABLED);
        return false;
      }
    }

    private bool IsDoffToArmorStandEnabled(out string errorCode) {
      errorCode = null;
      if (System.Config.EnableDoffToArmorStand) {
        return true;
      }
      else {
        errorCode = System.Error.GetErrorText(Constants.ERROR_DOFF_STAND_DISABLED);
        return false;
      }
    }
  }
}
