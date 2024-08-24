namespace Sandbox.Events.TurnEvents;

public record TradingRequestedEvent : BaseEvent {
	public ulong TradingRecipient { get; init; }
}
