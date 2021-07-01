using ProtoBuf;

namespace DoffAndDonAgain.Common.Network {
  [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
  public class DonArmorPacket {
    // EntityID of the armor stand the doffer is targeting
    public long ArmorStandEntityId;

    public DonArmorPacket(long entityId) {
      this.ArmorStandEntityId = entityId;
    }

    public DonArmorPacket() { }
  }
}
