using System.Collections.Generic;

namespace Celeste.Mod.DeathMarkers;

public class DeathMarkersSaveData : EverestModuleSaveData {
    public Dictionary<string, List<DeathMarkersSession.Death>> Deaths { get; set; } = [];
}