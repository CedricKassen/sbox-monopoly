using System;

public sealed class Player : Component
{
	private ulong _steamId;

	[Property]
	public ulong SteamId
	{
		get => _steamId;
		set
		{
			if (_steamId == 0)
			{
				_steamId = value;
			}
		}
	}

	private string _name;

	[Property]
	public string Name
	{
		get => _name;
		set
		{
			if (String.IsNullOrEmpty(_name))
			{
				_name = value;
			}
		}
	}

	[Property]
	public int Money { get; set; } = 2000;

	[Property]
	public int CurrentField { get; set; } = 0;
	
	[Property]
	public Connection Connection { get; set; }

	
	protected override void OnUpdate()
	{

	}
}
