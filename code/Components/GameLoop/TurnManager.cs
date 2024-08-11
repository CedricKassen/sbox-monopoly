using Sandbox.Events;
using Sandbox.Events.TurnEvents;

public class TurnManager : Component {
	public enum Phase {
		Rolling,
		RoundAction,
		PlayerAction
	}

	public enum PlayerActionType {
		Trade,
		Mortgage
	}

	public enum SpecialPropertyActionType {
		CommunityChest,
		Chance,
		Jail
	}

	[Property] public GameObject GameParentObject { get; set; }

	[Property] public Phase CurrentPhase { get; set; }

	[Property] public Lobby CurrentLobby { get; set; }

	[Property] public int CurrentPlayerIndex { get; set; }

	[Broadcast]
	public void EmitRolledEvent(int value) {
		GameParentObject.Dispatch(new RolledEvent { playerId = CurrentLobby.Players[0].SteamId, Number = value });
		CurrentPhase = Phase.RoundAction;
	}

	[Broadcast]
	public void EmitPropertyAquiredEvent(int propertyIndex) {
		GameParentObject.Dispatch(
			new PropertyAquiredEvent { playerId = CurrentLobby.Players[0].SteamId, PropertyIndex = propertyIndex });
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
		CurrentPlayerIndex = (CurrentPlayerIndex + 1) % CurrentLobby.Players.Count;

		foreach (var dice in Game.ActiveScene.GetAllComponents<Dice>()) {
			dice.Network.AssignOwnership(CurrentLobby.Players[CurrentPlayerIndex].Connection);
		}

		CurrentPhase = Phase.Rolling;
		GameParentObject.Dispatch(new TurnFinishedEvent());
	}
}
