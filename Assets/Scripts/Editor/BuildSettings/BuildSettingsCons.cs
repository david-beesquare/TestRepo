using UnityEngine;
using System.Collections;

public static class BuildSettingsCons
{
    /// Constants for the Build Setting Unity UI

    public const float kLineHeight = 15.0f;        
    public const float kTableLineHeight = 18.0f;        
    public const float kLineWidth = 500.0f;
    public const int kDefinesMaxLines = 15;
    public const float kRemoveButtonWidth = 25.0f;
    public const float kVerticalMargin = 10.0f;
    public const float kHorizontalMargin = 20.0f;

    public enum BuildSettingsPlatform
    {
        iOS = 0,
        Android
    }
}
