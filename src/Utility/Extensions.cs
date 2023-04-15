using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace DoffAndDonAgain.Utility {
  public static class GameExtensions {
    public static void ConsumeSaturation(this EntityAgent player, float amount) {
      player.GetBehavior<EntityBehaviorHunger>()?.ConsumeSaturation(amount);
    }

    public static bool CanBeTargetedFor(this EntityAgent engityAgent, EnumActionType actionType) {
      var result = engityAgent?.GetBehavior<EntityBehaviorDoffAndDonnable>()?.CanBeTargetedFor(actionType) ?? false;
      return result;
    }
  }
}
