using System;
using System.Threading.Tasks;
using EnumExtensions.Settings;
using EnumExtensions.Util;
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
                                IGameEventHandler<UseJailCardEvent>,
                                IGameEventHandler<TurnActionDoneEvent>, IGameEventHandler<NotEnoughFundsEvent>,
                                IGameEventHandler<PlayerBankruptEvent>, IGameEventHandler<StartMovementEvent>,
                                IGameEventHandler<StartBonusMove> {
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
		TurnManager.EmitPropertyAquiredEvent(eventArgs.playerId, eventArgs.PropertyIndex, true);

		if (Networking.IsHost) {
			TurnManager.EmitPlayerPaymentEvent(eventArgs.playerId, 2, eventArgs.Amount);
		}

		if (_auctionLocations.Any()) {
			TurnManager.EmitPropertyAuctionEvent(_auctionLocations.Pop(), eventArgs.playerId);
			return;
		}

		Player player = GetPlayerFromEvent(eventArgs.playerId);
		if (player.EliminatedPosition <= 0) {
			TurnManager.ChangePhase(player.SteamId, TurnManager.Phase.PlayerAction);
		}
		else {
			TurnManager.EmitTurnFinishedEvent();
		}


		IngameStateManager.State = IngameUI.IngameUiStates.None;
	}

	public async void OnGameEvent(StartRollEvent eventArgs) {
		if (_dice.Count == 0) {
			_dice = new(Game.ActiveScene.GetAllComponents<Dice>());
		}

		foreach (var dice in _dice) {
			dice.Roll(GetPlayerFromEvent(eventArgs.PlayerId));
		}

		TurnManager.ChangePhase(eventArgs.PlayerId, TurnManager.Phase.InAction);

		while (_dice.Any(dice => dice.IsRolling)) {
			await Task.DelayRealtimeSeconds(0.5f);
		}

		// If no third speed dice is present OR if player does not have round count above 0 to activate speed dice
		if (_dice.Count == 2 || GetPlayerFromEvent(eventArgs.PlayerId).RoundCount == 0) {
			TurnManager.EmitRolledEvent((ulong)Game.SteamId, _dice[0].GetRoll().AsInt(), _dice[1].GetRoll().AsInt());
		}
		else {
			TurnManager.EmitRolledEventWithSpeedDice((ulong)Game.SteamId, _dice[0].GetRoll().AsInt(),
				_dice[1].GetRoll().AsInt(), _dice[2].GetRoll().AsInt());
		}

		ShowDiceRoll();
	}

	[Broadcast]
	private async void ShowDiceRoll() {
		IngameStateManager.ShowRoll = true;
		await Task.DelayRealtimeSeconds(3f);
		IngameStateManager.ShowRoll = false;
	}

	public void OnGameEvent(TurnActionDoneEvent eventArgs) {
		TurnManager.ChangePhase(eventArgs.PlayerId, eventArgs.NewPhase);
	}

	public void OnGameEvent(BuildHouseEvent eventArgs) {
		var property = GetLocationFromPropertyIndex(eventArgs.PropertyIndex);
		if (property.Houses == 5) {
			GameSounds.PlayUI(UiSounds.BtnDeny);
			Log.Error("Tried to add house to maxed out house!");
			return;
		}

		var player = GetPlayerFromEvent(eventArgs.PlayerId);

		// Check if player can afford this house 
		if (player.Money - property.House_Cost < 0) {
			GameSounds.PlayUI(UiSounds.BtnDeny);
			Log.Error("Player cant afford this house!");
			return;
		}


		// Check if player builds evenly
		foreach (var member in property.GroupMembers) {
			// If one street of the group got more houses then current street prevent building we cant go from 2 1 1 -> 3 1 1
			if (property.Houses > LocationContainer.Children[member].Components.Get<GameLocation>().Houses) {
				GameSounds.PlayUI(UiSounds.BtnDeny);
				return;
			}
		}

		GameSounds.PlayUI(UiSounds.BtnPress);

		if (Networking.IsHost) {
			TurnManager.EmitPlayerPaymentEvent(player.SteamId, 2, property.House_Cost, TurnManager.CurrentPhase);
		}


		property.Houses++;
	}

	public void OnGameEvent(DestroyHouseEvent eventArgs) {
		var property = GetLocationFromPropertyIndex(eventArgs.PropertyIndex);
		if (property.Houses == 0) {
			GameSounds.PlayUI(UiSounds.BtnDeny);
			Log.Error("Tried to remove house from empty property!");
			return;
		}

		// Check if player builds evenly
		foreach (var member in property.GroupMembers) {
			// If one street of the group got less houses then current street prevent building we cant go from 2 1 1 -> 2 1 0
			if (property.Houses < LocationContainer.Children[member].Components.Get<GameLocation>().Houses) {
				GameSounds.PlayUI(UiSounds.BtnDeny);
				return;
			}
		}

		GameSounds.PlayUI(UiSounds.BtnPress);
		var player = GetPlayerFromEvent(eventArgs.PlayerId);

		TurnManager.EmitLocalPlayerPaymentEvent(2, player.SteamId, property.House_Cost / 2, TurnManager.CurrentPhase);

		property.Houses--;
	}

	public void OnGameEvent(MovementDoneEvent eventArgs) {
		var location = eventArgs.Location;
		IngameStateManager.OwnedFields.TryGetValue(location.GameObject.Name, out var fieldOwner);
		var currentPlayer = GetPlayerFromEvent(eventArgs.playerId);

		TurnManager.ChangePhase(currentPlayer.SteamId, TurnManager.Phase.InAction);

		if (fieldOwner == 0 || location.Type == GameLocation.PropertyType.Event) {
			if (location.EventId == "start") {
				Log.Info(LobbySettingsSystem.Current.DoublePayment);
				if (LobbySettingsSystem.Current.DoublePayment && Networking.IsHost) {
					TurnManager.EmitPlayerPaymentEvent(2, currentPlayer.SteamId, 200);
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

				Log.Info("Price " + price);

				TurnManager.EmitLocalPlayerPaymentEvent(eventArgs.playerId, fieldOwner, price);
				return;
			}

			if (location.Type == GameLocation.PropertyType.Railroad) {
				var railroadCount = IngameStateManager.OwnedFields
				                                      .Count(f => f.Value == fieldOwner && f.Key.Contains("railroad"));
				TurnManager.EmitLocalPlayerPaymentEvent(eventArgs.playerId, fieldOwner,
					location.Railroad_Rent[railroadCount - 1]);
				return;
			}

			if (location.Type == GameLocation.PropertyType.Utility) {
				var utilityCount = IngameStateManager.OwnedFields.Count(f => f.Value == fieldOwner &&
				                                                             (f.Key == "electricCompany" ||
				                                                              f.Key == "waterCompany"));

				Log.Info("Utilities owned: " + utilityCount);
				Log.Info("Last throw: " + currentPlayer.LastDiceCount);

				TurnManager.EmitLocalPlayerPaymentEvent(eventArgs.playerId, fieldOwner,
					location.Utility_Rent_Multiplier[utilityCount - 1] * currentPlayer.LastDiceCount);
				return;
			}
		}

		TurnManager.ChangePhase(currentPlayer.SteamId, TurnManager.Phase.PlayerAction);
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

		TurnManager.ChangePhase(player.SteamId, TurnManager.Phase.InAction);
		TurnManager.EmitLocalPlayerPaymentEvent(player.SteamId, 1, 50, TurnManager.Phase.Rolling);
		player.JailTurnCounter = 0;
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
			TurnManager.EmitPlayerPaymentEvent(player.SteamId, 2, costs);
			IngameStateManager.OwnedFields[location.Name] = eventArgs.playerId;
		}
	}

	public void OnGameEvent(PropertyAuctionEvent eventArgs) {
		bool anyPlayerHasEnoughFunds = false;

		IngameStateManager.AuctionBiddings = new NetDictionary<ulong, int>();
		foreach (var player in Lobby.Players) {
			anyPlayerHasEnoughFunds = anyPlayerHasEnoughFunds || player.Money >= 20;
			IngameStateManager.AuctionBiddings[player.SteamId] = 10;
		}

		if (!anyPlayerHasEnoughFunds) {
			IngameStateManager.AuctionBiddings = new NetDictionary<ulong, int>();
			Player player = GetPlayerFromEvent((ulong)Game.SteamId);

			if (player.localUiStateCache.Equals(IngameUI.LocalUIStates.None)) {
				_auctionLocations = new();
				TurnManager.EmitTurnFinishedEvent();
			}

			IngameStateManager.State = IngameUI.IngameUiStates.None;
			player.localUiState = player.localUiStateCache;
			player.localUiStateCache = IngameUI.LocalUIStates.None;
			return;
		}

		IngameStateManager.State = IngameUI.IngameUiStates.Auction;
		IngameStateManager.Data = LocationContainer.Children[eventArgs.PropertyIndex].Components.Get<GameLocation>();
	}

	public void OnGameEvent(PropertyMortgagedEvent eventArgs) {
		var property = GetLocationFromPropertyIndex(eventArgs.PropertyIndex);
		var price = property.Price / 2;

		if (!property.Mortgaged) {
			property.Mortgaged = true;
			GameSounds.PlayUI(UiSounds.BtnPress);
			if (Networking.IsHost) {
				Game.ActiveScene.Dispatch(
					new PlayerPaymentEvent(2, eventArgs.playerId, price, TurnManager.CurrentPhase));
			}
		}

		GameSounds.PlayUI(UiSounds.BtnDeny);
	}


	public void OnGameEvent(PropertyMortgagePayedEvent eventArgs) {
		var player = GetPlayerFromEvent(eventArgs.playerId);
		var property = GetLocationFromPropertyIndex(eventArgs.PropertyIndex);
		var price = (int)Math.Ceiling(property.Price / 2 * 1.1);

		if (player.Money > price && property.Mortgaged) {
			if (Networking.IsHost) {
				Game.ActiveScene.Dispatch(
					new PlayerPaymentEvent(eventArgs.playerId, 2, price, TurnManager.CurrentPhase));
			}

			GameSounds.PlayUI(UiSounds.BtnPress);
			property.Mortgaged = false;
		}

		GameSounds.PlayUI(UiSounds.BtnDeny);
	}

	public void OnGameEvent(RolledEvent eventArgs) {
		Player player = GetPlayerFromEvent(eventArgs.playerId);


		player.DoublesCount = eventArgs.Doubles ? player.DoublesCount + 1 : 0;

		// Player should not be in jail!
		if (player.JailTurnCounter <= 0) {
			player.LastDiceCount = eventArgs.Number;

			TurnManager.ChangePhase(eventArgs.playerId, TurnManager.Phase.InMovement);
			if (player.DoublesCount == 3) {
				TurnManager.EmitSpecialPropertyActionEvent(TurnManager.SpecialPropertyActionType.Police,
					player.SteamId);
				return;
			}

			// Show "Bonus move" instead of "End turn" after turn. Player move to next unowned or if nothing is
			// unowned next location he has to pay on
			player.HasBonusMove = eventArgs.Forward;
			if (eventArgs.Bus) {
				// Player can choose to move either the value of die one, two or both.
				TurnManager.ChangePhase(player.SteamId, TurnManager.Phase.ChooseMove);
			}
			else {
				// Player moves given amount
				MovementManager.StartMovement(player, eventArgs.Number);
			}


			return;
		}


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


	public void OnGameEvent(TurnFinishedEvent eventArgs) {
		var currentLobbyPlayers = TurnManager.CurrentLobby.AllPlayers;

		Log.Info("Turn finished");
		Log.Info("");

		if (currentLobbyPlayers.Count > 1 &&
		    currentLobbyPlayers.Count(player => !(player.EliminatedPosition > 0)) == 1) {
			IngameStateManager.State = IngameUI.IngameUiStates.EndScreen;
			return;
		}

		do {
			TurnManager.CurrentPlayerIndex = (TurnManager.CurrentPlayerIndex + 1) % currentLobbyPlayers.Count;
		} while (currentLobbyPlayers[TurnManager.CurrentPlayerIndex].EliminatedPosition > 0);

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
		Player currentPlayer = TurnManager.CurrentLobby.AllPlayers[TurnManager.CurrentPlayerIndex];


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

		Player currentPlayer = Lobby.AllPlayers[TurnManager.CurrentPlayerIndex];
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

		TurnManager.EmitLocalPlayerPaymentEvent(TradeState.TradingCreator.SteamId, TradeState.TradingPartner.SteamId,
			TradeState.TradingOfferAmount);
		TurnManager.EmitLocalPlayerPaymentEvent(TradeState.TradingPartner.SteamId, TradeState.TradingCreator.SteamId,
			TradeState.TradingRequestAmount);

		ResetTrading();
		CloseLocalUIForEveryPlayer();
	}

	private void CloseLocalUIForEveryPlayer() {
		foreach (var player in Lobby.Players) {
			player.LocalUiOpen = false;
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
		if ((ulong)Game.SteamId != eventArgs.PlayerId) {
			return;
		}

		GetPlayerFromEvent(eventArgs.PlayerId).localUiState = IngameUI.LocalUIStates.NotEnoughFunds;
		IngameStateManager.Data = eventArgs;
	}

	public void OnGameEvent(PlayerBankruptEvent eventArgs) {
		Player player = GetPlayerFromEvent(eventArgs.PlayerId);
		player.EliminatedPosition = ++Player.EliminatedCount;


		var locations = Game.ActiveScene.Children[0];

		if (eventArgs.Recipient is 1 or 2) {
			// auction for every street in players possession
			// refactor auction event so its gets an array/list of properties to handle

			foreach (var (key, value) in IngameStateManager.OwnedFields) {
				if (value != eventArgs.PlayerId) {
					continue;
				}

				IngameStateManager.OwnedFields[key] = 0;

				if (key.Contains("Jail")) {
					CardActionManager.RemoveBlockedCard(key.Contains("chance") ? 9 : 5);
					continue;
				}

				var gameLocation = locations.Children.First(go => go.Name.Equals(key)).Components.Get<GameLocation>();
				gameLocation.Houses = 0;
				_auctionLocations.Push(gameLocation.PropertyIndex);
			}

			if (_auctionLocations.Any()) {
				TurnManager.EmitPropertyAuctionEvent(_auctionLocations.Pop(), player.SteamId);
			}
			else {
				TurnManager.EmitTurnFinishedEvent();
			}
		}
		else {
			Player recipient = GetPlayerFromEvent(eventArgs.Recipient);
			int playerWorth = player.Money;
			foreach (var (key, value) in IngameStateManager.OwnedFields) {
				if (value != eventArgs.PlayerId) {
					continue;
				}

				IngameStateManager.OwnedFields[key] = recipient.SteamId;

				if (key.Contains("Jail")) {
					continue;
				}

				var gameLocation = locations.Children.First(go => go.Name.Equals(key)).Components.Get<GameLocation>();
				playerWorth += gameLocation.House_Cost * gameLocation.Houses;
			}

			TurnManager.EmitLocalPlayerPaymentEvent(eventArgs.PlayerId, eventArgs.Recipient, playerWorth);
			TurnManager.EmitTurnFinishedEvent();
		}

		player.localUiState = IngameUI.LocalUIStates.None;
	}

	public void OnGameEvent(StartMovementEvent eventArgs) {
		Player player = GetPlayerFromEvent(eventArgs.PlayerId);
		MovementManager.StartMovement(player, eventArgs.Amount);
	}

	public void OnGameEvent(StartBonusMove eventArgs) {
		Player player = GetPlayerFromEvent(eventArgs.PlayerId);
		player.HasBonusMove = false;
		int indexOfNextField = GetIndexOfNextFieldForSpeedDice(player.CurrentField, player.SteamId);

		if (indexOfNextField == 0) {
			TurnManager.EmitTurnFinishedEvent();
			return;
		}

		MovementManager.StartMovement(player, CardActionHelper.CalculateFieldsToTravel(player, indexOfNextField));
	}

	private int GetIndexOfNextFieldForSpeedDice(int currentField, ulong playerId) {
		var locations = LocationContainer.Children;

		int indexToNextForeignOwnedField = -1;
		Log.Info("CurrentField " + currentField);
		// Solange i nicht bei currentField landet
		for (var i = currentField + 1; Monopoly.Math.Mod(i, 40) != currentField; i++) {
			int modI = Monopoly.Math.Mod(i, 40);
			GameLocation gl = locations[modI].Components.Get<GameLocation>();
			if (gl.Type.Equals(GameLocation.PropertyType.Event)) {
				continue;
			}

			ulong fieldOwner = IngameStateManager.OwnedFields[locations[modI].Name];
			if (fieldOwner == 0) {
				// if we find any location NOBODY owns we can interrupt this loop
				return modI;
			}

			if (indexToNextForeignOwnedField == -1 && fieldOwner != playerId) {
				indexToNextForeignOwnedField = modI;
			}
		}

		// If no next field with other owner or unowned field is found return current field so player does not move
		return indexToNextForeignOwnedField != -1 ? indexToNextForeignOwnedField : currentField;
	}
}
