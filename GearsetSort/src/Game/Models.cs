using System.Collections.Generic;
using Dalamud.Interface.Textures;

namespace GearsetSort.Models;

public record Gearset
(
    int id,
    string name,
    SetClassJob classJob,
    IReadOnlyList<SetItem> items,
    int itemLevel
);

public record SetItem
(
    ISharedImmediateTexture texture,
    string name,
    byte majorCategory,
    IReadOnlyList<SetMateria> materia
);

public record SetClassJob
(
    ushort id,
    ISharedImmediateTexture icon
);

public record SetMateria
(
    ISharedImmediateTexture texture,
    string name
);