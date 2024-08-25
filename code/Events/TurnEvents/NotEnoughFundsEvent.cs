namespace Sandbox.Events.TurnEvents;

public record NotEnoughFundsEvent(ulong PlayerId, ulong Recipient, int Debts) : IGameEvent { };
