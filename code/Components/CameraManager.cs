using System.Threading;
using Sandbox;
using Sandbox.Utility;

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
			newCamera.IsMainCamera = false;
			newCamera.Enabled = false;
		}

		ActiveCamera = name;
	}
	
	protected override void OnUpdate()
	{
		if (Input.Pressed("Forward")) {
			SetActiveCamera("pTophat");
		}
		if (Input.Pressed("Left")) {
			SetActiveCamera("topdown");
		}
	}
}