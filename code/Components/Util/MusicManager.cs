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
			return;
		}

		if (SoundHandle is not null) ApplyOverrides(SoundHandle);

		if (TempSoundHandle is not null) ApplyOverrides(TempSoundHandle);

		if (SoundHandle.IsValid() && SoundLength <= Duration.Relative + RepeatOffset) {
			//SoundEvent.Volume = Volume;
			Duration = 0;
			TempSoundHandle = Sound.Play(SoundEvent);
		}

		if (SoundHandle.IsValid() && SoundHandle.IsPlaying) {
			return;
		}

		if (!SoundHandle.IsValid() && !TempSoundHandle.IsValid()) {
			//SoundEvent.Volume = Volume;
			Duration = 0;
			SoundHandle = Sound.Play(SoundEvent);
		}

		if (SoundHandle is { IsPlaying: false }) {
			SoundHandle = TempSoundHandle;
			TempSoundHandle = null;
		}
	}
}
