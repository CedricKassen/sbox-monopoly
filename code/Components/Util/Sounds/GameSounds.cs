using Sandbox.Audio;

namespace EnumExtensions.Util;

public static class GameSounds {
	// Order as in UiSounds enum!!!
	private static readonly string[] UISounds = {
		"ui.button.press", "ui.favourite", "ui.drag.stop", "ui.navigate.deny"
	};

	// Order as in UiSounds enum!!!
	private static readonly string[] SFXSounds = { "dice" };

	public static void PlayUI(UiSounds sound, float volumeOverwrite = -1f) {
		var handle = Sound.Play(UISounds[sound.AsInt()], Mixer.Master.GetChildren()[2]);
		if (volumeOverwrite >= 0) {
			handle.Volume = volumeOverwrite;
		}
	}

	public static void PlaySFX(SfxSounds sound, float volumeOverwrite = -1f) {
		var handle = Sound.Play(SFXSounds[sound.AsInt()], Mixer.Master.GetChildren()[1]);
		if (volumeOverwrite >= 0) {
			handle.Volume = volumeOverwrite;
		}
	}
}
