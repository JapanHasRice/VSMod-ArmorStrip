using DoffAndDonAgain.Common;
using DoffAndDonAgain.Common.Network;
using Vintagestory.API.Client;

namespace DoffAndDonAgain.Client {
  public class SwapInputHandler : ArmorManipulationInputHandler {
    public SwapInputHandler(DoffAndDonSystem system) : base(system) {
      if (System.Config.EnableSwap) {
        System.ClientAPI.Input.RegisterHotKey(Constants.SWAP_CODE, Constants.SWAP_DESC, Constants.DEFAULT_KEY, HotkeyType.CharacterControls, shiftPressed: true);
        System.ClientAPI.Input.SetHotKeyHandler(Constants.SWAP_CODE, OnTryToSwap);

        HandsRequired = System.Config.HandsNeededToSwap;
        SaturationRequired = System.Config.SaturationCostPerSwap;
      }
      else {
        System.ClientAPI.Input.RegisterHotKey(Constants.SWAP_CODE, Constants.SWAP_DISABLED_DESC, Constants.DEFAULT_KEY, HotkeyType.CharacterControls, shiftPressed: true);
        System.ClientAPI.Input.SetHotKeyHandler(Constants.SWAP_CODE, OnTryToSwapDisabled);
      }
    }

    private bool OnTryToSwap(KeyCombination kc) {
      string errorCode;
      long armorStandEntityId;
      if (CanPlayerSwap(out armorStandEntityId, out errorCode)) {
        SendSwapRequest(armorStandEntityId);
      }
      else {
        System.Error.TriggerFromClient(errorCode);
      }
      return true;
    }

    private void SendSwapRequest(long armorStandEntityId) {
      System.ClientChannel.SendPacket(new SwapArmorPacket(armorStandEntityId));
    }

    private bool CanPlayerSwap(out long armorStandEntityId, out string errorCode) {
      return IsTargetingArmorStand(out armorStandEntityId, out errorCode)
             && HasEnoughHandsFree(out errorCode)
             && HasEnoughSaturation(out errorCode);
    }

    private bool OnTryToSwapDisabled(KeyCombination kc) {
      System.Error.TriggerFromClient(Constants.ERROR_SWAP_DISABLED);
      return true;
    }
  }
}
