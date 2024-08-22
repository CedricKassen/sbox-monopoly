using System.Threading.Tasks;
using Monopoly.UI.Screens.GameLoop;
using Sandbox.Constants;
using Sandbox.Events;
using Sandbox.Events.TurnEvents;

namespace Sandbox.Components.GameLoop;

public class GameEventHandler : Component, IGameEventHandler<RolledEvent>, IGameEventHandler<MovementDoneEvent>,
    IGameEventHandler<PropertyAquiredEvent>, IGameEventHandler<PropertyAuctionEvent>,
    IGameEventHandler<AuctionFinishedEvent>, IGameEventHandler<PlayerPaymentEvent>,
    IGameEventHandler<TurnFinishedEvent>, IGameEventHandler<PropertyMortgagedEvent>,
    IGameEventHandler<PropertyMortgagePayedEvent>, IGameEventHandler<BuildHouseEvent>,
    IGameEventHandler<DestroyHouseEvent> {
    [Property] public GameObject LocationContainer { get; set; }
    [Property] public Lobby Lobby { get; set; }
    [Property] public MovementManager MovementManager { get; set; }
    [Property] public CardActionManager CardActionManager { get; set; }
    [Property] public IngameStateManager IngameStateManager { get; set; }
    [Property] public TurnManager TurnManager { get; set; }

    public void OnGameEvent(AuctionFinishedEvent eventArgs) {
        TurnManager.EmitPropertyAquiredEvent(eventArgs.playerId, eventArgs.PropertyIndex);

        if (Networking.IsHost) GetPlayerFromEvent(eventArgs.playerId).Money -= eventArgs.Amount;

        IngameStateManager.State = IngameUI.IngameUiStates.None;
    }

    public void OnGameEvent(BuildHouseEvent eventArgs) {
        var property = GetLocationFromPropertyIndex(eventArgs.PropertyIndex);
        if (property.Houses == 5) {
            Log.Error("Tried to add house to maxed out house!");
            return;
        }

        var player = GetPlayerFromEvent(eventArgs.PlayerId);
        player.Money -= property.House_Cost;
        property.Houses++;
    }

    public void OnGameEvent(DestroyHouseEvent eventArgs) {
        var property = GetLocationFromPropertyIndex(eventArgs.PropertyIndex);
        if (property.Houses == 0) {
            Log.Error("Tried to remove house from empty property!");
            return;
        }

        var player = GetPlayerFromEvent(eventArgs.PlayerId);
        player.Money += property.House_Cost / 2;
        property.Houses--;
    }

    public void OnGameEvent(MovementDoneEvent eventArgs) {
        IngameStateManager.OwnedFields.TryGetValue(eventArgs.Location.GameObject.Name, out var fieldOwner);
        var currentPlayer = GetPlayerFromEvent(eventArgs.playerId);

        TurnManager.ChangePhase(eventArgs.playerId, TurnManager.Phase.PlayerAction);

        if (fieldOwner == 0 || eventArgs.Location.Type == GameLocation.PropertyType.Event) {
            if (eventArgs.Location.EventId == "start") {
                if (Networking.IsHost) currentPlayer.Money += 200;
                return;
            }

            CardActionManager.DisplayCardFor(currentPlayer, eventArgs.Location);
            return;
        }

        // TODO if field action event is done!
        TurnManager.ChangePhase(currentPlayer.SteamId, TurnManager.Phase.PlayerAction);

        if (fieldOwner != eventArgs.playerId && !eventArgs.Location.Mortgaged) {
            if (eventArgs.Location.Type == GameLocation.PropertyType.Normal)
                TurnManager.EmitPlayerPaymentEvent(eventArgs.playerId, fieldOwner,
                    eventArgs.Location.Normal_Rent[eventArgs.Location.Houses]);

            if (eventArgs.Location.Type == GameLocation.PropertyType.Railroad) {
                var railroadCount = IngameStateManager.OwnedFields.Count(f => f.Value == fieldOwner &&
                                                                              (f.Key == "railroad1" ||
                                                                               f.Key == "railroad2" ||
                                                                               f.Key == "railroad3" ||
                                                                               f.Key == "railroad4"));
                TurnManager.EmitPlayerPaymentEvent(eventArgs.playerId, fieldOwner,
                    eventArgs.Location.Railroad_Rent[railroadCount - 1]);
            }

            if (eventArgs.Location.Type == GameLocation.PropertyType.Utility) {
                var utilityCount = IngameStateManager.OwnedFields.Count(f => f.Value == fieldOwner &&
                                                                             (f.Key == "electricCompany" ||
                                                                              f.Key == "waterCompany"));
                TurnManager.EmitPlayerPaymentEvent(eventArgs.playerId, fieldOwner,
                    eventArgs.Location.Utility_Rent_Multiplier[utilityCount - 1] * currentPlayer.LastDiceCount);
            }
        }
    }

    public void OnGameEvent(PlayerPaymentEvent eventArgs) {
        if (Networking.IsHost) {
            GetPlayerFromEvent(eventArgs.playerId).Money -= eventArgs.Amount;
            GetPlayerFromEvent(eventArgs.Recipient).Money += eventArgs.Amount;
        }
    }

    public void OnGameEvent(PropertyAquiredEvent eventArgs) {
        var location = LocationContainer.Children[eventArgs.PropertyIndex];
        var component = location.Components.Get<GameLocation>();
        var player = GetPlayerFromEvent(eventArgs.playerId);

        if (component.Price <= player.Money && Networking.IsHost) {
            player.Money -= component.Price;
            IngameStateManager.OwnedFields[location.Name] = eventArgs.playerId;
        }
    }

    public void OnGameEvent(PropertyAuctionEvent eventArgs) {
        IngameStateManager.AuctionBiddings = new NetDictionary<ulong, int>();
        IngameStateManager.State = IngameUI.IngameUiStates.Auction;

        foreach (var player in Lobby.Players) IngameStateManager.AuctionBiddings[player.SteamId] = 0;
    }

    public void OnGameEvent(PropertyMortgagedEvent eventArgs) {
        var property = GetLocationFromPropertyIndex(eventArgs.PropertyIndex);

        if (Networking.IsHost && !property.Mortgaged) {
            GetPlayerFromEvent(eventArgs.playerId).Money += property.Price / 2;
            property.Mortgaged = true;
        }
    }


    public void OnGameEvent(PropertyMortgagePayedEvent eventArgs) {
        var player = GetPlayerFromEvent(eventArgs.playerId);
        var property = GetLocationFromPropertyIndex(eventArgs.PropertyIndex);

        if (Networking.IsHost && player.Money > property.Price / 2 && property.Mortgaged) {
            player.Money -= property.Price / 2;
            property.Mortgaged = false;
        }
    }

    public void OnGameEvent(RolledEvent eventArgs) {
        var player = GetPlayerFromEvent(eventArgs.playerId);

        Log.Info(eventArgs.Doubles);
        Log.Info(eventArgs.Number);

        if (eventArgs.Doubles)
            player.DoublesCount++;
        else
            player.DoublesCount = 0;

        if (player.DoublesCount < 3)
            MovementManager.StartMovement(player, eventArgs.Number);
        else
            CardActionHelper.GoToJail(player, MovementManager);

        player.LastDiceCount = eventArgs.Number;
    }


    public void OnGameEvent(TurnFinishedEvent eventArgs) {
        TurnManager.CurrentPlayerIndex = (TurnManager.CurrentPlayerIndex + 1) % TurnManager.CurrentLobby.Players.Count;
        ChangeDiceOwnershipToCurrentPlayer();
    }

    protected override Task OnLoad() {
        ChangeDiceOwnershipToCurrentPlayer();
        return base.OnLoad();
    }

    private Player GetPlayerFromEvent(ulong playerId) {
        return Lobby.Players.Find(player => player.SteamId == playerId);
    }

    private GameLocation GetLocationFromPropertyIndex(int propertyIndex) {
        return LocationContainer.Children[propertyIndex].Components.Get<GameLocation>();
    }

    private void ChangeDiceOwnershipToCurrentPlayer() {
        var diceList = new List<Dice>(Game.ActiveScene.GetAllComponents<Dice>());

        if (Networking.IsHost && diceList.Count > 0)
            foreach (var dice in diceList)
                dice.Network.AssignOwnership(
                    TurnManager.CurrentLobby.Players[TurnManager.CurrentPlayerIndex].Connection);
    }
}