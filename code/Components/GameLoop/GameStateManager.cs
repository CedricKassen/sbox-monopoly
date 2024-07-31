using Sandbox;
using System.Diagnostics;
using Microsoft.VisualBasic;
using Sandbox.Events;
using Sandbox.Events.GameStateEvents;

public enum GameState
{
	INGAME, MENU, LOBBY_CREATION, LOBBY_SELECTION, INLOBBY, END_GAME
}

public sealed class GameStateManager : Component, IGameEventHandler<GameEndEvent>, IGameEventHandler<GameStartEvent>
{
	[Property] public GameState CurrentState { get; private set; }
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
		CurrentState = UiManager.ChangeState(GameState.MENU);
	}

	
	
}
