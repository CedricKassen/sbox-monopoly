using Sandbox;

public sealed class CameraManager : Component
{
	[Property]
	public CameraRegistryEntry[] Cameras { get; set; }
	public string ActiveCamera { get; set; }
	
	protected override void OnUpdate()
	{
		
	}
}

public class CameraRegistryEntry {
	public string Name { get; set; }
	public CameraComponent Camera { get; set; }
}