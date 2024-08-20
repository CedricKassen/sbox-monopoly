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

	[Property] public int Money { get; set; } = 2000;

	[Property] public int CurrentField { get; set; }

	[Property] public Connection Connection { get; set; }

	[Property]
	public IngameUI.LocalUIStates localUiState { get; set; } = IngameUI.LocalUIStates.None;
	
	[Property]
	public int LastDiceCount { get; set; } = 0;


	protected override void OnUpdate() { }
}
