using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class BuildSettingsSelectWindow : EditorWindow 
{
    /// Build settings data
    private BuildSettingsGroup m_buildSettings;

    /// Launch select build settings window
    [MenuItem("BuildSettings/Select..", false, 1)]
    static void LaunchWindow ()
    {
        // Get existing open window or if none, make a new one:
        BuildSettingsSelectWindow window = (BuildSettingsSelectWindow)EditorWindow.GetWindow(typeof(BuildSettingsSelectWindow));        
        window.minSize = new Vector2(BuildSettingsCons.kLineWidth, BuildSettingsCons.kLineHeight * 4);
        window.maxSize = window.minSize;
        window.title = "Build Settings";
        window.Show();        
    }

    /// Get build settings data when the window is enabled
    void OnEnable ()
    {
        if (m_buildSettings == null)
        {
            m_buildSettings = new BuildSettingsGroup();
            m_buildSettings.LoadFromFile();
        }
    }

    /// Save build settings data when the window is disabled (closed, out of focus)
    void OnDisable ()
    {        
        ApplyBuildSettings();
        m_buildSettings.SaveToFile();
    }

    /// Draw window UI
    void OnGUI()
    {
        GUILayout.Space(BuildSettingsCons.kVerticalMargin * 2);

        if (m_buildSettings.CurrentBuildSettingIndex >= 0)
        {            
            GUILayout.BeginHorizontal(GUILayout.Height(BuildSettingsCons.kLineHeight));
            {
                GUILayout.Space(BuildSettingsCons.kHorizontalMargin);
                m_buildSettings.ShowIntPopup();
                if (GUILayout.Button("OK", GUILayout.Width(30.0f), GUILayout.Height(BuildSettingsCons.kLineHeight)))
                {
                    Close();
                }
                GUILayout.Space(BuildSettingsCons.kHorizontalMargin);
            }
            GUILayout.EndHorizontal();            
        }
        else
        {
            GUILayout.BeginHorizontal(GUILayout.Height(BuildSettingsCons.kLineHeight));
            {
                GUILayout.Space(BuildSettingsCons.kHorizontalMargin);
                GUILayout.Label("There is no build setting created. Go to BuildSettings/Configure..");
                GUILayout.Space(BuildSettingsCons.kHorizontalMargin);
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(BuildSettingsCons.kVerticalMargin);            
    }

    /// Apply the selected build setting with the current build target selected in Unity
    private void ApplyBuildSettings ()
    {
        m_buildSettings.ApplyCurrentBuildSettings(EditorUserBuildSettings.selectedBuildTargetGroup);
    }        
}
