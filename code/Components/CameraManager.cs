public sealed class CameraManager : Component
{
	[Property]
	public Dictionary<string, CameraComponent> Cameras { get; set; }
	
	public string ActiveCamera { get; set; }

	public void SetActiveCamera(string name) {
		if (ActiveCamera != null) {
			if (Cameras.TryGetValue(ActiveCamera, out var oldCamera)) {
				oldCamera.IsMainCamera = false;
				oldCamera.Enabled = false;
			}
		}

		if (Cameras.TryGetValue(name, out var newCamera)) {
			newCamera.IsMainCamera = true;
			newCamera.Enabled = true;
		}

		ActiveCamera = name;
	}
}