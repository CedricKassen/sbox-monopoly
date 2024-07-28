using Sandbox;

public sealed class CameraManager : Component
{
	[Property]
	public Dictionary<string, CameraComponent> Cameras { get; set; }
	public string ActiveCamera { get; set; }

	public void SetActiveCamera(string name) {
		if (Cameras.TryGetValue(name, out var newCamera)) {
			newCamera.IsMainCamera = true;
			newCamera.Enabled = true;
		}
		
		if (Cameras.TryGetValue(ActiveCamera, out var oldCamera)) {
			oldCamera.IsMainCamera = false;
			oldCamera.Enabled = false;
		}

		ActiveCamera = name;
	}
	
	protected override void OnUpdate()
	{
	}
}