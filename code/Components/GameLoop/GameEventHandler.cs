using System;
using Sandbox.Events;
using Sandbox.Events.TurnEvents;

namespace Sandbox.Components.GameLoop;

public class GameEventHandler: Component, IGameEventHandler<RolledEvent> {
    [Property]
    public GameObject LocationContainer { get; set; }
    
    [Property]
    public Lobby Lobby { get; set; }
    
    public void OnGameEvent(RolledEvent eventArgs) {
        Player player = GetPlayerFromEvent(eventArgs);
        Rigidbody rigidbody = player.GameObject.Components.Get<Rigidbody>();

        player.CurrentField = (player.CurrentField + eventArgs.Number) % 40;        
        player.Transform.LerpTo(LocationContainer.Children[player.CurrentField].Transform.World, 1f);
    }

    private Player GetPlayerFromEvent(BaseEvent eventArgs) {
        return Lobby.Players.Find(player => player.SteamId == eventArgs.playerId);
    }
}