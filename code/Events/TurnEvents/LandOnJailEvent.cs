namespace Sandbox.Events.TurnEvents;

public record LandOnJailEvent(ulong playerId) : IGameEvent { }
