using Sandbox.Events;
using Sandbox.Events.GameStateEvents;

public enum GameState
{
	INGAME = 4, MENU = 0, LOBBY_CREATION = 1, LOBBY_SELECTION = 2, INLOBBY = 3, END_GAME = 5
}

public sealed class GameStateManager : Component, IGameEventHandler<GameEndEvent>, IGameEventHandler<GameStartEvent>
{
	[Property] [HostSync] private GameState CurrentState { get; set; }


	public void OnGameEvent(GameEndEvent eventArgs)
	{
		Log.Info("Event: " + "GameEndEvent");
		//CurrentState = UiManager.ChangeState(GameState.END_GAME);
	}

	public void OnGameEvent(GameStartEvent eventArgs)
	{
		Log.Info("Event: " + "GameStartEvent");
		//CurrentState = UiManager.ChangeState(GameState.INGAME);
	}
}
