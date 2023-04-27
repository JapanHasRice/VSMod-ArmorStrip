using System;
using DoffAndDonAgain.Client;
using DoffAndDonAgain.Common;
using DoffAndDonAgain.Server;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace DoffAndDonAgain {
  public class DoffAndDonSystem : ModSystem {
    public ICoreAPI Api { get; private set; }
    public EnumAppSide Side => Api.Side;
    public NetworkManager Network { get; private set; }
    public ErrorManager Error { get; private set; }
    public SoundManager Sounds { get; private set; }

    public ICoreClientAPI ClientAPI { get; private set; }
    public InputHandler InputHandler { get; private set; }

    public TransferHandler TransferHandler { get; private set; }

    public override void Start(ICoreAPI api) {
      base.Start(api);
      Api = api;
      if (api is ICoreClientAPI capi) {
        ClientAPI = capi;
      }

      api.RegisterEntityBehaviorClass(EntityBehaviorDoffAndDonnable.Name, typeof(EntityBehaviorDoffAndDonnable));

      Network = new NetworkManager(this);
      Error = new ErrorManager(this);
      Sounds = new SoundManager(this);
    }

    public override void StartClientSide(ICoreClientAPI api) {
      base.StartClientSide(api);

      InputHandler = new InputHandler(this);
    }

    public override void StartServerSide(ICoreServerAPI api) {
      base.StartServerSide(api);

      TransferHandler = new TransferHandler(this);
    }

    public event Action<DoffAndDonEventArgs> OnDoffKeyPressed;
    public bool TriggerDoffKeyPressed(KeyCombination keyCombination) {
      var eventArgs = new DoffAndDonEventArgs(keyCombination, EnumActionType.Doff);
      OnDoffKeyPressed?.Invoke(eventArgs);
      TriggerAfterInput(eventArgs);
      return true;
    }

    public event Action<DoffAndDonEventArgs> OnDonKeyPressed;
    public bool TriggerDonKeyPressed(KeyCombination keyCombination) {
      var eventArgs = new DoffAndDonEventArgs(keyCombination, EnumActionType.Don);
      OnDonKeyPressed?.Invoke(eventArgs);
      TriggerAfterInput(eventArgs);
      return true;
    }

    public event Action<DoffAndDonEventArgs> OnSwapKeyPressed;
    public bool TriggerSwapKeyPressed(KeyCombination keyCombination) {
      var eventArgs = new DoffAndDonEventArgs(keyCombination, EnumActionType.Swap);
      OnSwapKeyPressed?.Invoke(eventArgs);
      TriggerAfterInput(eventArgs);
      return true;
    }

    public event Action<DoffAndDonEventArgs> OnAfterInput;
    public void TriggerAfterInput(DoffAndDonEventArgs eventArgs) {
      OnAfterInput?.Invoke(eventArgs);
    }

    public event Action<DoffAndDonEventArgs> OnServerReceivedDoffRequest;
    public void TriggerServerReceivedDoffRequest(DoffAndDonEventArgs eventArgs) {
      OnServerReceivedDoffRequest?.Invoke(eventArgs);
      TriggerAfterServerHandledRequest(eventArgs);
    }

    public event Action<DoffAndDonEventArgs> OnServerReceivedDonRequest;
    public void TriggerServerReceivedDonRequest(DoffAndDonEventArgs eventArgs) {
      OnServerReceivedDonRequest?.Invoke(eventArgs);
      TriggerAfterServerHandledRequest(eventArgs);
    }

    public event Action<DoffAndDonEventArgs> OnServerReceivedSwapRequest;
    public void TriggerServerReceivedSwapRequest(DoffAndDonEventArgs eventArgs) {
      OnServerReceivedSwapRequest?.Invoke(eventArgs);
      TriggerAfterServerHandledRequest(eventArgs);
    }

    public event Action<DoffAndDonEventArgs> OnAfterServerHandledRequest;
    public void TriggerAfterServerHandledRequest(DoffAndDonEventArgs eventArgs) {
      OnAfterServerHandledRequest?.Invoke(eventArgs);
    }
  }
}
