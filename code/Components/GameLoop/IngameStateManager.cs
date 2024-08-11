using Monopoly.UI.Screens.GameLoop;

public sealed class IngameStateManager : Component {
	public object Data = null;

	public Dictionary<string, ulong> OwnedFields = new() {
		{ "brown1", 0 },
		{ "brown2", 0 },
		{ "lightBlue1", 0 },
		{ "lightBlue2", 0 },
		{ "lightBlue3", 0 },
		{ "pink1", 0 },
		{ "pink2", 0 },
		{ "pink3", 0 },
		{ "orange1", 0 },
		{ "orange2", 0 },
		{ "orange3", 0 },
		{ "red1", 0 },
		{ "red2", 0 },
		{ "red3", 0 },
		{ "yellow1", 0 },
		{ "yellow2", 0 },
		{ "yellow3", 0 },
		{ "green1", 0 },
		{ "green2", 0 },
		{ "green3", 0 },
		{ "blue1", 0 },
		{ "blue2", 0 },
		{ "railroad1", 0 },
		{ "railroad2", 0 },
		{ "railroad3", 0 },
		{ "railroad4", 0 },
		{ "electricCompany", 0 },
		{ "waterCompany", 0 },
		{ "chanceJailFree", 0 },
		{ "communityJailFree", 0 }
	};

	[Property] public IngameUI.IngameUiStates State = IngameUI.IngameUiStates.None;
}
