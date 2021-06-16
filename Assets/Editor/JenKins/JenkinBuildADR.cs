using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;

public class JenkinBuildADR : MonoBehaviour
{
    //Current version code = 22
    public const string mainifestResources = @"Editor/ResourceManifest/{0}_AndroidManifest.xml";
    //public const string mainifestTarget = @"Plugins/Android/com.google.firebase.firebase-common-16.0.0/AndroidManifest.xml";
    private const string RspFile = "csc.rsp";
    public const string PackageNameDefault = "com.gnt.toone";
    public static string PackageName = "com.gnt.toone";
    public static string AppName = "Toone";
    public static string AppVersion = "0.0.1";
    public static int BuildVersion = 36; //version code Android
    public static string AppBuildPath = "JenKinsBuild/Android/";
    public static string PrivateParam = "-define:LOGGER;LOGGER_FILE;LOGGER_SYNC_FILE;LOGGER_DOWNLOAD_FILE;";
    public static BuildOptions AppBuildOptions = BuildOptions.None;
    private static string keyStoreName = "KeyApplication/mobion_music_keystore.keystore";
    private static string keyStorePass = "mobion_music_keystore";
    private static string exportName = "Toone_{0}_{1}.apk";
    private static string splitBinary = "false";
    private static string timePull = "00:00";
    private static string buildNumber = "0000";
    private static string versionCode = "0000";
    private static string[] PrivateParamArray;
    public const string AndroidManifestPath = "Assets/Plugins/Android/AndroidManifest.xml";
    
    private const int baseVersion = 1000;

    public static void init()
    {
        AssetDatabase.Refresh();
        setParamFromRemoteJenkin();
        //Fix transparent status bar
        PlayerSettings.statusBarHidden = true;
        PlayerSettings.bundleVersion = AppVersion;
        PlayerSettings.applicationIdentifier = PackageName;
        PlayerSettings.productName = AppName;
        PlayerSettings.SplashScreen.showUnityLogo = false;
        PlayerSettings.Android.bundleVersionCode = BuildVersion;
        PlayerSettings.Android.keystoreName = keyStoreName;
        PlayerSettings.Android.keyaliasName = "mobion music";
        PlayerSettings.Android.keystorePass = keyStorePass;
        PlayerSettings.Android.keyaliasPass = keyStorePass;
        
        if (splitBinary == "false")
            PlayerSettings.Android.useAPKExpansionFiles = false;
        else
            PlayerSettings.Android.useAPKExpansionFiles = true;
    }
  
    
    private static void WriteVersionToResouce(string text)
    {
        string temporaryTextFileName = "build_version";
        File.WriteAllText(Application.dataPath + "/Resources/" + temporaryTextFileName + ".txt", text);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void setParamFromRemoteJenkin()
    {
        Debug.Log("setParamFromRemoteJenkin ");
        PackageName = CommandLineReader.GetCustomArgument("BUNDLE_ID").ToLower();
        AppName = CommandLineReader.GetCustomArgument("APPNAME");
        AppVersion = CommandLineReader.GetCustomArgument("APPVERSION");
        PrivateParam = CommandLineReader.GetCustomArgument("PRIVATEPARAM");
        exportName = CommandLineReader.GetCustomArgument("NAME_EXPORT");
        splitBinary = CommandLineReader.GetCustomArgument("SPLIT_BINARY");
        timePull = CommandLineReader.GetCustomArgument("TIME_PULL");
        buildNumber = CommandLineReader.GetCustomArgument("BUILD_NUMBER");
        versionCode = CommandLineReader.GetCustomArgument("BUILDVERSION");

        WriteVersionToResouce(buildNumber + "#" + timePull);

        var buildOption = CommandLineReader.GetCustomArgument("BUILDOPTIONS");
        if (!string.IsNullOrEmpty(buildOption))
        {

            var options = buildOption.Split('|');
            if (options.Length > 0)
            {
                AppBuildOptions = BuildOptions.Development;
                foreach (var opt in options)
                {
                    AppBuildOptions |= (BuildOptions)(Enum.Parse(typeof(BuildOptions), opt, true));
                }
            }
        }

        CleanResources();
        SavePrivateParam();
        SplitParam();
        if (PackageName == PackageNameDefault)
        {
            //build app to buycoin
        }
        else
        {
            try
            {
                //2874 1.0.2; 
                //2875 1.0.3; 
                var number = int.Parse(buildNumber);

                BuildVersion = number + baseVersion;

            }
            catch (Exception e)
            {
                Debug.LogError("Error: Cannot parse build version - " + e.Message);
            }
        }


    }

    private static bool IsExistParam(string param)
    {
        foreach (string tmp in PrivateParamArray)
        {
            if (tmp == param)
                return true;
        }
        return false;
    }

    private static void SplitParam()
    {
        string t1 = PrivateParam;
        t1 = t1.Substring("-define:".Length);
        PrivateParamArray = t1.Split(';');
    }

    [MenuItem("Window/ADRBuild")]
    static void ADRBuild()
    {
        init();


        try
        {
            bool buildOk = BuildPlayer();

            if (!buildOk)
            {
                Debug.LogError("Build player failed");
                return;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }
    static bool BuildPlayer()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);
        string fullpath = System.IO.Path.GetFullPath(AppBuildPath);
        Directory.CreateDirectory(fullpath);
        if (!Directory.Exists(fullpath))
            Directory.CreateDirectory(fullpath);
        List<string> scenePaths = new List<string>();
        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e.enabled)
                scenePaths.Add(e.path);

        }
        var errorStr = BuildPipeline.BuildPlayer(scenePaths.ToArray(), fullpath + exportName + ".apk", BuildTarget.Android, AppBuildOptions);
        return (errorStr != null);
    }

    public static void CleanResources()
    {

    }

    public static void SavePrivateParam()
    {
        Debug.Log("Did write gmcs " + PrivateParam);
        var smcsFile = Path.Combine(Application.dataPath, RspFile);
        File.WriteAllText(smcsFile, PrivateParam);
       
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        //            Logger.Log("Jenkins Auto Build private param : ===>" + PrivateParam);
    }

}
