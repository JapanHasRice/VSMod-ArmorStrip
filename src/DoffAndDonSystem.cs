using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace DoffAndDonAgain {
  public class DoffAndDonSystem : ModSystem {
    public ICoreClientAPI ClientAPI { get; private set; }
    public IClientNetworkChannel ClientChannel { get; private set; }

    public ICoreServerAPI ServerAPI { get; private set; }
    public IServerNetworkChannel ServerChannel { get; private set; }

    public override void Start(ICoreAPI api) {
      base.Start(api);
      api.Network.RegisterChannel(Constants.CHANNEL_NAME)
        .RegisterMessageType(typeof(DoffArmorPacket))
        .RegisterMessageType(typeof(DonArmorPacket))
        .RegisterMessageType(typeof(ArmorStandInventoryUpdatedPacket))
        .RegisterMessageType(typeof(SwapArmorPacket));
    }

    public override void StartClientSide(ICoreClientAPI api) {
      base.StartClientSide(api);

      ClientAPI = api;
      ClientChannel = api.Network.GetChannel(Constants.CHANNEL_NAME);
    }

    public override void StartServerSide(ICoreServerAPI api) {
      base.StartServerSide(api);

      ServerAPI = api;
      ServerChannel = api.Network.GetChannel(Constants.CHANNEL_NAME);
    }
  }
}
