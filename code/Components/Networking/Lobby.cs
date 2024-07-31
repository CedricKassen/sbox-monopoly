using Sandbox;

public sealed class Lobby : Component, Component.INetworkListener
{
	public void OnActive(Connection channel)
	{
		Log.Info( $"Player '{channel.DisplayName}' has joined the game" );
	}
}
