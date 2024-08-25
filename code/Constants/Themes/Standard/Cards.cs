namespace Sandbox.Constants;

public static class Cards {
	public static readonly List<Card> Chance_Standard = new() {
		new Card(1, AdvanceToBoardwalk,
			"Advance to Boardwalk"),
		new Card(2, AdvanceToGo,
			"Advance to Go (Collect $200)"),
		new Card(3, AdvanceToIllinoisAvenue,
			"Advance to Illinois Avenue. If you pass Go, collect $200"),
		new Card(4, AdvanceToStCharlesPlace,
			"Advance to St. Charles Place. If you pass Go, collect $200"),
		new Card(5, MoveToNextLine,
			"Advance to the nearest Railroad. If unowned, you may buy it from the Bank. If owned, pay wonder twice the rental to which they are otherwise entitled"),
		new Card(6, MoveToNextLine,
			"Advance to the nearest Railroad. If unowned, you may buy it from the Bank. If owned, pay wonder twice the rental to which they are otherwise entitled"),
		new Card(7, MoveToNextUtility,
			"Advance token to nearest Utility. If unowned, you may buy it from the Bank. If owned, throw dice and pay owner a total ten times amount thrown."),
		new Card(8, GetFifty,
			"Bank pays you dividend of $50"),
		new Card(9, GetOutOfJailCardChance,
			"Get Out of Jail Free"),
		new Card(10, BackwardsThree, "Go Back 3 Spaces"),
		new Card(11, GoToJail,
			"Go to Jail. Go directly to Jail, do not pass Go, do not collect $200",
			"ui/jail.png"),
		new Card(12, RenovateHouses,
			"Make general repairs on all your property. For each house pay $25. For each hotel pay $100"),
		new Card(13, SpeedFine, "Speeding fine $15"),
		new Card(14, AdvanceToReadingRailroad,
			"Take a trip to Reading Railroad. If you pass Go, collect $200"),
		new Card(15, Chairman,
			"You have been elected Chairman of the Board. Pay each player $50"),
		new Card(16, LoanMatures,
			"Your building loan matures. Collect $150")
	};

	public static readonly List<Card> CommunityChest_Standard = new() {
		new Card(1, AdvanceToGo,
			"Advance to Go (Collect $200)"),
		new Card(2, BankError,
			"Bank error in your favor. Collect $200"),
		new Card(3, DoctorsFee,
			"Doctor’s fee. Pay $50"),
		new Card(4, GetFifty,
			"From sale of stock you get $50"),
		new Card(5, GetOutOfJailCardCommunity,
			"Get Out of Jail Free"),
		new Card(6, GoToJail,
			"Go to Jail. Go directly to jail, do not pass Go, do not collect $200"),
		new Card(7, GetOneHundred,
			"Holiday fund matures. Receive $100"),
		new Card(8, IncomingTax,
			"Income tax refund. Collect $20"),
		new Card(9, Birthday,
			"It is your birthday. Collect $10 from every player"),
		new Card(10, GetOneHundred,
			"Life insurance matures. Collect $100"),
		new Card(11, HospitalFee,
			"Pay hospital fees of $100"),
		new Card(12, DoctorsFee,
			"Pay school fees of $50"),
		new Card(13, GetConsultancyFee,
			"Receive $25 consultancy fee"),
		new Card(14, RenovateStreet,
			"You are assessed for street repair. $40 per house. $115 per hotel"),
		new Card(15, Beauty,
			"You have won second prize in a beauty contest. Collect $10"),
		new Card(16, GetOneHundred,
			"You inherit $100")
	};

	private static void AdvanceToGo(Player player, MovementManager move, TurnManager turnManager,
	                                Dictionary<int, bool> blocked = null,
	                                IngameStateManager stateManager = null, Card card = null) {
		CardActionHelper.MoveTo(40, player, move);
	}

	private static void AdvanceToBoardwalk(Player player, MovementManager move, TurnManager turnManager,
	                                       Dictionary<int, bool> blocked = null,
	                                       IngameStateManager stateManager = null, Card card = null) {
		CardActionHelper.MoveTo(39, player, move);
	}

	private static void AdvanceToReadingRailroad(Player player, MovementManager move, TurnManager turnManager,
	                                             Dictionary<int, bool> blocked = null,
	                                             IngameStateManager stateManager = null, Card card = null) {
		CardActionHelper.MoveTo(5, player, move);
	}

	private static void AdvanceToIllinoisAvenue(Player player, MovementManager move, TurnManager turnManager,
	                                            Dictionary<int, bool> blocked = null,
	                                            IngameStateManager stateManager = null, Card card = null) {
		CardActionHelper.MoveTo(24, player, move);
	}

	private static void AdvanceToStCharlesPlace(Player player, MovementManager move, TurnManager turnManager,
	                                            Dictionary<int, bool> blocked = null,
	                                            IngameStateManager stateManager = null, Card card = null) {
		CardActionHelper.MoveTo(11, player, move);
	}

	private static void MoveToNextLine(Player player, MovementManager move, TurnManager turnManager,
	                                   Dictionary<int, bool> blocked = null,
	                                   IngameStateManager stateManager = null, Card card = null) {
		var targetLocation = CardActionHelper.FindNearestLine(player.CurrentField);
		CardActionHelper.MoveTo(targetLocation, player, move);
		player.Tags.Add(PlayerTags.RAILROAD_EVENT.ToString());
	}

