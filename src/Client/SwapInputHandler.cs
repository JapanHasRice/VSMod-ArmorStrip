using DoffAndDonAgain.Common;
using DoffAndDonAgain.Common.Network;
using Vintagestory.API.Client;

namespace DoffAndDonAgain.Client {
  public class SwapInputHandler : ArmorManipulationInputHandler {
    protected bool SwapEnabled { get; set; } = true;

    public SwapInputHandler(DoffAndDonSystem system) : base(system) {
      System.ClientAPI.Input.RegisterHotKey(Constants.SWAP_CODE, Constants.SWAP_DESC, Constants.DEFAULT_KEY, HotkeyType.CharacterControls, shiftPressed: true);
      System.ClientAPI.Input.SetHotKeyHandler(Constants.SWAP_CODE, OnTryToSwap);
    }

    protected override void LoadServerSettings(DoffAndDonServerConfig serverSettings) {
      HandsRequired = serverSettings.HandsNeededToSwap.Value;
      SaturationRequired = serverSettings.SaturationCostPerSwap.Value;
      SwapEnabled = serverSettings.EnableSwap.Value;
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
      armorStandEntityId = -1;
      return IsSwapEnabled(out errorCode)
             && IsTargetingArmorStand(out armorStandEntityId, out errorCode)
             && HasEnoughHandsFree(out errorCode)
             && HasEnoughSaturation(out errorCode);
    }

    private bool IsSwapEnabled(out string errorCode) {
      errorCode = null;
      if (SwapEnabled) {
        return true;
      }
      else {
        errorCode = System.Error.GetErrorText(Constants.ERROR_SWAP_DISABLED);
        return false;
      }
    }
  }
}
