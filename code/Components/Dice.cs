using System.Threading.Tasks;
using Sandbox;
using Sandbox.Utility;

public sealed class Dice : Component {
	private bool _rolling = false;
	private float _timer = 0;
	
	[Property]
	public float Time { get; set; } = 0;
	
	[Property]
	public MotionPath MotionPath { get; set; }
	
	[Property]
	public bool Rolling {
		get {
			return _rolling;
		}
		set {
			_rolling = value;
			_timer = 0;
			Time = 0;
			MotionPath.Time = 0;
		}
	}

	protected override void OnStart() {
		MotionPath.Time = 0;
	}

	protected override void OnUpdate() {
		_timer += 1 * RealTime.Delta;
		Time = Easing.QuadraticIn(_timer % 1 / 1);
		
		if (Time > MotionPath.Time && Rolling) {
			MotionPath.Time = Time;
		}
	}
}
