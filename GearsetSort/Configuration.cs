using System.Collections.Generic;
using System.Text.Json.Serialization;
using ECommons.Configuration;
using System;

namespace GearsetSort;

[Serializable]
public class Config : IEzConfig
{
    public bool InsertMode = false;

    public void Save()
    {
        EzConfig.Save();
    }
}
