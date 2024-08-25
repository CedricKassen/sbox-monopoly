namespace Sandbox.Events.TurnEvents;

public record PayJailFineEvent(ulong playerId) : IGameEvent { }
