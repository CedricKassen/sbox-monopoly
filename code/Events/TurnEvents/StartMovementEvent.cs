namespace Sandbox.Events.TurnEvents;

public record StartMovementEvent(ulong PlayerId, int Amount) : IGameEvent;
