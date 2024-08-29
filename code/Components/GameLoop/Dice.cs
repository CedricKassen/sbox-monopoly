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

		GameSounds.PlaySFX(SfxSounds.Dice, 5);
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
