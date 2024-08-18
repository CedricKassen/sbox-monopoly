namespace Sandbox.Events.LobbyEvents;

public record ChangePawnSelectionEvent(int pawnId, ulong callerSteamId) : IGameEvent { }
