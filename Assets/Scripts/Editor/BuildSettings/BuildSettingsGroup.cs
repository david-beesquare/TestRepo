using UnityEngine;
using System.Collections;
using MiniJSONWrapper;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class BuildSettingsGroup : IJsonSerializer, IJsonDeserializer
{
    /// Save file path
    private string BuildSettingsFilePath
    {
        get
        {
            return Path.Combine(Path.Combine(Application.dataPath, "Editor" + Path.DirectorySeparatorChar), "BuildSettings.json");
        }
    }

    /// Build settings list
    private List<BuildSettingData> m_buildSettings;
    public List<BuildSettingData> BuildSettings
    {
        get
        {
            return m_buildSettings;
        }
    }

    /// Current selected build setting
    private int m_currentBuildSettingIndex;
    public int CurrentBuildSettingIndex
    {
        get
        {
            return m_currentBuildSettingIndex;
        }
        set
        {
            if ((value >= 0) && (value < m_buildSettings.Count))
            {
                m_currentBuildSettingIndex = value;
            }
        }
    }

    public string CurrentBuildSettingName
    {
        get
        {
            if(m_currentBuildSettingIndex >= 0)
            {
                return m_buildSettings[m_currentBuildSettingIndex].Name;
            }
            else
            {
                return "NULL";
            }
        }
    }

    /// Arrays for the build settings drop list
    private string[] m_buildSettingsNames;
    private int[] m_buildSettingsIndexes;

    /// The current build setting selected data
    public BuildSettingData CurrentBuildSettingData
    {
        get
        {
            if ((m_currentBuildSettingIndex >= 0) && (m_currentBuildSettingIndex < m_buildSettings.Count))
            {
                return m_buildSettings[m_currentBuildSettingIndex];
            }
            else
            {
                return null;
            }
        }
    }

    /// Constructors, builds and empty list of build settings
    public BuildSettingsGroup()
    {
        m_buildSettings = new List<BuildSettingData>();
        m_currentBuildSettingIndex = -1;
    }

    /// Rebuilds the list of names and indexes for the current build settings created
    /// This method should be called every time a new build setting is added or removed
    /// @return The total number of build settings
    private int RefreshBuildSettingsLists()
    {
        int index = 0;
        m_buildSettingsNames = new string[m_buildSettings.Count];
        m_buildSettingsIndexes = new int[m_buildSettings.Count];
        foreach (BuildSettingData bsData in m_buildSettings)
        {
            m_buildSettingsNames[index] = bsData.Name;
            m_buildSettingsIndexes[index] = index;
            ++index;
        }

        return index;
    }

    /// Build Editor UI Pop up element to show the different build settings available
    /// @return True if the build setting selected has changed
    public bool ShowIntPopup()
    {
        int selectedBuildSetting = EditorGUILayout.IntPopup("Build Settings:", m_currentBuildSettingIndex, m_buildSettingsNames, m_buildSettingsIndexes, GUILayout.Height(BuildSettingsCons.kLineHeight));
        if (selectedBuildSetting != m_currentBuildSettingIndex)
        {
            m_currentBuildSettingIndex = selectedBuildSetting;
            return true;
        }

        return false;
    }

    /// Add a new build setting in the list
    /// @param name Build setting name
    public void AddNewBuildSetting(string name)
    {
        BuildSettingData bs = new BuildSettingData(name);
        m_buildSettings.Add(bs);

        int index = RefreshBuildSettingsLists();
        m_currentBuildSettingIndex = index - 1;
    }

    /// Remove the current selected build setting from the list
    public void RemoveSelectedBuildSetting()
    {
        m_buildSettings.RemoveAt(m_currentBuildSettingIndex);
        RefreshBuildSettingsLists();
        m_currentBuildSettingIndex--;
    }

    /// Load from the saved build settings file the current list of build settings
    public void LoadFromFile()
    {
        if (File.Exists(BuildSettingsFilePath))
        {
            StreamReader reader = new StreamReader(BuildSettingsFilePath);
            string jsonString = reader.ReadToEnd();
            reader.Close();

            // fill object with data
            JsonObject.FillObject(jsonString, this);
        }
    }

    /// Save the current list of build settings to a file
    public void SaveToFile()
    {
        JsonObject jsonRoot = JsonObject.GetSerializedObject(this);
        string jsonString = jsonRoot.ToString();
        StreamWriter writer = new StreamWriter(BuildSettingsFilePath);
        writer.WriteLine(jsonString);
        writer.Close();
    }

    /// Set the specified build setting as the current
    /// @param buildSettingName Build setting name
    private void SetCurrentBuildSetting(string buildSettingName)
    {
        int index = 0;
        foreach (string name in m_buildSettingsNames)
        {
            if (name.Equals(buildSettingName))
            {
                break;
            }
            ++index;
        }

        if (index < m_buildSettingsNames.Length)
        {
            m_currentBuildSettingIndex = index;
        }
    }

    /// Apply the current build settings, adding their defines to the specified player build target
    /// @param buildTarget Unity Player build target (iOS / Android)
    public void ApplyCurrentBuildSettings(BuildTargetGroup buildTarget)
    {
        if (this.CurrentBuildSettingData != null)
        {
            string defines = "";
            foreach (string define in this.CurrentBuildSettingData.Defines)
            {
                defines += define + ";";
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, defines);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log("Apply Build Settings: " + defines);
        }
    }

    /// Apply the specified build setting with the specified build target
    /// @param buildSettingsName Build setting name
    /// @param buildTarget Unity Player build target (iOS / Android)
    public void ApplyBuildSettings(string buildSettingsName, BuildTargetGroup buildTarget)
    {
        SetCurrentBuildSetting(buildSettingsName);
        ApplyCurrentBuildSettings(buildTarget);
    }

    /// Deserialize build settings
    public void OnJsonDeserialize(JsonObject jsonObject)
    {
        // build setting params
        m_currentBuildSettingIndex = jsonObject.GetInt("currentBuildSetting");

        // build settings
        m_buildSettings = jsonObject.GetObjectList<BuildSettingData>("settingsList");

        // refresh drop down list
        if (m_buildSettings.Count > 0)
        {
            RefreshBuildSettingsLists();
        }
    }

    /// Serialize build settings
    public void OnJsonSerialize(JsonObject jsonObject)
    {
        jsonObject.SetInt("currentBuildSetting", m_currentBuildSettingIndex);
        jsonObject.SetObjectList<BuildSettingData>("settingsList", m_buildSettings);
    }
}
