using System;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.Utility;

public sealed class Dice : Component {
	private bool _isrolling;

	public bool IsRolling {
		get { return _isrolling; }
	}
	
	[Property]
	public Rigidbody Rigidbody { get; set; }

	protected override void OnUpdate() {
		if (Rigidbody.Velocity == 0) {
			_isrolling = false;
		}
	}

	public void Roll() {
		if (_isrolling) return;
		
		_isrolling = true;
		Rigidbody.Velocity += Vector3.Up * 1000;
		Rigidbody.AngularVelocity += Vector3.Random * 10;
	}

	public Vector3 GetRotation() {
		return GameObject.Transform.Rotation.Angles().AsVector3();
	}

	public int GetRollValue() {
		Vector3 rotation = GetRotation();
		
		if (Math.Abs(rotation.z - 0f) <= 30) {
			if (Math.Abs(rotation.x - 0) <= 30) {
				return 6;
			}
			
			if (Math.Abs(rotation.x - 90) <= 30) {
				return 4;
			}
			
			if (Math.Abs(rotation.x + 90) <= 30) {
				return 3;
			}
		}

		if (Math.Abs(rotation.z - 90f) <= 30) {
			if (Math.Abs(rotation.x - 0) <= 1) {
				return 2;
			}
		}
		
		if (Math.Abs(rotation.z + 90f) <= 30) {
			if (Math.Abs(rotation.x - 0) <= 1) {
				return 5;
			}
		}
		
		if ((Math.Abs(rotation.z - 180f) <= 30) | (Math.Abs(rotation.z + 180f) <= 30)) {
			if (Math.Abs(rotation.x - 0) <= 30) {
				return 1;
			}
		}
		

		return 0;
	}
}
