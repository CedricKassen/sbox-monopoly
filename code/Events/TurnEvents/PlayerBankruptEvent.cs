namespace Sandbox.Events.TurnEvents;

public record PlayerBankruptEvent(ulong PlayerId, ulong Recipient) : IGameEvent;
