using Monopoly.UI.Screens.GameLoop;

public sealed class Player : Component {
	private string _name;
	private ulong _steamId;

	[Property]
	public ulong SteamId {
		get => _steamId;
		set {
			if (_steamId == 0) {
				_steamId = value;
			}
		}
	}

	[Property]
	public string Name {
		get => _name;
		set {
			if (string.IsNullOrEmpty(_name)) {
				_name = value;
			}
		}
	}

	public override string ToString() {
		return Name + ": PlayerLastThrow(" + LastDiceCount + "), PlayerDoubles(" + DoublesCount +
		       "), EliminatedPosition(" + EliminatedPosition + "), HasBonusMove(" + HasBonusMove + "), RoundCount(" +
		       RoundCount + "), JailCounter(" + JailTurnCounter + "), Money(" + Money + ")";
	}

	public static int EliminatedCount { get; set; } = 0;

	[HostSync, Property, HideIf("EliminatedPosition", 0)]
	public int EliminatedPosition { get; set; }

	[Property, HostSync] public int Money { get; set; } = 2000;

	[Property, HostSync] public int CurrentField { get; set; }

	[Property] public Connection Connection { get; set; }

	[Property] public IngameUI.LocalUIStates localUiState { get; set; } = IngameUI.LocalUIStates.None;
	[Property] public IngameUI.LocalUIStates localUiStateCache { get; set; } = IngameUI.LocalUIStates.None;

	[Property, HostSync] public int LastDiceCount { get; set; } = 0;

	[Property, HostSync] public int DoublesCount { get; set; } = 0;

	[Property] public bool LocalUiOpen { get; set; } = false;
	[Property, HostSync] public bool HasBonusMove { get; set; } = false;
	[Property, HostSync] public int RoundCount { get; set; }

	[Property] public GameLocation CurrentHoverLocation { get; set; } = null;

	[Description("0 Means player is not in jail, 1 means the NEXT turn is the first and so forth")]
	[Property, HostSync]
	public int JailTurnCounter { get; set; } = 0;


	protected override void OnUpdate() { }
}
