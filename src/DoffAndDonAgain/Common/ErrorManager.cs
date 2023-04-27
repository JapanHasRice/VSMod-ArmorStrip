using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;

namespace DoffAndDonAgain.Common {
  public class Error {
    public string Message { get; }
    public string ModId { get; }
    public ErrorArg[] Args { get; }

    public Error(string message, string modId, params ErrorArg[] args) {
      Message = message ?? "";
      ModId = modId;
      Args = args ?? new ErrorArg[0];
    }

    public string Translate(string langCode = null) {
      for (int i = 0; i < Args?.Length; i++) {
        Args[i].LangCode = langCode;
      }
      return langCode == null ? Lang.Get(Message.WithModPrefix(ModId), Args) : Lang.GetL(langCode, Message.WithModPrefix(ModId), Args);
    }
  }

  public class ErrorArg {
    protected bool ShouldTranslate { get; }
    protected string ModId { get; }
    protected object Arg { get; }
    public string LangCode { get; set; }

    public ErrorArg(object arg, bool shouldTranslate = true, string modId = Constants.MOD_ID) {
      Arg = arg;
      ShouldTranslate = shouldTranslate;
      ModId = modId;
    }

    public override string ToString() {
      var output = Arg?.ToString();
      if (output != null && ShouldTranslate) {
        return LangCode == null ? Lang.Get(output.WithModPrefix(ModId)) : Lang.GetL(LangCode ?? "en", output.WithModPrefix(ModId));
      }
      return output;
    }
  }

  public class ErrorManager {
    protected ICoreClientAPI capi { get; }

    private static readonly string DoffToTheGround = "Doff to the ground";
    private static readonly string DoffToAnEntity = "Doff to an entity";
    private static readonly string InvalidEntityTargetError = "Cannot {0} with {1}.";
    private static readonly string MustTargetEntityError = "Need to be targeting something to {0}.";
    private static readonly string DisabledError = "{0} is disabled by configuration";

    private static readonly string TargetLostError = "Server could not locate the targeted entity.";

    private static readonly string UndoffableError = "Nothing to doff or the target cannot take the items.";
    private static readonly string UndonnableError = "Nothing to don or you do not have room.";
    private static readonly string UnSwappableError = "Nothing to swap, or none of the items could be exchanged.";

    private static readonly string BothHandsFreeError = "Need both hands free.";
    private static readonly string OneHandFreeError = "Need at least 1 free hand.";

    public static void SetInvalidEntityTargetError(DoffAndDonEventArgs eventArgs, Entity entity) {
      eventArgs.Error = new Error(
        InvalidEntityTargetError,
        Constants.MOD_ID,
        new ErrorArg(eventArgs.ActionType),
        new ErrorArg(entity?.GetName() ?? "", shouldTranslate: false)
      );
    }

    public static void SetMustTargetEntityError(DoffAndDonEventArgs eventArgs) {
      eventArgs.Error = new Error(
        MustTargetEntityError,
        Constants.MOD_ID,
        new ErrorArg(eventArgs.ActionType)
      );
    }

    public static void SetDisabledError(DoffAndDonEventArgs eventArgs) {
      object action;
      switch (eventArgs.ActionType) {
        case EnumActionType.Doff:
          action = eventArgs.TargetType == EnumTargetType.Nothing ? DoffToTheGround : DoffToAnEntity;
          break;
        default:
          action = eventArgs.TargetType;
          break;
      }
      eventArgs.Error = new Error(
        DisabledError,
        Constants.MOD_ID,
        new ErrorArg(action)
      );
    }

    public static void SetTargetLostError(DoffAndDonEventArgs eventArgs)
      => eventArgs.Error = new Error(TargetLostError, Constants.MOD_ID);

    public static void SetCannotTransferError(DoffAndDonEventArgs eventArgs) {
      switch (eventArgs.ActionType) {
        case EnumActionType.Doff:
          eventArgs.Error = new Error(UndoffableError, Constants.MOD_ID);
          break;
        case EnumActionType.Don:
          eventArgs.Error = new Error(UndonnableError, Constants.MOD_ID);
          break;
        case EnumActionType.Swap:
          eventArgs.Error = new Error(UnSwappableError, Constants.MOD_ID);
          break;
      }
    }

    public static void SetHandsFreeError(DoffAndDonEventArgs eventArgs, int handsNeeded)
      => eventArgs.Error = new Error(handsNeeded == 1 ? OneHandFreeError : BothHandsFreeError, Constants.MOD_ID);

    public ErrorManager(DoffAndDonSystem doffAndDonSystem) {
      capi = doffAndDonSystem.ClientAPI;

      doffAndDonSystem.OnAfterInput += OnAfterInput;
      doffAndDonSystem.OnAfterServerHandledRequest += OnAfterServerHandledRequest;
    }

    protected void OnAfterInput(DoffAndDonEventArgs eventArgs) {
      if (!eventArgs.Successful && eventArgs.Error != null) {
        TriggerFromClient(eventArgs);
      }
    }

    protected void OnAfterServerHandledRequest(DoffAndDonEventArgs eventArgs) {
      if (!eventArgs.Successful && eventArgs.Error != null && eventArgs.ForPlayer != null) {
        TriggerFromServer(eventArgs);
      }
    }

    protected void TriggerFromClient(DoffAndDonEventArgs eventArgs) {
      capi?.TriggerIngameError(this, eventArgs.Error.Message, eventArgs.Error.Translate());
    }

    protected void TriggerFromServer(DoffAndDonEventArgs eventArgs) {
      var langCode = eventArgs.ForPlayer.LanguageCode;
      eventArgs.ForPlayer?.SendIngameError(eventArgs.Error.Message, eventArgs.Error.Translate(langCode));
    }
  }
}
