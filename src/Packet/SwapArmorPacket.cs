using ProtoBuf;

namespace DoffAndDonAgain {
  [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
  public class SwapArmorPacket {
    // EntityID of the armor stand the doffer is targeting, if any.
    public long ArmorStandEntityId;

    public SwapArmorPacket(long entityId) {
      this.ArmorStandEntityId = entityId;
    }

    public SwapArmorPacket() { }
  }
}
