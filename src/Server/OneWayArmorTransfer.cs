using System.Collections.Generic;
using DoffAndDonAgain.Utility;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace DoffAndDonAgain.Server {
  public abstract class OneWayArmorTransfer {
    // Return true to indicate a successful doffing.
    protected delegate bool OnDoffWithoutDonner(IServerPlayer player, ItemSlot couldNotBeDonnedSlot);

    protected delegate void OnDonnedOneOrMore();

    protected DoffAndDonSystem System { get; set; }

    public OneWayArmorTransfer(DoffAndDonSystem system) {
      if (system.Side != EnumAppSide.Server) {
        system.Api.Logger.Warning("{0} is a server object instantiated on the client, ignoring.", nameof(OneWayArmorTransfer));
        return;
      }
      System = system;
      LoadServerSettings(system.Api);
    }

    protected void LoadServerSettings(ICoreAPI api) {
      var configSystem = api.ModLoader.GetModSystem<DoffAndDonConfigurationSystem>();
      if (configSystem == null) {
        api.Logger.Error("[{0}] {1} was not loaded. Using defaults.", nameof(OneWayArmorTransfer), nameof(DoffAndDonConfigurationSystem));
        LoadServerSettings(new DoffAndDonServerConfig());
        return;
      }

      LoadServerSettings(configSystem.ServerSettings);
    }

    protected abstract void LoadServerSettings(DoffAndDonServerConfig serverSettings);

    protected bool TransferArmor(IServerPlayer initiatingPlayer, EntityAgent doffer, EntityAgent donner = null, OnDoffWithoutDonner onDoffWithoutDonner = null, OnDonnedOneOrMore onDonnedOneOrMore = null) {
      if (doffer == null) { return false; }
      bool doffed = false;
      var dofferArmorSlots = doffer.GetArmorSlots();

      if (donner == null) {
        foreach (var slot in dofferArmorSlots) {
          if (slot.Empty) { continue; }
          doffed = onDoffWithoutDonner?.Invoke(initiatingPlayer, slot) ?? true || doffed;
        }
      }
      else {
        bool donnerDonned = false;
        var donnerArmorSlots = donner.GetArmorSlots();
        for (var i = 0; i < dofferArmorSlots.Count; i++) {
          var sourceSlot = dofferArmorSlots[i];
          if (sourceSlot.Empty) { continue; }

          var sinkSlot = donnerArmorSlots[i];
          if (sinkSlot != null && sourceSlot.TryPutInto(doffer.World, sinkSlot) > 0) {
            donnerDonned = true;
            sinkSlot.MarkDirty();
            doffed = true;
            System.Sounds.PlayArmorShufflingSounds(initiatingPlayer, sinkSlot.Itemstack.Item);
          }
          else {
            doffed = onDoffWithoutDonner?.Invoke(initiatingPlayer, sourceSlot) ?? true || doffed;
          }
        }
        if (donnerDonned) {
          onDonnedOneOrMore?.Invoke();
        }
      }
      return doffed;
    }

    // Tool transfer only available from armor stand to player
    protected bool TransferTool(IServerPlayer donningPlayer, EntityAgent armorStand, bool donOnlyToActiveHotbar, bool donOnlyToHotbar, OnDonnedOneOrMore onDonnedOneOrMore = null) {
      if (donningPlayer == null || armorStand == null || armorStand.RightHandItemSlot.Empty) {
        return false;
      }

      // skipSlots is an empty list instead of null due to a crash when in creative mode
      ItemSlot sinkSlot;
      if (donOnlyToActiveHotbar) {
        sinkSlot = donningPlayer.InventoryManager.ActiveHotbarSlot;
      }
      else if (donOnlyToHotbar) {
        sinkSlot = donningPlayer.InventoryManager.GetBestSuitedHotbarSlot(armorStand.RightHandItemSlot.Inventory, armorStand.RightHandItemSlot);
      }
      else {
        sinkSlot = donningPlayer.InventoryManager.GetBestSuitedSlot(armorStand.RightHandItemSlot, onlyPlayerInventory: true, skipSlots: new List<ItemSlot>());
      }

      if (sinkSlot != null && armorStand.RightHandItemSlot.TryPutInto(donningPlayer.Entity.World, sinkSlot) > 0) {
        sinkSlot.MarkDirty();
        onDonnedOneOrMore?.Invoke();
        return true;
      }

      return false;
    }

    // Sample OnDoffWithoutDonner implementation.
    // Keep the armor equipped to the doffer when it cannot be donned by the target
    protected bool KeepUndonnableOnDoff(IServerPlayer doffer, ItemSlot couldNotBeDonnedSlot) {
      return false; // A doff that fails this way does not count for saturation depletion.
    }

    // Sample OnDoffWithoutDonner implementation.
    // Drop armor that cannot be donned by the target when doffing.
    protected bool DropUndonnableOnDoff(IServerPlayer doffer, ItemSlot couldNotBeDonnedSlot) {
      if (doffer == null) return false;
      if (doffer.InventoryManager.DropItem(couldNotBeDonnedSlot, true)) {
        System.Sounds.PlayWooshSound(doffer);
        return true;
      }
      else {
        return false;
      }
    }
  }
}
