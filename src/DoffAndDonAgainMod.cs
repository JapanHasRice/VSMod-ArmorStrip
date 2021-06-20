using DoffAndDonAgain.Config;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace DoffAndDonAgain {
  public abstract class DoffAndDonAgainMod : ModSystem {
    #region Variables and Properties
    protected ICoreAPI Api;
    protected INetworkChannel Channel;
    protected float SaturationCostPerDoff;
    protected float SaturationCostPerDon;
    protected float SaturationCostPerSwap;
    protected bool DropArmorWhenDoffingToStand;

    #endregion

    #region Initialization

    public override bool ShouldLoad(EnumAppSide forSide) {
      return forSide == ForSide();
    }

    public abstract EnumAppSide ForSide();

    public override void Start(ICoreAPI api) {
      base.Start(api);
      Api = api;

      LoadConfigs();
      SetupNetwork();
    }

    private void LoadConfigs() {
      var config = DoffAndDonAgainConfig.LoadOrCreateDefault(Api);

      SaturationCostPerDoff = config.SaturationCostPerDoff;
      SaturationCostPerDon = config.SaturationCostPerDon;
      SaturationCostPerSwap = (SaturationCostPerDoff + SaturationCostPerDon) * 0.6f;
      DropArmorWhenDoffingToStand = config.DropArmorWhenDoffingToStand;
    }

    private void SetupNetwork() {
      Channel = Api.Network.RegisterChannel(Constants.CHANNEL_NAME)
        .RegisterMessageType(typeof(DoffArmorPacket))
        .RegisterMessageType(typeof(DonArmorPacket))
        .RegisterMessageType(typeof(ArmorStandInventoryUpdatedPacket))
        .RegisterMessageType(typeof(SwapArmorPacket));
    }

    #endregion

    protected EntityArmorStand GetEntityArmorStandById(EntityPlayer aroundPlayer, long? armorStandEntityId, float horRange = 10, float vertRange = 10) {
      return armorStandEntityId == null ? null : aroundPlayer.World.GetNearestEntity(aroundPlayer.Pos.AsBlockPos.ToVec3d(), horRange, vertRange, (Entity entity) => {
        return entity.EntityId == armorStandEntityId;
      }) as EntityArmorStand;
    }

    protected string GetErrorText(string errorCode, string errorFallbackText) {
      return Lang.GetIfExists($"doffanddonagain:ingameerror-{errorCode}") ?? errorFallbackText;
    }
  }
}
