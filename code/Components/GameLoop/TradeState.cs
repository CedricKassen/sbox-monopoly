using Sandbox;

public sealed class TradeState : Component {
	[Property, Sync]
	public Player TradingCreator { get; set; }

	[Property, Sync]
	public Player TradingPartner { get; set; }

	[Property, Sync]
	public int TradingOfferAmount { get; set; } = 0;

	[Property, Sync]
	public int TradingRequestAmount { get; set; } = 0;

	[Property, Sync]
	public NetDictionary<string, bool> OfferTradeFields { get; set; } = new() {
		{ "brown1", false },
		{ "brown2", false },
		{ "lightBlue1", false },
		{ "lightBlue2", false },
		{ "lightBlue3", false },
		{ "pink1", false },
		{ "pink2", false },
		{ "pink3", false },
		{ "orange1", false },
		{ "orange2", false },
		{ "orange3", false },
		{ "red1", false },
		{ "red2", false },
		{ "red3", false },
		{ "yellow1", false },
		{ "yellow2", false },
		{ "yellow3", false },
		{ "green1", false },
		{ "green2", false },
		{ "green3", false },
		{ "blue1", false },
		{ "blue2", false },
		{ "railroad1", false },
		{ "railroad2", false },
		{ "railroad3", false },
		{ "railroad4", false },
		{ "electricCompany", false },
		{ "waterCompany", false },
		{ "chanceJailFree", false },
		{ "communityJailFree", false }
	};

	[Property, Sync]
	public NetDictionary<string, bool> RequestTradeFields { get; set; } = new() {
		{ "brown1", false },
		{ "brown2", false },
		{ "lightBlue1", false },
		{ "lightBlue2", false },
		{ "lightBlue3", false },
		{ "pink1", false },
		{ "pink2", false },
		{ "pink3", false },
		{ "orange1", false },
		{ "orange2", false },
		{ "orange3", false },
		{ "red1", false },
		{ "red2", false },
		{ "red3", false },
		{ "yellow1", false },
		{ "yellow2", false },
		{ "yellow3", false },
		{ "green1", false },
		{ "green2", false },
		{ "green3", false },
		{ "blue1", false },
		{ "blue2", false },
		{ "railroad1", false },
		{ "railroad2", false },
		{ "railroad3", false },
		{ "railroad4", false },
		{ "electricCompany", false },
		{ "waterCompany", false },
		{ "chanceJailFree", false },
		{ "communityJailFree", false }
	};
}
