using Sandbox.Events;
using Sandbox.Events.TurnEvents;

namespace Sandbox.Components.GameLoop;

public class GameEventHandler : Component, IGameEventHandler<RolledEvent>, IGameEventHandler<MovementDoneEvent>, IGameEventHandler<PropertyAquiredEvent>
{
	[Property] public GameObject LocationContainer { get; set; }

	[Property] public Lobby Lobby { get; set; }

	[Property] public MovementManager MovementManager { get; set; }
	[Property] public CardActionManager CardActionManager { get; set; }
	[Property] public IngameStateManager IngameStateManager { get; set; }

	public void OnGameEvent(MovementDoneEvent eventArgs) {
		CardActionManager.DisplayCardFor(GetPlayerFromEvent(eventArgs), eventArgs.Location);
	}

	public void OnGameEvent(RolledEvent eventArgs)
	{
		MovementManager.StartMovement(GetPlayerFromEvent(eventArgs), eventArgs.Number);
	}

	public void OnGameEvent(PropertyAquiredEvent eventArgs) {
		GameObject location = LocationContainer.Children[eventArgs.PropertyIndex];
		IngameStateManager.OwnedFields[location.Name] = eventArgs.playerId;
	}

	private Player GetPlayerFromEvent(BaseEvent eventArgs)
	{
		return Lobby.Players.Find(player => player.SteamId == eventArgs.playerId);
	}
}
