using System;
using System.Threading.Tasks;
using Sandbox.Components.GameLoop;
using Sandbox.Events;
using Sandbox.Events.GameStateEvents;
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
		if (null == Prefab) {
			return "Kaputt";
		}

		return Prefab.Name;
	}
}

public sealed class Lobby : Component, Component.INetworkListener, IGameEventHandler<ChangePawnSelectionEvent> {
	[HostSync] public NetDictionary<int, ulong> SelectedPawns { get; set; } = new();

	[Property] public long MaxPlayers { get; set; } = 5;

	[Property] private GameObject LobbyPlayer { get; set; }
	[Property] public List<PawnWrapper> PlayerPrefabs { get; set; }

	[Property] public List<Player> Players => new(Game.ActiveScene.GetAllComponents<Player>());

	[Property] public GameObject SpawnLocation { get; set; }

	[Property] public Panel LobbyPanel { get; set; }

	[Property] public GameObject DicePrefab { get; set; }

	public override int GetHashCode() {
		return HashCode.Combine(Players, SelectedPawns.Values);
	}

	public void OnGameEvent(ChangePawnSelectionEvent eventArgs) {
		// Only host should change this stuff
		if (!Networking.IsHost) {
			return;
		}

		// First we have to search for the target pawn
		var pawn = PlayerPrefabs.First(pawnWrapper => pawnWrapper.Id == eventArgs.pawnId);
		var callerSteamId = eventArgs.callerSteamId;

		var lobbySelectedPawns = SelectedPawns;
		var pawnOwnership = lobbySelectedPawns.First(pair => pair.Key.Equals(pawn.Id)).Value;

		// owned by 0 means unowned! 

		// Do nothing if client somehow pressed pawn that is claimed by another player
		if (pawnOwnership != 0 && pawnOwnership != callerSteamId) {
			return;
		}

		// If pawn is ours just deselect it
		if (pawnOwnership == callerSteamId) {
			lobbySelectedPawns[pawn.Id] = 0;
			return;
		}

		// Pawn must be unowned

		// If we own another pawn deselect the other pawn
		var firstOrDefault = lobbySelectedPawns.FirstOrDefault(pair => pair.Value == callerSteamId);
		if (!firstOrDefault.Equals(new KeyValuePair<int, ulong>())) {
			lobbySelectedPawns[firstOrDefault.Key] = 0;
		}

		lobbySelectedPawns[pawn.Id] = callerSteamId;
	}


	public void OnActive(Connection conn) {
		Log.Info(Networking.IsHost ? "Lobby created!" : $"Player '{conn.DisplayName}' joined!");


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
		if (Scene.IsEditor || !Networking.IsHost) {
			return;
		}

		// Prefill SelectedPawns with claimed if of 0 for every possible pawn
		PlayerPrefabs.ForEach(pawn => { SelectedPawns.Add(pawn.Id, 0L); });


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


			var diceSpawned = false;

			foreach (var pair in SelectedPawns) {
				if (pair.Value == 0) {
					continue;
				}

				var conn = Connection.All.First(con => con.SteamId == pair.Value);

				var playerObj = PlayerPrefabs.First(prep => prep.Id == pair.Key)
				                             .Prefab.Clone(SpawnLocation.Transform.World,
					                             name: conn.DisplayName + " - Pawn");

				playerObj.BreakFromPrefab();
				playerObj.Children[0].BreakFromPrefab();
				var player = playerObj.Children[0].Components.Get<Player>();
				player.Name = conn.DisplayName;
				player.SteamId = conn.SteamId;
				player.Connection = conn;

				playerObj.NetworkSpawn(conn);

				if (!diceSpawned) {
					GameObject parent = Game.ActiveScene.Children[1];

					var dice1 = DicePrefab.Clone();
					dice1.BreakFromPrefab();
					dice1.NetworkSpawn(conn);
					parent.Children[0].AddSibling(dice1, true);

					var dice2 = DicePrefab.Clone();
					dice2.BreakFromPrefab();
					dice2.NetworkSpawn(conn);
					parent.Children[0].AddSibling(dice2, true);

					diceSpawned = true;
				}
			}

			EmitStartGame();
		}
	}


	public void EmitStartGame() {
		Game.ActiveScene.Dispatch(new GameStartEvent());
	}
}
