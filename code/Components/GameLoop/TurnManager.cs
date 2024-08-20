using Sandbox.Events;
using Sandbox.Events.TurnEvents;

public class TurnManager : Component {
	public enum Phase {
		Rolling,
		RoundAction,
		Auction,
		PlayerAction
	}

	public enum PlayerActionType {
		Trade,
		Mortgage
	}

	public enum SpecialPropertyActionType {
		CommunityChest,
		Chance,
		Jail,
		Tax,
		None
	}

	[Property] public GameObject GameParentObject { get; set; }

	[Property, HostSync] public Phase CurrentPhase { get; set; }

	[Property] public Lobby CurrentLobby { get; set; }

	[Property, HostSync] public int CurrentPlayerIndex { get; set; }

	[Broadcast]
	public void EmitRolledEvent(int value) {
		GameParentObject.Dispatch(new RolledEvent { playerId = CurrentLobby.Players[0].SteamId, Number = value });
		CurrentPhase = Phase.RoundAction;
	}

	[Broadcast]
	public void EmitPropertyAquiredEvent(int propertyIndex, ulong playerId = 0) {
		GameParentObject.Dispatch(
			new PropertyAquiredEvent { playerId = playerId != 0 ? playerId : CurrentLobby.Players[0].SteamId, PropertyIndex = propertyIndex });
		CurrentPhase = Phase.PlayerAction;
	}
	
	[Broadcast]
	public void EmitPropertyAuctionEvent(int propertyIndex) {
		GameParentObject.Dispatch(
			new PropertyAuctionEvent() { PropertyIndex = propertyIndex });
		CurrentPhase = Phase.Auction;
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

	[Broadcast]
	public void EmitAuctionBidEvent(ulong PlayerId, int Amount) {
		GameParentObject.Dispatch(new AuctionBidEvent {playerId = PlayerId, Amount = Amount});
	}

	[Broadcast]
	public void EmitAuctionFinishedEvent(int PropertyIndex, ulong PlayerId, int Amount) {
		CurrentPhase = Phase.PlayerAction;
		GameParentObject.Dispatch(new AuctionFinishedEvent {PropertyIndex = PropertyIndex, playerId = PlayerId, Amount = Amount});
	}
}
