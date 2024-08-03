using Sandbox;
using Sandbox.Network;

public sealed class Lobby : Component, Component.INetworkListener
{

	[Property] public long maxPlayers = 5;
	[Property] public long playerNum = 0;

	[Property] public List<Player> players = new List<Player>();
	

	public void OnActive(Connection conn)
	{
		Log.Info( $"Player '{conn.DisplayName}' tritt bei" );
		playerNum++;
		players.Add(new Player(conn.SteamId, conn.DisplayName));

	}

	public void OnDisconnected(Connection conn)
	{
		players.Remove(players.Find(player => player.steamId == conn.SteamId));
		Log.Info("On Disconneced");
		GameNetworkSystem.Disconnect();
	}

	public override int GetHashCode()
	{
		return System.HashCode.Combine(playerNum);
	}
	
}

public class Player
{
	public ulong steamId { get; }
	public string name;
	private int money = 2000;
	private List<string> cards = new List<string>();

	public Player(ulong steamId, string name)
	{
		this.steamId = steamId;
		this.name = name;
	}
}

