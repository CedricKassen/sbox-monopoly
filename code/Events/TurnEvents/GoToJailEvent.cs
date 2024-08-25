namespace Sandbox.Events.TurnEvents;

public record GoToJailEvent(ulong playerId) : IGameEvent { }
