using DoffAndDonAgain.Client;
using DoffAndDonAgain.Common;
using DoffAndDonAgain.Common.Network;
using DoffAndDonAgain.Server;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace DoffAndDonAgain {
  public class DoffAndDonSystem : ModSystem {
    public ICoreAPI Api { get; private set; }
    public EnumAppSide Side => Api.Side;
    public ErrorManager Error { get; private set; }
    public DoffAndDonAgainConfig Config { get; private set; }

    public ICoreClientAPI ClientAPI { get; private set; }
    public IClientNetworkChannel ClientChannel { get; private set; }
    public ArmorStandRerenderHandler ArmorStandRerenderHandler { get; private set; }
    public DoffInputHandler DoffInputHandler { get; private set; }
    public DonInputHandler DonInputHandler { get; private set; }
    public SwapInputHandler SwapInputHandler { get; private set; }

    public ICoreServerAPI ServerAPI { get; private set; }
    public IServerNetworkChannel ServerChannel { get; private set; }
    public DoffHandler DoffHandler { get; private set; }
    public DonHandler DonHandler { get; private set; }
    public SwapHandler SwapHandler { get; private set; }

    public override void Start(ICoreAPI api) {
      base.Start(api);
      Api = api;
      api.Network.RegisterChannel(Constants.MOD_ID)
        .RegisterMessageType(typeof(DoffArmorPacket))
        .RegisterMessageType(typeof(DonArmorPacket))
        .RegisterMessageType(typeof(ArmorStandInventoryUpdatedPacket))
        .RegisterMessageType(typeof(SwapArmorPacket));

      Error = new ErrorManager(this);
      Config = DoffAndDonAgainConfig.LoadOrCreateDefault(api);
    }

    public override void StartClientSide(ICoreClientAPI api) {
      base.StartClientSide(api);

      ClientAPI = api;
      ClientChannel = api.Network.GetChannel(Constants.MOD_ID);

      ArmorStandRerenderHandler = new ArmorStandRerenderHandler(this);
      DoffInputHandler = new DoffInputHandler(this);
      DonInputHandler = new DonInputHandler(this);
      SwapInputHandler = new SwapInputHandler(this);
    }

    public override void StartServerSide(ICoreServerAPI api) {
      base.StartServerSide(api);

      ServerAPI = api;
      ServerChannel = api.Network.GetChannel(Constants.MOD_ID);

      DoffHandler = new DoffHandler(this);
      DonHandler = new DonHandler(this);
      SwapHandler = new SwapHandler(this);
    }
  }
}
