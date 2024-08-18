using System.Threading.Tasks;
using Sandbox.Events;
using Sandbox.Events.LobbyEvents;
using Sandbox.Network;
using Sandbox.UI;

public class PawnWrapper {
	private static int IdCounter;

	public PawnWrapper() {
		Id = ++IdCounter;
	}

	public int Id { get; }
	public GameObject Prefab { get; set; }
	public string ImgPath { get; set; }

	public int Width { get; set; }
	public int Height { get; set; }

	public override string ToString() {
		return Prefab.Name;
	}
}

public sealed class Lobby : Component, Component.INetworkListener, IGameEventHandler<ChangePawnSelectionEvent> {
	[Property] public Dictionary<PawnWrapper, ulong> SelectedPawns = new();
	[Property] public long MaxPlayers { get; set; } = 5;

	[Property] private GameObject LobbyPlayer { get; set; }
	[Property] public List<PawnWrapper> PlayerPrefabs { get; set; }

	[Property] public List<Player> Players => new(Game.ActiveScene.GetAllComponents<Player>());

	[Property] public GameObject SpawnLocation { get; set; }

	[Property] public Panel LobbyPanel { get; set; }

	public void OnGameEvent(ChangePawnSelectionEvent eventArgs) {
		// First we have to search for the target pawn
		var pawn = PlayerPrefabs.First(pawnWrapper => pawnWrapper.Id == eventArgs.pawnId);
		var callerSteamId = eventArgs.callerSteamId;

		var lobbySelectedPawns = SelectedPawns;
		var pawnOwnership = lobbySelectedPawns.First(pair => pair.Key.Equals(pawn)).Value;

		// owned by 0 means unowned! 

		// Do nothing if client somehow pressed pawn that is claimed by another player
		if (pawnOwnership != 0 && pawnOwnership != callerSteamId) {
			return;
		}

		// If pawn is ours just deselect it
		if (pawnOwnership == callerSteamId) {
			lobbySelectedPawns[pawn] = 0;
			return;
		}

		// Pawn must be unowned

		// If we own another pawn deselect the other pawn
		if (lobbySelectedPawns.ContainsValue(callerSteamId)) {
			lobbySelectedPawns[lobbySelectedPawns.First(pair => pair.Value == callerSteamId).Key] = 0;
		}

		lobbySelectedPawns[pawn] = callerSteamId;
	}


	public void OnActive(Connection conn) {
		if (Networking.IsHost) {
			Log.Info($"Lobby created!");
		}
		else {
			Log.Info($"Player '{conn.DisplayName}' joined!");
		}


		var playerObj = LobbyPlayer
			.Clone(SpawnLocation.Transform.World, name: conn.DisplayName + " - Lobby");
		playerObj.BreakFromPrefab();
		var player = playerObj.Components.Get<Player>();
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

	[Broadcast]
	public void PawnSelectionChanged(int pawnId, ulong callerSteamId) {
		Game.ActiveScene.Dispatch(new ChangePawnSelectionEvent(pawnId, callerSteamId));
	}

	protected override async Task OnLoad() {
		// Prefill SelectedPawns with claimed if of 0 for every possible pawn
		PlayerPrefabs.ForEach(pawn => { SelectedPawns.Add(pawn, 0L); });

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

	public void InitializePlayers() {
		if (Networking.IsHost) {
			Players.ForEach(ply => ply.GameObject.Destroy());

			foreach (var pair in SelectedPawns) {
				if (pair.Value == 0) {
					continue;
				}

				var conn = Connection.All.First(con => con.SteamId == pair.Value);
				var playerObj =
					pair.Key.Prefab.Clone(SpawnLocation.Transform.World, name: conn.DisplayName + " - Pawn");

				playerObj.BreakFromPrefab();
				playerObj.Children[0].BreakFromPrefab();
				var player = playerObj.Children[0].Components.Get<Player>();
				player.Name = conn.DisplayName;
				player.SteamId = conn.SteamId;
				player.Connection = conn;

				playerObj.NetworkSpawn(conn);
			}
		}
	}
}
