using System;
using EnumExtensions.Util;

/*
 * We are using an Enum for the dice Faces so we can represent other faces more readable in the code.
 */
public enum DiceFace {
	Zero,
	One,
	Two,
	Three,
	Four,
	Five,
	Six,
	Seven,
	Eight,
	Nine,
	Bus,
	Forward
}

public sealed class Dice : Component, Component.ICollisionListener {
	public bool IsRolling { get; private set; }

	[Property] public Rigidbody Rigidbody { get; set; }

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

	public void Roll() {
		if (IsRolling || !_rollPhases.Any(phase => phase.Equals(TurnManager.CurrentPhase))) {
			return;
		}

		IsRolling = true;
		Rigidbody.Velocity += Vector3.Up * new Random().Next(400, 700);
		Rigidbody.AngularVelocity +=
			new Vector3(GetRandomFloat() * 1.2f, GetRandomFloat() * 1.2f, GetRandomFloat() * 0.5f);

		// Add force to center
		Vector3 direction = new Vector3(0, 0, 0) - Transform.Position;
		direction = direction.Normal;
		Rigidbody.Velocity += direction * 100f;

		GameSounds.PlaySFX(SfxSounds.Dice, 5);
	}

	private float GetRandomFloat() {
		var rng = new Random();
		return rng.Next(4, 7) * (1 + rng.NextSingle());
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
