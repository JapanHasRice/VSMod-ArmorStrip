using ProtoBuf;

namespace DoffAndDonAgain.Common.Network {
  [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
  public class DoffArmorPacket {
    // EntityID of the armor stand the doffer is targeting, if any.
    public long? ArmorStandEntityId;

    public DoffArmorPacket(long? entityId) {
      this.ArmorStandEntityId = entityId;
    }

    public DoffArmorPacket() { }
  }
}
