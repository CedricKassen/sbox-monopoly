namespace Sandbox.Events.TurnEvents;

public record PlayerPaymentEvent : BaseEvent {
    public int Amount { get; init; }
    public ulong Recipient { get; init; }
};
