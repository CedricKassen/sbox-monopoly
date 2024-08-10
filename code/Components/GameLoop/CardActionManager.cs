using System;
using System.Threading.Tasks;
using Monopoly.UI.Screens.GameLoop;
using Sandbox.Constants;

public sealed class CardActionManager : Component
{
	[Property] private List<Card> ChanceCards;

	[Property] private List<Card> CommunityCards;

	[Property] private bool IsChanceJailCardPresent = true;

	[Property] private bool IsCommunityJailCardPresent = true;

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

		if (!IsChanceJailCardPresent)
		{
			ChanceCards.Remove(ChanceCards.Find(card => card.ActionId == 9));
		}

		CardActionHelper.Shuffle(ChanceCards);
	}

	private void FillCommunityCards()
	{
		CommunityCards = new List<Card>(Cards.CommunityChest_Standard);

		if (!IsCommunityJailCardPresent)
		{
			CommunityCards.Remove(CommunityCards.Find(card => card.ActionId == 5));
		}

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

		int targetLocation;
		switch (card.ActionId)
		{
			case 1:
				// Go to Broadwalk
				MovementManager.StartMovement(player, CalculateFieldsToTravel(player.CurrentField, 39));
				break;
			case 2:
				// Go to Go
				MovementManager.StartMovement(player, CalculateFieldsToTravel(player.CurrentField, 40));
				break;
			case 3:
				// Go to Illinois Avenue
				MovementManager.StartMovement(player, CalculateFieldsToTravel(player.CurrentField, 24));
				break;
			case 4:
				// Go to St. Charles Place
				MovementManager.StartMovement(player, CalculateFieldsToTravel(player.CurrentField, 11));
				break;
			case 5:
			case 6:
				// Go to next line
				targetLocation = FindNearestLine(player.CurrentField);
				MovementManager.StartMovement(player, CalculateFieldsToTravel(player.CurrentField, targetLocation));
				player.Tags.Add(PlayerTags.RAILROAD_EVENT.ToString());
				break;
			case 7:
				// Go to next Utility Field 12 & 28
				targetLocation = player.CurrentField is < 12 or > 28 ? 12 : 28;
				MovementManager.StartMovement(player, CalculateFieldsToTravel(player.CurrentField, targetLocation));
				player.Tags.Add(PlayerTags.UTILITY_EVENT.ToString());
				break;
			case 8:
				// Bank divident
				player.Money += 50;
				break;
			case 9:
				// Get out of Jail Card
				IsChanceJailCardPresent = false;
				IngameStateManager.OwnedFields["chanceJailFree"] = player.SteamId;
				break;
			case 10:
				// Move back 3
				MovementManager.StartMovement(player, -3);
				break;
			case 11:
				GoToJail(player);
				break;
			case 12:
				// Repair houses (house: 25, hotel 100)
				break;
			case 13:
				// Speeding fine
				player.Money -= 15;
				break;
			case 14:
				// Go to Reading Railroad
				MovementManager.StartMovement(player, CalculateFieldsToTravel(player.CurrentField, 5));
				break;
			case 15:
				CollectFromAll(player, 50);
				break;
			case 16:
				// loan matures 
				player.Money += 150;
				break;
		}

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
				IsCommunityJailCardPresent = false;
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
