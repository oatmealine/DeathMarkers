using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.DeathMarkers;

public class DeathMarkersSession : EverestModuleSession {
    public record Death(string Room, Vector2 Position, int Amount = 1) {
        public Vector2 Position { get; set; } = Position;
        public int Amount { get; set; } = Amount;
    };
    
    public List<Death> Deaths { get; set; } = [];
}