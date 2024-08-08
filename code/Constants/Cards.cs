namespace Sandbox.Constants;

public class Card
{
	public Card(int actionId, string text)
	{
		ActionId = actionId;
		Text = text;
	}

	public int ActionId { get; }
	public string Text { get; }

	public override string ToString()
	{
		return Text;
	}
}

public static class Cards
{
	public static readonly List<Card> Chance = new()
	{
		new Card(1, "Advance to Boardwalk"),
		new Card(2, "Advance to Go (Collect $200)"),
		new Card(3,
			"Advance to Illinois Avenue. If you pass Go, collect $200"),
		new Card(4, "Advance to St. Charles Place. If you pass Go, collect $200"),
		new Card(5,
			"Advance to the nearest Railroad. If unowned, you may buy it from the Bank. If owned, pay wonder twice the rental to which they are otherwise entitled"),
		new Card(6,
			"Advance to the nearest Railroad. If unowned, you may buy it from the Bank. If owned, pay wonder twice the rental to which they are otherwise entitled"),
		new Card(7,
			"Advance token to nearest Utility. If unowned, you may buy it from the Bank. If owned, throw dice and pay owner a total ten times amount thrown."),
		new Card(8, "Bank pays you dividend of $50"),
		new Card(9, "Get Out of Jail Free"),
		new Card(10, "Go Back 3 Spaces"),
		new Card(11, "Go to Jail. Go directly to Jail, do not pass Go, do not collect $200"),
		new Card(12, "Make general repairs on all your property. For each house pay $25. For each hotel pay $100"),
		new Card(13, "Speeding fine $15"),
		new Card(14, "Take a trip to Reading Railroad. If you pass Go, collect $200"),
		new Card(15, "You have been elected Chairman of the Board. Pay each player $50"),
		new Card(16, "Your building loan matures. Collect $150")
	};

	public static readonly List<Card> CommunityChest = new()
	{
		new Card(1, "Advance to Go (Collect $200)"),
		new Card(2, "Bank error in your favor. Collect $200"),
		new Card(3, "Doctor’s fee. Pay $50"),
		new Card(4, "From sale of stock you get $50"),
		new Card(5, "Get Out of Jail Free"),
		new Card(6, "Go to Jail. Go directly to jail, do not pass Go, do not collect $200"),
		new Card(7, "Holiday fund matures. Receive $100"),
		new Card(8, "Income tax refund. Collect $20"),
		new Card(9, "It is your birthday. Collect $10 from every player"),
		new Card(10, "Life insurance matures. Collect $100"),
		new Card(11, "Pay hospital fees of $100"),
		new Card(12, "Pay school fees of $50"),
		new Card(13, "Receive $25 consultancy fee"),
		new Card(14, "You are assessed for street repair. $40 per house. $115 per hotel"),
		new Card(15, "You have won second prize in a beauty contest. Collect $10"),
		new Card(16, "You inherit $100")
	};
}
