using System.Threading.Tasks;
using EnumExtensions.Settings;
using Sandbox.Audio;

public sealed class MusicManager : BaseSoundComponent {
	[Property] public float SoundLength = 238.21062f;

	public RealTimeSince Duration;
	private SoundHandle TempSoundHandle;

	[Property] [Group("Sound")] public float RepeatOffset { get; set; }


	protected override void OnDestroy() {
		SoundHandle.Stop();
		base.OnDestroy();
	}

	[Broadcast]
	public override void StopSound() {
		if (TempSoundHandle != null) {
			TempSoundHandle.Stop();
		}

		SoundHandle.Stop();
	}

	protected override Task OnLoad() {
		foreach (var sound in SoundEvent.Sounds) sound?.Preload();

		return base.OnLoad();
	}


	protected override void OnUpdate() {
		if (!UserSettingsSystem.Loaded) {
			UserSettingsSystem.Load();
			return;
		}

		if (SoundHandle.IsValid() && SoundLength <= Duration.Relative + RepeatOffset) {
			Duration = 0;
			TempSoundHandle = Sound.Play(SoundEvent);
			TempSoundHandle.TargetMixer = Mixer.FindMixerByName("music");
			UserSettingsSystem.Load();
		}

		if (SoundHandle.IsValid() && SoundHandle.IsPlaying) {
			return;
		}

		if (!SoundHandle.IsValid() && !TempSoundHandle.IsValid()) {
			Duration = 0;
			SoundHandle = Sound.Play(SoundEvent);
			SoundHandle.TargetMixer = Mixer.FindMixerByName("music");
			UserSettingsSystem.Load();
		}

		if (SoundHandle is { IsPlaying: false }) {
			SoundHandle = TempSoundHandle;
			TempSoundHandle = null;
		}
	}
}
