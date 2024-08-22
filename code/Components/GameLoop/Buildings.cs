public sealed class Buildings : Component {
    [Property] private GameLocation GameLocation { get; set; }

    protected override void OnStart() {
        Log.Info(GameObject.Parent);
        GameLocation = GameObject.Parent.Components.Get<GameLocation>();
    }

    protected override void OnUpdate() {
        Log.Info(GameLocation.Name);
    }
}