using System.Threading.Tasks;

public enum Colors {
	Brown,
	LightBlue,
	Pink,
	Orange,
	Red,
	Yellow,
	Green,
	Blue,
	Black,
	Gray
}

namespace EnumExtensions {
	public static class ColorExtensions {
		public static string ToClass(this Colors color) {
			var str = color.ToString();
			return char.ToLower(str[0]) + str[1..];
		}
	}
}


public sealed class GameLocation : Component {
	public enum PropertyType {
		Normal,
		Event,
		Utility,
		Railroad
	}

	[Property] public PropertyType Type { get; set; } = PropertyType.Normal;

	[Property]
	[HideIf("Type", PropertyType.Event), ShowIf("EventId", "income_tax"), ShowIf("EventId", "luxury_tax")]
	public string Name { get; set; }

	[Property]
	[HideIf("Type", PropertyType.Event)]
	public Colors Color { get; set; }

	[Property]
	[HideIf("Type", PropertyType.Event)]
	public int Price { get; set; } = 0;

	[Property]
	[ShowIf("Type", PropertyType.Normal)]
	[Description("Array of the indices of all members of the group to which this location belongs")]
	public int[] GroupMembers { get; set; }

	[Property]
	[ShowIf("Type", PropertyType.Normal)]
	public int[] Normal_Rent { get; set; } = { 0, 1, 2, 3, 4, 5 };

	[Property]
	[ShowIf("Type", PropertyType.Normal)]
	public int House_Cost { get; set; } = 0;

	[Property]
	[ShowIf("Type", PropertyType.Railroad)]
	public int[] Railroad_Rent { get; set; } = { 25, 50, 100, 200 };

	[Property]
	[ShowIf("Type", PropertyType.Utility)]
	public int[] Utility_Rent_Multiplier { get; set; } = { 4, 10 };

	[Property]
	[ShowIf("Type", PropertyType.Event)]
	public string EventId { get; set; }

	[Property]
	[ShowIf("Type", PropertyType.Normal)]
	public int Houses { get; set; } = 0;

	[Property]
	[HideIf("Type", PropertyType.Event)]
	public bool Mortgaged { get; set; } = false;

	[Property] public int PropertyIndex { get; set; }

	protected override Task OnLoad() {
		PropertyIndex = GameObject.Parent.Children.FindIndex(o => o.Id == GameObject.Id);
		return base.OnLoad();
	}

	protected override void OnUpdate() { }
}
