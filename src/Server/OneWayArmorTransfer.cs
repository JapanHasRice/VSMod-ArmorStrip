using DoffAndDonAgain.Utility;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace DoffAndDonAgain.Server {
  public abstract class OneWayArmorTransfer {
    // Return true to indicate a successful doffing.
    protected delegate bool OnDoffWithoutDonner(IServerPlayer player, ItemSlot couldNotBeDonnedSlot);

    protected delegate void OnDonnedOneOrMore();

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
        doffer.Entity.World.PlaySoundAt(new AssetLocation("sounds/player/quickthrow"), doffer.Entity, randomizePitch: true, range: 10);
        return true;
      }
      else {
        return false;
      }
    }
  }
}
