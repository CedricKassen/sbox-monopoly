using System;

namespace Sandbox.Constants;

public delegate void CardAction(Player player, MovementManager movementManager = null,
	Dictionary<Card, bool> blockedCards = null, IngameStateManager ingameStateManager = null, Card card = null);

public class Card
{
	public Card(int actionId, CardAction action, string text, string imageUrl = null)
	{
		ActionId = actionId;
		Text = text;
		ImageUrl = imageUrl;
		Action = action;
	}

	public int ActionId { get; }
	public string Text { get; }
	public string ImageUrl { get; }

	public CardAction Action { get; }


	public override string ToString()
	{
		return Text;
	}
}

public static class CardActionHelper
{
	public static void Shuffle<T>(IList<T> list)
	{
		var rng = new Random();
		var n = list.Count;
		while (n > 1)
		{
			n--;
			var k = rng.Next(n + 1);
			(list[k], list[n]) = (list[n], list[k]);
		}
	}

	public static int CalculateFieldsToTravel(Player player, int targetField)
	{
		var currentField = player.CurrentField;

		if (currentField < targetField)
		{
			return targetField - currentField;
		}

		return 40 - currentField + targetField;
	}

	public static void CollectFromAll(Player player, int amount)
	{
		List<Player> allPlayers = new(Game.ActiveScene.GetAllComponents<Player>());
		player.Money -= amount * allPlayers.Count;
		allPlayers.ForEach(otherPlayer => otherPlayer.Money += amount);
	}

	public static int FindNearestLine(int playerCurrentField)
	{
		var lines = new List<int> { 5, 15, 25, 35 };
		return lines.Find(field => playerCurrentField < field || (playerCurrentField > 35 && field == 5));
	}

	public static void GoToJail(Player player, MovementManager movementManager)
	{
		var currentPos = player.CurrentField;

		var fieldsToTravel = 0;
		if (currentPos < 10)
		{
			movementManager.StartMovement(player, CalculateFieldsToTravel(player, 10));
		}
		else
		{
			movementManager.StartMovement(player, -(currentPos - 10));
		}
	}

	public static void MoveTo(int fieldIndex, Player player, MovementManager movementManager)
	{
		movementManager.StartMovement(player, CalculateFieldsToTravel(player, fieldIndex));
	}
}
