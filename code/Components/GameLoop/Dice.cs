using System;
using EnumExtensions.Util;

/*
 * We are using an Enum for the dice Faces so we can represent other faces more readable in the code.
 */
public enum DiceFace {
	One = 1,
	Two = 2,
	Three = 3,
	Four = 4,
	Five = 5,
	Six = 6,
	Bus = 7,
	Forward = 8
}

public sealed class Dice : Component, Component.ICollisionListener {
	public bool IsRolling { get; private set; }

	[Property] public Rigidbody Rigidbody { get; set; }
	[Property] public bool IsSpecialDice { get; set; }

	[Property] public TurnManager TurnManager { get; private set; }


	[Property,
	 Description(
		 "Represented on what axis (positive) an specific side correlate with. Used as Index to access the face.")]
	public Vector3Int DirectionValues { get; set; }

	[Property, Description("Automatically calculated from the positive dice values. Used as Index to access the face.")]
	public Vector3Int OpposingDirectionValues { get; set; }

	[Property, Description("Define all 6 sides of the dice!")]
	public DiceFace[] Faces { get; set; }


	private readonly TurnManager.Phase[] _rollPhases = { TurnManager.Phase.Rolling, TurnManager.Phase.Jail };

	protected override void OnStart() {
		OpposingDirectionValues = 7 * Vector3Int.One - DirectionValues;
		TurnManager = Game.ActiveScene.GetAllComponents<TurnManager>().First();
	}

	public void OnCollisionStart(Collision collision) {
		if (collision.Other.GameObject.Tags.Contains("dice_wall")) {
			Vector3 direction = new Vector3(0, 0, 0) - collision.Self.Body.Position;
			direction = direction.Normal;

			Rigidbody.Velocity = direction * 100f;
		}
	}

	protected override void OnUpdate() {
		if (Rigidbody.Velocity.IsNearZeroLength) {
			IsRolling = false;
		}
	}


	public void Roll(Player player) {
		// If dice is special AND player is in first round around the board OR the current phase is Jail DON'T throw dice
		if (IsSpecialDice && (player.RoundCount <= 0 || TurnManager.CurrentPhase.Equals(TurnManager.Phase.Jail))) {
			return;
		}

		if (IsRolling || !_rollPhases.Any(phase => phase.Equals(TurnManager.CurrentPhase))) {
			return;
		}

		IsRolling = true;
		Rigidbody.Velocity += Vector3.Up * GetRandomFloat(400, 650);
		Rigidbody.Velocity += Vector3.Left * GetRandomFloat(2, 5);
		Rigidbody.Velocity += Vector3.Backward * GetRandomFloat(2, 5);
		Rigidbody.AngularVelocity +=
			new Vector3(GetRandomFloat() * 1.2f, GetRandomFloat() * 1.5f, GetRandomFloat() * 0.6f);

		// Add force to center
		Vector3 direction = new Vector3(0, 0, 0) - Transform.Position;
		direction = direction.Normal;
		Rigidbody.Velocity += direction * 100f;

		GameSounds.PlaySFX(SfxSounds.Dice, 5);
	}

	private float GetRandomFloat(int min = 4, int max = 7) {
		var rng = new Random(GameObject.GetHashCode());
		return rng.Next(min, max) * (1 + rng.NextSingle());
	}


	public DiceFace GetRoll() {
		// Credits to Shade
		Transform diceTransform = Rigidbody.PhysicsBody.Transform;


		float[] scalarProducts = new float[3];

		// get axis products
		scalarProducts[0] = Vector3.Dot(Vector3.Up, diceTransform.Left);
		scalarProducts[1] = Vector3.Dot(Vector3.Up, diceTransform.Forward);
		scalarProducts[2] = Vector3.Dot(Vector3.Up, diceTransform.Up);

		float maxScalar = 0;
		int maxIndex = 0;


		// biggest axis should be the one at top
		for (var i = 0; i < scalarProducts.Length; i++) {
			if (Math.Abs(maxScalar) < Math.Abs(scalarProducts[i])) {
				maxIndex = i;
				maxScalar = scalarProducts[i];
			}
		}

		int direction;

		// Check which side of axis is on top 
		if (maxIndex == 0) { // Right
			if (maxScalar >= 0) {
				direction = DirectionValues.y;
			}
			else {
				direction = OpposingDirectionValues.y;
			}
		}
		else if (maxIndex == 1) { // Forward
			if (maxScalar >= 0) {
				direction = DirectionValues.x;
			}
			else {
				direction = OpposingDirectionValues.x;
			}
		}
		else { // Up
			if (maxScalar >= 0) {
				direction = DirectionValues.z;
			}
			else {
				direction = OpposingDirectionValues.z;
			}
		}


		return Faces[direction - 1];
	}
}
