using EnumExtensions.Util;
using Sandbox.Constants;
using Sandbox.Events;
using Sandbox.Events.TurnEvents;
using Sandbox.ModelEditor;

public class TurnManager : Component {
	public enum Phase {
		Rolling,
		RoundAction,
		Auction,
		PlayerAction,
		InAction,
		Jail,
		InMovement
	}

	public enum PlayerActionType {
		Trade,
		Mortgage
	}

	public enum SpecialPropertyActionType {
		CommunityChest,
		Chance,
		Police,
		Jail,
		Tax,
		None
	}

	[Property] public GameObject GameParentObject { get; set; }

	[Property, HostSync] public Phase CurrentPhase { get; set; }

	[Property] public Lobby CurrentLobby { get; set; }

	[Property] public CardActionManager CardActionManager { get; set; }

	[Property, HostSync] public int CurrentPlayerIndex { get; set; }

	public void EmitStartRollEvent() {
		GameParentObject.Dispatch(new StartRollEvent());
	}

	[Broadcast]
	public void EmitRolledEvent(ulong playerId, int dice1, int dice2) {
		var doubles = dice1 == dice2;
		GameParentObject.Dispatch(new RolledEvent { playerId = playerId, Number = dice1 + dice2, Doubles = doubles });
	}

	[Broadcast]
	public void EmitPropertyAquiredEvent(ulong playerId, int propertyIndex, bool fromAuction) {
		GameParentObject.Dispatch(
			new PropertyAquiredEvent { playerId = playerId, PropertyIndex = propertyIndex, FromAuction = fromAuction });
		ChangePhase(playerId, Phase.PlayerAction);
	}

	[Broadcast]
	public void EmitPropertyAuctionEvent(int propertyIndex, ulong playerId) {
		GameParentObject.Dispatch(
			new PropertyAuctionEvent { PropertyIndex = propertyIndex });
		ChangePhase(playerId, Phase.Auction);
	}

	[Broadcast]
	public void EmitPlayerPaymentEvent(ulong playerId, ulong recipientId, int amount,
	                                   Phase newPhase = Phase.PlayerAction) {
		GameParentObject.Dispatch(new PlayerPaymentEvent(playerId, recipientId, amount, newPhase));
	}

	[Broadcast(NetPermission.HostOnly)]
	public void EmitSpecialPropertyActionEvent(SpecialPropertyActionType type, ulong playerId) {
		GameParentObject.Dispatch(new SpecialPropertyActionEvent());

		if (type == SpecialPropertyActionType.Jail) {
			GameParentObject.Dispatch(new LandOnJailEvent(playerId));
		}
		else if (type == SpecialPropertyActionType.Police) {
			GameParentObject.Dispatch(new GoToJailEvent(playerId));
		}
	}

	[Broadcast]
	public void EmitPlayerActionEvent(PlayerActionType type) {
		GameParentObject.Dispatch(new PlayerActionEvent());
	}

	[Broadcast]
	public void EmitTurnFinishedEvent() {
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
		GameParentObject.Dispatch(new PropertyMortgagedEvent { PropertyIndex = propertyIndex, playerId = playerId });
	}

	[Broadcast]
	public void EmitBuildHouseEvent(int propertyIndex, ulong playerId) {
		GameParentObject.Dispatch(new BuildHouseEvent(propertyIndex, playerId));
	}

	[Broadcast]
	public void EmitDestroyHouseEvent(int propertyIndex, ulong playerId) {
		GameParentObject.Dispatch(new DestroyHouseEvent(propertyIndex, playerId));
	}

	[Broadcast]
	public void EmitPropertyMortgagePayedEvent(int propertyIndex, ulong playerId) {
		GameParentObject.Dispatch(new PropertyMortgagePayedEvent {
			PropertyIndex = propertyIndex, playerId = playerId
		});
	}

	[Broadcast]
	public void EmitEventCardClosedEvent(int cardId, ulong playerId, bool isChance) {
		Card card = isChance
			? CardActionManager.GetChanceCardFromActionId(cardId)
			: CardActionManager.GetCommunityCardFromActionId(cardId);
		GameParentObject.Dispatch(new EventCardClosedEvent(card, playerId));
	}

	[Broadcast]
	public void EmitTurnActionDoneEvent(bool senderIsBank, ulong recipient, ulong sender,
	                                    Phase phase = Phase.PlayerAction) {
		GameParentObject.Dispatch(senderIsBank
			? new TurnActionDoneEvent(recipient, phase)
			: new TurnActionDoneEvent(sender, phase));
	}

	[Broadcast]
	public void ChangePhase(ulong playerId, Phase phase) {
		Log.Info("Try changing phase to " + phase);
		var player = CurrentLobby.Players.Find(player => player.SteamId == playerId);

		if (player.EliminatedPosition > 0) {
			CurrentPhase = Phase.Rolling;
		}

		if (phase.Equals(Phase.PlayerAction)) {
			CurrentPhase = player.DoublesCount is > 0 and < 3 ? Phase.Rolling : Phase.PlayerAction;
			if (player.DoublesCount == 3) {
				player.DoublesCount = 0;
			}

			return;
		}

		CurrentPhase = phase;
	}

	[Broadcast]
	public void EmitPayJailFineEvent(ulong playerId) {
		// Money sound so every one knows player payed
		GameParentObject.Dispatch(new PayJailFineEvent(playerId));
	}

	[Broadcast]
	public void EmitUseJailCardEvent(ulong playerId) {
		GameParentObject.Dispatch(new UseJailCardEvent(playerId));
	}

	[Broadcast]
	public void EmitTradingRequestedEvent(ulong PlayerId, ulong TradingRecipient) {
		GameParentObject.Dispatch(
			new TradingRequestedEvent { TradingRecipient = TradingRecipient, playerId = PlayerId });
	}

	[Broadcast]
	public void EmitTradingAcceptedEvent(ulong PlayerId) {
		GameSounds.PlayUI(UiSounds.BtnSuccess);
		GameParentObject.Dispatch(
			new TradingAcceptedEvent() { playerId = PlayerId });
	}

	[Broadcast]
	public void EmitTradingDeniedEvent(ulong PlayerId) {
		GameSounds.PlayUI(UiSounds.BtnSuccess);
		GameParentObject.Dispatch(
			new TradingDeniedEvent() { playerId = PlayerId });
	}

	[Broadcast]
	public void EmitPayoutFreeParkingEvent(ulong playerId) {
		GameParentObject.Dispatch(new PayoutFreeParkingEvent(playerId));
	}

	[Broadcast]
	public void EmitPlayerBankruptEvent(ulong player, ulong recipient) {
		GameParentObject.Dispatch(new PlayerBankruptEvent(player, recipient));
	}
}
