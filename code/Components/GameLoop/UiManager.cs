using System.Diagnostics;
using Sandbox;
using Sandbox.UI;

public sealed class UiManager : Component
{
	[Property] public GameEndUI GameEndUi { get; set; }
	[Property] public IngameUI IngameUi { get; set; }
	[Property] public LobbyCreationUI LobbyCreationUi { get; set; }
	[Property] public LobbyHubUI HubUi { get; set; }
	[Property] public MainMenuUI MainMenuUi { get; set; }


	public GameState ChangeState(GameState newState)
	{
		
		Debug.Write("Change state to: " + newState);
		disableEverything();
		
		switch (newState)
		{
			case GameState.MENU:
				MainMenuUi.Visible = true;
				break;
			case GameState.INGAME:
				IngameUi.Visible = true;
				break;
			case GameState.END_GAME:
				GameEndUi.Visible = true;
				break;
			case GameState.LOBBY_JOIN:
				HubUi.Visible = true;
				break;
			case GameState.LOBBY_CREATE: 
				LobbyCreationUi.Visible = true;
				break;
		}

		return newState;
	}

	private void disableEverything()
	{
		MainMenuUi.Visible = false;
		GameEndUi.Visible = false;
		IngameUi.Visible = false;
		LobbyCreationUi.Visible = false;
		HubUi.Visible = false;
	}
}
