using System;
using System.Collections.Generic;
using ProtoBuf;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace DoffAndDonAgain.Common {
  public enum EnumActionType {
    Doff,
    Don,
    Swap
  }

  public enum EnumTargetType {
    NotSet,
    Nothing,
    EntityAgent
  }

  [ProtoContract]
  public class DoffAndDonEventArgs : EventArgs {
    public KeyCombination KeyCombination { get; }

    [ProtoMember(1)]
    public EnumActionType ActionType { get; private set; }

    [ProtoMember(2)]
    public EnumTargetType TargetType { get; set; } = EnumTargetType.NotSet;

    [ProtoMember(3)]
    public long? TargetEntityAgentId { get; set; } = null;

    [ProtoMember(4)]
    public bool Successful { get; set; } = false;

    [ProtoMember(5)]
    public int[] ClientArmorSlotIds { get; set; } = new int[0];

    [ProtoMember(6)]
    public int[] ClientClothingSlotIds { get; set; } = new int[0];

    [ProtoMember(7)]
    public int[] ClientMiscDonFromSlotIds { get; set; } = new int[0];

    public string ErrorCode { get; set; } = "";

    public string[] ErrorArgs { get; set; } = new string[0];

    public IServerPlayer ForPlayer { get; set; } = null;

    public List<ItemWearable> MovedArmor { get; } = new List<ItemWearable>();

    public List<ItemWearable> DroppedArmor { get; } = new List<ItemWearable>();

    public DoffAndDonEventArgs(KeyCombination keyCombination, EnumActionType actionType) {
      KeyCombination = keyCombination;
      ActionType = actionType;
    }

    private DoffAndDonEventArgs() { }
  }
}
