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

		IsRolling = true;
		Rigidbody.Velocity += Vector3.Up * 1000;
		Rigidbody.AngularVelocity += Vector3.Random * 10;
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
