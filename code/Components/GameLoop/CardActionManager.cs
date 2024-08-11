using System.Threading.Tasks;
using Monopoly.UI.Screens.GameLoop;
using Sandbox.Constants;

public sealed class CardActionManager : Component
{
	[Property] private readonly Dictionary<Card, bool> BlockedCards = new();
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
		ChanceCards = new List<Card>(Cards.Chance_Standard);

		ChanceCards.RemoveAll(card => BlockedCards.ContainsKey(card));

		CardActionHelper.Shuffle(ChanceCards);
	}

	private void FillCommunityCards()
	{
		CommunityCards = new List<Card>(Cards.CommunityChest_Standard);

		CommunityCards.RemoveAll(card => BlockedCards.ContainsKey(card));

		CardActionHelper.Shuffle(CommunityCards);
	}


	public void DisplayCardFor(Player player, GameLocation location)
	{
		Log.Info("Show " + location.EventId);

		switch (location.Type)
		{
			case GameLocation.PropertyType.Event:
				if (location.EventId == "chance")
				{
					DisplayChance(player);
				}

				if (location.EventId == "community")
				{
					DisplayCommunity(player);
				}

				break;
			default:
				DisplayPropertyCard(player, location);
				break;
		}
	}

	private void DisplayChance(Player player)
	{
		var card = ChanceCards[0];
		ChanceCards.Remove(card);

		Log.Info("Drew " + card.Text);

		IngameStateManager.Data = card;
		IngameStateManager.State = IngameUI.IngameUiStates.Chance;

		card.Action(player, MovementManager, BlockedCards, IngameStateManager, card);

		if (ChanceCards.Count == 0)
		{
			FillChangeCards();
		}
	}

	private void DisplayCommunity(Player player)
	{
		var card = CommunityCards[0];
		CommunityCards.Remove(card);

		Log.Info("Drew " + card.Text);

		IngameStateManager.Data = card;
		IngameStateManager.State = IngameUI.IngameUiStates.Community_Chest;

		card.Action(player, MovementManager, BlockedCards, IngameStateManager, card);

		if (CommunityCards.Count == 0)
		{
			FillCommunityCards();
		}
	}


	private void DisplayPropertyCard(Player player, GameLocation location)
	{
		IngameStateManager.State = IngameUI.IngameUiStates.Buying;
		IngameStateManager.Data = location;
	}
}
