using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using Facebook.Unity.Settings;
using UnityEditor.Build.Reporting;

public class JenkinBuildADR : MonoBehaviour
{
    //Current version code = 22
    public const string mainifestResources = @"Editor/ResourceManifest/{0}_AndroidManifest.xml";
    //public const string mainifestTarget = @"Plugins/Android/com.google.firebase.firebase-common-16.0.0/AndroidManifest.xml";
    private const string RspFile = "csc.rsp";
    public const string PackageNameDefault = "com.gnt.ludo.dev";
    public static string PackageName = "com.gnt.ludo.dev";
    public static string AppName = "Ludo";
    public static string AppVersion = "0.0.1";
    public static int BuildVersion = 36; //version code Android
    public static string AppBuildPath = "JenKinsBuild/Android/";
    public static string PrivateParam = "-define:LOGGER;LOGGER_FILE;LOGGER_SYNC_FILE;LOGGER_DOWNLOAD_FILE;";
    public static BuildOptions AppBuildOptions = BuildOptions.None;
    private static string keyStoreName = "KeyApplication/mobion_music_keystore.keystore";
    private static string keyStorePass = "mobion_music_keystore";
    private static string exportName = "Ludo_{0}_{1}.apk";
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

        // tien.nt: for build iap
        var test_iap = CommandLineReader.GetCustomArgument("TEST_IAP");
        if (!string.IsNullOrEmpty(test_iap))
        {
            if (test_iap == "TRUE")
            {
                PlayerSettings.Android.keystoreName = "KeyApplication/ludo_testiap.keystore";
                PlayerSettings.Android.keyaliasName = "jp.co.gnt.ludo.test";
                PlayerSettings.Android.keystorePass = "123456";
                PlayerSettings.Android.keyaliasPass = "123456";
            }
        }

        // tien.nt: for product
        var is_product = CommandLineReader.GetCustomArgument("PRODUCTION");
        if (!string.IsNullOrEmpty(is_product))
        {
            if (is_product == "TRUE")
            {
                PlayerSettings.Android.keystoreName = "KeyApplication/nandora.keystore";
                PlayerSettings.Android.keyaliasName = "com.app.nandora";
                PlayerSettings.Android.keystorePass = "Abc@123!";
                PlayerSettings.Android.keyaliasPass = "Abc@123!";

                OverwriteFileConfigByProduction();
            }
        }
        AddFacebookConfig();
        AddDeepLink();
        OnUpdateAndroidManifest();
    }
    private static  void OnUpdateAndroidManifest()
    {
        ManifestHelper manifest = new ManifestHelper(AndroidManifestPath);
 
        manifest.SetVersions(PlayerSettings.bundleVersion, int.Parse(versionCode));
        manifest.SetPackageName(PackageName);
        manifest.SetFacebookApplication(FacebookSettings.AppId);
        manifest.Save(AndroidManifestPath);
    }
    
    private static void AddFacebookConfig()
    {
        switch(GlobalDefinePopup.ServerSetting)
        {
            case "TEST":
                FacebookSettings.SelectedAppIndex = 1;
                break;
            case "PRODUCT":
                FacebookSettings.SelectedAppIndex = 2;
                break;
            default:
                FacebookSettings.SelectedAppIndex = 0;
                break;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("LoginSocial FacebookId : " + FacebookSettings.SelectedAppIndex);
    }

    private static void AddDeepLink()
    {
        string manifestPath = Application.dataPath + "/Plugins/Android/AndroidManifest.xml";
        if (!File.Exists(manifestPath))
        {
            Debug.LogError(" DEEP LINK ERROR: can not find " + manifestPath);
            return;
        }
        string data = File.ReadAllText(manifestPath);
        string findText = "deeplink_do_not_remove";
        string replaceText = "ludotestapi.sgcharo.com";
        
        switch(GlobalDefinePopup.ServerSetting)
        {
            case "TEST":
                replaceText = "api01.ludo-draft.com";
                break;
            case "TESTVN":
                replaceText = "ludotestapi.sgcharo.com";
                break;
            case "PRODUCT":
                replaceText = "api01.appnandora.com";
                break;
            default:
                replaceText = "ludodevapi.sgcharo.com";
                break;
        }
        
        Debug.Log("Sharelink: " + replaceText);
        if (data.IndexOf(findText) >= 0)
        {
            data = data.Replace(findText, replaceText);
            File.WriteAllText(manifestPath, data);
        }
        else
        {
            Debug.LogError(" DEEP LINK ERROR: can not find config text (scheme)" + findText);
            return;
        }

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
        
        if (PrivateParam.Contains("TESTVN"))
        {
            GlobalDefinePopup.ServerSetting = "TESTVN";
        } 
        else if (PrivateParam.Contains("TEST"))
        {
            GlobalDefinePopup.ServerSetting = "TEST";
        }
        else if(PrivateParam.Contains("PRODUCT"))
        {
            GlobalDefinePopup.ServerSetting = "PRODUCT";
        }
        else //DEV
        {
            GlobalDefinePopup.ServerSetting = "DEV";
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        //            Logger.Log("Jenkins Auto Build private param : ===>" + PrivateParam);
    }

    public static void OverwriteFileConfigByProduction()
    {
        string path = Application.dataPath + "/Editor/FileConfig/Firebase/PRODUCTION.google-services.json";
        if (!File.Exists(path))
        {
            Debug.LogError(" Cannot found file config: " + path);
            return;
        }

        string old_path = Application.dataPath + "/google-services.json";
        if (!File.Exists(path))
        {
            Debug.LogError(" Cannot found old file config: " + old_path);
            return;
        }

        File.WriteAllText(old_path, File.ReadAllText(path));
    }

}
