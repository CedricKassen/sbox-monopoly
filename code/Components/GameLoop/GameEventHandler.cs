using Monopoly.UI.Screens.GameLoop;
using Sandbox.Events;
using Sandbox.Events.TurnEvents;

namespace Sandbox.Components.GameLoop;

public class GameEventHandler : Component, IGameEventHandler<RolledEvent>, IGameEventHandler<MovementDoneEvent>,
                                IGameEventHandler<PropertyAquiredEvent>, IGameEventHandler<PropertyAuctionEvent>, IGameEventHandler<AuctionFinishedEvent>, IGameEventHandler<PlayerPaymentEvent> {
	[Property] public GameObject LocationContainer { get; set; }
	[Property] public Lobby Lobby { get; set; }
	[Property] public MovementManager MovementManager { get; set; }
	[Property] public CardActionManager CardActionManager { get; set; }
	[Property] public IngameStateManager IngameStateManager { get; set; }
	[Property] public TurnManager TurnManager { get; set; }

	public void OnGameEvent(MovementDoneEvent eventArgs) {
		IngameStateManager.OwnedFields.TryGetValue(eventArgs.Location.GameObject.Name, out var fieldOwner);
		var currentPlayer = GetPlayerFromEvent(eventArgs.playerId);
		
		if (fieldOwner == 0 || eventArgs.Location.Type == GameLocation.PropertyType.Event) {
			CardActionManager.DisplayCardFor(currentPlayer, eventArgs.Location);
			return;
		}

		if (fieldOwner != eventArgs.playerId) {
			if (eventArgs.Location.Type == GameLocation.PropertyType.Normal) {
				TurnManager.EmitPlayerPaymentEvent(eventArgs.playerId, fieldOwner, eventArgs.Location.Normal_Rent[eventArgs.Location.Houses] );
			}

			if (eventArgs.Location.Type == GameLocation.PropertyType.Railroad) {
				var railroadCount = IngameStateManager.OwnedFields.Count(f => f.Value == fieldOwner &&
				                                                              (f.Key == "railroad1" ||
				                                                               f.Key == "railroad2" ||
				                                                               f.Key == "railroad3" ||
				                                                               f.Key == "railroad4")); 
				TurnManager.EmitPlayerPaymentEvent(eventArgs.playerId, fieldOwner, eventArgs.Location.Railroad_Rent[railroadCount - 1] );
			}

			if (eventArgs.Location.Type == GameLocation.PropertyType.Utility) {
				var utilityCount = IngameStateManager.OwnedFields.Count(f => f.Value == fieldOwner &&
				                                                              (f.Key == "electricCompany" ||
				                                                               f.Key == "waterCompany"));
				TurnManager.EmitPlayerPaymentEvent(eventArgs.playerId, fieldOwner, eventArgs.Location.Utility_Rent_Multiplier[utilityCount - 1] * currentPlayer.LastDiceCount );
			}
		}
	}

	public void OnGameEvent(PropertyAquiredEvent eventArgs) {
		var location = LocationContainer.Children[eventArgs.PropertyIndex];
		var component = location.Components.Get<GameLocation>();
		var player = GetPlayerFromEvent(eventArgs.playerId);
		
		if (component.Price <= player.Money) {
			player.Money -= component.Price;
			IngameStateManager.OwnedFields[location.Name] = eventArgs.playerId;
		}
	}

	public void OnGameEvent(RolledEvent eventArgs) {
		var player = GetPlayerFromEvent(eventArgs.playerId);
		MovementManager.StartMovement(player, eventArgs.Number);
		player.LastDiceCount = eventArgs.Number;
	}

	public void OnGameEvent(PropertyAuctionEvent eventArgs) {
		IngameStateManager.AuctionBiddings = new NetDictionary<ulong, int>();

		foreach (var player in Lobby.Players) {
			IngameStateManager.AuctionBiddings[player.SteamId] = 0;
		}
	}

	public void OnGameEvent(AuctionFinishedEvent eventArgs) {
		TurnManager.EmitPropertyAquiredEvent(eventArgs.playerId, eventArgs.PropertyIndex);
		GetPlayerFromEvent(eventArgs.playerId).Money -= eventArgs.Amount;
		IngameStateManager.State = IngameUI.IngameUiStates.None;
	}
	
	private Player GetPlayerFromEvent(ulong playerId) {
		return Lobby.Players.Find(player => player.SteamId == playerId);
	}

	public void OnGameEvent(PlayerPaymentEvent eventArgs) {
		GetPlayerFromEvent(eventArgs.playerId).Money -= eventArgs.Amount;
		GetPlayerFromEvent(eventArgs.Recipient).Money += eventArgs.Amount;
	}
}
