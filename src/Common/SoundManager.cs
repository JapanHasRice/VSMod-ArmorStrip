using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace DoffAndDonAgain.Common {
  public class SoundManager {
    private DoffAndDonSystem System { get; }

    public SoundManager(DoffAndDonSystem system) {
      System = system;
    }

    public void PlayArmorShufflingSounds(IPlayer player, Item movedArmor, Item otherMovedArmor = null) {
      bool playedFirstSound = false;
      AssetLocation[] wearableSounds = (movedArmor as ItemWearable)?.FootStepSounds;
      if (wearableSounds != null && wearableSounds.Length != 0) {
        AssetLocation armorSound = wearableSounds[System.Api.World.Rand.Next(wearableSounds.Length)];
        PlaySoundAt(armorSound, player, range: 10);
        playedFirstSound = true;
      }
      AssetLocation[] otherWearableSounds = (otherMovedArmor as ItemWearable)?.FootStepSounds;
      if (otherWearableSounds != null && otherWearableSounds.Length != 0) {
        AssetLocation armorSound = otherWearableSounds[System.Api.World.Rand.Next(otherWearableSounds.Length)];
        if (playedFirstSound) {
          System.Api.World.RegisterCallback((float dt) => { PlaySoundAt(armorSound, player, range: 10); }, 300);
        }
        else {
          PlaySoundAt(armorSound, player, range: 10);
        }
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
