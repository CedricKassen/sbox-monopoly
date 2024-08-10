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

