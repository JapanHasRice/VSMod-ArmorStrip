using Vintagestory.API.Common;

namespace DoffAndDonAgain.Common {
  public class SoundManager {
    private DoffAndDonSystem System { get; }

    public SoundManager(DoffAndDonSystem system) {
      System = system;

      system.Event.OnAfterServerHandledRequest += OnAfterServerHandledRequest;
    }

    private void OnAfterServerHandledRequest(ArmorActionEventArgs eventArgs) {
      if (!eventArgs.Successful) {
        return;
      }

      if (eventArgs.MovedArmor.Count > 0) {
        PlayArmorShufflingSounds(eventArgs);
      }
    }

    private void PlayArmorShufflingSounds(ArmorActionEventArgs eventArgs) {
      int delayMillis = 0;
      foreach (var wearable in eventArgs.MovedArmor) {
        if ((wearable.FootStepSounds?.Length ?? 0) == 0) {
          continue;
        }

        var sound = wearable.FootStepSounds[System.Api.World.Rand.Next(wearable.FootStepSounds.Length)];
        if (delayMillis > 0) {
          System.Api.World.RegisterCallback((dt) => { PlaySoundAt(sound, eventArgs.ForPlayer, range: 10); }, delayMillis);
          delayMillis += 100;
        }
        else {
          PlaySoundAt(sound, eventArgs.ForPlayer, range: 10);
          delayMillis += 300;
        }
      }

      foreach (var wearable in eventArgs.DroppedArmor) {
        if ((wearable.FootStepSounds?.Length ?? 0) == 0) {
          continue;
        }

        var sound = wearable.FootStepSounds[System.Api.World.Rand.Next(wearable.FootStepSounds.Length)];
        PlaySoundAt(sound, eventArgs.ForPlayer, range: 10);
      }
      if (eventArgs.DroppedArmor.Count > 0) {
        System.Api.World.RegisterCallback((dt) => PlayWooshSound(eventArgs.ForPlayer), 0);
      }
    }

    public void PlayWooshSound(IPlayer player) {
      PlaySoundAt(new AssetLocation("sounds/player/quickthrow"), player, range: 10);
    }

    private void PlaySoundAt(AssetLocation sound, IPlayer player, bool randomizePitch = true, float range = 32, float volume = 1) {
      System.Api.World.PlaySoundAt(sound, player.Entity, randomizePitch: randomizePitch, range: range, volume: volume);
    }
  }
}
