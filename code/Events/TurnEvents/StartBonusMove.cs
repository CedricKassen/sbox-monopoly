namespace Sandbox.Events.TurnEvents;

public record StartBonusMove(ulong PlayerId) : IGameEvent;
