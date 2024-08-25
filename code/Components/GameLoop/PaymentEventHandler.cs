﻿using Sandbox.Events;
using Sandbox.Events.TurnEvents;

namespace Sandbox.Components.GameLoop;

public class PaymentEventHandler : Component, IGameEventHandler<PlayerPaymentEvent>,
                                   IGameEventHandler<PayoutFreeParkingEvent>,
                                   IGameEventHandler<NotEnoughFundsEvent> {
	[Property, HostSync] public int BankBalance { get; private set; }
	[Property] public Lobby Lobby { get; set; }
	[Property] public TurnManager TurnManager { get; set; }

	public void OnGameEvent(PayoutFreeParkingEvent eventArgs) {
		Player recipient = GetPlayerFromId(eventArgs.Recipient);
		recipient.Money += BankBalance;
		BankBalance = 0;
	}

	public void OnGameEvent(NotEnoughFundsEvent eventArgs) {
		Player player = GetPlayerFromId(eventArgs.PlayerId);
		int debts = eventArgs.Debts;

		// set player ui stat for player that is in depts
		// if player have enough funds after this dispatch PlayerPaymentEvent again
		// if player can't get enough funds and presses bankrupt button do: 
		/*
		 * debts to player -> give everything to the player
		 * debts to bank -> remaining money to bank and auction for every property
		 */

		// Dispatch PaymentDone Event to continue with normal gameloop after a cards starts the payment process. 
		// Maybe action have to dispatchs an event to say that action is done, so we can do the same after a payment? 
		// Trading should not use the Payment Event stuff!
	}

	public void OnGameEvent(PlayerPaymentEvent eventArgs) {
		if (!Networking.IsHost) {
			return;
		}

		Player payingPlayer = GetPlayerFromId(eventArgs.PlayerId);
		Player recipient = GetPlayerFromId(eventArgs.Recipient);


		if (payingPlayer.Money - eventArgs.Amount < 0) {
			Game.ActiveScene.Dispatch(
				new NotEnoughFundsEvent(payingPlayer.SteamId, recipient.SteamId, eventArgs.Amount));
			return;
		}


		// Payment to band
		if (recipient == null) {
			BankBalance += eventArgs.Amount;
		}
		else {
			// Paying player can be bank (id 1)
			if (payingPlayer != null) {
				payingPlayer.Money -= eventArgs.Amount;
			}

			recipient.Money += eventArgs.Amount;
		}
	}

	private Player GetPlayerFromId(ulong playerId) {
		return playerId == 1 ? null : Lobby.Players.Find(player => player.SteamId == playerId);
	}
}
