using System.Threading.Tasks;

public sealed class UiboardController : Component {
    [Property]
    private CameraComponent Camera { get; set; }


    protected override Task OnLoad() {
        return base.OnLoad();
    }

    protected override void OnUpdate() {
        var direction = Camera.Transform.Rotation.Forward;
        var maxDistance = 10000;
        
        var startPosition = Camera.Transform.Position;

        startPosition.x -= (Mouse.Position.y - Screen.Height / 2) / 2;
        startPosition.y -= (Mouse.Position.y - Screen.Height / 2) / 2;

        startPosition.x += (Mouse.Position.x - Screen.Width / 2) / 2;
        startPosition.y -= (Mouse.Position.x - Screen.Width / 2) / 2;
        
        var endPosition = startPosition + direction * maxDistance;
        var trace = Scene.Trace.Ray(startPosition, endPosition).IgnoreGameObject(Camera.GameObject).Run();

        if (trace.Hit) {
            var box = new BBox( new Vector3( -10, -10, -10 ), new Vector3( 10, 10, 10 ) );
            var finalBox = box.Translate( trace.HitPosition - box.Center + Vector3.Up * 10 );
            Gizmo.Draw.Sprite(finalBox.Center, 10, "ui/jail.png");
        }
    }
}