using DoffAndDonAgain.Common;
using DoffAndDonAgain.Common.Network;
using DoffAndDonAgain.Utility;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace DoffAndDonAgain.Server {
  public class SwapHandler {
    protected bool ShouldSwap { get; set; } = true;
    protected float SaturationCostPerSwap { get; set; } = 0f;

    private DoffAndDonSystem System;

    public SwapHandler(DoffAndDonSystem system) {
      if (system.Side != EnumAppSide.Server) {
        system.Api.Logger.Warning("{0} is a server object instantiated on the client, ignoring.", nameof(DoffHandler));
        return;
      }
      System = system;

      System.ServerChannel.SetMessageHandler<SwapArmorPacket>(OnSwapArmorPacket);
      LoadServerSettings(system.Api);
    }

    protected void LoadServerSettings(ICoreAPI api) {
      var configSystem = api.ModLoader.GetModSystem<DoffAndDonConfigurationSystem>();
      if (configSystem == null) {
        api.Logger.Error("[{0}] {1} was not loaded. Using defaults.", nameof(SwapHandler), nameof(DoffAndDonConfigurationSystem));
        LoadServerSettings(new DoffAndDonServerConfig());
        return;
      }

      LoadServerSettings(configSystem.ServerSettings);
    }

    protected virtual void LoadServerSettings(DoffAndDonServerConfig serverSettings) {
      ShouldSwap = serverSettings.EnableSwap.Value;
      SaturationCostPerSwap = serverSettings.SaturationCostPerSwap.Value;
    }

    private void OnSwapArmorPacket(IServerPlayer player, SwapArmorPacket packet) {
      bool swapped = false;
      var armorStand = player.Entity.GetEntityArmorStandById(packet.ArmorStandEntityId);
      if (armorStand == null) {
        System.Error.TriggerFromServer(Constants.ERROR_TARGET_LOST, player);
      }
      else {
        if (ShouldSwap) {
          swapped = SwapArmorWithStand(player, armorStand);
          OnSwapcompleted(player, swapped);
        }
        else {
          System.Error.TriggerFromServer(Constants.ERROR_SWAP_DISABLED, player);
        }
      }
    }

    private bool SwapArmorWithStand(IServerPlayer swapper, EntityArmorStand armorStand) {
      if (swapper == null || armorStand == null) { return false; }
      bool swappedAnything = false;
      var playerArmorSlots = swapper.Entity.GetArmorSlots();
      var armorStandArmorSlots = armorStand.GetArmorSlots();

      for (int i = 0; i < playerArmorSlots.Count; i++) {
        if (playerArmorSlots[i].Empty && armorStandArmorSlots[i].Empty) { continue; }
        bool swapped = playerArmorSlots[i].TryFlipWith(armorStandArmorSlots[i]);
        swappedAnything = swapped || swappedAnything;
        if (swapped) {
          System.Sounds.PlayArmorShufflingSounds(swapper, playerArmorSlots[i]?.Itemstack?.Item, armorStandArmorSlots[i]?.Itemstack?.Item);
        }
      }

      return swappedAnything;
    }

    private void OnSwapcompleted(IServerPlayer player, bool successful) {
      if (successful) {
        OnSuccessfulSwap(player);
      }
      else {
        System.Error.TriggerFromServer(Constants.ERROR_COULD_NOT_SWAP, player);
      }
    }

    private void OnSuccessfulSwap(IServerPlayer player) {
      player.Entity.ConsumeSaturation(SaturationCostPerSwap);
    }
  }
}
