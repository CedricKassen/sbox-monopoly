namespace Sandbox.Events.TurnEvents;

public record PropertyAquiredEvent : BaseEvent {
    public int PropertyIndex { get; init; }
};