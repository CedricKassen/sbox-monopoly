using System.Net.Mail;

namespace Sandbox.Events.TurnEvents;

public record PaymentDoneEvent(ulong PlayerId) : IGameEvent;
