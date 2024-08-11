using Sandbox.Events;
using Sandbox.Events.TurnEvents;

namespace Sandbox.Components.GameLoop;

public class GameEventHandler : Component, IGameEventHandler<RolledEvent>, IGameEventHandler<MovementDoneEvent>,
                                IGameEventHandler<PropertyAquiredEvent> {
	[Property] public GameObject LocationContainer { get; set; }

	[Property] public Lobby Lobby { get; set; }

	[Property] public MovementManager MovementManager { get; set; }
	[Property] public CardActionManager CardActionManager { get; set; }
	[Property] public IngameStateManager IngameStateManager { get; set; }

	public void OnGameEvent(MovementDoneEvent eventArgs) {
		IngameStateManager.OwnedFields.TryGetValue(eventArgs.Location.GameObject.Name, out var ownedField);
		if (ownedField == 0 || eventArgs.Location.Type == GameLocation.PropertyType.Event) {
			CardActionManager.DisplayCardFor(GetPlayerFromEvent(eventArgs), eventArgs.Location);
			return;
		}

		if (ownedField != (ulong)Game.SteamId) {
			Log.Info("Pay " + eventArgs.Location.Normal_Rent[0]);
		}
	}

	public void OnGameEvent(PropertyAquiredEvent eventArgs) {
		var location = LocationContainer.Children[eventArgs.PropertyIndex];
		IngameStateManager.OwnedFields[location.Name] = eventArgs.playerId;
	}

	public void OnGameEvent(RolledEvent eventArgs) {
		MovementManager.StartMovement(GetPlayerFromEvent(eventArgs), eventArgs.Number);
	}

	private Player GetPlayerFromEvent(BaseEvent eventArgs) {
		return Lobby.Players.Find(player => player.SteamId == eventArgs.playerId);
	}
}
