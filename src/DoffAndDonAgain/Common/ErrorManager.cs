using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace DoffAndDonAgain.Common {
  public class ErrorManager {
    private ICoreClientAPI capi { get; }

    public ErrorManager(DoffAndDonSystem doffAndDonSystem) {
      capi = doffAndDonSystem.ClientAPI;

      doffAndDonSystem.OnAfterInput += OnAfterInput;
      doffAndDonSystem.OnAfterServerHandledRequest += OnAfterServerHandledRequest;
    }

    private void OnAfterInput(DoffAndDonEventArgs eventArgs) {
      if (eventArgs.Successful) {
        return;
      }

      TriggerFromClient(eventArgs.ErrorCode, eventArgs.ErrorArgs);
    }

    private void OnAfterServerHandledRequest(DoffAndDonEventArgs eventArgs) {
      if (eventArgs.Successful) {
        return;
      }

      TriggerFromServer(eventArgs.ErrorCode, eventArgs.ForPlayer, eventArgs.ErrorArgs);
    }

    public void TriggerFromClient(string errorCode, params object[] args) {
      ModPrefixArgs(errorCode, args);
      capi?.TriggerIngameError(this, errorCode, Lang.Get(errorCode, args));
    }

    public void TriggerFromServer(string errorCode, IServerPlayer forPlayer, params object[] args) {
      ModPrefixArgs(errorCode, args);
      var playersLangCode = forPlayer?.LanguageCode ?? "en";
      forPlayer?.SendIngameError(errorCode, Lang.GetL(playersLangCode, errorCode, args));
    }

    private static void ModPrefixArgs(string errorCode, params object[] args) {
      errorCode = errorCode?.WithModPrefix();
      for (int i = 0; i < args?.Length; i++) {
        args[i] = Lang.Get(args[i].ToString().WithModPrefix());
      }
    }
  }
}
