namespace Sandbox.Events.TurnEvents;

public record PayoutFreeParkingEvent(ulong PlayerId) : IGameEvent { };
