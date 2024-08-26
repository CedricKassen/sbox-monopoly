namespace Sandbox.Events.TurnEvents;

public record PlayerPaymentEvent(
	ulong PlayerId,
	ulong Recipient,
	int Amount,
	TurnManager.Phase Phase = TurnManager.Phase.PlayerAction) : IGameEvent;
