namespace Sandbox.Events.TurnEvents;

public record TurnActionDoneEvent(ulong PlayerId, TurnManager.Phase NewPhase = TurnManager.Phase.PlayerAction)
	: IGameEvent;
