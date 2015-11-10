using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class BuildSettingsConfigureWindow : EditorWindow
{
    /// Build settings data
    private BuildSettingsGroup m_buildSettings;
    
    /// Name for the new build setting to create
    private string m_newBuildSettingName;

    /// Scroll position for the defines list
    private Vector2 m_definesScrollPos;

    /// New definiton to add into the current build setting
    private string m_defineToAdd;    

    /// Launch build setting window
    [MenuItem("BuildSettings/Configure..", false, 2)]
    static void LaunchWindow()
    {
        // Get existing open window or if none, make a new one:
        BuildSettingsConfigureWindow window = (BuildSettingsConfigureWindow)EditorWindow.GetWindow(typeof(BuildSettingsConfigureWindow));
        window.minSize = new Vector2(BuildSettingsCons.kLineWidth, BuildSettingsCons.kLineWidth);
        window.maxSize = window.minSize;
        window.title = "Build Settings";

        window.Show();
    }

    /// Get build settings data when the window is enabled
    void OnEnable()
    {
        if (m_buildSettings == null)
        {
            m_buildSettings = new BuildSettingsGroup();
            m_buildSettings.LoadFromFile();

            m_newBuildSettingName = "";
            m_defineToAdd = "";        
        }        
    }

    /// Draw window UI
    void OnGUI()
    {
        // styles
        GUIStyle tableBgStyle = new GUIStyle(GUI.skin.box);

        GUILayout.Space(BuildSettingsCons.kVerticalMargin * 2);

        // add new build setting
        GUILayout.BeginHorizontal(GUILayout.Height(BuildSettingsCons.kLineHeight));
        {
            GUILayout.Space(BuildSettingsCons.kHorizontalMargin);
            GUILayout.Label("New Build Setting Name:");
            m_newBuildSettingName = GUILayout.TextField(m_newBuildSettingName, GUILayout.MinWidth(200), GUILayout.Height(BuildSettingsCons.kLineHeight));
            if (GUILayout.Button("Add", GUILayout.Height(BuildSettingsCons.kLineHeight)))
            {
                m_buildSettings.AddNewBuildSetting(m_newBuildSettingName);                
                m_newBuildSettingName = "";
                m_buildSettings.SaveToFile();
            }
            GUILayout.Space(BuildSettingsCons.kHorizontalMargin);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(BuildSettingsCons.kVerticalMargin);

        // list of build settings
        if (m_buildSettings.CurrentBuildSettingIndex >= 0)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(BuildSettingsCons.kLineHeight));
            {
                GUILayout.Space(BuildSettingsCons.kHorizontalMargin);
                m_buildSettings.ShowIntPopup();
                
                if (GUILayout.Button("X", GUILayout.Width(BuildSettingsCons.kRemoveButtonWidth), GUILayout.Height(BuildSettingsCons.kLineHeight)))
                {
                    // remove existing setting
                    m_buildSettings.RemoveSelectedBuildSetting();
                    m_buildSettings.SaveToFile();                 
                }
                GUILayout.Space(BuildSettingsCons.kHorizontalMargin);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(BuildSettingsCons.kVerticalMargin);

            GUILayout.BeginHorizontal(GUILayout.Height(BuildSettingsCons.kLineHeight));
            {
                GUILayout.Space(BuildSettingsCons.kHorizontalMargin);

                // build setting platform
                BuildSettingsCons.BuildSettingsPlatform newPlatform = (BuildSettingsCons.BuildSettingsPlatform)EditorGUILayout.EnumPopup("Platform: ", m_buildSettings.CurrentBuildSettingData.Platform, GUILayout.MaxWidth(300));
                if(newPlatform != m_buildSettings.CurrentBuildSettingData.Platform)
                {
                    m_buildSettings.CurrentBuildSettingData.Platform = newPlatform;
                    m_buildSettings.SaveToFile();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(BuildSettingsCons.kVerticalMargin);

            // list of defines to enable for each setting            
            float tableHeight = (BuildSettingsCons.kLineHeight * BuildSettingsCons.kDefinesMaxLines) + (BuildSettingsCons.kVerticalMargin * (BuildSettingsCons.kDefinesMaxLines - 1));
            m_definesScrollPos = EditorGUILayout.BeginScrollView(m_definesScrollPos, tableBgStyle, GUILayout.Height(tableHeight));
            {
                GUILayout.Space(BuildSettingsCons.kVerticalMargin);
                
                // new define to add
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(BuildSettingsCons.kHorizontalMargin);
                    m_defineToAdd = GUILayout.TextField(m_defineToAdd, GUILayout.Height(BuildSettingsCons.kTableLineHeight));
                    if (GUILayout.Button("Add", GUILayout.Width(BuildSettingsCons.kLineWidth * 0.25f), GUILayout.Height(BuildSettingsCons.kLineHeight)))
                    {
                        if (m_defineToAdd != "")
                        {
                            m_buildSettings.CurrentBuildSettingData.Defines.Add(m_defineToAdd);
                            m_defineToAdd = "";
                            m_buildSettings.SaveToFile();
                        }
                    }
                    GUILayout.Space(BuildSettingsCons.kHorizontalMargin);
                }
                GUILayout.EndHorizontal();
                
                GUILayout.Space(BuildSettingsCons.kVerticalMargin);

                // already existing defines
                if (m_buildSettings.CurrentBuildSettingData != null)
                {
                    int iDefine = 0;
                    List<string> defines = null;
                    foreach (string define in m_buildSettings.CurrentBuildSettingData.Defines)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(BuildSettingsCons.kHorizontalMargin);
                            string newDefine = GUILayout.TextArea(define, GUILayout.Height(BuildSettingsCons.kTableLineHeight));
                            if (newDefine != define)
                            {
                                // update existing define
                                if (defines == null)
                                {
                                    defines = new List<string>(m_buildSettings.CurrentBuildSettingData.Defines);
                                }
                                defines[iDefine] = newDefine;                                
                            }
                            if (GUILayout.Button("X", GUILayout.Width(BuildSettingsCons.kRemoveButtonWidth), GUILayout.Height(BuildSettingsCons.kLineHeight)))
                            {
                                // remove existing define
                                if (defines == null)
                                {
                                    defines = new List<string>(m_buildSettings.CurrentBuildSettingData.Defines);
                                }
                                defines.RemoveAt(iDefine);                                
                            }
                            GUILayout.Space(BuildSettingsCons.kHorizontalMargin);
                        }
                        GUILayout.EndHorizontal();

                        ++iDefine;
                        GUILayout.Space(BuildSettingsCons.kVerticalMargin);
                    }

                    // reallocate defines array if it has changed
                    if (defines != null)
                    {
                        m_buildSettings.CurrentBuildSettingData.Defines.Clear();
                        m_buildSettings.CurrentBuildSettingData.Defines.AddRange(defines);
                        m_buildSettings.SaveToFile();
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }
    }       
}
