namespace Sandbox.Events.TurnEvents;

public record UseJailCardEvent(ulong playerId) : IGameEvent { }
