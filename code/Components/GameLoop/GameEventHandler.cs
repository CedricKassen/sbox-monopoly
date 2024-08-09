using Sandbox.Events;
using Sandbox.Events.TurnEvents;

namespace Sandbox.Components.GameLoop;

public class GameEventHandler : Component, IGameEventHandler<RolledEvent>, IGameEventHandler<MovementDoneEvent>
{
	[Property] public GameObject LocationContainer { get; set; }

	[Property] public Lobby Lobby { get; set; }

	[Property] public MovementManager MovementManager { get; set; }
	[Property] public CardActionManager CardActionManager { get; set; }

	public void OnGameEvent(MovementDoneEvent eventArgs) {
		CardActionManager.DisplayCardFor(GetPlayerFromEvent(eventArgs), eventArgs.Location);
	}

	public void OnGameEvent(RolledEvent eventArgs)
	{
		MovementManager.StartMovement(GetPlayerFromEvent(eventArgs), eventArgs.Number);
	}

	private Player GetPlayerFromEvent(BaseEvent eventArgs)
	{
		return Lobby.Players.Find(player => player.SteamId == eventArgs.playerId);
	}
}