	private static void MoveToNextUtility(Player player, MovementManager move, TurnManager turnManager,
	                                      Dictionary<int, bool> blocked = null,
	                                      IngameStateManager stateManager = null, Card card = null) {
		var targetLocation = player.CurrentField is < 12 or > 28 ? 12 : 28;
		CardActionHelper.MoveTo(targetLocation, player, move);
		player.Tags.Add(PlayerTags.UTILITY_EVENT.ToString());
	}

	private static void DoctorsFee(Player player, MovementManager move, TurnManager turnManager,
	                               Dictionary<int, bool> blocked = null,
	                               IngameStateManager stateManager = null, Card card = null) {
		player.Money -= 50;
	}

	private static void BankError(Player player, MovementManager move, TurnManager turnManager,
	                              Dictionary<int, bool> blocked = null,
	                              IngameStateManager stateManager = null, Card card = null) {
		player.Money += 200;
	}

	private static void GetFifty(Player player, MovementManager move, TurnManager turnManager,
	                             Dictionary<int, bool> blocked = null,
	                             IngameStateManager stateManager = null, Card card = null) {
		player.Money += 50;
	}

	private static void GetOneHundred(Player player, MovementManager move, TurnManager turnManager,
	                                  Dictionary<int, bool> blocked = null,
	                                  IngameStateManager stateManager = null, Card card = null) {
		player.Money += 100;
	}

	private static void SpeedFine(Player player, MovementManager move, TurnManager turnManager,
	                              Dictionary<int, bool> blocked = null,
	                              IngameStateManager stateManager = null, Card card = null) {
		player.Money -= 15;
	}

	private static void Birthday(Player player, MovementManager move, TurnManager turnManager,
	                             Dictionary<int, bool> blocked = null,
	                             IngameStateManager stateManager = null, Card card = null) {
		CardActionHelper.CollectFromAll(player, 10);
	}

	private static void LoanMatures(Player player, MovementManager move, TurnManager turnManager,
	                                Dictionary<int, bool> blocked = null,
	                                IngameStateManager stateManager = null, Card card = null) {
		player.Money += 150;
	}

	private static void HospitalFee(Player player, MovementManager move, TurnManager turnManager,
	                                Dictionary<int, bool> blocked = null,
	                                IngameStateManager stateManager = null, Card card = null) {
		player.Money -= 100;
	}

	private static void GetConsultancyFee(Player player, MovementManager move, TurnManager turnManager,
	                                      Dictionary<int, bool> blocked = null,
	                                      IngameStateManager stateManager = null, Card card = null) {
		player.Money += 25;
	}

	private static void Beauty(Player player, MovementManager move, TurnManager turnManager,
	                           Dictionary<int, bool> blocked = null,
	                           IngameStateManager stateManager = null, Card card = null) {
		player.Money += 10;
	}

	private static void IncomingTax(Player player, MovementManager move, TurnManager turnManager,
	                                Dictionary<int, bool> blocked = null,
	                                IngameStateManager stateManager = null, Card card = null) {
		player.Money += 20;
	}

	private static void Chairman(Player player, MovementManager move, TurnManager turnManager,
	                             Dictionary<int, bool> blocked = null,
	                             IngameStateManager stateManager = null, Card card = null) {
		CardActionHelper.PayToAll(player, 50);
	}

	private static void RenovateHouses(Player player, MovementManager move, TurnManager turnManager,
	                                   Dictionary<int, bool> blocked = null,
	                                   IngameStateManager stateManager = null, Card card = null) {
		CardActionHelper.PayForHouses(player, 25, 100, move, stateManager);
	}

	private static void RenovateStreet(Player player, MovementManager move, TurnManager turnManager,
	                                   Dictionary<int, bool> blocked = null,
	                                   IngameStateManager stateManager = null, Card card = null) {
		CardActionHelper.PayForHouses(player, 40, 115, move, stateManager);
	}


	private static void BackwardsThree(Player player, MovementManager move, TurnManager turnManager,
	                                   Dictionary<int, bool> blocked = null,
	                                   IngameStateManager stateManager = null, Card card = null) {
		move.StartMovement(player, -3);
	}

	private static void GoToJail(Player player, MovementManager move, TurnManager turnManager,
	                             Dictionary<int, bool> blocked = null,
	                             IngameStateManager stateManager = null, Card card = null) {
		turnManager.EmitSpecialPropertyActionEvent(TurnManager.SpecialPropertyActionType.Police, player.SteamId);
	}


	private static void GetOutOfJailCardChance(Player player, MovementManager move, TurnManager turnManager,
	                                           Dictionary<int, bool> blocked = null,
	                                           IngameStateManager stateManager = null, Card card = null) {
		blocked.Add(card.ActionId, true);
		stateManager.OwnedFields["chanceJailFree"] = player.SteamId;
	}

	private static void GetOutOfJailCardCommunity(Player player, MovementManager move, TurnManager turnManager,
	                                              Dictionary<int, bool> blocked = null,
	                                              IngameStateManager stateManager = null, Card card = null) {
		blocked.Add(card.ActionId, true);
		stateManager.OwnedFields["communityJailFree"] = player.SteamId;
	}
}
