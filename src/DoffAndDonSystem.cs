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
    public DoffAndDonEventApi Event { get; } = new DoffAndDonEventApi();
    public NetworkManager Network { get; private set; }
    public ErrorManager Error { get; private set; }
    public SoundManager Sounds { get; private set; }

    public ICoreClientAPI ClientAPI { get; private set; }
    public IClientNetworkChannel ClientChannel { get; private set; }
    public ArmorManipulationInputHandler ArmorManipulationInputHandler { get; private set; }

    public ICoreServerAPI ServerAPI { get; private set; }
    public IServerNetworkChannel ServerChannel { get; private set; }
    public ArmorTransferHandler ArmorTransferHandler { get; private set; }

    public override void Start(ICoreAPI api) {
      base.Start(api);
      Api = api;

      api.RegisterEntityBehaviorClass("doffanddonnable", typeof(EntityBehaviorDoffAndDonnable));

      Network = new NetworkManager(this);
      Error = new ErrorManager(this);
      Sounds = new SoundManager(this);
    }

    public override void StartClientSide(ICoreClientAPI api) {
      base.StartClientSide(api);

      ClientAPI = api;
      ClientChannel = api.Network.GetChannel(Constants.MOD_ID);

      ArmorManipulationInputHandler = new ArmorManipulationInputHandler(this);
    }

    public override void StartServerSide(ICoreServerAPI api) {
      base.StartServerSide(api);

      ServerAPI = api;
      ServerChannel = api.Network.GetChannel(Constants.MOD_ID);

      ArmorTransferHandler = new ArmorTransferHandler(this);
    }
  }
}
