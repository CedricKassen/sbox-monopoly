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

	[Property]
	public GameObject GameParentObject { get; set; }

	[Property, HostSync]
	public Phase CurrentPhase { get; set; }

	[Property]
	public Lobby CurrentLobby { get; set; }

	[Property, HostSync]
	public int CurrentPlayerIndex { get; set; }

	[Broadcast]
	public void EmitRolledEvent(ulong playerId, int value) {
		GameParentObject.Dispatch(new RolledEvent { playerId = playerId, Number = value });
		CurrentPhase = Phase.RoundAction;
	}

	[Broadcast]
	public void EmitPropertyAquiredEvent(ulong playerId, int propertyIndex) {
		GameParentObject.Dispatch(
			new PropertyAquiredEvent { playerId = playerId, PropertyIndex = propertyIndex });
		CurrentPhase = Phase.PlayerAction;
	}

	[Broadcast]
	public void EmitPropertyAuctionEvent(int propertyIndex) {
		GameParentObject.Dispatch(
			new PropertyAuctionEvent() { PropertyIndex = propertyIndex });
		CurrentPhase = Phase.Auction;
	}

	[Broadcast]
	public void EmitPlayerPaymentEvent(ulong playerId, ulong recipientId, int amount) {
		GameParentObject.Dispatch(new PlayerPaymentEvent {
			playerId = playerId, Amount = amount, Recipient = recipientId
		});
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
		CurrentPhase = Phase.Rolling;
		GameParentObject.Dispatch(new TurnFinishedEvent());
	}

	[Broadcast]
	public void EmitAuctionBidEvent(ulong PlayerId, int Amount) {
		GameParentObject.Dispatch(new AuctionBidEvent { playerId = PlayerId, Amount = Amount });
	}

	[Broadcast]
	public void EmitAuctionFinishedEvent(int PropertyIndex, ulong PlayerId, int Amount) {
		CurrentPhase = Phase.PlayerAction;
		GameParentObject.Dispatch(new AuctionFinishedEvent {
			PropertyIndex = PropertyIndex, playerId = PlayerId, Amount = Amount
		});
	}

	[Broadcast]
	public void EmitPropertyMortgagedEvent(int PropertyIndex, ulong PlayerId) {
		CurrentPhase = Phase.PlayerAction;
		GameParentObject.Dispatch(new PropertyMortgagedEvent { PropertyIndex = PropertyIndex, playerId = PlayerId });
	}

	[Broadcast]
	public void EmitPropertyMortgagePayedEvent(int PropertyIndex, ulong PlayerId) {
		CurrentPhase = Phase.PlayerAction;
		GameParentObject.Dispatch(new PropertyMortgagePayedEvent {
			PropertyIndex = PropertyIndex, playerId = PlayerId
		});
	}

	[Broadcast]
	public void EmitTradingRequestedEvent(ulong PlayerId, ulong TradingRecipient) {
		GameParentObject.Dispatch(
			new TradingRequestedEvent { TradingRecipient = TradingRecipient, playerId = PlayerId });
	}

	[Broadcast]
	public void EmitTradingAcceptedEvent(ulong PlayerId) {
		GameParentObject.Dispatch(
			new TradingAcceptedEvent() { playerId = PlayerId });
	}

	[Broadcast]
	public void EmitTradingDeniedEvent(ulong PlayerId) {
		GameParentObject.Dispatch(
			new TradingDeniedEvent() { playerId = PlayerId });
	}
}
