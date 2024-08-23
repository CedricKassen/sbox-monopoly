namespace Sandbox.Events.TurnEvents;

public record DestroyHouseEvent(int PropertyIndex, ulong PlayerId) : IGameEvent {
}