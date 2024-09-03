namespace Sandbox.Events.TurnEvents;

public record RolledEvent : BaseEvent {
	public int Number { get; init; }
	public bool Doubles { get; init; }
	public bool Bus { get; init; }
	public bool Forward { get; init; }
}
