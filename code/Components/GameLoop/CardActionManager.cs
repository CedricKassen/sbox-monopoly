using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Monopoly.UI.Screens.GameLoop;
using Sandbox.Constants;
using Sandbox.Events;
using Sandbox.Events.GameStateEvents;

public sealed class CardActionManager : Component, IGameEventHandler<GameStartEvent> {
	[Property] private readonly Dictionary<int, bool> BlockedCards = new();

	[HostSync] private NetList<int> ChanceCardsOrder { get; set; }

	[HostSync] private NetList<int> CommunityCardsOrder { get; set; }

	[Property] private List<Card> ChanceCards;

	[Property] private List<Card> CommunityCards;

	[Property] public MovementManager MovementManager { get; set; }

	[Property] public IngameStateManager IngameStateManager { get; set; }

	[Property] public TurnManager TurnManager { get; set; }

	public void RemoveBlockedCard(int actionId) {
		BlockedCards.Remove(actionId);
	}

	private void RefillChangeCards() {
		if (Networking.IsHost) {
			ChanceCardsOrder = CreateCardOrderListFromCardCount(Cards.Chance_Standard.Count);
			FillChanceCards();
		}
	}

	private void RefillCommunityCards() {
		if (Networking.IsHost) {
			CommunityCardsOrder = CreateCardOrderListFromCardCount(Cards.CommunityChest_Standard.Count);
			FillCommunityCards();
		}
	}

	[Broadcast]
	private void FillCommunityCards() {
		CommunityCards = new();
		foreach (var index in CommunityCardsOrder) {
			Card card = Cards.CommunityChest_Standard[index];
			if (BlockedCards.ContainsKey(card.ActionId)) {
				continue;
			}

			CommunityCards.Add(card);
		}
	}

	[Broadcast]
	private void FillChanceCards() {
		ChanceCards = new();
		foreach (var index in ChanceCardsOrder) {
			Card card = Cards.Chance_Standard[index];

			if (BlockedCards.ContainsKey(card.ActionId)) {
				continue;
			}

			ChanceCards.Add(card);
		}
	}

	private NetList<int> CreateCardOrderListFromCardCount(int count) {
		List<int> indexes = new();
		for (int i = 0; i < count; i++) {
			indexes.Add(i);
		}

		CardActionHelper.Shuffle(indexes);
		return CardActionHelper.CreateNetList(indexes);
	}


	public void DisplayCardFor(Player player, GameLocation location) {
		switch (location.Type) {
			case GameLocation.PropertyType.Event:
				if (location.EventId == "chance") {
					DisplayChance(player);
					TurnManager.EmitSpecialPropertyActionEvent(TurnManager.SpecialPropertyActionType.Chance,
						player.SteamId);
				}
				else if (location.EventId == "community") {
					DisplayCommunity(player);
					TurnManager.EmitSpecialPropertyActionEvent(TurnManager.SpecialPropertyActionType.CommunityChest,
						player.SteamId);
				}
				else if (location.EventId == "income_tax") {
					var percentageTax = (int)Math.Ceiling(player.Money * 0.1);
					player.Money -= percentageTax < 200 ? percentageTax : 200;
					TurnManager.EmitSpecialPropertyActionEvent(TurnManager.SpecialPropertyActionType.Tax,
						player.SteamId);
				}
				else if (location.EventId == "luxury_tax") {
					player.Money -= 100;
					TurnManager.EmitSpecialPropertyActionEvent(TurnManager.SpecialPropertyActionType.Tax,
						player.SteamId);
				}
				else if (location.EventId == "police") {
					TurnManager.EmitSpecialPropertyActionEvent(TurnManager.SpecialPropertyActionType.Police,
						player.SteamId);
				}
				else if (location.EventId == "jail") {
					TurnManager.EmitSpecialPropertyActionEvent(TurnManager.SpecialPropertyActionType.Jail,
						player.SteamId);
				}
				else if (location.EventId == "") {
					TurnManager.EmitSpecialPropertyActionEvent(TurnManager.SpecialPropertyActionType.None,
						player.SteamId);
				}

				break;
			default:
				DisplayPropertyCard(player, location);
				break;
		}
	}

	private void DisplayChance(Player player) {
		var card = ChanceCards[0];
		ChanceCards.Remove(card);

		Log.Info("Drew " + card.Text);
		Log.Info(player);
		Log.Info(card.Action);

		IngameStateManager.Data = card;
		IngameStateManager.State = IngameUI.IngameUiStates.Chance;


		Log.Info("before action");
		card.Action(player,
			MovementManager,
			TurnManager,
			BlockedCards,
			IngameStateManager,
			card);
		Log.Info("after action");

		if (ChanceCards.Count == 0) {
			RefillChangeCards();
		}
	}

	private void DisplayCommunity(Player player) {
		var card = CommunityCards[0];
		CommunityCards.Remove(card);

		Log.Info("Drew " + card.Text);


		IngameStateManager.Data = card;
		IngameStateManager.State = IngameUI.IngameUiStates.Community_Chest;

		card.Action(player, MovementManager, TurnManager, BlockedCards, IngameStateManager, card);

		if (CommunityCards.Count == 0) {
			RefillCommunityCards();
		}
	}


	private void DisplayPropertyCard(Player player, GameLocation location) {
		// IngameStateManager.State = IngameUI.IngameUiStates.Buying;
		player.localUiState = IngameUI.LocalUIStates.Buying;
		IngameStateManager.Data = location;
	}

	public void OnGameEvent(GameStartEvent eventArgs) {
		RefillChangeCards();
		RefillCommunityCards();
	}
}
