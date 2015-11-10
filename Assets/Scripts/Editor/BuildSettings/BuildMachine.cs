using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

/// Methods for building the unity project from the command line
/// Output:
///     Android: apk file in <project_folder>/Builds/android.apk
///     iOS: XCode project in <project_folder>/Builds/xcode
public class BuildMachine
{
	/// Custom command line parameter to retrieve that will specify the build setting to apply before the build is made
    private const string kCommandLineParameterBuildSettingsName = "buildSettings";
    private const string kCommandLineParameterPlatform = "platform";
    private const string kCommandLineParameterVersion = "version";

    /// Platform parameter names
    private const string kParamPlatformIOS = "iOS";
    private const string kParamPlatformAndroid = "Android";

    /// Null Version (then don't increase it)
    private const string kParamNullVersion = "NULL";
	
    /// Project folder to store the output builds
    private const string kTargetBuildFolderPath = "Builds/";

    /// Android generated apk file name
	private const string kAndroidTargetPath = "android.apk";

    /// iOS folder name to store the generated XCode project
	private const string kIOSTargetPath = "xcode";

    /// Generate the output build full path
	private static string BuildPath
	{
		get
		{
			return Application.dataPath + "/../" + kTargetBuildFolderPath;
		}
	}    

    /// Step 1: Apply build settings
    public static void PrepareBuild ()
    {
        // increase build version
        string version = GetCommandLineParameter(kCommandLineParameterVersion);
        if(!version.Equals(kParamNullVersion))
        {
            PlayerSettings.bundleVersion = version;
            PlayerSettings.iOS.buildNumber = version;
            PlayerSettings.Android.bundleVersionCode = GetVersionInteger(version);
        }

        // apply build settings
        //string platformName = GetCommandLineParameter(kCommandLineParameterPlatform);
        //if (platformName.Equals(kParamPlatformAndroid))
        //{
        //    ApplyBuildSettingsFromCommandLine(BuildTargetGroup.Android);
        //}
        //else // if(platformName.Equals(kParamPlatformIOS))
        //{
        //    ApplyBuildSettingsFromCommandLine(BuildTargetGroup.iOS);
        //}
    }
    
    /// Step 2: Perform the build
    public static void Build ()
    {
        string platformName = GetCommandLineParameter(kCommandLineParameterPlatform);
        if (platformName.Equals(kParamPlatformAndroid))
        {
            PlayerSettings.Android.keystorePass = "android";
            PlayerSettings.Android.keyaliasPass = "android";
            BuildPipeline.BuildPlayer(GetScenesList(), BuildPath + kAndroidTargetPath, BuildTarget.Android, BuildOptions.None);
        }
        else // if(platformName.Equals(kParamPlatformIOS))
        {
            BuildPipeline.BuildPlayer(GetScenesList(), BuildPath + kIOSTargetPath, BuildTarget.iOS, BuildOptions.None);
        }
    }    

    private static int GetVersionInteger (string versionString)
    {
        int dotIndex = versionString.IndexOf('.');
        while(dotIndex >= 0)
        {
            versionString = versionString.Remove(dotIndex, 1);
            dotIndex = versionString.IndexOf('.');
        }
        return System.Int32.Parse(versionString);
    }

    /// Get the build settings name to apply from the command line input and apply it
    private static void ApplyBuildSettingsFromCommandLine (BuildTargetGroup buildTarget)
	{
		string buildSettingsName = GetCommandLineParameter(kCommandLineParameterBuildSettingsName);
		BuildSettingsGroup buildSettings = new BuildSettingsGroup();
		buildSettings.LoadFromFile();
		buildSettings.ApplyBuildSettings(buildSettingsName, buildTarget);
	}

    /// Get the specified parameter value from the command line input
    /// @param parameterName Name of the parameter with the format -parameterName
	private static string GetCommandLineParameter (string parameterName)
	{
		string[] args = System.Environment.GetCommandLineArgs();
		int iParam = 0;
		string targetParam = "-" + parameterName;
		foreach (string argument in args)
		{
			iParam++;
			if(argument.Equals(targetParam))
			{
				return args[iParam];
			}
		}

		return "";
	}

    /// Get the list of scene names that are added to the build list
	private static string[] GetScenesList ()
	{
		List<string> enabledScenes = new List<string>();
		foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes) 
		{
			if(scene.enabled)
			{
				enabledScenes.Add(scene.path);
			}
		}

		return enabledScenes.ToArray();
	}

}
