using ProtoBuf;

namespace ArmorStrip {
  [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
  public class DropAllArmorPacket {
    public long? ArmorStandEntityId;
  }
}
