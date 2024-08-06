using Sandbox;
using Sandbox.Network;

public sealed class Lobby : Component, Component.INetworkListener
{

	public long maxPlayers = 5;
	[Property] [HostSync] public long playerNum { get; set; } = 0;

	[Property] public GameObject PlayerPrefab { get; set; }
	[Property] public List<Player> players;
	
	public void OnActive(Connection conn)
	{
		Log.Info( $"Player '{conn.DisplayName}' tritt bei" );

		GameObject playerObj = PlayerPrefab.Clone();
		playerObj.Name = conn.DisplayName + " - Network";
		playerObj.Components.Get<Player>().Name = conn.DisplayName;
		playerObj.Components.Get<Player>().SteamId = conn.SteamId;

		
		playerObj.NetworkSpawn();
		playerObj.Network.AssignOwnership(conn);
	
		playerNum++;
	}

	public void OnDisconnected(Connection conn)
	{
		IEnumerable<Player> currentPlayers = Scene.GetAllComponents<Player>();
		Player player = currentPlayers.First(player => player.SteamId == conn.SteamId);
		player.GameObject.Destroy();
		playerNum--;
	}

	public List<Player> getCurrentPlayers()
	{
		return new List<Player>(Scene.GetAllComponents<Player>());
	}

	public override int GetHashCode()
	{
		return System.HashCode.Combine(playerNum);
	}
	
}

