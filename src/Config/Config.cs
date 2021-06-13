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

    public static DoffAndDonAgainConfig Load(ICoreAPI api) {
      DoffAndDonAgainConfig config = null;
      try {
        config = api.LoadModConfig<DoffAndDonAgainConfig>(Constants.FILENAME);
      }
      catch (JsonReaderException e) {
        api.Logger.Error("Unable to parse config JSON. Correct syntax errors and retry, or delete {0} and load the world again to generate a new configuration file with default settings.", Constants.FILENAME);
        throw e;
      }
      catch (Exception e) {
        api.Logger.Error("I don't know what happened. Delete {0} in the config folder and try again.", Constants.FILENAME);
        throw e;
      }

      if (config == null) {
        api.Logger.Notification("{0} configuration file not found. Generating with default settings.", Constants.FILENAME);
        config = new DoffAndDonAgainConfig();
      }

      config.SaturationCostPerDoff = Math.Max(config.SaturationCostPerDoff, Constants.MIN_DOFF_COST);
      config.SaturationCostPerDon = Math.Max(config.SaturationCostPerDon, Constants.MIN_DON_COST);

      config.HandsNeededToDoff = GameMath.Clamp(config.HandsNeededToDoff, Constants.MIN_HANDS_FREE, Constants.MAX_HANDS_FREE);

      Save(api, config);
      return config;
    }

    public static void Save(ICoreAPI api, DoffAndDonAgainConfig config) {
      api.StoreModConfig(config, Constants.FILENAME);
    }
  }
}
