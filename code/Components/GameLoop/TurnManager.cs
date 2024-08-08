using Sandbox.Events;
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

    [Property]
    public int CurrentPlayerIndex { get; set; } = 0;

    [Broadcast]
    public void EmitRolledEvent(int value) {
        GameParentObject.Dispatch(new RolledEvent{ playerId = CurrentLobby.Players[0].SteamId, Number = value });
        CurrentPhase = Phase.RoundAction;
    }

    [Broadcast]
    public void EmitPropertyAquiredEvent(string property) {
        GameParentObject.Dispatch(new PropertyAquiredEvent());
        CurrentPhase = Phase.PlayerAction;
    }
    
    [Broadcast]
    public void EmitPlayerPaymentEvent(ulong playerId, int amount) {
        GameParentObject.Dispatch(new PlayerPaymentEvent());
        CurrentPhase = Phase.PlayerAction;
    }
    
    [Broadcast]
    public void EmitSpecialPropertyActionEvent(SpecialPropertyActionType type) {
        GameParentObject.Dispatch(new SpecialPropertyActionEvent());

        if (type == SpecialPropertyActionType.Jail) {
            GameParentObject.Dispatch(new TurnFinishedEvent());
            CurrentPhase = Phase.Rolling;
        }
        
        CurrentPhase = Phase.PlayerAction;
    }

    [Broadcast]
    public void EmitPlayerActionEvent(PlayerActionType type) {
        GameParentObject.Dispatch(new PlayerActionEvent());
    }

    [Broadcast]
    public void EmitTurnFinishedEvent() {
        GameParentObject.Dispatch(new TurnFinishedEvent());
        CurrentPlayerIndex = (CurrentPlayerIndex + 1) % CurrentLobby.Players.Count;
        
        foreach (var dice in Game.ActiveScene.GetAllComponents<Dice>()) {
            dice.Network.AssignOwnership(CurrentLobby.Players[CurrentPlayerIndex].Connection);
        }
        CurrentPhase = Phase.Rolling;
    }
}