using System;
using System.Threading.Tasks;
using Monopoly.UI.Screens.GameLoop;
using Sandbox.Constants;
using Sandbox.Events;
using Sandbox.Events.TurnEvents;

namespace Sandbox.Components.GameLoop;

public class GameEventHandler : Component, IGameEventHandler<RolledEvent>, IGameEventHandler<MovementDoneEvent>,
                                IGameEventHandler<PropertyAquiredEvent>, IGameEventHandler<PropertyAuctionEvent>,
                                IGameEventHandler<AuctionFinishedEvent>, IGameEventHandler<PlayerPaymentEvent>,
                                IGameEventHandler<TurnFinishedEvent>, IGameEventHandler<PropertyMortgagedEvent>,
                                IGameEventHandler<PropertyMortgagePayedEvent>, IGameEventHandler<BuildHouseEvent>,
                                IGameEventHandler<DestroyHouseEvent>, IGameEventHandler<GoToJailEvent>,
                                IGameEventHandler<LandOnJailEvent> {
	[Property] public GameObject LocationContainer { get; set; }
	[Property] public Lobby Lobby { get; set; }
	[Property] public MovementManager MovementManager { get; set; }
	[Property] public CardActionManager CardActionManager { get; set; }
	[Property] public IngameStateManager IngameStateManager { get; set; }
	[Property] public TurnManager TurnManager { get; set; }

	public void OnGameEvent(AuctionFinishedEvent eventArgs) {
		TurnManager.EmitPropertyAquiredEvent(eventArgs.playerId, eventArgs.PropertyIndex, true);

		if (Networking.IsHost) {
			GetPlayerFromEvent(eventArgs.playerId).Money -= eventArgs.Amount;
		}

		IngameStateManager.State = IngameUI.IngameUiStates.None;
	}

	public void OnGameEvent(BuildHouseEvent eventArgs) {
		var property = GetLocationFromPropertyIndex(eventArgs.PropertyIndex);
		if (property.Houses == 5) {
			Log.Error("Tried to add house to maxed out house!");
			return;
		}

		var player = GetPlayerFromEvent(eventArgs.PlayerId);

		// Check if player can afford this house 
		if (player.Money - property.House_Cost < 0) {
			Log.Warning("Player can afford this house!");
			return;
		}

		// Check if player builds evenly
		foreach (var member in property.GroupMembers) {
			// If one street of the group got more houses then current street prevent destroying we cant go from 2 1 1 -> 3 1 1
			if (property.Houses > LocationContainer.Children[member].Components.Get<GameLocation>().Houses) {
				return;
			}
		}

		if (Networking.IsHost) {
			player.Money -= property.House_Cost;
		}

		property.Houses++;
	}

	public void OnGameEvent(DestroyHouseEvent eventArgs) {
		var property = GetLocationFromPropertyIndex(eventArgs.PropertyIndex);
		if (property.Houses == 0) {
			Log.Error("Tried to remove house from empty property!");
			return;
		}

		// Check if player builds evenly
		foreach (var member in property.GroupMembers) {
			// If one street of the group got less houses then current street prevent building we cant go from 2 1 1 -> 2 1 0
			if (property.Houses < LocationContainer.Children[member].Components.Get<GameLocation>().Houses) {
				return;
			}
		}


		var player = GetPlayerFromEvent(eventArgs.PlayerId);
		if (Networking.IsHost) {
			player.Money += property.House_Cost / 2;
		}

		property.Houses--;
	}

	public void OnGameEvent(MovementDoneEvent eventArgs) {
		var location = eventArgs.Location;
		IngameStateManager.OwnedFields.TryGetValue(location.GameObject.Name, out var fieldOwner);
		var currentPlayer = GetPlayerFromEvent(eventArgs.playerId);

		TurnManager.ChangePhase(eventArgs.playerId, TurnManager.Phase.PlayerAction);

		if (fieldOwner == 0 || location.Type == GameLocation.PropertyType.Event) {
			if (location.EventId == "start") {
				if (Networking.IsHost) {
					currentPlayer.Money += 200;
				}

				return;
			}

			CardActionManager.DisplayCardFor(currentPlayer, location);
			return;
		}

		// TODO if field action event is done!
		TurnManager.ChangePhase(currentPlayer.SteamId, TurnManager.Phase.PlayerAction);

		if (fieldOwner != eventArgs.playerId && !location.Mortgaged) {
			if (location.Type == GameLocation.PropertyType.Normal) {
				var price = 0;
				// If location has zero houses we need to check if the location owner owns the whole group
				if (location.Houses == 0) {
					price = OwnsAllFrom(location, eventArgs.playerId)
						? location.Normal_Rent[0] * 2
						: location.Normal_Rent[0];
				}
				else {
					price = location.Normal_Rent[location.Houses];
				}


				TurnManager.EmitPlayerPaymentEvent(eventArgs.playerId, fieldOwner, price);
			}

			if (location.Type == GameLocation.PropertyType.Railroad) {
				var railroadCount = IngameStateManager.OwnedFields
				                                      .Count(f => f.Value == fieldOwner && f.Key.Contains("railroad"));
				TurnManager.EmitPlayerPaymentEvent(eventArgs.playerId, fieldOwner,
					location.Railroad_Rent[railroadCount - 1]);
			}

			if (location.Type == GameLocation.PropertyType.Utility) {
				var utilityCount = IngameStateManager.OwnedFields.Count(f => f.Value == fieldOwner &&
				                                                             (f.Key == "electricCompany" ||
				                                                              f.Key == "waterCompany"));

				TurnManager.EmitPlayerPaymentEvent(eventArgs.playerId, fieldOwner,
					location.Utility_Rent_Multiplier[utilityCount - 1] * currentPlayer.LastDiceCount);
			}
		}
	}

	public void OnGameEvent(PlayerPaymentEvent eventArgs) {
		if (Networking.IsHost) {
			GetPlayerFromEvent(eventArgs.playerId).Money -= eventArgs.Amount;
			GetPlayerFromEvent(eventArgs.Recipient).Money += eventArgs.Amount;
		}
	}

	public void OnGameEvent(GoToJailEvent eventArgs) {
		Player player = GetPlayerFromEvent(eventArgs.playerId);
		CardActionHelper.GoToJail(player, MovementManager);
		TurnManager.ChangePhase(eventArgs.playerId, TurnManager.Phase.InAction);
	}

	public void OnGameEvent(LandOnJailEvent eventArgs) {
		Player player = GetPlayerFromEvent(eventArgs.playerId);
		if (Networking.IsHost) {
			player.JailTurnCounter++;
		}

		TurnManager.EmitTurnFinishedEvent(eventArgs.playerId);
	}

	public void OnGameEvent(PropertyAquiredEvent eventArgs) {
		var location = LocationContainer.Children[eventArgs.PropertyIndex];
		var gameLocation = location.Components.Get<GameLocation>();
		var player = GetPlayerFromEvent(eventArgs.playerId);

		// if property was acquired from auction money got already subtracted
		var costs = eventArgs.FromAuction ? 0 : gameLocation.Price;

		if (costs <= player.Money && Networking.IsHost) {
			player.Money -= costs;
			IngameStateManager.OwnedFields[location.Name] = eventArgs.playerId;
		}
	}

	public void OnGameEvent(PropertyAuctionEvent eventArgs) {
		IngameStateManager.AuctionBiddings = new NetDictionary<ulong, int>();
		IngameStateManager.State = IngameUI.IngameUiStates.Auction;

		foreach (var player in Lobby.Players) {
			IngameStateManager.AuctionBiddings[player.SteamId] = 0;
		}
	}

	public void OnGameEvent(PropertyMortgagedEvent eventArgs) {
		var property = GetLocationFromPropertyIndex(eventArgs.PropertyIndex);

		if (Networking.IsHost && !property.Mortgaged) {
			GetPlayerFromEvent(eventArgs.playerId).Money += property.Price / 2;
		}

		property.Mortgaged = true;
	}


	public void OnGameEvent(PropertyMortgagePayedEvent eventArgs) {
		var player = GetPlayerFromEvent(eventArgs.playerId);
		var property = GetLocationFromPropertyIndex(eventArgs.PropertyIndex);
		var price = (int)Math.Ceiling(property.Price / 2 * 1.1);

		if (Networking.IsHost && player.Money > price && property.Mortgaged) {
			player.Money -= price;
		}

		property.Mortgaged = false;
	}

	public void OnGameEvent(RolledEvent eventArgs) {
		var player = GetPlayerFromEvent(eventArgs.playerId);

		Log.Info(eventArgs.Doubles);
		Log.Info(eventArgs.Number);

		if (eventArgs.Doubles) {
			player.DoublesCount++;
		}
		else {
			player.DoublesCount = 0;
		}

		if (player.DoublesCount < 3) {
			MovementManager.StartMovement(player, eventArgs.Number);
		}
		else {
			CardActionHelper.GoToJail(player, MovementManager);
		}

		player.LastDiceCount = eventArgs.Number;
	}


	public void OnGameEvent(TurnFinishedEvent eventArgs) {
		TurnManager.CurrentPlayerIndex = (TurnManager.CurrentPlayerIndex + 1) % TurnManager.CurrentLobby.Players.Count;
		ChangeDiceOwnershipToCurrentPlayer();
	}

	private bool OwnsAllFrom(GameLocation location, ulong playerId) {
		if (!location.Type.Equals(GameLocation.PropertyType.Normal)) {
			return false;
		}

		return location.GroupMembers
		               .All(member =>
			               IngameStateManager.OwnedFields[location.GameObject.Parent.Children[member].Name] ==
			               playerId);
	}

	protected override Task OnLoad() {
		ChangeDiceOwnershipToCurrentPlayer();
		return base.OnLoad();
	}

	private Player GetPlayerFromEvent(ulong playerId) {
		return Lobby.Players.Find(player => player.SteamId == playerId);
	}

	private GameLocation GetLocationFromPropertyIndex(int propertyIndex) {
		return LocationContainer.Children[propertyIndex].Components.Get<GameLocation>();
	}

	private void ChangeDiceOwnershipToCurrentPlayer() {
		var diceList = new List<Dice>(Game.ActiveScene.GetAllComponents<Dice>());

		if (Networking.IsHost && diceList.Count > 0) {
			foreach (var dice in diceList) {
				dice.Network.AssignOwnership(
					TurnManager.CurrentLobby.Players[TurnManager.CurrentPlayerIndex].Connection);
			}
		}
	}
}
