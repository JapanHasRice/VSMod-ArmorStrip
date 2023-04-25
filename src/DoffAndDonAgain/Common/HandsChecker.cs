using Vintagestory.API.Common;

namespace DoffAndDonAgain.Common {
  public static class HandsChecker {
    public static bool VerifyEnoughHandsFree(DoffAndDonEventArgs eventArgs, EntityPlayer playerEntity, int handsNeeded)
      => (HandsRequiredReference?[handsNeeded] ?? VerifyBothHandsFree)(eventArgs, playerEntity);

    private static Func<DoffAndDonEventArgs, EntityPlayer, bool>[] HandsRequiredReference = new Func<DoffAndDonEventArgs, EntityPlayer, bool>[3] {
      LookMomNoHands,
      VerifyOneHandFree,
      VerifyBothHandsFree
    };

    private static bool LookMomNoHands(DoffAndDonEventArgs eventArgs, EntityPlayer playerEntity)
      => true;

    private static bool VerifyOneHandFree(DoffAndDonEventArgs eventArgs, EntityPlayer playerEntity) {
      if (playerEntity.IsRightHandEmpty() || playerEntity.IsLeftHandEmpty()) {
        return true;
      }

      eventArgs.ErrorCode = Constants.ERROR_ONE_HAND;
      return false;
    }

    private static bool VerifyBothHandsFree(DoffAndDonEventArgs eventArgs, EntityPlayer playerEntity) {
      if (playerEntity.IsLeftHandEmpty() && playerEntity.IsRightHandEmpty()) {
        return true;
      }

      eventArgs.ErrorCode = Constants.ERROR_BOTH_HANDS;
      return false;
    }

    private static bool IsRightHandEmpty(this EntityPlayer playerEntity) => playerEntity?.RightHandItemSlot.Empty ?? false;

    private static bool IsLeftHandEmpty(this EntityPlayer playerEntity) => playerEntity?.LeftHandItemSlot.Empty ?? false;
  }
}
