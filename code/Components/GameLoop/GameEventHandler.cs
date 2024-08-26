using System;
using System.Threading.Tasks;
using EnumExtensions.Settings;
using Monopoly.UI.Screens.GameLoop;
using Sandbox.Constants;
using Sandbox.Events;
using Sandbox.Events.TurnEvents;
using Sandbox.UI;

namespace Sandbox.Components.GameLoop;

public class GameEventHandler : Component, IGameEventHandler<RolledEvent>, IGameEventHandler<MovementDoneEvent>,
                                IGameEventHandler<PropertyAquiredEvent>, IGameEventHandler<PropertyAuctionEvent>,
                                IGameEventHandler<AuctionFinishedEvent>, IGameEventHandler<EventCardClosedEvent>,
                                IGameEventHandler<TurnFinishedEvent>, IGameEventHandler<PropertyMortgagedEvent>,
                                IGameEventHandler<TradingRequestedEvent>, IGameEventHandler<TradingAcceptedEvent>,
                                IGameEventHandler<TradingDeniedEvent>, IGameEventHandler<PropertyMortgagePayedEvent>,
                                IGameEventHandler<BuildHouseEvent>, IGameEventHandler<DestroyHouseEvent>,
                                IGameEventHandler<GoToJailEvent>, IGameEventHandler<LandOnJailEvent>,
                                IGameEventHandler<StartRollEvent>, IGameEventHandler<PayJailFineEvent>,
                                IGameEventHandler<UseJailCardEvent>, IGameEventHandler<DebugEvent>,
                                IGameEventHandler<TurnActionDoneEvent>, IGameEventHandler<NotEnoughFundsEvent>,
                                IGameEventHandler<PlayerBankruptEvent> {
	[Property] public GameObject LocationContainer { get; set; }
	[Property] public Lobby Lobby { get; set; }
	[Property] public MovementManager MovementManager { get; set; }
	[Property] public CardActionManager CardActionManager { get; set; }
	[Property] public IngameStateManager IngameStateManager { get; set; }
	[Property] public TurnManager TurnManager { get; set; }
	[Property] public TradeState TradeState { get; set; }

	private List<Dice> _dice = new();

	private Stack<int> _auctionLocations = new();

	public void OnGameEvent(AuctionFinishedEvent eventArgs) {
		TurnManager.ChangePhase(eventArgs.playerId, TurnManager.Phase.PlayerAction);
		TurnManager.EmitPropertyAquiredEvent(eventArgs.playerId, eventArgs.PropertyIndex, true);

		if (Networking.IsHost) {
			GetPlayerFromEvent(eventArgs.playerId).Money -= eventArgs.Amount;
		}

		if (_auctionLocations.Any()) {
			TurnManager.EmitPropertyAuctionEvent(_auctionLocations.Pop(), eventArgs.playerId);
			return;
		}

		IngameStateManager.State = IngameUI.IngameUiStates.None;
	}

	public async void OnGameEvent(StartRollEvent eventArgs) {
		if (_dice.Count == 0) {
			_dice = new(Game.ActiveScene.GetAllComponents<Dice>());
		}


		foreach (var dice in _dice) {
			dice.Roll();
		}

		while (_dice.Any(dice => dice.IsRolling)) {
			await Task.DelayRealtimeSeconds(0.5f);
		}

		TurnManager.EmitRolledEvent((ulong)Game.SteamId, _dice[0].GetRollValue(), _dice[1].GetRollValue());
	}

	public void OnGameEvent(TurnActionDoneEvent eventArgs) {
		TurnManager.ChangePhase(eventArgs.PlayerId, eventArgs.NewPhase);
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

		TurnManager.ChangePhase(currentPlayer.SteamId, TurnManager.Phase.InAction);

		if (fieldOwner == 0 || location.Type == GameLocation.PropertyType.Event) {
			if (location.EventId == "start") {
				if (LobbySettingsSystem.Current.DoublePayment && Networking.IsHost) {
					currentPlayer.Money += 200;
				}

				TurnManager.ChangePhase(currentPlayer.SteamId, TurnManager.Phase.PlayerAction);
				return;
			}

			if (location.EventId == "parking") {
				TurnManager.ChangePhase(currentPlayer.SteamId, TurnManager.Phase.PlayerAction);
				if (LobbySettingsSystem.Current.CollectFreeParking) {
					TurnManager.EmitPayoutFreeParkingEvent(currentPlayer.SteamId);
				}

				return;
			}

			CardActionManager.DisplayCardFor(currentPlayer, location);
			return;
		}

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

		TurnManager.ChangePhase(currentPlayer.SteamId, TurnManager.Phase.PlayerAction);
	}

	public void OnGameEvent(PlayerPaymentEvent eventArgs) {
		if (Networking.IsHost) {
			GetPlayerFromEvent(eventArgs.PlayerId).Money -= eventArgs.Amount;
			GetPlayerFromEvent(eventArgs.Recipient).Money += eventArgs.Amount;
		}
	}

	public void OnGameEvent(EventCardClosedEvent eventArgs) {
		Card card = eventArgs.card;
		card.Action(GetPlayerFromEvent(eventArgs.playerId), MovementManager, TurnManager,
			CardActionManager.BlockedCards, IngameStateManager, card);
		IngameStateManager.State = IngameUI.IngameUiStates.None;
	}

	public void OnGameEvent(GoToJailEvent eventArgs) {
		Player player = GetPlayerFromEvent(eventArgs.playerId);


		if (Networking.IsHost) {
			// Set Jail Status here
			player.JailTurnCounter++;
		}

		CardActionHelper.GoToJail(player, MovementManager);
		TurnManager.ChangePhase(eventArgs.playerId, TurnManager.Phase.InMovement);
	}

	public void OnGameEvent(PayJailFineEvent eventArgs) {
		Player player = GetPlayerFromEvent(eventArgs.playerId);


		if (Networking.IsHost) {
			player.Money -= 50;
			player.JailTurnCounter = 0;
		}

		TurnManager.ChangePhase(player.SteamId, TurnManager.Phase.Rolling);
	}

	public void OnGameEvent(UseJailCardEvent eventArgs) {
		var player = GetPlayerFromEvent(eventArgs.playerId);

		if (Networking.IsHost) {
			string ownedJailFreeCard = IngameStateManager.OwnedFields
			                                             .First(pair =>
				                                             pair.Key.Contains("JailFree") &&
				                                             pair.Value == player.SteamId).Key;
			player.JailTurnCounter = 0;

			IngameStateManager.OwnedFields[ownedJailFreeCard] = 0;
			CardActionManager.RemoveBlockedCard(ownedJailFreeCard.Contains("chance") ? 9 : 5);
		}

		TurnManager.ChangePhase(player.SteamId, TurnManager.Phase.Rolling);
	}

	public void OnGameEvent(LandOnJailEvent eventArgs) {
		Player player = GetPlayerFromEvent(eventArgs.playerId);


		// If player lands on the jail field with > 0 it means he was sent to jail so we end the turn immediately 
		if (player.JailTurnCounter >= 1) {
			// We need to box this if because this event is broadcast end every player should go in the first if if its true
			if (!player.IsProxy) {
				TurnManager.EmitTurnFinishedEvent();
			}
		}
		else {
			TurnManager.ChangePhase(player.SteamId, TurnManager.Phase.PlayerAction);
		}
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
		IngameStateManager.Data = LocationContainer.Children[eventArgs.PropertyIndex].Components.Get<GameLocation>();

		foreach (var player in Lobby.Players) {
			IngameStateManager.AuctionBiddings[player.SteamId] = 10;
		}
	}

	public void OnGameEvent(PropertyMortgagedEvent eventArgs) {
		var property = GetLocationFromPropertyIndex(eventArgs.PropertyIndex);
		var price = property.Price / 2;

		if (Networking.IsHost && !property.Mortgaged) {
			property.Mortgaged = true;

			if (Networking.IsHost) {
				Game.ActiveScene.Dispatch(new PlayerPaymentEvent(2, eventArgs.playerId, price));
			}
		}
	}


	public void OnGameEvent(PropertyMortgagePayedEvent eventArgs) {
		var player = GetPlayerFromEvent(eventArgs.playerId);
		var property = GetLocationFromPropertyIndex(eventArgs.PropertyIndex);
		var price = (int)Math.Ceiling(property.Price / 2 * 1.1);

		if (Networking.IsHost && player.Money > price && property.Mortgaged) {
			Game.ActiveScene.Dispatch(new PlayerPaymentEvent(eventArgs.playerId, 2, price));
		}

		property.Mortgaged = false;
	}

	public void OnGameEvent(RolledEvent eventArgs) {
		var player = GetPlayerFromEvent(eventArgs.playerId);


		TurnManager.ChangePhase(eventArgs.playerId, TurnManager.Phase.InMovement);

		player.DoublesCount = eventArgs.Doubles ? player.DoublesCount + 1 : 0;

		// Player should not be in jail!
		if (player.JailTurnCounter <= 0) {
			if (player.DoublesCount < 3) {
				MovementManager.StartMovement(player, eventArgs.Number);
			}
			else {
				TurnManager.EmitSpecialPropertyActionEvent(TurnManager.SpecialPropertyActionType.Police,
					player.SteamId);
			}

			return;
		}

		player.LastDiceCount = eventArgs.Number;

		// Release player if he rolls double
		if (player.DoublesCount > 0) {
			player.DoublesCount = 0;
			player.JailTurnCounter = 0;
			TurnManager.ChangePhase(player.SteamId, TurnManager.Phase.Rolling);
		}
		else {
			// Change to Action Phase so player can handle its properties etc.
			if (Networking.IsHost) {
				player.JailTurnCounter++;
			}

			TurnManager.ChangePhase(player.SteamId, TurnManager.Phase.PlayerAction);
		}

		// 4 means the next turn would be the fourth turn in jail
		if (player.JailTurnCounter == 4) {
			// Force player to use card or pay caution
			TurnManager.ChangePhase(player.SteamId, TurnManager.Phase.Jail);
		}
	}

	public void OnGameEvent(DebugEvent eventArgs) {
		IngameStateManager.State = IngameUI.IngameUiStates.EndScreen;
	}


	public void OnGameEvent(TurnFinishedEvent eventArgs) {
		Log.Info("Turn over!");
		Log.Info("");


		var currentLobbyPlayers = TurnManager.CurrentLobby.Players;

		if (currentLobbyPlayers.Count(player => !(player.EliminatedPosition > 0)) == 1) {
			IngameStateManager.State = IngameUI.IngameUiStates.EndScreen;
			return;
		}

		do {
			TurnManager.CurrentPlayerIndex = (TurnManager.CurrentPlayerIndex + 1) % currentLobbyPlayers.Count;
		} while (!(currentLobbyPlayers[TurnManager.CurrentPlayerIndex].EliminatedPosition > 0));

		SetCurrentPlayersJailState();
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


	private Player GetPlayerFromEvent(ulong playerId) {
		return Lobby.Players.Find(player => player.SteamId == playerId);
	}

	private GameLocation GetLocationFromPropertyIndex(int propertyIndex) {
		return LocationContainer.Children[propertyIndex].Components.Get<GameLocation>();
	}

	[Broadcast]
	private void SetCurrentPlayersJailState() {
		Player currentPlayer = TurnManager.CurrentLobby.Players[TurnManager.CurrentPlayerIndex];


		if (currentPlayer.JailTurnCounter > 0) {
			TurnManager.ChangePhase(currentPlayer.SteamId, TurnManager.Phase.Jail);
		}
		else {
			TurnManager.ChangePhase(currentPlayer.SteamId, TurnManager.Phase.Rolling);
		}
	}

	private void ChangeDiceOwnershipToCurrentPlayer() {
		if (!Networking.IsHost) {
			return;
		}

		if (_dice.Count == 0) {
			_dice = new(Game.ActiveScene.GetAllComponents<Dice>());
		}

		Player currentPlayer = Lobby.Players[TurnManager.CurrentPlayerIndex];
		_dice.ForEach(dice => dice.GameObject.Network.AssignOwnership(currentPlayer.Connection));
	}

	public void OnGameEvent(TradingRequestedEvent eventArgs) {
		IngameStateManager.State = IngameUI.IngameUiStates.Trade;
	}

	public void OnGameEvent(TradingDeniedEvent eventArgs) {
		IngameStateManager.State = IngameUI.IngameUiStates.None;

		ResetTrading();
		CloseLocalUIForEveryPlayer();
	}

	public void OnGameEvent(TradingAcceptedEvent eventArgs) {
		IngameStateManager.State = IngameUI.IngameUiStates.None;

		foreach (var (key, value) in TradeState.OfferTradeFields) {
			if (value) {
				IngameStateManager.OwnedFields[key] = TradeState.TradingPartner.SteamId;
			}
		}

		foreach (var (key, value) in TradeState.RequestTradeFields) {
			if (value) {
				IngameStateManager.OwnedFields[key] = TradeState.TradingCreator.SteamId;
			}
		}

		if (Networking.IsHost) {
			TradeState.TradingCreator.Money -= TradeState.TradingOfferAmount;
			TradeState.TradingCreator.Money += TradeState.TradingRequestAmount;

			TradeState.TradingPartner.Money += TradeState.TradingOfferAmount;
			TradeState.TradingPartner.Money -= TradeState.TradingRequestAmount;
		}

		ResetTrading();
		CloseLocalUIForEveryPlayer();
	}

	private void CloseLocalUIForEveryPlayer() {
		foreach (var player in Lobby.Players) {
			player.localUiState = IngameUI.LocalUIStates.None;
		}
	}

	private void ResetTrading() {
		TradeState.TradingCreator = null;
		TradeState.TradingPartner = null;
		TradeState.TradingOfferAmount = 0;
		TradeState.TradingRequestAmount = 0;

		foreach (var key in TradeState.OfferTradeFields.Keys) {
			TradeState.OfferTradeFields[key] = false;
			TradeState.RequestTradeFields[key] = false;
		}
	}

	public void OnGameEvent(NotEnoughFundsEvent eventArgs) {
		GetPlayerFromEvent(eventArgs.PlayerId).localUiState = IngameUI.LocalUIStates.NotEnoughFunds;
		IngameStateManager.Data = eventArgs;
	}

	public void OnGameEvent(PlayerBankruptEvent eventArgs) {
		Player player = GetPlayerFromEvent(eventArgs.PlayerId);

		var locations = Game.ActiveScene.Children[0];

		if (eventArgs.Recipient is 1 or 2) {
			// auction for every street in players possession
			// refactor auction event so its gets an array/list of properties to handle

			foreach (var (key, value) in IngameStateManager.OwnedFields) {
				if (key.Contains("Jail") && value == eventArgs.PlayerId) {
					continue;
				}

				var gameLocation = locations.Children.First(go => go.Name.Equals(key)).Components.Get<GameLocation>();
				gameLocation.Houses = 0;
				_auctionLocations.Push(gameLocation.PropertyIndex);
			}

			if (_auctionLocations.Any()) {
				TurnManager.EmitPropertyAuctionEvent(_auctionLocations.Pop(), player.SteamId);
			}

			return;
		}


		Player recipient = GetPlayerFromEvent(eventArgs.Recipient);

		TurnManager.EmitPlayerPaymentEvent(eventArgs.PlayerId, eventArgs.Recipient, player.Money);

		foreach (var key in IngameStateManager.OwnedFields.Keys) {
			IngameStateManager.OwnedFields[key] = recipient.SteamId;
		}

		player.EliminatedPosition = ++Player.EliminatedCount;
		player.GameObject.Enabled = false;

		TurnManager.EmitTurnFinishedEvent();
	}
}
