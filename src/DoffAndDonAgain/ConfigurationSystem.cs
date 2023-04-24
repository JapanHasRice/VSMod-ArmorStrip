using Newtonsoft.Json;
using ProtoBuf;
using RiceConfig;

namespace DoffAndDonAgain {
  public class ConfigSystem : RiceConfig.ClientOnlyConfigurationSystem<ClientSettings> {
    public override string ChannelName => "japanhasrice.doffanddonagainconfig";
    public override string ClientConfigFilename => "DoffAndDonAgain/ClientConfig.json";
  }

  [ProtoContract]
  public class ClientSettings : RiceConfig.ClientConfig {
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
  }
}
