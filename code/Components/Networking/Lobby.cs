using System.Threading.Tasks;
using Sandbox.Network;
using Sandbox.UI;

public sealed class Lobby : Component, Component.INetworkListener
{
	[Property] public long MaxPlayers { get; set; } = 5;

	[Property] public GameObject PlayerPrefab { get; set; }

	[Property] public List<Player> Players => new(Game.ActiveScene.GetAllComponents<Player>());

	[Property] public GameObject SpawnLocation { get; set; }

	public void OnActive(Connection conn)
	{
		Log.Info($"Player '{conn.DisplayName}' tritt bei");

		var playerObj = PlayerPrefab.Clone(SpawnLocation.Transform.World, name: conn.DisplayName + " - Network");
		playerObj.Components.Get<Player>().Name = conn.DisplayName;
		playerObj.Components.Get<Player>().SteamId = conn.SteamId;
		playerObj.Components.Get<Player>().Connection = conn;

		playerObj.NetworkSpawn(conn);
	}

	public void OnDisconnected(Connection conn)
	{
		var currentPlayers = Scene.GetAllComponents<Player>();
		var player = currentPlayers.First(player => player.SteamId == conn.SteamId);
		player.GameObject.Destroy();
	}

	protected override async Task OnLoad()
	{
		if (Scene.IsEditor)
		{
			return;
		}

		if (!GameNetworkSystem.IsActive)
		{
			LoadingScreen.Title = "Creating Lobby";
			await Task.DelayRealtimeSeconds(0.1f);
			GameNetworkSystem.CreateLobby();
		}
	}

	[Broadcast]
	public void ChangeGameScene(Panel panel, string url)
	{
		panel.Navigate(url);
	}
}
