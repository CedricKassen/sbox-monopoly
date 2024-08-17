using System;

public sealed class UIBoardController : Component {
    [Property]
    private CameraComponent Camera { get; set; }
    private GameObject _tempObjectStore = new GameObject();
    private Vector3 scale;
    
    protected override void OnUpdate() {
        var maxDistance = 10000;
        var trace = Scene
            .Trace
            .Ray(Camera.ScreenPixelToRay(Mouse.Position), maxDistance)
            .IgnoreGameObject(Camera.GameObject)
            .WithoutTags("ignore_trace")
            .Run();

        if (trace.GameObject == null || _tempObjectStore.Id != trace.GameObject.Id) {
            Game.ActiveScene.GetAllObjects(true).First(o => _tempObjectStore.Id == o.Id).Transform.Scale = scale;
            
            if (trace.Hit) {
                _tempObjectStore = trace.GameObject;
                scale = _tempObjectStore.Transform.Scale;
                trace.GameObject.Transform.Scale *= 2f;
            }
        }
    }
}