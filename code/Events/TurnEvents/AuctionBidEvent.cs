namespace Sandbox.Events.TurnEvents;

public record AuctionBidEvent : BaseEvent {
    public int Amount { get; init; }    
}
