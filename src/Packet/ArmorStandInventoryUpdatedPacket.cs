using ProtoBuf;

namespace DoffAndDonAgain {
  [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
  public class ArmorStandInventoryUpdatedPacket {
    // EntityID of the armor stand the doffer is targeting, if any.
    public long ArmorStandEntityId = -1;

    public ArmorStandInventoryUpdatedPacket(long entityId) {
      this.ArmorStandEntityId = entityId;
    }

    public ArmorStandInventoryUpdatedPacket() {}
  }
}
