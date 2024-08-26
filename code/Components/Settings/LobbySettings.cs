namespace EnumExtensions.Settings;

public class LobbySettings {
	[Title("Double paycheck on go")]
	[Description("Collect double the money when landing directly on go")]
	public bool DoublePayment { get; set; } = false;

	[Title("Payout on free parking")]
	[Description("Collect all money that was payed into the bank. Taxes e.g.")]
	public bool CollectFreeParking { get; set; } = false;
}

public class LobbySettingsSystem {
	private static LobbySettings current { get; set; }

	public static LobbySettings Current {
		get {
			if (current is null) {
				Load();
			}

			return current;
		}
		set => current = value;
	}

	public static string FilePath => "lobbysettings.json";

	public static void Save() {
		FileSystem.Data.WriteJson(FilePath, Current);
	}

	public static void Load() {
		Current = FileSystem.Data.ReadJson(FilePath, new LobbySettings());
	}
}
