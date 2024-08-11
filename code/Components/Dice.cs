using System;

public sealed class Dice : Component {
	public bool IsRolling { get; private set; }

	[Property] public Rigidbody Rigidbody { get; set; }

	protected override void OnUpdate() {
		if (Rigidbody.Velocity == 0) {
			IsRolling = false;
		}
	}

	public void Roll() {
		if (IsRolling) {
			return;
		}

		var rng = new Random();
		IsRolling = true;
		Rigidbody.Velocity += Vector3.Up * rng.Next(400, 700);
		Rigidbody.AngularVelocity +=
			Vector3.Random * (Vector3.Random + Vector3.Random * rng.Next(1, 2)) * rng.Next(10, 15);
		var sound = Sound.Play("dice", Transform.World.Position);
		sound.Volume = 1f;
		Log.Info(sound.Name);
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
