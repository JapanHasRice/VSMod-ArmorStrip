using Newtonsoft.Json;
using ProtoBuf;
using RiceConfig;

namespace DoffAndDonAgain {
  public class DoffAndDonConfigurationSystem : ServerOnlyConfigurationSystem<DoffAndDonServerConfig> {
    public override string ChannelName => "japanhasrice.doffanddonagainconfig";

    public override string ServerConfigFilename => "DoffAndDonAgain/ServerConfig.json";
  }

  [ProtoContract]
  public class DoffAndDonServerConfig : ServerConfig {
    public string GameplaySectionTitle { get; } = "=== Gameplay Settings ===";

    public string DoffConfigOptions { get; } = "--- Doff: Remove armor ---";

    [ProtoMember(1)]
    [JsonProperty, JsonConverter(typeof(SettingConverter<bool>))]
    public Setting<bool> EnableDoffToGround { get; set; } = new Setting<bool> {
      Default = true,
      Description = "If enabled, player can quickly unequip armor and throw it on the ground."
    };

    [ProtoMember(2)]
    [JsonProperty, JsonConverter(typeof(SettingConverter<bool>))]
    public Setting<bool> EnableDoffToArmorStand { get; set; } = new Setting<bool> {
      Default = true,
      Description = "If enabled, player can quickly unequip armor, placing it on the currently targeted armor stand."
    };

    [ProtoMember(3)]
    [JsonProperty, JsonConverter(typeof(SettingConverter<float>))]
    public Setting<float> SaturationCostPerDoff { get; set; } = new Setting<float> {
      Default = 0.0f,
      Min = 0.0f,
      Description = "Satiety required to doff."
    };

    [ProtoMember(4)]
    [JsonProperty, JsonConverter(typeof(SettingConverter<int>))]
    public Setting<int> HandsNeededToDoff { get; set; } = new Setting<int> {
      Default = 2,
      Min = 0,
      Max = 2,
      Description = "Number of available (empty) hands needed to doff."
    };

    [JsonProperty, JsonConverter(typeof(SettingConverter<bool>))]
    public Setting<bool> DropArmorWhenDoffingToStand { get; set; } = new Setting<bool> {
      Default = false,
      Description = "If enabled, when doffing to an armor stand, any armor that cannot be placed on the stand is dropped to the ground as if the player had doffed without an armor stand."
    };


    public string DonConfigOptions { get; } = "--- Don: Equip armor from an armor stand ---";

    [ProtoMember(5)]
    [JsonProperty, JsonConverter(typeof(SettingConverter<bool>))]
    public Setting<bool> EnableDon { get; set; } = new Setting<bool> {
      Default = true,
      Description = "If enabled, player can quickly equip armor from an armor stand they are currently targeting."
    };

    [ProtoMember(6)]
    [JsonProperty, JsonConverter(typeof(SettingConverter<float>))]
    public Setting<float> SaturationCostPerDon { get; set; } = new Setting<float> {
      Default = 0.0f,
      Min = 0.0f,
      Description = "Satiety required to don armor from an armor stand."
    };

    [ProtoMember(7)]
    [JsonProperty, JsonConverter(typeof(SettingConverter<int>))]
    public Setting<int> HandsNeededToDon { get; set; } = new Setting<int> {
      Default = 2,
      Min = 0,
      Max = 2,
      Description = "Number of available (empty) hands needed to don."
    };

    [JsonProperty, JsonConverter(typeof(SettingConverter<bool>))]
    public Setting<bool> EnableToolDonning { get; set; } = new Setting<bool> {
      Default = true,
      Description = "If enabled, when targeted armor stand has an equipped tool, player will attempt to take the tool into their inventory. See additional options below for rules on how the tool can be placed in the player's inventory, and be mindful of the empty hands setting."
    };

    [JsonProperty, JsonConverter(typeof(SettingConverter<bool>))]
    public Setting<bool> DonToolOnlyToActiveHotbar { get; set; } = new Setting<bool> {
      Default = true,
      Description = "If enabled, tools donned from armor stands will only be placed in the currently active hotbar slot."
    };

    [JsonProperty, JsonConverter(typeof(SettingConverter<bool>))]
    public Setting<bool> DonToolOnlyToHotbar { get; set; } = new Setting<bool> {
      Default = true,
      Description = "If enabled, tools donned from armor stands will only be placed in an available hotbar slot."
    };


    public string SwapConfigOptions { get; } = "--- Swap: Exchange armor with an armor stand ---";

    [ProtoMember(8)]
    [JsonProperty, JsonConverter(typeof(SettingConverter<bool>))]
    public Setting<bool> EnableSwap { get; set; } = new Setting<bool> {
      Default = true,
      Description = "If enabled, player can quickly swap armor with an armor stand they are currently targeting."
    };

    [ProtoMember(9)]
    [JsonProperty, JsonConverter(typeof(SettingConverter<float>))]
    public Setting<float> SaturationCostPerSwap { get; set; } = new Setting<float> {
      Default = 0.0f,
      Min = 0.0f,
      Description = "Satiety required to swap armor with an armor stand."
    };

    [ProtoMember(10)]
    [JsonProperty, JsonConverter(typeof(SettingConverter<int>))]
    public Setting<int> HandsNeededToSwap { get; set; } = new Setting<int> {
      Default = 2,
      Min = 0,
      Max = 2,
      Description = "Number of available (empty) hands needed to swap."
    };
  }
}
