using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace DoffAndDonAgain.Common {
  public class NetworkManager {
    protected DoffAndDonSystem DoffAndDonSystem { get; }
    protected IClientNetworkChannel ClientChannel { get; }

    public NetworkManager(DoffAndDonSystem doffAndDonSystem) {
      DoffAndDonSystem = doffAndDonSystem;
      var channel = doffAndDonSystem.Api.Network.RegisterChannel(Constants.MOD_ID);
      channel.RegisterMessageType<DoffAndDonEventArgs>();

      if (doffAndDonSystem.Side == EnumAppSide.Client) {
        ClientChannel = (channel as IClientNetworkChannel);
      }
      else {
        var serverChannel = (channel as IServerNetworkChannel);
        serverChannel.SetMessageHandler<DoffAndDonEventArgs>(OnReceivedActionEventFromClient);
      }

      doffAndDonSystem.OnAfterInput += OnAfterInput;
    }

    public void OnAfterInput(DoffAndDonEventArgs eventArgs) {
      if (!eventArgs.Successful) {
        return;
      }

      ClientChannel?.SendPacket(eventArgs);
    }

    public void OnReceivedActionEventFromClient(IServerPlayer fromPlayer, DoffAndDonEventArgs eventArgs) {
      eventArgs.ForPlayer = fromPlayer;
      eventArgs.Successful = false;
      switch (eventArgs.ActionType) {
        case EnumActionType.Doff:
          DoffAndDonSystem.TriggerServerReceivedDoffRequest(eventArgs);
          break;
        case EnumActionType.Don:
          DoffAndDonSystem.TriggerServerReceivedDonRequest(eventArgs);
          break;
        case EnumActionType.Swap:
          DoffAndDonSystem.TriggerServerReceivedSwapRequest(eventArgs);
          break;
      }
    }
  }
}
