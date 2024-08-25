using System;

public sealed class Dice : Component {
	public bool IsRolling { get; private set; }

	[Property] public Rigidbody Rigidbody { get; set; }

	[Property] public TurnManager TurnManager { get; set; }

	private TurnManager.Phase[] RollPhases = new[] { TurnManager.Phase.Rolling, TurnManager.Phase.Jail };

	protected override void OnUpdate() {
		if (Rigidbody.Velocity == 0) {
			IsRolling = false;
		}
	}

	public void Roll() {
		if (IsRolling || !RollPhases.Any(phase => phase.Equals(TurnManager.CurrentPhase))) {
			return;
		}

		IsRolling = true;
		Rigidbody.Velocity += Vector3.Up * new Random().Next(400, 700);
		Rigidbody.AngularVelocity +=
			new Vector3(GetRandomFloat() * 1.2f, GetRandomFloat() * 1.2f, GetRandomFloat() * 0.5f);
		var sound = Sound.Play("dice", Transform.World.Position);
		sound.Volume = 1f;
	}

	private float GetRandomFloat() {
		var rng = new Random();
		return rng.Next(4, 7) * (1 + rng.NextSingle());
	}

	public Vector3 GetRotation() {
		return GameObject.Transform.Rotation.Angles().AsVector3();
	}

	public int GetRollValue() {
		var rotation = GetRotation();

		if (Math.Abs(rotation.z - 0f) <= 44) {
			if (Math.Abs(rotation.x - 0) <= 44) {
				return 6;
			}

			if (Math.Abs(rotation.x - 90) <= 44) {
				return 4;
			}

			if (Math.Abs(rotation.x + 90) <= 44) {
				return 3;
			}
		}

		if (Math.Abs(rotation.z - 90f) <= 44) {
			if (Math.Abs(rotation.x - 0) <= 44) {
				return 2;
			}
		}

		if (Math.Abs(rotation.z + 90f) <= 44) {
			if (Math.Abs(rotation.x - 0) <= 44) {
				return 5;
			}
		}

		if (Math.Abs(rotation.z - 180f) <= 44 || Math.Abs(rotation.z + 180f) <= 30) {
			if (Math.Abs(rotation.x - 0) <= 44) {
				return 1;
			}
		}


		return 0;
	}
}
