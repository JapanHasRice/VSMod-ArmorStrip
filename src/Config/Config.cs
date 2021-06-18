using System;
using Newtonsoft.Json;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace DoffAndDonAgain.Config {
  public class DoffAndDonAgainConfig {
    public string GameplaySectionTitle = "=== Gameplay Settings ===";

    public string SaturationCostPerDoffDescription = Constants.SaturationCostPerDoffDescription;
    public float SaturationCostPerDoff = Constants.DEFAULT_DOFF_COST;

    public string SaturationCostPerDonDescription = Constants.SaturationCostPerDonDescription;
    public float SaturationCostPerDon = Constants.DEFAULT_DON_COST;

    public string HandsNeededToDoffDescription = Constants.HandsNeededToDoffDescription;
    public int HandsNeededToDoff = Constants.DEFAULT_HANDS_FREE;

    public string DropArmorWhenDoffingToStandDescription = Constants.DropArmorWhenDoffingToStandDescription;
    public bool DropArmorWhenDoffingToStand = Constants.DEFAULT_DROP_ON_STAND_DOFF;

    public static DoffAndDonAgainConfig LoadOrCreateDefault(ICoreAPI api) {
      DoffAndDonAgainConfig config = TryLoadModConfig(api, Constants.FILENAME);

      if (config == null) {
        api.Logger.Notification("{0} configuration file not found. Generating with default settings.", Constants.FILENAME);
        config = new DoffAndDonAgainConfig();
      }

      // Saving here either places the newly generated config file in the ModConfig folder
      // or updates the existing configuration file with new/removed settings
      Save(api, config, Constants.FILENAME);

      Clamp(config);

      return config;
    }

    // Throws exception if the config file exists, but had parsing errors.
    // Returns null if no config file exists.
    private static DoffAndDonAgainConfig TryLoadModConfig(ICoreAPI api, string filename) {
      DoffAndDonAgainConfig config = null;
      try {
        config = api.LoadModConfig<DoffAndDonAgainConfig>(filename);
      }
      catch (JsonReaderException e) {
        api.Logger.Error("Unable to parse config JSON. Correct syntax errors and retry, or delete {0} and load the world again to generate a new configuration file with default settings.", filename);
        throw e;
      }
      catch (Exception e) {
        api.Logger.Error("I don't know what happened. Delete {0} in the config folder and try again.", filename);
        throw e;
      }
      return config;
    }

    public static void Clamp(DoffAndDonAgainConfig config) {
      if (config == null) { return; }
      config.SaturationCostPerDoff = Math.Max(config.SaturationCostPerDoff, Constants.MIN_DOFF_COST);
      config.SaturationCostPerDon = Math.Max(config.SaturationCostPerDon, Constants.MIN_DON_COST);

      config.HandsNeededToDoff = GameMath.Clamp(config.HandsNeededToDoff, Constants.MIN_HANDS_FREE, Constants.MAX_HANDS_FREE);
    }

    public static void Save(ICoreAPI api, DoffAndDonAgainConfig config, string filename) {
      api.StoreModConfig(config, filename);
    }
  }
}
