using Sandbox.Constants;

namespace Sandbox.Events.TurnEvents;

public record EventCardClosedEvent(Card card, ulong playerId) : IGameEvent { }
