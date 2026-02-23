using Dalamud.Configuration;

namespace GearsetSort;

[Serializable]
public class Config : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool Insert { get; set; } = true;

    public void Save()
    {
        PluginInterface.SavePluginConfig(this);
    }
}

// add job icons
// colourise role
// sort out items in preview