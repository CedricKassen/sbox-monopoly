using System;
using System.Threading.Tasks;
using Monopoly.UI.Screens.GameLoop;
using Sandbox.Constants;

public sealed class CardActionManager : Component
{
	[Property] private List<Card> ChanceCards;
	[Property] private List<Card> CommunityCards;

	[Property] public MovementManager MovementManager { get; set; }
	[Property] public IngameStateManager IngameStateManager { get; set; }

	protected override Task OnLoad()
	{
		FillChangeCards();
		FillCommunityCards();
		return base.OnLoad();
	}

	private void FillChangeCards()
	{
		ChanceCards = new List<Card>(Cards.Chance);
		Shuffle(ChanceCards);
	}

	private void FillCommunityCards()
	{
		CommunityCards = new List<Card>(Cards.CommunityChest);
		Shuffle(ChanceCards);
	}

	private static void Shuffle<T>(IList<T> list)
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


	public void DisplayCardFor(Player player, GameLocation location)
	{
		Log.Info("Show " + location.EventId);

		switch (location.Type) {
			case GameLocation.PropertyType.Event:
				if (location.EventId == "chance") {
					DisplayChance(player, location);
				}
				if (location.EventId == "community")
				{
					DisplayCommunity(player, location);
				}

				break;
			default:
				DisplayPropertyCard(player, location);
				break;
		}
	}

	private void DisplayChance(Player player, GameLocation location)
	{
		var card = ChanceCards[0];
		ChanceCards.Remove(card);


		switch (card.ActionId)
		{
			default:
			case 2:
				Log.Info("Advance to Go");
				MovementManager.StartMovement(player, 39 - player.CurrentField + 1);
				break;
			case 10:
				// TODO implement backwards movement
				break;
		}

		if (ChanceCards.Count == 0)
		{
			FillChangeCards();
		}
	}

	private void DisplayCommunity(Player getPlayerFromEvent, GameLocation location)
	{
		var card = CommunityCards[0];
		CommunityCards.Remove(card);

		if (CommunityCards.Count == 0)
		{
			FillCommunityCards();
		}
	}

	private void DisplayPropertyCard(Player player, GameLocation location) {
		IngameStateManager.State = IngameUI.IngameUiStates.Buying;
		IngameStateManager.Data = location;
	}
}
