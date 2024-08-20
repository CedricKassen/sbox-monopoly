namespace Sandbox.Events.TurnEvents;

public record AuctionFinishedEvent : BaseEvent {
    public int Amount { get; init; }    
    public int PropertyIndex { get; init; }    
}
