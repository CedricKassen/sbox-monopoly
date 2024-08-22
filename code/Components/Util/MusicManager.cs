using System.Threading.Tasks;

public sealed class MusicManager : BaseSoundComponent {
	[Property] public float MusicDuration = 238.21062f;
	[Property] private TimeSince MusicStart;
	private SoundHandle TempSoundHandle;

	[Property] [Group("Sound")] public float RepeatOffset { get; set; }


	protected override Task OnLoad() {
		foreach (var sound in SoundEvent.Sounds) {
			sound?.Preload();
		}

		return base.OnLoad();
	}

	protected override void OnUpdate() {
		if (SoundHandle is not null) {
			ApplyOverrides(SoundHandle);
		}

		if (TempSoundHandle is not null) {
			ApplyOverrides(TempSoundHandle);
		}

		if (MusicDuration <= MusicStart.Relative + RepeatOffset) {
			MusicStart = 0;
			SoundEvent.Volume = Volume;
			TempSoundHandle = Sound.Play(SoundEvent);
		}

		if (SoundHandle.IsValid() && SoundHandle.IsPlaying) {
			return;
		}


		if (!SoundHandle.IsValid() && !TempSoundHandle.IsValid()) {
			MusicStart = 0;
			SoundEvent.Volume = Volume;
			SoundHandle = Sound.Play(SoundEvent);
		}

		if (SoundHandle.IsValid() && !SoundHandle.IsPlaying) {
			SoundHandle = TempSoundHandle;
			TempSoundHandle = null;
		}
	}
}
