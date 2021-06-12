using System;
using Newtonsoft.Json;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace ShakeItDoff {
  public class ShakeItDoffConfig {
    public string GameplaySectionTitle = "=== Gameplay Settings ===";

    private const float DEFAULT_DOFF_COST = 20;
    private const float MIN_DOFF_COST = 0;
    public string SaturationCostPerDoffDescription = $"Satiety required to quickly remove all of your armor. [Default: {DEFAULT_DOFF_COST}, Min: {MIN_DOFF_COST}]";
    public float SaturationCostPerDoff = DEFAULT_DOFF_COST;

    private const int DEFAULT_HANDS_FREE = 2;
    private const int MIN_HANDS_FREE = 0;
    private const int MAX_HANDS_FREE = 2;
    public string HandsNeededToDoffDescription = $"Number of available (empty) hands needed to quickly remove all of your armor. [Default: {DEFAULT_HANDS_FREE}, Min: {MIN_HANDS_FREE}, Max: {MAX_HANDS_FREE}]";
    public int HandsNeededToDoff = DEFAULT_HANDS_FREE;


    public const string filename = "ShakeItDoffConfig.json";
    public static ShakeItDoffConfig Load(ICoreAPI api) {
      ShakeItDoffConfig config = null;
      try {
        config = api.LoadModConfig<ShakeItDoffConfig>(filename);
      }
      catch (JsonReaderException e) {
        api.Logger.Error("Unable to parse config JSON. Correct syntax errors and retry, or delete {0} and load the world again to generate a new configuration file with default settings.", filename);
        throw e;
      }
      catch (Exception e) {
        api.Logger.Error("I don't know what happened. Delete {0} in the config folder and try again.", filename);
        throw e;
      }

      if (config == null) {
        api.Logger.Notification("{0} configuration file not found. Generating with default settings.", filename);
        config = new ShakeItDoffConfig();
      }

      config.SaturationCostPerDoff = Math.Max(config.SaturationCostPerDoff, MIN_DOFF_COST);

      config.HandsNeededToDoff = GameMath.Clamp(config.HandsNeededToDoff, MIN_HANDS_FREE, MAX_HANDS_FREE);

      Save(api, config);
      return config;
    }

    public static void Save(ICoreAPI api, ShakeItDoffConfig config) {
      api.StoreModConfig(config, filename);
    }
  }
}
