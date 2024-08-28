using Sandbox;

public sealed class GuardTrigger : Component, Component.ITriggerListener {
	[Property] private List<Collider> _dice { get; set; } = new();

	public void OnTriggerEnter(Collider other) {
		if (other.Tags.Contains("dice")) {
			_dice.Add(other);
		}
	}

	public void OnTriggerExit(Collider other) {
		_dice.Remove(other);
	}

	protected override void OnUpdate() {
		_dice.ForEach(dice => {
			Vector3 direction = new Vector3(0, 0, 0) - dice.Transform.Position;
			direction = direction.Normal;

			dice.Rigidbody.Velocity = direction * 100f;
		});
	}
}
