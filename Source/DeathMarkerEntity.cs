using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DeathMarkers;

[CustomEntity("DeathMarkers/DeathMarker")]
public class DeathMarkerEntity : Entity {
    public float Delay;
    public float Anim;
    public PlayerDeadBody DeadBody;

    public DeathMarkerEntity(Vector2 position, float delay, PlayerDeadBody deadBody) : base(position) {
        Delay = delay;
        DeadBody = deadBody;
        Tag = Tags.HUD;

        Depth = -9000;
        if (delay == 0f) Depth = -9001; // hacky way of making the first one bigger
    }

    public override void Update() {
        base.Update();
        
        var dt = Engine.DeltaTime;
        if (DeadBody != null && DeadBody.finished) dt *= 1.75f;

        if (Delay > 0f) {
            Delay -= dt;
            return;
        }

        Anim = Calc.Approach(Anim, 1f, dt / 0.35f);
    }

    public override void Render() {
        base.Render();
        
        var dropPos = Position - ((Level) Scene).Camera.Position.Floor();
        if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
            dropPos.X = 320f - dropPos.X;
        dropPos.X *= 6f;
        dropPos.Y *= 6f;

        var pos = Vector2.Lerp(dropPos - new Vector2(0f, 85f), dropPos, Ease.BounceOut(Anim));
        var alpha = Ease.SineOut(Anim);
        
        GFX.Gui["deathmarker"].DrawJustified(pos, new Vector2(0.5f, 1.0f), Color.White * alpha);
    }
}