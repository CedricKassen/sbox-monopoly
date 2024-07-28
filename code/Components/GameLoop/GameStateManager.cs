using Sandbox;
using System.Diagnostics;
using Sandbox.Events;
using Sandbox.Events.GameStateEvents;

public enum GameState
{
	INGAME, MENU, LOBBY_CREATE, LOBBY_JOIN, END_GAME
}

public sealed class GameStateManager : Component, IGameEventHandler<GameEndEvent>, IGameEventHandler<GameStartEvent>, IGameEventHandler<CreateLobbyEvent>, IGameEventHandler<JoinLobbyEvent>, IGameEventHandler<LeaveLobbyEvent>
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
	
	public void OnGameEvent( CreateLobbyEvent eventArgs )
	{
		Log.Info("Event: " + "CreateLobbyEvent");
		CurrentState = UiManager.ChangeState(GameState.LOBBY_CREATE);
	}
	
	public void OnGameEvent( JoinLobbyEvent eventArgs )
	{
		Log.Info("Event: " + "JoinLobbyEvent");
		CurrentState = UiManager.ChangeState(GameState.LOBBY_JOIN);
	}
	
	public void OnGameEvent( LeaveLobbyEvent eventArgs )
	{
		Log.Info("Event: " + "LeaveLobbyEvent");
		CurrentState = UiManager.ChangeState(GameState.MENU);
	}

	protected override void OnStart()
	{
		Log.Info("Start");
		CurrentState = UiManager.ChangeState(GameState.MENU);
	}

	protected override void OnUpdate()
	{
		Transform.Position += Vector3.Forward;
		Log.Info("Update");
		base.OnUpdate();
	}
	
	
}
