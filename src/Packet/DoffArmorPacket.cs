using ProtoBuf;

namespace ShakeItDoff {
  [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
  public class DoffArmorPacket {
    // EntityID of the armor stand the doffer is targeting, if any.
    public long? ArmorStandEntityId;
  }
}
