using Sandbox;
using Sandbox.Network;

public sealed class Lobby : Component, Component.INetworkListener
{

	public long maxPlayers = 5;
	[Property] [HostSync] public long playerNum { get; set; } = 0;

	[Property] public GameObject PlayerPrefab { get; set; }

	[Property]
	public List<Player> players {
		get {
			return new List<Player>(Scene.GetAllComponents<Player>());
		}
	}

	[Property] public GameObject SpawnLocation { get; set; }
	
	public void OnActive(Connection conn)
	{
		Log.Info( $"Player '{conn.DisplayName}' tritt bei" );

		GameObject playerObj = PlayerPrefab.Clone(SpawnLocation.Transform.World, name: conn.DisplayName + " - Network");
		playerObj.Components.Get<Player>().Name = conn.DisplayName;
		playerObj.Components.Get<Player>().SteamId = conn.SteamId;
		playerObj.Components.Get<Player>().Connection = conn;
		
		playerObj.NetworkSpawn(conn);
	}

	public void OnDisconnected(Connection conn)
	{
		IEnumerable<Player> currentPlayers = Scene.GetAllComponents<Player>();
		Player player = currentPlayers.First(player => player.SteamId == conn.SteamId);
		player.GameObject.Destroy();
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

