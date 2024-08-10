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

		ChanceCards.RemoveAll(card => BlockedCards.ContainsKey(card));

		CardActionHelper.Shuffle(ChanceCards);
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

		switch (card.ActionId)
		{
			case 1:
				// Go to Go
				MovementManager.StartMovement(player,
					CardActionHelper.CalculateFieldsToTravel(player, 40));
				break;
			case 2:
				// Bank error 
				player.Money += 200;
				break;
			case 3:
				// Doctors fee
				player.Money -= 50;
				break;
			case 4:
				// Sale Stock
				player.Money += 50;
				break;
			case 5:
				// Get out of Jail Card
				BlockedCards.Add(card, true);
				IngameStateManager.OwnedFields["communityJailFree"] = player.SteamId;
				break;
			case 6:
				CardActionHelper.GoToJail(player, MovementManager);
				break;
			case 7:
				// Holiday fund
				player.Money += 100;
				break;
			case 8:
				// Income tax refund
				player.Money += 20;
				break;
			case 9:
				CardActionHelper.CollectFromAll(player, 10);
				break;
			case 10:
				// Life insurance
				player.Money += 100;
				break;
			case 11:
				// hospital fee
				player.Money -= 100;
				break;
			case 12:
				// school fees
				player.Money -= 100;
				break;
			case 13:
				// Receive consultancy fee
				player.Money += 25;
				break;
			case 14:
				// Street Repairs House: 40 Hotel: 115
				break;
			case 15:
				// beauty contest
				player.Money += 10;
				break;
			case 16:
				// Inherit
				player.Money += 100;
				break;
		}

		if (ChanceCards.Count == 0)
		{
			FillChangeCards();
		}

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
