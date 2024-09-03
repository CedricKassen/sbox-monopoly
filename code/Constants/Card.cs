using System;
using Sandbox.Events;
using Sandbox.Events.TurnEvents;

namespace Sandbox.Constants;

public delegate void CardAction(Player player, MovementManager movementManager = null, TurnManager turnManager = null,
                                Dictionary<int, bool> blockedCards = null,
                                IngameStateManager ingameStateManager = null, Card card = null);

public class Card {
	public Card(int actionId, CardAction action, string text, string imageUrl = null) {
		ActionId = actionId;
		Text = text;
		ImageUrl = imageUrl;
		Action = action;
	}

	public int ActionId { get; }
	public string Text { get; }
	public string ImageUrl { get; }

	public CardAction Action { get; }


	public override string ToString() {
		return Text + "(" + ActionId + ")";
	}
}

public static class CardActionHelper {
	public static void Shuffle<T>(IList<T> list) {
		var rng = new Random();
		var n = list.Count();
		while (n > 1) {
			n--;
			var k = rng.Next(n + 1);
			(list[k], list[n]) = (list[n], list[k]);
		}
	}

	/*
	 * Creates an new NetList from normal list that are not matching the predicate
	 */
	public static NetList<T> CreateNetList<T>(List<T> list) {
		NetList<T> newList = new();
		foreach (var obj in list) {
			newList.Add(obj);
		}

		return newList;
	}

	public static void PayForHouses(Player player, int housePrice, int hotelPrice, MovementManager move,
	                                IngameStateManager stateManager) {
		var amount = 0;

		foreach (var locationObj in move.LocationContainer.Children) {
			var location = locationObj.Components.Get<GameLocation>();


			// Check if location can be owned and is owned by player 
			if (location.Name == null || location.Type.Equals(GameLocation.PropertyType.Event) &&
			    location.EventId.Contains("tax")) {
				continue;
			}


			var ownerId = stateManager.OwnedFields[locationObj.Name];
			if (ownerId != player.SteamId) {
				continue;
			}

			amount += location.Houses != 5 ? location.Houses * housePrice : hotelPrice;
		}

		Game.ActiveScene.Dispatch(new PlayerPaymentEvent(player.SteamId, 1, amount));

		Log.Info(player.Name + " payed " + amount + " for houses and hotels!");
	}

	public static int CalculateFieldsToTravel(Player player, int targetField) {
		var currentField = player.CurrentField;

		if (currentField < targetField) {
			return targetField - currentField;
		}

		int result = 40 - currentField + targetField;
		if (result == 0) {
			Log.Error("Calculated movement distance was zero!");
		}

		return result;
	}

	public static void CollectFromAll(Player player, int amount, TurnManager turnManager) {
		List<Player> allPlayers = new(Game.ActiveScene.GetAllComponents<Player>());
		if (allPlayers.Count <= 0) {
			turnManager.ChangePhase(player.SteamId, TurnManager.Phase.PlayerAction);
			return;
		}

		allPlayers.ForEach(otherPlayer =>
			Game.ActiveScene.Dispatch(new PlayerPaymentEvent(otherPlayer.SteamId, player.SteamId, amount)));
	}

	public static void PayToAll(Player player, int amount, TurnManager turnManager) {
		List<Player> allPlayers =
			new(Game.ActiveScene.GetAllComponents<Player>().Where(ply => ply.SteamId != player.SteamId));
		if (allPlayers.Count <= 0) {
			turnManager.ChangePhase(player.SteamId, TurnManager.Phase.PlayerAction);
			return;
		}

		allPlayers.ForEach(otherPlayer =>
			Game.ActiveScene.Dispatch(new PlayerPaymentEvent(player.SteamId, otherPlayer.SteamId, amount)));
	}

	public static int FindNearestLine(int playerCurrentField) {
		var lines = new List<int> { 5, 15, 25, 35 };
		return lines.Find(field => playerCurrentField < field || (playerCurrentField > 35 && field == 5));
	}

	public static void GoToJail(Player player, MovementManager movementManager) {
		var currentPos = player.CurrentField;
		player.HasBonusMove = false;

		if (currentPos < 10) {
			movementManager.StartMovement(player, CalculateFieldsToTravel(player, 10));
		}
		else {
			movementManager.StartMovement(player, -(currentPos - 10));
		}
	}

	public static void MoveTo(int fieldIndex, Player player, MovementManager movementManager) {
		movementManager.StartMovement(player, CalculateFieldsToTravel(player, fieldIndex));
	}
}
