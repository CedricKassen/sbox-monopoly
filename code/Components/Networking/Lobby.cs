using System.Threading.Tasks;
using Sandbox.Network;
using Sandbox.UI;

public sealed class Lobby : Component, Component.INetworkListener {
	[Property] public long MaxPlayers { get; set; } = 5;

	[Property] public List<GameObject> PlayerPrefabs { get; set; }

	[Property] public List<Player> Players => new(Game.ActiveScene.GetAllComponents<Player>());

	[Property] public GameObject SpawnLocation { get; set; }

	[Property] public Panel LobbyPanel { get; set; }

	public void OnActive(Connection conn) {
		Log.Info($"Player '{conn.DisplayName}' tritt bei");

		var playerObj = PlayerPrefabs[0].Clone(SpawnLocation.Transform.World, name: conn.DisplayName + " - Network");
		playerObj.BreakFromPrefab();
		playerObj.Children[0].BreakFromPrefab();
		var player = playerObj.Children[0].Components.Get<Player>();
		Log.Info(player);
		player.Name = conn.DisplayName;
		player.SteamId = conn.SteamId;
		player.Connection = conn;

		playerObj.NetworkSpawn(conn);
	}

	public void OnDisconnected(Connection conn) {
		var currentPlayers = Scene.GetAllComponents<Player>();
		var player = currentPlayers.First(player => player.SteamId == conn.SteamId);
		player.GameObject.Destroy();
	}

	protected override async Task OnLoad() {
		if (Scene.IsEditor) {
			return;
		}

		if (!GameNetworkSystem.IsActive) {
			LoadingScreen.Title = "Creating Lobby";
			await Task.DelayRealtimeSeconds(0.1f);
			GameNetworkSystem.CreateLobby();
		}
	}

	[Broadcast(NetPermission.HostOnly)]
	public void StartGame() {
		LobbyPanel.Navigate("/ingame");
	}
}
