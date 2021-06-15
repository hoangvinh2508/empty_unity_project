using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class JenkinBuildWebGL
{
 
    public static string AppName = "Ludo";
    public static string AppVersion = "0.0.1";	
    public static string AppBuildPath;
    public static string BUILDVERSION = "0.0.1";
    private static string buildNumber = "0000";
    public static string PrivateParam = "-define:LOGGER;LOGGER_FILE;LOGGER_SYNC_FILE;LOGGER_DOWNLOAD_FILE;";
    private const string RspFile = "smcs.rsp";
    private static string timePull = "00:00";
    public static BuildOptions AppBuildOptions = BuildOptions.None;
    
    public static void init(){
        AssetDatabase.Refresh ();
        setParamFromRemoteJenkin ();
        PlayerSettings.bundleVersion = AppVersion;
        PlayerSettings.iOS.buildNumber = BUILDVERSION;
        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.productName = AppName;
    }
    
    private static void WriteVersionToResouce(string text)
    {
        string temporaryTextFileName = "build_version";
        File.WriteAllText(Application.dataPath + "/Resources/" + temporaryTextFileName + ".txt", text);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    private static void setParamFromRemoteJenkin(){
        AppName = CommandLineReader.GetCustomArgument ("APPNAME");
        AppVersion = CommandLineReader.GetCustomArgument ("APPVERSION");
        PrivateParam = CommandLineReader.GetCustomArgument ("PRIVATEPARAM");
        BUILDVERSION = CommandLineReader.GetCustomArgument ("BUILDVERSION");
        timePull = CommandLineReader.GetCustomArgument("TIME_PULL");
        buildNumber = CommandLineReader.GetCustomArgument("BUILD_NUMBER");

        WriteVersionToResouce(buildNumber + "#" + timePull);
        string buildOption = CommandLineReader.GetCustomArgument ("BUILDOPTIONS");

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
        SavePrivateParam ();
    }
    
    public static void CleanResources () {
        
    }

    //[MenuItem( "Tools/SavePrivateParam" )]
    public static void SavePrivateParam(){
        var smcsFile = Path.Combine(Application.dataPath, RspFile);
        Debug.Log ("Did write gmcs " + PrivateParam);
        File.WriteAllText (smcsFile, PrivateParam);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if (PrivateParam.Contains("TESTBACKUP"))
        {
            AppBuildPath = "JenKinsBuild/Ludo-test-backup/Ludo";
        } 
        else if (PrivateParam.Contains("TEST"))
        {
            AppBuildPath = "JenKinsBuild/Ludo-test/Ludo";
        }
        else if(PrivateParam.Contains("PREVIEW"))
        {
            AppBuildPath = "JenKinsBuild/Ludo-preview/Ludo";
        }
        else if(PrivateParam.Contains("PREREGISTER"))
        {
            AppBuildPath = "JenKinsBuild/Ludo-pre/Ludo";
        }
        else if(PrivateParam.Contains("PRODUCT"))
        {
            AppBuildPath = "JenKinsBuild/Ludo-product-bk/Ludo";
        }
        else
        {
            AppBuildPath = "JenKinsBuild/Ludo-dev/Ludo";
        }
    }
    
    public static void WebGLBuild (){
        init ();
        try {
            bool buildOk = BuildPlayer();
			
            if (!buildOk) {
                Debug.LogError("Build player failed");
                return;
            }
            Logger.Log("Build player Success !!!");
        } catch (Exception ex) {
            Debug.LogWarning(ex.Message);
        }
    }
    static bool BuildPlayer()	{
        EditorUserBuildSettings.SwitchActiveBuildTarget (BuildTarget.WebGL);
        string fullpath = System.IO.Path.GetFullPath(AppBuildPath);
        if (Directory.Exists (fullpath)) {
            Logger.Log("Unity Build ==> Delete current output older file webgl :" + fullpath);
            Directory.Delete (fullpath, true);
        }
        Directory.CreateDirectory(fullpath);
        List<string> scenePaths = new List<string>();		
        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes) {
            if (e.enabled)
                scenePaths.Add(e.path);

        }
        var errorStr = BuildPipeline.BuildPlayer(scenePaths.ToArray(), fullpath, BuildTarget.WebGL, AppBuildOptions);
	
        //FrameworkPostProcessor.OnPostProcessBuild (BuildTarget.iOS, fullpath);
        return (errorStr == null);
    }
    
}
