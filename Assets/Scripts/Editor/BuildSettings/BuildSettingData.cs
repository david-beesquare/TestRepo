using UnityEngine;
using System.Collections;
using MiniJSONWrapper;
using System.Collections.Generic;

public class BuildSettingData : IJsonSerializer, IJsonDeserializer
{
    /// List of definitions to enable for the build setting
    public List<string> Defines { get; private set; }

    /// Build setting name
    public string Name { get; set; }

    /// Build setting platform
    public BuildSettingsCons.BuildSettingsPlatform Platform { get; set; } 

    /// Create empty build setting
    public BuildSettingData()
    {
        this.Defines = new List<string>();
    }

    /// Create build setting with name
    /// @param name Build setting name
    public BuildSettingData(string name)
    {
        this.Name = name;
        this.Defines = new List<string>();
    }

    /// Deserealize build setting
    public void OnJsonDeserialize(JsonObject jsonObject)
    {
        // build setting params
        Name = jsonObject.GetString("name");
        Platform = (BuildSettingsCons.BuildSettingsPlatform) System.Enum.Parse(typeof(BuildSettingsCons.BuildSettingsPlatform), jsonObject.GetString("platform"));        

        // defines        
        Defines = jsonObject.GetStringList("defineList");
    }

    /// Serialize build setting
    public void OnJsonSerialize(JsonObject jsonObject)
    {
        jsonObject.SetString("name", Name);
        jsonObject.SetString("platform", Platform.ToString());
        jsonObject.SetStringList("defineList", Defines);
    }
}
