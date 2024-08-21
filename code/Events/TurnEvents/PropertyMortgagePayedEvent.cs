namespace Sandbox.Events.TurnEvents;

public record PropertyMortgagePayedEvent : BaseEvent {
    public int PropertyIndex { get; init; }
};
