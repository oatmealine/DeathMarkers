using System;
using System.Collections.Generic;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DeathMarkers;

public class DeathMarkersModule : EverestModule {
    public static DeathMarkersModule Instance { get; private set; }

    public override Type SettingsType => typeof(DeathMarkersSettings);
    public static DeathMarkersSettings Settings => (DeathMarkersSettings) Instance._Settings;
    
    public override Type SessionType => typeof(DeathMarkersSession);
    public static DeathMarkersSession Session => (DeathMarkersSession) Instance._Session;
    
    public override Type SaveDataType => typeof(DeathMarkersSaveData);
    public static DeathMarkersSaveData SaveData => (DeathMarkersSaveData) Instance._SaveData;

    public DeathMarkersModule() {
        Instance = this;
    }

    public override void Load() {
        On.Celeste.Player.Die += PlayerOnDie;
    }

    public override void Unload() {
        On.Celeste.Player.Die -= PlayerOnDie;
    }

    public static List<DeathMarkersSession.Death> GetDeaths(string level) {
        switch (Settings.Mode) {
            case DeathMarkersSettings.SaveMode.SaveData: {
                if (!SaveData.Deaths.ContainsKey(level)) SaveData.Deaths[level] = [];
                return SaveData.Deaths[level];
            }
            case DeathMarkersSettings.SaveMode.Session: {
                return Session.Deaths;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static void AppendDeath(Vector2 deathMarkerPos, string roomName, string levelName) {
        var deaths = GetDeaths(levelName);
        if (Settings.ClumpDeaths) {
            foreach (var death in deaths) {
                if (death.Room != roomName) continue;
                if (Vector2.DistanceSquared(deathMarkerPos, death.Position) < 100f) {
                    death.Position = Vector2.Lerp(death.Position, deathMarkerPos, 1f / (death.Amount + 1));
                    death.Amount++;
                    // hacky way to move it to the end of the list
                    deaths.Remove(death);
                    deaths.Add(death);
                    return;
                }
            }
        }
        
        var newDeath = new DeathMarkersSession.Death(roomName, deathMarkerPos);
        deaths.Add(newDeath);
    }

    private static PlayerDeadBody PlayerOnDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats) {
        var result = orig(self, direction, evenIfInvincible, registerDeathInStats);
        if (!Settings.Enabled) return result;
        if (result == null) return null;
        
        var quickDeath = result.bounce == Vector2.Zero;

        var level = self.SceneAs<Level>();
        var roomName = level.Session.Level;
        var levelName = level.Session.Area.SID;
        var aabb = level.Bounds;
       
        var deathPos = self.Position.Clamp(aabb.Left, aabb.Top, aabb.Right, aabb.Bottom);
        
        var deathMarkerPos = deathPos - level.LevelOffset;
        AppendDeath(deathMarkerPos, roomName, levelName);
        
        var deaths = GetDeaths(levelName);
        
        // summon entities
        var i = 0;
        foreach (var death in deaths) {
            i++;
            if (death.Room != roomName) continue;
            var delay = Calc.Random.NextFloat(0.25f) + (quickDeath ? 0.25f : 0.4f);
            if (i > deaths.Count - 1) delay = 0f;
            var marker = new DeathMarkerEntity(death.Position + level.LevelOffset, delay, result);
            self.Scene.Add(marker);
        }

        return result;
    }

    public override void CreateModMenuSection(TextMenu menu, bool inGame, EventInstance snapshot) {
        base.CreateModMenuSection(menu, inGame, snapshot);
        menu.Add(new TextMenu.Button(Dialog.Clean("modoptions_deathmarkers_cleardeaths")).Pressed(() => {
            SaveData.Deaths.Clear();
            Session.Deaths.Clear();
            Audio.Play("event:/ui/main/savefile_delete");
        }));
    }
}