using System;
using Sandbox.Events;
using Sandbox.Events.TurnEvents;

namespace Sandbox.Components.GameLoop;

public class PaymentEventHandler : Component, IGameEventHandler<PlayerPaymentEvent>,
                                   IGameEventHandler<PayoutFreeParkingEvent> {
	[Property, HostSync] public int BankBalance { get; private set; }

	[Property] public Lobby Lobby { get; set; }

	[Property] public TurnManager TurnManager { get; set; }

	public void OnGameEvent(PayoutFreeParkingEvent eventArgs) {
		Player recipient = GetPlayerFromId(eventArgs.PlayerId);
		Log.Info("Payout " + BankBalance + " to " + recipient.Name);
		recipient.Money += BankBalance;
		BankBalance = 0;
	}

	public void OnGameEvent(PlayerPaymentEvent eventArgs) {
		if (!Networking.IsHost) {
			return;
		}

		if (eventArgs.PlayerId == 0) {
			throw new Exception("PlayerPaymentEvent was thrown without a PlayerId");
		}

		if (eventArgs.Recipient == 0) {
			throw new Exception("PlayerPaymentEvent was thrown without a Recipient");
		}

		Player sender = GetPlayerFromId(eventArgs.PlayerId);
		Player recipient = GetPlayerFromId(eventArgs.Recipient);

		bool senderIsBank = eventArgs.PlayerId == 2;
		bool recipientIsBank = eventArgs.Recipient == 2;
		bool recipientIsFreeParking = eventArgs.Recipient == 1;


		if (!senderIsBank) {
			if (sender.Money - eventArgs.Amount < 0) {
				Game.ActiveScene.Dispatch(
					new NotEnoughFundsEvent(sender.SteamId, eventArgs.Recipient, eventArgs.Amount));
				return;
			}

			sender.Money -= eventArgs.Amount;
		}

		if (!recipientIsBank && !recipientIsFreeParking) {
			recipient.Money += eventArgs.Amount;
		}

		if (recipientIsFreeParking) {
			BankBalance += eventArgs.Amount;
		}

		Log.Info((senderIsBank ? "Bank" : recipient.Name) + " paid " + eventArgs.Amount + " to " +
		         (recipient != null ? recipient.Name : recipientIsBank ? "Bank" : "FreeParking"));
		TurnManager.EmitTurnActionDoneEvent(senderIsBank, eventArgs.Recipient, eventArgs.PlayerId);
	}

	private Player GetPlayerFromId(ulong playerId) {
		return playerId == 1 ? null : Lobby.Players.Find(player => player.SteamId == playerId);
	}
}
