using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace GearsetSort.Models;
public record Gearset
(
    int id,
    string name,
    byte classJob,
    IReadOnlyList<SetItem> items,
    int itemLevel
);

public record SetItem
(
    uint iconId,
    string name,
    byte majorCategory,
    IReadOnlyList<SetMateria> materia,
    RaptureGearsetModule.GearsetItem content
);

public record SetMateria
(
    ushort iconId,
    string name
);