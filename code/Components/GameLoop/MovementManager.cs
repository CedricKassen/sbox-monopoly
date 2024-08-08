using System;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

public sealed class MovementManager : Component
{
	[Property] private int CurrentField;
	
	private float _timer;
	[Property] private Player Player;
	[Property] private Rigidbody PlayerBody;
	[Property] private int ToTravel;
	[Property] private int Travelled;
	[Property] public GameObject LocationContainer { get; set; }
	[Property] public float SpeedMultiplier { get; set; }

	public void StartMovement(Player player, int fieldsToTravel)
	{
		ToTravel = fieldsToTravel;
		Player = player;
		PlayerBody = player.GameObject.Components.Get<Rigidbody>();
		CurrentField = (player.CurrentField + 1) % 40;;
	}

	protected override void OnUpdate()
	{
		UpdateMove();
	}

	private void UpdateMove()
	{
		if (Player == null) {
			return;
		}
	
		if (_timer < 1 / SpeedMultiplier) {
			GameObject location = LocationContainer.Children[CurrentField];
			Player.Transform.LerpTo(location.Transform.World, _timer * SpeedMultiplier);
			_timer += Time.Delta;
			
			return;
		}
		
		CurrentField = (CurrentField + 1) % 40;
		Travelled++;
		_timer = 0;

		if (ToTravel == Travelled) {
			Player.CurrentField = CurrentField - 1;
			ToTravel = 0;
			Travelled = 0;
			Player = null;
			PlayerBody = null;
		}
	}
}
