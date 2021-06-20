using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace DoffAndDonAgain {
  public class DoffAndDonAgainServerMod : DoffAndDonAgainMod {
    #region Variables and Properties
    protected ICoreServerAPI ServerAPI;
    protected IServerNetworkChannel ServerChannel;

    #endregion

    #region Delegates
    protected delegate void OnDonnedOneOrMore();

    // Return true to indicate a successful doffing.
    protected delegate bool OnDoffWithoutDonner(IServerPlayer player, ItemSlot couldNotBeDonnedSlot);
    #endregion

    #region Initialization
    public override EnumAppSide ForSide() {
      return EnumAppSide.Server;
    }

    public override void Start(ICoreAPI api) {
      base.Start(api);
      ServerAPI = api as ICoreServerAPI;
      ServerChannel = Channel as IServerNetworkChannel;

      LoadServerConfigs();
      SetupServerNetwork();
    }

    private void LoadServerConfigs() {
      // var config = DoffAndDonAgainConfig.LoadOrCreateDefault(Api);
    }

    private void SetupServerNetwork() {
      ServerChannel
        .SetMessageHandler<DoffArmorPacket>(OnDoff)
        .SetMessageHandler<DonArmorPacket>(OnDon)
        .SetMessageHandler<SwapArmorPacket>(OnSwap);
    }

    #endregion

    protected void BroadcastArmorStandUpdated(EntityArmorStand armorStand) {
      if (armorStand == null || armorStand.World?.Side != EnumAppSide.Server) { return; }

      armorStand.WatchedAttributes.MarkAllDirty();
      ServerAPI.World.RegisterCallback((IWorldAccessor world, BlockPos pos, float dt) => {
        ServerChannel.BroadcastPacket(new ArmorStandInventoryUpdatedPacket(armorStand.EntityId));
      }, armorStand.Pos.AsBlockPos, 500);
    }

    protected void Doff(IServerPlayer doffer, EntityArmorStand armorStand) {
      OnDoffWithoutDonner dropOrKeepItem = null;
      if (!DropArmorWhenDoffingToStand && armorStand != null) {
        dropOrKeepItem = KeepUndonnableOnDoff;
      }
      else {
        dropOrKeepItem = DropUndonnableOnDoff;
      }
      OnDonnedOneOrMore updateArmorStandRender = () => { BroadcastArmorStandUpdated(armorStand); };
      bool doffed = Doff(initiatingPlayer: doffer,
                         doffer: doffer.Entity,
                         donner: armorStand,
                         onDoffWithoutDonner: dropOrKeepItem,
                         onDonnedOneOrMore: updateArmorStandRender);

      if (doffed) { OnSuccessfulDoff(doffer); }
    }

    protected bool Doff(IServerPlayer initiatingPlayer, EntityAgent doffer, EntityAgent donner = null, OnDoffWithoutDonner onDoffWithoutDonner = null, OnDonnedOneOrMore onDonnedOneOrMore = null) {
      if (doffer == null) { return false; }
      bool doffed = false;

      if (donner == null) {
        foreach (var slot in doffer.GetFilledArmorSlots()) {
          if (slot.Empty) { continue; }
          doffed = onDoffWithoutDonner?.Invoke(initiatingPlayer, slot) ?? true || doffed;
        }
      }
      else {
        bool donnerDonned = false;
        foreach (var slot in doffer.GetFilledArmorSlots()) {
          if (slot.Empty) { continue; }

          ItemSlot sinkSlot = GetAvailableSlotOn(donner, slot);
          if (sinkSlot != null && slot.TryPutInto(doffer.World, sinkSlot) > 0) {
            donnerDonned = true;
            sinkSlot.MarkDirty();
            doffed = true;
          }
          else {
            doffed = onDoffWithoutDonner?.Invoke(initiatingPlayer, slot) ?? true || doffed;
          }
        }
        if (donnerDonned) {
          onDonnedOneOrMore?.Invoke();
        }
      }
      return doffed;
    }

    protected void Don(IServerPlayer donner, EntityArmorStand armorStand) {
      OnDonnedOneOrMore updateArmorStandRender = () => { BroadcastArmorStandUpdated(armorStand); };
      bool donned = Doff(initiatingPlayer: donner,
                         doffer: armorStand,
                         donner: donner.Entity,
                         onDoffWithoutDonner: KeepUndonnableOnDoff,
                         onDonnedOneOrMore: updateArmorStandRender);

      if (donned) { OnSuccessfulDon(donner); }
    }

    protected bool DropUndonnableOnDoff(IServerPlayer doffer, ItemSlot couldNotBeDonnedSlot) {
      if (doffer == null) return false;
      return doffer.InventoryManager.DropItem(couldNotBeDonnedSlot, true);
    }

    protected ItemSlot GetAvailableSlotOn(EntityAgent entityAgent, ItemSlot sourceSlot) {
      // Do not use IInventory#GetBestSuitedSlot because it always returns a null slot for the character's gear inventory
      foreach (var slot in entityAgent.GearInventory) {
        if (slot.CanHold(sourceSlot)) {
          return slot.Empty ? slot : null;
        }
      }
      return null;
    }

    protected bool KeepUndonnableOnDoff(IServerPlayer doffer, ItemSlot couldNotBeDonnedSlot) {
      return false; // False so that a doff that fails this way does not count for saturation depletion.
    }

    protected void OnDoff(IServerPlayer doffer, DoffArmorPacket packet) {
      Doff(doffer, GetEntityArmorStandById(doffer.Entity, packet.ArmorStandEntityId));
    }

    protected void OnDon(IServerPlayer donner, DonArmorPacket packet) {
      Don(donner, GetEntityArmorStandById(donner.Entity, packet.ArmorStandEntityId));
    }

    protected void OnSwap(IServerPlayer swapper, SwapArmorPacket packet) {
      Swap(swapper, GetEntityArmorStandById(swapper.Entity, packet.ArmorStandEntityId));
    }

    protected void OnSuccessfulDoff(IServerPlayer doffer) {
      doffer.Entity.GetBehavior<EntityBehaviorHunger>()?.ConsumeSaturation(SaturationCostPerDoff);
    }

    protected void OnSuccessfulDon(IServerPlayer donner) {
      donner.Entity.GetBehavior<EntityBehaviorHunger>()?.ConsumeSaturation(SaturationCostPerDon);
    }

    protected void OnSuccessfulSwap(IServerPlayer swapper) {
      swapper.Entity.GetBehavior<EntityBehaviorHunger>()?.ConsumeSaturation(SaturationCostPerSwap);
    }

    protected void Swap(IServerPlayer swapper, EntityArmorStand armorStand) {
      if (swapper == null || armorStand == null) { return; }
      bool swapped = false;

      var playerArmorSlots = swapper.Entity.GetArmorSlots();
      var armorStandArmorSlots = armorStand.GetArmorSlots();
      for (int i = 0; i < playerArmorSlots.Count; i++) {
        if (playerArmorSlots[i].Empty && armorStandArmorSlots[i].Empty) { continue; }
        swapped = playerArmorSlots[i].TryFlipWith(armorStandArmorSlots[i]) || swapped;
      }

      if (swapped) {
        OnSuccessfulSwap(swapper);
        BroadcastArmorStandUpdated(armorStand);
      }
    }

    protected void TriggerError(IServerPlayer player, string errorCode, string errorFallbackText) {
      player?.SendIngameError(errorCode, GetErrorText(errorCode, errorFallbackText));
    }
  }
}
