using Sandbox;
using Sandbox.Events;
using Sandbox.Events.GameStateEvents;
using Sandbox.Events.TurnEvents;

public class TurnManager : Component {
    public enum Phase {
        Rolling,
        RoundAction,
        PlayerAction
    }
    
    public enum SpecialPropertyActionType {
        CommunityChest,
        Chance,
        Jail
    }
    
    public enum PlayerActionType {
        Trade,
        Mortgage
    }
    
    [Property]
    public GameObject GameParentObject { get; set; }
    [Property]
    public Phase CurrentPhase { get; set; }
    [Property]
    public Lobby CurrentLobby { get; set; }

    public void EmitRolledEvent(int value) {
        GameParentObject.Dispatch(new RolledEvent{ playerId = CurrentLobby.players[0].SteamId, Number = value });
        CurrentPhase = Phase.RoundAction;
    }

    public void EmitPropertyAquiredEvent(string property) {
        GameParentObject.Dispatch(new PropertyAquiredEvent());
        CurrentPhase = Phase.PlayerAction;
    }
    
    public void EmitPlayerPaymentEvent(ulong playerId, int amount) {
        GameParentObject.Dispatch(new PlayerPaymentEvent());
        CurrentPhase = Phase.PlayerAction;
    }
    
    public void EmitSpecialPropertyActionEvent(SpecialPropertyActionType type) {
        GameParentObject.Dispatch(new SpecialPropertyActionEvent());

        if (type == SpecialPropertyActionType.Jail) {
            GameParentObject.Dispatch(new TurnFinishedEvent());
            CurrentPhase = Phase.Rolling;
        }
        
        CurrentPhase = Phase.PlayerAction;
    }

    public void EmitPlayerActionEvent(PlayerActionType type) {
        GameParentObject.Dispatch(new PlayerActionEvent());
    }

    public void EmitTurnFinishedEvent() {
        GameParentObject.Dispatch(new TurnFinishedEvent());
        CurrentPhase = Phase.Rolling;
    }
}