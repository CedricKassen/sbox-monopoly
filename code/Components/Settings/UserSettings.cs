using Sandbox.Audio;

namespace EnumExtensions.Settings;

/*
 * "Inspirated" from this commit https://github.com/Facepunch/sbox-hc2/commit/4bf77ce163d73ab20c260e3e4e17174e9417411f#diff-29169d649930f36f393d783647f43b3b97c87abded320e44bb022ab9ac7a5e6f
 */
public class UserSettings {
	[Title("Master")]
	[Description("The overall volume")]
	[Range(0, 100, 1)]
	public float MasterVolume { get; set; } = 100;

	[Title("Music")]
	[Description("How loud any music will play")]
	[Range(0, 100, 1)]
	public float MusicVolume { get; set; } = 100;

	[Title("SFX")]
	[Description("Most effects in the game")]
	[Range(0, 100, 1)]
	public float SFXVolume { get; set; } = 100;

	[Title("UI")]
	[Description("interface sounds")]
	[Range(0, 100, 1)]
	public float UIVolume { get; set; } = 100;
}

public class UserSettingsSystem {
	private static UserSettings current { get; set; }

	public static UserSettings Current {
		get {
			if (current is null) {
				Load();
			}

			return current;
		}
		set => current = value;
	}

	public static string FilePath => "gamesettings.json";

	public static void Save() {
		ApplyMixer();

		FileSystem.Data.WriteJson(FilePath, Current);
	}

	public static void Load() {
		Current = FileSystem.Data.ReadJson(FilePath, new UserSettings());

		ApplyMixer();
	}


	private static void ApplyMixer() {
		Mixer.Master.Volume = Current.MasterVolume / 100;
		var channel = Mixer.Master.GetChildren();
		channel[0].Volume = Current.MusicVolume / 100;
		channel[1].Volume = Current.SFXVolume / 100;
		channel[2].Volume = Current.UIVolume / 100;
	}
}
