using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace DoffAndDonAgain.Common {
  public class NetworkManager {
    protected DoffAndDonSystem System { get; }
    protected IClientNetworkChannel ClientChannel { get; }
    protected IServerNetworkChannel ServerChannel { get; }

    public NetworkManager(DoffAndDonSystem system) {
      System = system;
      var channel = system.Api.Network.RegisterChannel(Constants.MOD_ID);
      channel.RegisterMessageType<ArmorActionEventArgs>();

      if (system.Side == EnumAppSide.Client) {
        ClientChannel = (channel as IClientNetworkChannel);
      }
      else {
        ServerChannel = (channel as IServerNetworkChannel);
        ServerChannel.SetMessageHandler<ArmorActionEventArgs>(OnReceivedActionEventFromClient);
      }

      system.Event.OnAfterInput += OnAfterInput;
    }

    public void OnAfterInput(ArmorActionEventArgs eventArgs) {
      if (!eventArgs.Successful) {
        return;
      }

      ClientChannel.SendPacket(eventArgs);
    }

    public void OnReceivedActionEventFromClient(IServerPlayer fromPlayer, ArmorActionEventArgs eventArgs) {
      eventArgs.ForPlayer = fromPlayer;
      eventArgs.Successful = false;
      switch (eventArgs.ActionType) {
        case EnumActionType.Doff:
          System.Event.TriggerServerReceivedDoffRequest(eventArgs);
          break;
        case EnumActionType.Don:
          System.Event.TriggerServerReceivedDonRequest(eventArgs);
          break;
        case EnumActionType.Swap:
          System.Event.TriggerServerReceivedSwapRequest(eventArgs);
          break;
      }
    }
  }
}
