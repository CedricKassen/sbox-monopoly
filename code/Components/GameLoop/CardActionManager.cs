using System;
using System.Threading.Tasks;
using Monopoly.UI.Screens.GameLoop;
using Sandbox.Constants;

public sealed class CardActionManager : Component {
    [Property]
    private readonly bool IsCommunityJailCardPresent = true;

    [Property]
    private List<Card> ChanceCards;

    [Property]
    private List<Card> CommunityCards;

    [Property]
    private bool IsChanceJailCardPresent = true;

    [Property]
    public MovementManager MovementManager { get; set; }

    [Property]
    public IngameStateManager IngameStateManager { get; set; }

    protected override Task OnLoad() {
        FillChangeCards();
        FillCommunityCards();
        return base.OnLoad();
    }

    private void FillChangeCards() {
        ChanceCards = new List<Card>(Cards.Chance);

        if (!IsChanceJailCardPresent) ChanceCards.Remove(ChanceCards.Find(card => card.ActionId == 9));

        Shuffle(ChanceCards);
    }

    private void FillCommunityCards() {
        CommunityCards = new List<Card>(Cards.CommunityChest);

        if (!IsCommunityJailCardPresent) CommunityCards.Remove(CommunityCards.Find(card => card.ActionId == 5));

        Shuffle(ChanceCards);
    }

    private static void Shuffle<T>(IList<T> list) {
        var rng = new Random();
        var n = list.Count;
        while (n > 1) {
            n--;
            var k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }


    public void DisplayCardFor(Player player, GameLocation location) {
        Log.Info("Show " + location.EventId);

        switch (location.Type) {
            case GameLocation.PropertyType.Event:
                if (location.EventId == "chance") DisplayChance(player, location);

                if (location.EventId == "community") DisplayCommunity(player, location);

                break;
            default:
                DisplayPropertyCard(player, location);
                break;
        }
    }

    private void DisplayChance(Player player, GameLocation location) {
        var card = ChanceCards[0];
        ChanceCards.Remove(card);

        Log.Info("Draw Chance: " + card.Text);

        IngameStateManager.Data = card;
        IngameStateManager.State = IngameUI.IngameUiStates.Chance;

        int targetLocation;
        switch (card.ActionId) {
            case 1:
                // Go to Broadwalk
                MovementManager.StartMovement(player, CalculateFieldsToTravel(player.CurrentField, 39));
                break;
            case 2:
                // Go to Go
                MovementManager.StartMovement(player, CalculateFieldsToTravel(player.CurrentField, 40));
                break;
            case 3:
                // Go to Illinois Avenue
                MovementManager.StartMovement(player, CalculateFieldsToTravel(player.CurrentField, 24));
                break;
            case 4:
                // Go to St. Charles Place
                MovementManager.StartMovement(player, CalculateFieldsToTravel(player.CurrentField, 11));
                break;
            case 5:
            case 6:
                // Go to next line
                targetLocation = FindNearestLine(player.CurrentField);
                MovementManager.StartMovement(player, CalculateFieldsToTravel(player.CurrentField, targetLocation));
                player.Tags.Add(PlayerTags.RAILROAD_EVENT.ToString());
                break;
            case 7:
                // Go to next Utility Field 12 & 28
                targetLocation = player.CurrentField is < 12 or > 28 ? 12 : 28;
                MovementManager.StartMovement(player, CalculateFieldsToTravel(player.CurrentField, targetLocation));
                player.Tags.Add(PlayerTags.UTILITY_EVENT.ToString());
                break;
            case 8:
                // Bank divident
                player.Money += 50;
                break;
            case 9:
                // Get out of Jail Card
                IsChanceJailCardPresent = false;
                IngameStateManager.OwnedFields["chanceJailFree"] = player.SteamId;
                break;
            case 10:
                MovementManager.StartMovement(player, (player.CurrentField - 3) % 40);
                break;
            case 11:
                // Go to Jail
                break;
            case 12:
                // Repair houses (house: 25, hotel 100)
                break;
            case 13:
                // Speeding fine
                player.Money -= 15;
                break;
            case 14:
                // Go to Reading Railroad
                MovementManager.StartMovement(player, CalculateFieldsToTravel(player.CurrentField, 5));
                break;
            case 15:
                // Pay each player
                List<Player> allPlayers = new(Game.ActiveScene.GetAllComponents<Player>());
                // TODO Check Bankruptcy
                player.Money -= 50 * allPlayers.Count;

                allPlayers.ForEach(otherPlayer => otherPlayer.Money += 50);
                break;
            case 16:
                // loan matures 
                player.Money += 150;
                break;
        }

        if (ChanceCards.Count == 0) FillChangeCards();
    }

    private int CalculateFieldsToTravel(int currentField, int targetField) {
        if (currentField < targetField) return targetField - currentField;

        return 40 - currentField + targetField;
    }

    private int FindNearestLine(int playerCurrentField) {
        var lines = new List<int> { 5, 15, 25, 35 };
        return lines.Find(field => playerCurrentField < field || (playerCurrentField > 35 && field == 5));
    }


    private void DisplayCommunity(Player getPlayerFromEvent, GameLocation location) {
        var card = CommunityCards[0];
        CommunityCards.Remove(card);

        if (CommunityCards.Count == 0) FillCommunityCards();
    }

    private void DisplayPropertyCard(Player player, GameLocation location) {
        IngameStateManager.State = IngameUI.IngameUiStates.Buying;
        IngameStateManager.Data = location;
    }
}