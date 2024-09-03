namespace Sandbox.Events.TurnEvents;

public record StartRollEvent(ulong PlayerId) : IGameEvent { }
