using System.Threading.Tasks;

public sealed class UiboardController : Component {
    [Property]
    private CameraComponent Camera { get; set; }


    protected override Task OnLoad() {
        return base.OnLoad();
    }

    protected override void OnUpdate() {
        var maxDistance = 10000;

        var trace = Scene.Trace.Ray(Camera.ScreenPixelToRay(Mouse.Position), 10000).Run();

        if (trace.Hit) {
            var box = new BBox( new Vector3( -10, -10, -10 ), new Vector3( 10, 10, 10 ) );
            var finalBox = box.Translate( trace.HitPosition - box.Center + Vector3.Up * 10 );
            Gizmo.Draw.Sprite(finalBox.Center, 10, "ui/jail.png");
        }
    }
}