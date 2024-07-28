using Sandbox;

public sealed class CameraManager : Component
{
	[Property]
	public Dictionary<string, CameraComponent> Cameras { get; set; }
	public string ActiveCamera { get; set; }
	
	protected override void OnUpdate()
	{
		
	}
}