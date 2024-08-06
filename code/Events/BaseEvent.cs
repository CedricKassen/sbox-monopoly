namespace Sandbox.Events;

public record BaseEvent: IGameEvent {
    public ulong playerId { get; init; }
}