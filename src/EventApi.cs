using System;
using System.Collections.Generic;
using ProtoBuf;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace DoffAndDonAgain {
  public enum EnumActionType {
    Doff,
    Don,
    Swap
  }

  public enum EnumTargetType {
    NotSet,
    Nothing,
    ArmorStand
  }

  [ProtoContract]
  public class ArmorActionEventArgs : EventArgs {
    public KeyCombination KeyCombination { get; }

    [ProtoMember(1)]
    public EnumActionType ActionType { get; private set; }

    [ProtoMember(2)]
    public EnumTargetType TargetType { get; set; } = EnumTargetType.NotSet;

    [ProtoMember(3)]
    public long? ArmorStandEntityId { get; set; } = null;

    [ProtoMember(4)]
    public bool Successful { get; set; } = false;

    [ProtoMember(5)]
    public string ErrorCode { get; set; } = "";

    [ProtoMember(6)]
    public string[] ErrorArgs { get; set; } = new string[0];

    [ProtoMember(7)]
    public bool DoffExcessToGround { get; set; } = false;

    public IServerPlayer ForPlayer { get; set; } = null;

    public List<ItemWearable> MovedArmor { get; } = new List<ItemWearable>();

    public List<ItemWearable> DroppedArmor { get; } = new List<ItemWearable>();

    public ArmorActionEventArgs(KeyCombination keyCombination, EnumActionType actionType) {
      KeyCombination = keyCombination;
      ActionType = actionType;
    }

    private ArmorActionEventArgs() { }
  }

  public delegate void ActionWithRefArg<T>(ref T t1);

  public class DoffAndDonEventApi {
    public event ActionWithRefArg<ArmorActionEventArgs> OnDoffKeyPressed;
    public bool TriggerDoffKeyPressed(KeyCombination keyCombination) {
      var eventArgs = new ArmorActionEventArgs(keyCombination, EnumActionType.Doff);
      OnDoffKeyPressed?.Invoke(ref eventArgs);
      TriggerAfterInput(eventArgs);
      return eventArgs.Successful;
    }

    public event ActionWithRefArg<ArmorActionEventArgs> OnDonKeyPressed;
    public bool TriggerDonKeyPressed(KeyCombination keyCombination) {
      var eventArgs = new ArmorActionEventArgs(keyCombination, EnumActionType.Don);
      OnDonKeyPressed?.Invoke(ref eventArgs);
      TriggerAfterInput(eventArgs);
      return eventArgs.Successful;
    }

    public event ActionWithRefArg<ArmorActionEventArgs> OnSwapKeyPressed;
    public bool TriggerSwapKeyPressed(KeyCombination keyCombination) {
      var eventArgs = new ArmorActionEventArgs(keyCombination, EnumActionType.Swap);
      OnSwapKeyPressed?.Invoke(ref eventArgs);
      TriggerAfterInput(eventArgs);
      return eventArgs.Successful;
    }

    public event Action<ArmorActionEventArgs> OnAfterInput;
    public void TriggerAfterInput(ArmorActionEventArgs eventArgs) {
      OnAfterInput?.Invoke(eventArgs);
    }

    public event ActionWithRefArg<ArmorActionEventArgs> OnServerReceivedDoffRequest;
    public void TriggerServerReceivedDoffRequest(ArmorActionEventArgs eventArgs) {
      OnServerReceivedDoffRequest?.Invoke(ref eventArgs);
      TriggerAfterServerHandledRequest(eventArgs);
    }

    public event ActionWithRefArg<ArmorActionEventArgs> OnServerReceivedDonRequest;
    public void TriggerServerReceivedDonRequest(ArmorActionEventArgs eventArgs) {
      OnServerReceivedDonRequest?.Invoke(ref eventArgs);
      TriggerAfterServerHandledRequest(eventArgs);
    }

    public event ActionWithRefArg<ArmorActionEventArgs> OnServerReceivedSwapRequest;
    public void TriggerServerReceivedSwapRequest(ArmorActionEventArgs eventArgs) {
      OnServerReceivedSwapRequest?.Invoke(ref eventArgs);
      TriggerAfterServerHandledRequest(eventArgs);
    }

    public event Action<ArmorActionEventArgs> OnAfterServerHandledRequest;
    public void TriggerAfterServerHandledRequest(ArmorActionEventArgs eventArgs) {
      OnAfterServerHandledRequest?.Invoke(eventArgs);
    }
  }
}
