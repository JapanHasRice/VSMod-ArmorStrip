using Newtonsoft.Json;
using RiceConfig;

namespace DoffAndDonAgain {
  public class ConfigSystem : ClientOnlyConfigurationSystem<ClientSettings> {
    public override string ChannelName => "japanhasrice.doffanddonagainconfig";
    public override string ClientConfigFilename => "DoffAndDonAgain/ClientConfig.json";
  }

  public class ClientSettings : ClientConfig {
    // GENERAL
    [JsonProperty, JsonConverter(typeof(SettingConverter<int>))]
    public Setting<int> HandsNeeded { get; set; } = new Setting<int> {
      Default = 0,
      Min = 0,
      Max = 2,
      Description = "Number of empty hands required to perform a Doff, Don, or Swap. The greatest value between the client and server configs will be used."
    };

    [JsonProperty, JsonConverter(typeof(SettingConverter<float>))]
    public Setting<float> SaturationCost { get; set; } = new Setting<float> {
      Default = 0f,
      Min = 0f,
      Description = "Amount of satiety consumed when performing a Doff, Don, or Swap. The greatest value between the client and server configs will be used."
    };

    // DOFF
    [JsonProperty, JsonConverter(typeof(SettingConverter<bool>))]
    public Setting<bool> DoffArmorToGround { get; set; } = new Setting<bool> {
      Default = true,
      Description = "If disabled, equipped armor will not be dropped to the ground when doffing without a target. If enabled, follows the Server's settings."
    };

    [JsonProperty, JsonConverter(typeof(SettingConverter<bool>))]
    public Setting<bool> DoffClothingToGround { get; set; } = new Setting<bool> {
      Default = false,
      Description = "If disabled, equipped clothing will not be dropped to the ground when doffing without a target. If enabled, follows the Server's settings."
    };

    [JsonProperty, JsonConverter(typeof(SettingConverter<bool>))]
    public Setting<bool> DoffArmorToEntities { get; set; } = new Setting<bool> {
      Default = true,
      Description = "If disabled, equipped armor will not be doffed to entities. If enabled, follows the Server's settings."
    };

    [JsonProperty, JsonConverter(typeof(SettingConverter<bool>))]
    public Setting<bool> DoffClothingToEntities { get; set; } = new Setting<bool> {
      Default = true,
      Description = "If disabled, equipped clothing will not be doffed to entities. If enabled, follows the Server's settings."
    };

    [JsonProperty, JsonConverter(typeof(SettingConverter<bool>))]
    public Setting<bool> DropUnplaceableArmor { get; set; } = new Setting<bool> {
      Default = false,
      Description = "If disabled, equipped armor that cannot be doffed to an entity will be dropped onto the ground. If enabled, follows the Server's settings."
    };

    [JsonProperty, JsonConverter(typeof(SettingConverter<bool>))]
    public Setting<bool> DropUnplaceableClothing { get; set; } = new Setting<bool> {
      Default = false,
      Description = "If disabled, equipped clothing that cannot be doffed to an entity will be dropped onto the ground. If enabled, follows the Server's settings."
    };

    // DON
    [JsonProperty, JsonConverter(typeof(SettingConverter<bool>))]
    public Setting<bool> DonArmorFromEntities { get; set; } = new Setting<bool> {
      Default = true,
      Description = "If disabled, armor will not be donned from entities. If enabled, follows the Server's settings."
    };

    [JsonProperty, JsonConverter(typeof(SettingConverter<bool>))]
    public Setting<bool> DonClothingFromEntities { get; set; } = new Setting<bool> {
      Default = true,
      Description = "If disabled, clothing will not be donned from entities. If enabled, follows the Server's settings."
    };

    [JsonProperty, JsonConverter(typeof(SettingConverter<bool>))]
    public Setting<bool> DonMiscFromEntities { get; set; } = new Setting<bool> {
      Default = true,
      Description = "If disabled, misc. items will not be donned from entities. If enabled, follows the Server's settings."
    };

    [JsonProperty, JsonConverter(typeof(SettingConverter<bool>))]
    public Setting<bool> DonMiscOnlyToActiveHotbar { get; set; } = new Setting<bool> {
      Default = false,
      Description = "If enabled, misc. items donned from entities can only be placed in the currently active hotbar slot."
    };

    [JsonProperty, JsonConverter(typeof(SettingConverter<bool>))]
    public Setting<bool> DonMiscOnlyToHotbar { get; set; } = new Setting<bool> {
      Default = true,
      Description = "If enabled, misc. items donned from entities can only be placed in an available hotbar slot."
    };

    // SWAP
    [JsonProperty, JsonConverter(typeof(SettingConverter<bool>))]
    public Setting<bool> SwapArmorWithEntities { get; set; } = new Setting<bool> {
      Default = true,
      Description = "If disabled, armor will not be swapped targeted entities. If enabled, follows the Server's settings."
    };

    [JsonProperty, JsonConverter(typeof(SettingConverter<bool>))]
    public Setting<bool> SwapClothingWithEntities { get; set; } = new Setting<bool> {
      Default = true,
      Description = "If disabled, clothing will not be swapped with targeted entities. If enabled, follows the Server's settings."
    };
  }
}
