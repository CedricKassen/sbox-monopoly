namespace Sandbox.Events.TurnEvents;

public record BuildHouseEvent(int PropertyIndex, ulong PlayerId) : IGameEvent {
}