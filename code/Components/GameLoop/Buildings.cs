public sealed class Buildings : Component {
    private GameObject FirstHouse;
    private GameObject FourthHouse;
    private GameObject Hotel;
    private int HousesDisplayed;
    private GameObject SecondHouse;
    private GameObject ThirdHouse;
    [Property] private GameLocation GameLocation { get; set; }

    protected override void OnStart() {
        // Get GameLocation for built houses information
        GameLocation = GameObject.Parent.Components.Get<GameLocation>();

        GameObject.BreakFromPrefab();
        var children = GameObject.Children;
        children.ForEach(go => go.BreakFromPrefab());
        children[1].Children.ForEach(go => go.BreakFromPrefab());

        Hotel = children[0];
        FirstHouse = children[1].Children[0];
        SecondHouse = children[1].Children[1];
        ThirdHouse = children[1].Children[2];
        FourthHouse = children[1].Children[3];
    }

    protected override void OnUpdate() {
        if (HousesDisplayed != GameLocation.Houses) {
            HousesDisplayed = GameLocation.Houses;
            EnableBuilding(HousesDisplayed);
        }
    }

    private void EnableBuilding(int index) {
        FirstHouse.Enabled = index >= 4 && index != 5;
        SecondHouse.Enabled = index >= 3 && index != 5;
        ThirdHouse.Enabled = index >= 2 && index != 5;
        FourthHouse.Enabled = index >= 1 && index != 5;
        Hotel.Enabled = 5 == index;
    }
}