using Sandbox.Audio;

namespace EnumExtensions.Util;

public static class GameSounds {
	// Order as in UiSounds enum!!!
	private static readonly string[] UISounds = {
		"ui.navigate.press", "ui.favourite", "ui.drag.stop", "ui.navigate.deny"
	};

	// Order as in UiSounds enum!!!
	private static readonly string[] SFXSounds = { };

	public static void PlayUI(UiSounds sound) {
		Sound.Play(UISounds[sound.AsInt()], Mixer.Master.GetChildren()[2]);
	}

	public static void PlaySFX(SfxSounds sound) {
		Sound.Play(UISounds[sound.AsInt()], Mixer.Master.GetChildren()[1]);
	}
}
