using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;

public class JenkinBuildIOS  {
	public static string BundleId = "jp.co.gnt.toone";
	public static string AppName = "Toone";
	public static string AppVersion = "0.0.1";	
	public static string AppBuildPath = "JenKinsBuild/ios";
	public static string PrivateParam = "-define:LOGGER;LOGGER_FILE;LOGGER_SYNC_FILE;LOGGER_DOWNLOAD_FILE;";
	public static string BUILDVERSION = "0.0.1";
    private static string timePull = "00:00";
    private static string buildNumber = "0000";
	public static BuildOptions AppBuildOptions = BuildOptions.None;

	public static void init(){
		AssetDatabase.Refresh ();
		setParamFromRemoteJenkin ();
		PlayerSettings.bundleVersion = AppVersion;
		PlayerSettings.iOS.buildNumber = BUILDVERSION;
		PlayerSettings.applicationIdentifier = BundleId;
		PlayerSettings.productName = AppName;
        //PlayerSettings.iOS.appleEnableAutomaticSigning = false;
        PlayerSettings.SplashScreen.showUnityLogo = false;
    }
	

    private static void WriteVersionToResouce(string text)
    {
        string temporaryTextFileName = "build_version";
        File.WriteAllText(Application.dataPath + "/Resources/" + temporaryTextFileName + ".txt", text);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

	private static void setParamFromRemoteJenkin(){
		BundleId = CommandLineReader.GetCustomArgument ("BUNDLE_ID").ToLower();
		AppName = CommandLineReader.GetCustomArgument ("APPNAME");
		AppVersion = CommandLineReader.GetCustomArgument ("APPVERSION");
		PrivateParam = CommandLineReader.GetCustomArgument ("PRIVATEPARAM");
		BUILDVERSION = CommandLineReader.GetCustomArgument ("BUILDVERSION");
        timePull = CommandLineReader.GetCustomArgument("TIME_PULL");
        buildNumber = CommandLineReader.GetCustomArgument("BUILD_NUMBER");

        //WriteVersionToResouce(buildNumber + "#" + timePull);

		string buildOption = CommandLineReader.GetCustomArgument ("BUILDOPTIONS");
		//string ENABLE_CODE_SIGN_ENTITLEMENTS = CommandLineReader.GetCustomArgument ("CODE_SIGN_ENTITLEMENTS");
		//if (!string.IsNullOrEmpty (ENABLE_CODE_SIGN_ENTITLEMENTS)) {
		//	FrameworkPostProcessor.ENABLE_CODE_SIGN_ENTITLEMENTS = true;
		//}

		if (!string.IsNullOrEmpty (buildOption)) {

			string[] options = buildOption.Split('|');
			if(options != null && options.Length > 0)
			{
				AppBuildOptions = BuildOptions.Development;
				foreach(string opt in options)
				{
					AppBuildOptions |= (BuildOptions)(Enum.Parse (typeof(BuildOptions), opt, true));
				}
			}
		}

		CleanResources();
	}
	
	static void IOSBuild (){
		init ();
		try {
			bool buildOk = BuildPlayer();
			
			if (!buildOk) {
				Debug.LogError("Build player failed");
				return;
			}
			
		} catch (Exception ex) {
			Debug.LogWarning(ex.Message);
		}
	}
	
	static bool BuildPlayer()	{
		EditorUserBuildSettings.SwitchActiveBuildTarget (BuildTarget.iOS);
		string fullpath = System.IO.Path.GetFullPath(AppBuildPath);
		if (Directory.Exists (fullpath)) {
			Directory.Delete (fullpath, true);
		}
		Directory.CreateDirectory(fullpath);
		List<string> scenePaths = new List<string>();		
		foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes) {
			if (e.enabled)
			scenePaths.Add(e.path);

		}
		var errorStr = BuildPipeline.BuildPlayer(scenePaths.ToArray(), fullpath, BuildTarget.iOS, AppBuildOptions);
		Debug.LogError(errorStr);
		//FrameworkPostProcessor.OnPostProcessBuild (BuildTarget.iOS, fullpath);
		return (errorStr == null);
	}

	public static void CleanResources () {
		
	}

	[MenuItem ("Window/IOSBuildVersion")]
	static void IOSBuildVersion (){
		PlayerSettings.iOS.buildNumber = BUILDVERSION;
	}
}
