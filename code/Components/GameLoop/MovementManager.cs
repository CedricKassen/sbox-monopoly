using System.Threading.Tasks;
using Monopoly;
using Sandbox.Constants;
using Sandbox.Events;
using Sandbox.Events.TurnEvents;

public sealed class MovementManager : Component {
	private readonly TaskCompletionSource<bool> tcs = new();

	private float _timer;
	[Property] private bool Backwards;
	[Property] private int CurrentField;
	[Property] private Player Player;
	[Property] private Rigidbody PlayerBody;
	[Property] private int ToTravel;
	[Property] private int Travelled;
	[Property] public GameObject LocationContainer { get; set; }
	[Property] public float SpeedMultiplier { get; set; }


	[Button("Go to next chance field")]
	public void GoToNextChance() {
		var player = Game.ActiveScene.GetAllComponents<Player>().First();

		var lines = new List<int> { 7, 22, 36 };
		var next = lines.Find(field => player.CurrentField < field || (player.CurrentField > 36 && field == 7));
		CardActionHelper.MoveTo(next, player, this);
	}

	[Button("Go to next community field")]
	public void GoToNextCommunity() {
		var player = Game.ActiveScene.GetAllComponents<Player>().First();

		var lines = new List<int> { 2, 17, 33 };
		var next = lines.Find(field => player.CurrentField < field || (player.CurrentField > 33 && field == 2));
		CardActionHelper.MoveTo(next, player, this);
	}

	public void StartMovement(Player player, int fieldsToTravel) {
		Backwards = fieldsToTravel < 0;

		// If movement is backwards, first iteration is used to rotate player
		fieldsToTravel -= Backwards ? 1 : 0;

		ToTravel = fieldsToTravel;
		Player = player;
		PlayerBody = player.GameObject.Components.Get<Rigidbody>();

		// If movement is backwards, first iteration is used to rotate player so the starting field is the current field not the next one
		CurrentField = (player.CurrentField + (Backwards ? 0 : 1)) % 40;

		Log.Info("Move " + ToTravel + " Fields");
	}


	protected override void OnUpdate() {
		UpdateMove();
	}

	private void UpdateMove() {
		if (Player == null) {
			return;
		}

		if (_timer < 1 / SpeedMultiplier) {
			var location = LocationContainer.Children[CurrentField];
			var position = location.Transform.World;
			if (Backwards) {
				position.Rotation.RotateAroundAxis(location.Transform.Position, 180f);
			}

			Player.Transform.Parent.Transform.LerpTo(position, _timer * SpeedMultiplier);
			_timer += Time.Delta;

			return;
		}

		var currentField = CurrentField + (Backwards ? -1 : 1);
		CurrentField = Math.Mod(currentField, 40);
		Travelled++;
		_timer = 0;

		if (ToTravel == Travelled || ToTravel == -Travelled) {
			CurrentField += Backwards ? 1 : -1;

			Player.CurrentField = CurrentField;
			var steamID = Player.SteamId;

			ToTravel = 0;
			Travelled = 0;
			Backwards = false;
			Player = null;
			PlayerBody = null;

			var currentLocation = LocationContainer.Children[CurrentField].Components.Get<GameLocation>();
			Game.ActiveScene.Dispatch(new MovementDoneEvent { playerId = steamID, Location = currentLocation });
		}
	}
}
