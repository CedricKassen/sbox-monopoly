using System;
using System.Threading.Tasks;
using EnumExtensions.Settings;
using EnumExtensions.Util;
using Sandbox.Components.GameLoop;
using Sandbox.Events;
using Sandbox.Events.GameStateEvents;
using Sandbox.Events.LobbyEvents;
using Sandbox.Network;
using Sandbox.UI;

public class PawnWrapper {
	public int Id { get; set; }
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

	[Property] public long MaxPlayers { get; set; } = 6;

	[Property] private GameObject LobbyPlayer { get; set; }
	[Property] private TurnManager TurnManager { get; set; }
	[Property] public List<PawnWrapper> PlayerPrefabs { get; set; }

	[Property]
	public List<Player> Players =>
		new(Game.ActiveScene.GetAllComponents<Player>().Where(player => player.EliminatedPosition <= 0));

	[Property]
	public List<Player> AllPlayers {
		get {
			if (Game.ActiveScene.IsValid()) {
				return new(Game.ActiveScene.GetAllComponents<Player>());
			}

			return new();
		}
	}

	[Property] public GameObject SpawnLocation { get; set; }

	[Property] public Panel LobbyPanel { get; set; }

	[Property] public GameObject DicePrefab { get; set; }
	[Property] public GameObject SpeedDicePrefab { get; set; }
	[Property, HostSync] private bool GameActive { get; set; } = false;

	public override int GetHashCode() {
		return HashCode.Combine(AllPlayers, Players);
	}

	public void OnGameEvent(ChangePawnSelectionEvent eventArgs) {
		// Only host should change this stuff
		if ((ulong)Game.SteamId == eventArgs.callerSteamId) {
			GameSounds.PlaySFX(SfxSounds.Ploop);
		}

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

		if (GameActive) {
			JoinInActiveGame();
			return;
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
		var name = player.Name;
		player.GameObject.Destroy();

		Log.Info(name + " left and was destroyed!");
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

				if (LobbySettingsSystem.Current.SpeedDice) {
					player.Money += 1000;
				}

				playerObj.NetworkSpawn(conn);

				if (!diceSpawned) {
					GameObject parent = Game.ActiveScene.Children[1];

					var dice1 = DicePrefab.Clone();
					dice1.BreakFromPrefab();
					SetRandomRotation(dice1);
					dice1.NetworkSpawn(conn);
					parent.Children[0].AddSibling(dice1, true);

					var dice2 = DicePrefab.Clone();
					dice2.BreakFromPrefab();
					SetRandomRotation(dice2);
					dice2.NetworkSpawn(conn);
					parent.Children[0].AddSibling(dice2, true);

					if (LobbySettingsSystem.Current.SpeedDice) {
						GameObject spawner =
							Game.ActiveScene.Children[1].Children.First(go => go.Tags.Contains("speed_dice_spawner"));
						var speedDice = SpeedDicePrefab.Clone();
						speedDice.BreakFromPrefab();
						SetRandomRotation(speedDice);
						speedDice.Transform.Position = spawner.Transform.Position;
						speedDice.NetworkSpawn(conn);
						parent.Children[0].AddSibling(speedDice, true);
					}

					diceSpawned = true;
				}
			}

			GameActive = true;
			EmitStartGame();
		}
	}

	private void SetRandomRotation(GameObject go) {
		Random rnd = new Random(go.GetHashCode());
		Rotation newRot = new();
		newRot.w = rnd.NextSingle();
		newRot.x = rnd.NextSingle();
		newRot.y = rnd.NextSingle();
		newRot.z = rnd.NextSingle();
		go.Transform.LocalRotation = newRot;
	}

	[Broadcast]
	public void JoinInActiveGame() {
		if (!Players.Exists(player => player.SteamId == (ulong)Game.SteamId)) {
			Log.Error("You are spectating an active game!");
			LobbyPanel.Navigate("/ingame");
		}
	}

	public void EmitStartGame() {
		Game.ActiveScene.Dispatch(new GameStartEvent());
	}
}
