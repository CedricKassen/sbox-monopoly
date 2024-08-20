public sealed class MusicManager : BaseSoundComponent {
	[Property] private TimeSince MusicStart;
	private SoundHandle TempSoundHandle;

	[Property] [Group("Sound")] public float RepeatOffset { get; set; }


	protected override void OnUpdate() {
		if (SoundHandle is not null) {
			ApplyOverrides(SoundHandle);
			Log.Info("Over");
			Log.Info(SoundHandle.Volume);
		}

		if (TempSoundHandle is not null) {
			ApplyOverrides(TempSoundHandle);
		}

		Log.Info(SoundEvent.Sounds[0]);

		if (SoundEvent.Sounds[0].Duration <= MusicStart.Relative + RepeatOffset) {
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
			Log.Info("Start");
			Log.Info(Volume);
			SoundHandle = Sound.Play(SoundEvent);
			Log.Info(SoundHandle.Volume);
		}

		if (SoundHandle.IsValid() && !SoundHandle.IsPlaying) {
			SoundHandle = TempSoundHandle;
			TempSoundHandle = null;
		}
	}
}
