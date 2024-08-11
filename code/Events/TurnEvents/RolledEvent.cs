namespace Sandbox.Events.TurnEvents;

public record RolledEvent : BaseEvent {
	public int Number { get; init; }
}
