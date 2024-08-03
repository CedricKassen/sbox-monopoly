using Sandbox;
using System.Diagnostics;
using Microsoft.VisualBasic;
using Sandbox.Events;
using Sandbox.Events.GameStateEvents;

public enum GameState
{
	INGAME = 4, MENU = 0, LOBBY_CREATION = 1, LOBBY_SELECTION = 2, INLOBBY = 3, END_GAME = 5
}

public sealed class GameStateManager : Component, IGameEventHandler<GameEndEvent>, IGameEventHandler<GameStartEvent>
{
	[Property] [Sync] private int StartState { get; set; } = 0;
	[Property] GameState CurrentState { get; set; }
	[Property] public UiManager UiManager { get; private set; }


	public void OnGameEvent( GameEndEvent eventArgs )
	{
		Log.Info("Event: " + "GameEndEvent");
		CurrentState = UiManager.ChangeState(GameState.END_GAME);
	}

	public void OnGameEvent( GameStartEvent eventArgs )
	{
		Log.Info("Event: " + "GameStartEvent");
		CurrentState = UiManager.ChangeState(GameState.INGAME);
	}
	

	protected override void OnStart()
	{
		Log.Info("Change to " + (GameState)StartState);
		CurrentState = UiManager.ChangeState(CurrentState);
	}

	
	
}
