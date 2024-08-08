using Sandbox.Events;
using Sandbox.Events.TurnEvents;

namespace Sandbox.Components.GameLoop;

public class GameEventHandler : Component, IGameEventHandler<RolledEvent>
{
	[Property] public GameObject LocationContainer { get; set; }

	[Property] public Lobby Lobby { get; set; }

	[Property] public MovementManager MovementManager { get; set; }

	public void OnGameEvent(RolledEvent eventArgs)
	{
		MovementManager.StartMovement(GetPlayerFromEvent(eventArgs), eventArgs.Number);
	}

	private Player GetPlayerFromEvent(BaseEvent eventArgs)
	{
		return Lobby.Players.Find(player => player.SteamId == eventArgs.playerId);
	}
}
