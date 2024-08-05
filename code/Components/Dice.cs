using System;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.Utility;

public sealed class Dice : Component {
	private bool _isRolling;

	public bool IsRolling {
		get { return _isRolling; }
	}
	
	[Property]
	public Rigidbody Rigidbody { get; set; }

	protected override void OnUpdate() {
		if (Rigidbody.Velocity == 0) {
			_isRolling = false;
		}
	}

	public void Roll() {
		if (_isRolling) return;
		
		_isRolling = true;
		Rigidbody.Velocity += Vector3.Up * 1000;
		Rigidbody.AngularVelocity += Vector3.Random * 10;
	}

	public Vector3 GetRotation() {
		return GameObject.Transform.Rotation.Angles().AsVector3();
	}

	public int GetRollValue() {
		Vector3 rotation = GetRotation();
		
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
		
		if ((Math.Abs(rotation.z - 180f) <= 44) || (Math.Abs(rotation.z + 180f) <= 30)) {
			if (Math.Abs(rotation.x - 0) <= 44) {
				return 1;
			}
		}
		

		return 0;
	}
}
