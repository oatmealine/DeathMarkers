namespace Celeste.Mod.DeathMarkers;

public class DeathMarkersSettings : EverestModuleSettings {
    [SettingSubText("modoptions_deathmarkers_enabled_desc")]
    public bool Enabled { get; set; } = true;
    
    public enum SaveMode {
        Session,
        SaveData
    }

    private SaveMode _mode = SaveMode.Session;
    [SettingSubText("modoptions_deathmarkers_mode_desc")]
    public SaveMode Mode {
        get => _mode;
        set {
            var scene = Celeste.Instance.scene;
            if (scene.GetType() == typeof(Level)) {
                var level = (Level) scene;
                var sid = level.Session.Area.SID;

                var saveData = DeathMarkersModule.SaveData;
                var session = DeathMarkersModule.Session;
                
                // transfer data between savedata and session
                if (_mode == SaveMode.Session && value == SaveMode.SaveData) {
                    saveData.Deaths[sid].Clear();
                    foreach (var death in session.Deaths) {
                        saveData.Deaths[sid].Add(death);
                    }
                    session.Deaths.Clear();
                }
                if (_mode == SaveMode.SaveData && value == SaveMode.Session) {
                    session.Deaths.Clear();
                    foreach (var death in saveData.Deaths[sid]) {
                        session.Deaths.Add(death);
                    }
                    saveData.Deaths[sid].Clear();
                }       
            }
            
            _mode = value;
        }
    }

    public bool ClumpDeaths { get; set; } = true;
}