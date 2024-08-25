namespace Sandbox.Events.TurnEvents;

public record PayoutFreeParkingEvent : BaseEvent {
	public ulong Recipient { get; init; }
};
