namespace Sandbox.Constants;

public static class Cards
{
	public static readonly List<Card> Chance_Standard = new()
	{
		new Card(1, AdvanceToBoardwalk, "Advance to Boardwalk"),
		new Card(2, AdvanceToGo, "Advance to Go (Collect $200)"),
		new Card(3, AdvanceToIllinoisAvenue, "Advance to Illinois Avenue. If you pass Go, collect $200"),
		new Card(4, AdvanceToStCharlesPlace, "Advance to St. Charles Place. If you pass Go, collect $200"),
		new Card(5, MoveToNextLine,
			"Advance to the nearest Railroad. If unowned, you may buy it from the Bank. If owned, pay wonder twice the rental to which they are otherwise entitled"),
		new Card(6, MoveToNextLine,
			"Advance to the nearest Railroad. If unowned, you may buy it from the Bank. If owned, pay wonder twice the rental to which they are otherwise entitled"),
		new Card(7, MoveToNextUtility,
			"Advance token to nearest Utility. If unowned, you may buy it from the Bank. If owned, throw dice and pay owner a total ten times amount thrown."),
		new Card(8, BankDividend, "Bank pays you dividend of $50"),
		new Card(9, GetOutOfJailCard, "Get Out of Jail Free"),
		new Card(10, BackwardsThree, "Go Back 3 Spaces"),
		new Card(11, GoToJail, "Go to Jail. Go directly to Jail, do not pass Go, do not collect $200",
			"ui/jail.png"),
		new Card(12, Renovate,
			"Make general repairs on all your property. For each house pay $25. For each hotel pay $100"),
		new Card(13, SpeedFine, "Speeding fine $15"),
		new Card(14, AdvanceToReadingRailroad, "Take a trip to Reading Railroad. If you pass Go, collect $200"),
		new Card(15, Chairman, "You have been elected Chairman of the Board. Pay each player $50"),
		new Card(16, LoanMatures, "Your building loan matures. Collect $150")
	};

	public static readonly List<Card> CommunityChest_Standard = new()
	{
		new Card(1, null, "Advance to Go (Collect $200)"),
		new Card(2, null, "Bank error in your favor. Collect $200"),
		new Card(3, null, "Doctor’s fee. Pay $50"),
		new Card(4, null, "From sale of stock you get $50"),
		new Card(5, null, "Get Out of Jail Free"),
		new Card(6, null, "Go to Jail. Go directly to jail, do not pass Go, do not collect $200"),
		new Card(7, null, "Holiday fund matures. Receive $100"),
		new Card(8, null, "Income tax refund. Collect $20"),
		new Card(9, null, "It is your birthday. Collect $10 from every player"),
		new Card(10, null, "Life insurance matures. Collect $100"),
		new Card(11, null, "Pay hospital fees of $100"),
		new Card(12, null, "Pay school fees of $50"),
		new Card(13, null, "Receive $25 consultancy fee"),
		new Card(14, null, "You are assessed for street repair. $40 per house. $115 per hotel"),
		new Card(15, null, "You have won second prize in a beauty contest. Collect $10"),
		new Card(16, null, "You inherit $100")
	};

	private static void AdvanceToGo(Player player, MovementManager move, Dictionary<Card, bool> blocked = null,
		IngameStateManager stateManager = null, Card card = null)
	{
		CardActionHelper.MoveTo(40, player, move);
	}

	private static void AdvanceToBoardwalk(Player player, MovementManager move, Dictionary<Card, bool> blocked = null,
		IngameStateManager stateManager = null, Card card = null)
	{
		CardActionHelper.MoveTo(39, player, move);
	}

	private static void AdvanceToReadingRailroad(Player player, MovementManager move,
		Dictionary<Card, bool> blocked = null,
		IngameStateManager stateManager = null, Card card = null)
	{
		CardActionHelper.MoveTo(5, player, move);
	}

	private static void AdvanceToIllinoisAvenue(Player player, MovementManager move,
		Dictionary<Card, bool> blocked = null,
		IngameStateManager stateManager = null, Card card = null)
	{
		CardActionHelper.MoveTo(24, player, move);
	}

	private static void AdvanceToStCharlesPlace(Player player, MovementManager move,
		Dictionary<Card, bool> blocked = null,
		IngameStateManager stateManager = null, Card card = null)
	{
		CardActionHelper.MoveTo(11, player, move);
	}

	private static void MoveToNextLine(Player player, MovementManager move,
		Dictionary<Card, bool> blocked = null,
		IngameStateManager stateManager = null, Card card = null)
	{
		var targetLocation = CardActionHelper.FindNearestLine(player.CurrentField);
		CardActionHelper.MoveTo(targetLocation, player, move);
		player.Tags.Add(PlayerTags.RAILROAD_EVENT.ToString());
	}

	private static void MoveToNextUtility(Player player, MovementManager move,
		Dictionary<Card, bool> blocked = null,
		IngameStateManager stateManager = null, Card card = null)
	{
		var targetLocation = player.CurrentField is < 12 or > 28 ? 12 : 28;
		CardActionHelper.MoveTo(targetLocation, player, move);
		player.Tags.Add(PlayerTags.UTILITY_EVENT.ToString());
	}

	private static void BankDividend(Player player, MovementManager move,
		Dictionary<Card, bool> blocked = null,
		IngameStateManager stateManager = null, Card card = null)
	{
		player.Money += 50;
	}

	private static void SpeedFine(Player player, MovementManager move,
		Dictionary<Card, bool> blocked = null,
		IngameStateManager stateManager = null, Card card = null)
	{
		player.Money -= 15;
	}

	private static void LoanMatures(Player player, MovementManager move,
		Dictionary<Card, bool> blocked = null,
		IngameStateManager stateManager = null, Card card = null)
	{
		player.Money += 150;
	}

	private static void Chairman(Player player, MovementManager move,
		Dictionary<Card, bool> blocked = null,
		IngameStateManager stateManager = null, Card card = null)
	{
		CardActionHelper.CollectFromAll(player, 50);
	}

	private static void Renovate(Player player, MovementManager move,
		Dictionary<Card, bool> blocked = null,
		IngameStateManager stateManager = null, Card card = null)
	{
		Log.Error("Not yet implemented!");
	}

	private static void BackwardsThree(Player player, MovementManager move,
		Dictionary<Card, bool> blocked = null,
		IngameStateManager stateManager = null, Card card = null)
	{
		move.StartMovement(player, -3);
	}

	private static void GoToJail(Player player, MovementManager move,
		Dictionary<Card, bool> blocked = null,
		IngameStateManager stateManager = null, Card card = null)
	{
		CardActionHelper.GoToJail(player, move);
	}

	private static void GetOutOfJailCard(Player player, MovementManager move,
		Dictionary<Card, bool> blocked = null,
		IngameStateManager stateManager = null, Card card = null)
	{
		blocked.Add(card, true);
		stateManager.OwnedFields["chanceJailFree"] = player.SteamId;
	}
}
