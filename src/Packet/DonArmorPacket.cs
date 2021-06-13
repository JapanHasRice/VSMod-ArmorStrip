using ProtoBuf;

namespace DoffAndDonAgain {
  [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
  public class DonArmorPacket {
    // EntityID of the armor stand the doffer is targeting, if any.
    public long? ArmorStandEntityId;

    public DonArmorPacket(long? entityId) {
      this.ArmorStandEntityId = entityId;
    }

    public DonArmorPacket() {}
  }
}
