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

	[Property] [HostSync] public Phase CurrentPhase { get; set; }

	[Property] public Lobby CurrentLobby { get; set; }

	[Property] [HostSync] public int CurrentPlayerIndex { get; set; }

	[Broadcast]
	public void EmitRolledEvent(ulong playerId, int dice1, int dice2) {
		var doubles = dice1 == dice2;
		GameParentObject.Dispatch(new RolledEvent { playerId = playerId, Number = dice1 + dice2, Doubles = doubles });
	}

	[Broadcast]
	public void EmitPropertyAquiredEvent(ulong playerId, int propertyIndex) {
		GameParentObject.Dispatch(
			new PropertyAquiredEvent { playerId = playerId, PropertyIndex = propertyIndex });
		ChangePhase(playerId, Phase.PlayerAction);
	}

	[Broadcast]
	public void EmitPropertyAuctionEvent(int propertyIndex, ulong playerId) {
		GameParentObject.Dispatch(
			new PropertyAuctionEvent { PropertyIndex = propertyIndex });
		ChangePhase(playerId, Phase.Auction);
	}

	[Broadcast]
	public void EmitPlayerPaymentEvent(ulong playerId, ulong recipientId, int amount) {
		GameParentObject.Dispatch(new PlayerPaymentEvent {
			playerId = playerId, Amount = amount, Recipient = recipientId
		});
		ChangePhase(playerId, Phase.PlayerAction);
	}

	[Broadcast]
	public void EmitSpecialPropertyActionEvent(SpecialPropertyActionType type, ulong playerId) {
		GameParentObject.Dispatch(new SpecialPropertyActionEvent());

		if (type == SpecialPropertyActionType.Jail) {
			GameParentObject.Dispatch(new TurnFinishedEvent());
			ChangePhase(playerId, Phase.Rolling);
		}

		ChangePhase(playerId, Phase.PlayerAction);
	}

	[Broadcast]
	public void EmitPlayerActionEvent(PlayerActionType type) {
		GameParentObject.Dispatch(new PlayerActionEvent());
	}

	[Broadcast]
	public void EmitTurnFinishedEvent(ulong playerId) {
		ChangePhase(playerId, Phase.Rolling);
		GameParentObject.Dispatch(new TurnFinishedEvent());
	}

	[Broadcast]
	public void EmitAuctionBidEvent(ulong playerId, int amount) {
		GameParentObject.Dispatch(new AuctionBidEvent { playerId = playerId, Amount = amount });
	}

	[Broadcast]
	public void EmitAuctionFinishedEvent(int propertyIndex, ulong playerId, int amount) {
		ChangePhase(playerId, Phase.PlayerAction);
		GameParentObject.Dispatch(new AuctionFinishedEvent {
			PropertyIndex = propertyIndex, playerId = playerId, Amount = amount
		});
	}

	[Broadcast]
	public void EmitPropertyMortgagedEvent(int propertyIndex, ulong playerId) {
		ChangePhase(playerId, Phase.PlayerAction);
		GameParentObject.Dispatch(new PropertyMortgagedEvent { PropertyIndex = propertyIndex, playerId = playerId });
	}

	[Broadcast]
	public void EmitPropertyMortgagePayedEvent(int propertyIndex, ulong playerId) {
		ChangePhase(playerId, Phase.PlayerAction);
		CurrentPhase = Phase.PlayerAction;
		GameParentObject.Dispatch(new PropertyMortgagePayedEvent {
			PropertyIndex = propertyIndex, playerId = playerId
		});
	}

	[Broadcast]
	public void ChangePhase(ulong playerId, Phase phase) {
		var player = CurrentLobby.Players.Find(player => player.SteamId == playerId);

		if (phase.Equals(Phase.PlayerAction)) {
			CurrentPhase = player.DoublesCount is > 0 and < 3 ? Phase.Rolling : Phase.PlayerAction;
			if (player.DoublesCount == 3) {
				player.DoublesCount = 0;
			}

			return;
		}

		CurrentPhase = phase;
	}
}
