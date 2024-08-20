using Monopoly.UI.Screens.GameLoop;
using Sandbox.Events;
using Sandbox.Events.TurnEvents;

public sealed class IngameStateManager : Component, IGameEventHandler<AuctionBidEvent> {
	public object Data = null;

	[Property, HostSync] public NetDictionary<string, ulong> OwnedFields { get; set; } = new() {
		{ "brown1", 0 },
		{ "brown2", 0 },
		{ "lightBlue1", 0 },
		{ "lightBlue2", 0 },
		{ "lightBlue3", 0 },
		{ "pink1", 0 },
		{ "pink2", 0 },
		{ "pink3", 0 },
		{ "orange1", 0 },
		{ "orange2", 0 },
		{ "orange3", 0 },
		{ "red1", 0 },
		{ "red2", 0 },
		{ "red3", 0 },
		{ "yellow1", 0 },
		{ "yellow2", 0 },
		{ "yellow3", 0 },
		{ "green1", 0 },
		{ "green2", 0 },
		{ "green3", 0 },
		{ "blue1", 0 },
		{ "blue2", 0 },
		{ "railroad1", 0 },
		{ "railroad2", 0 },
		{ "railroad3", 0 },
		{ "railroad4", 0 },
		{ "electricCompany", 0 },
		{ "waterCompany", 0 },
		{ "chanceJailFree", 0 },
		{ "communityJailFree", 0 }
	};

	[Property] public TurnManager TurnManager { get; set; }
	[Property] public GameObject LocationContainer { get; set; }
	[Property, HostSync] public IngameUI.IngameUiStates State { get; set; } = IngameUI.IngameUiStates.None;
	[Property, HostSync] public NetDictionary<ulong, int> AuctionBiddings { get; set; } = new();
	[Property] public readonly int AuctionTime = 8;
	[Property, HostSync] public float AuctionTimer { get; set; }

	public void OnGameEvent(AuctionBidEvent eventArgs) {
		var currentMax = GetSortedBiddings()[0].Value;
		AuctionBiddings[eventArgs.playerId] = currentMax + eventArgs.Amount;

		AuctionTimer = AuctionTime;
	}

	protected override void OnUpdate() {
		if (AuctionTimer > 0) {
			AuctionTimer -= Time.Delta;
		}

		if (AuctionTimer < 0) {
			AuctionTimer = 0;
			
			var location = Data as GameLocation;
			var locationIndex = LocationContainer.Children.FindIndex(c =>
					c.Components.Get<GameLocation>().Id == location.Id);
			var biddingList = GetSortedBiddings();
			
			TurnManager.EmitAuctionFinishedEvent(locationIndex, biddingList[0].Key, biddingList[0].Value);
		}
	}

	public List<KeyValuePair<ulong, int>> GetSortedBiddings() {
		var list = AuctionBiddings.ToList();
		list.Sort((left, right) => left.Value > right.Value ? 1 : -1);
		
		return list;
	}
}
