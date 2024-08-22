namespace Sandbox.Events.TurnEvents;

public record PropertyMortgagedEvent : BaseEvent {
    public int PropertyIndex { get; init; }
};
