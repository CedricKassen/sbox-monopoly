namespace Sandbox.Events.TurnEvents;

public record PropertyAuctionEvent : IGameEvent {
    public int PropertyIndex { get; init; }    
}
