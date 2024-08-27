using Sandbox;

public sealed class GuardTrigger : Component, Component.ITriggerListener {
	[Property] private Collider _dice { get; set; }

	public void OnTriggerEnter(Collider other) {
		if (other.Tags.Contains("dice")) {
			_dice = other;
		}
	}

	public void OnTriggerExit(Collider other) {
		_dice = null;
	}

	protected override void OnUpdate() {
		if (_dice.IsValid()) {
			Vector3 direction = new Vector3(0, 0, 0) - _dice.Transform.Position;
			direction = direction.Normal;

			_dice.Rigidbody.Velocity = direction * 100f;
		}
	}
}
