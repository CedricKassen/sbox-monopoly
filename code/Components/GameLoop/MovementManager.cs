public sealed class MovementManager : Component
{
	[Property] private int CurrentField;

	private float nextUpdate;
	[Property] private Player Player;
	[Property] private Rigidbody PlayerBody;
	[Property] private int ToTravel;
	[Property] private int Travelled;
	[Property] public GameObject LocationContainer { get; set; }

	public void StartMovement(Player player, int fieldsToTravel)
	{
		ToTravel = fieldsToTravel;
		Player = player;
		PlayerBody = player.GameObject.Components.Get<Rigidbody>();
		CurrentField = player.CurrentField;
	}

	protected override void OnUpdate()
	{
		if (Time.Now > nextUpdate)
		{
			UpdateMove();
			nextUpdate++;
		}
	}

	private void UpdateMove()
	{
		if (Player == null)
		{
			return;
		}

		CurrentField = (CurrentField + 1) % 40;
		Log.Info(CurrentField);
		var location = LocationContainer.Children[CurrentField];
		Player.Transform.LerpTo(location.Transform.World, 1f);
		Travelled++;

		if (CurrentField % 10 == 0)
		{
			var ro = PlayerBody.Transform.LocalRotation;
			PlayerBody.AngularVelocity += Vector3.Up * 10;
			Log.Info("On Corner");
		}

		if (ToTravel == Travelled)
		{
			Player.CurrentField = CurrentField;
			ToTravel = 0;
			Travelled = 0;
			Player = null;
			PlayerBody = null;
		}
	}
}
