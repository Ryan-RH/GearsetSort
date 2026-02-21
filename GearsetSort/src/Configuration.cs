using Dalamud.Configuration;

namespace GearsetSort;

[Serializable]
public class Config : IPluginConfiguration
{
    public int Version { get; set; } = 0;


    public void Save()
    {
        Svc.PluginInterface.SavePluginConfig(this);
    }
}
