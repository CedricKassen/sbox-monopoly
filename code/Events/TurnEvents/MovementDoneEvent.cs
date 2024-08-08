namespace Sandbox.Events.TurnEvents;

public record MovementDoneEvent : BaseEvent
{
	public GameLocation Location { get; init; }
}
