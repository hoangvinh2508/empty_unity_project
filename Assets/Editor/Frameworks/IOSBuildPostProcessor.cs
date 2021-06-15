using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.Xml;
using System.Xml.Serialization;
using Xml2CSharp;

public class IOSBuildPostProcessor
{
    [PostProcessBuildAttribute(1)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target == BuildTarget.iOS)
        {
            // Read.
            string projectPath = PBXProject.GetPBXProjectPath(path);
            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromString(File.ReadAllText(projectPath));
            string targetName = PBXProject.GetUnityTargetName();
            string targetGUID = pbxProject.TargetGuidByName(targetName);

            pbxProject.SetBuildProperty(targetGUID, "ENABLE_BITCODE", "NO");
            //pbxProject.SetBuildProperty(targetGUID, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
            pbxProject.AddBuildProperty(targetGUID, "OTHER_LDFLAGS", "-ObjC");

            // framework
            pbxProject.AddFrameworkToProject(targetGUID, "StoreKit.framework", false);
            pbxProject.AddFrameworkToProject(targetGUID, "UserNotifications.framework", false);
            pbxProject.AddFrameworkToProject(targetGUID, "AuthenticationServices.framework", true);
            pbxProject.AddFrameworkToProject(targetGUID, "StoreKit.framework", false);
            pbxProject.AddFrameworkToProject(targetGUID, "MessageUI.framework", false);

            // Write.
            File.WriteAllText(projectPath, pbxProject.WriteToString());

            // capbility
            var capManager = new ProjectCapabilityManager(projectPath, "ludo.entitlements", targetName);
            capManager.AddPushNotifications(true);//NOTE: true = development | false = production
            capManager.AddBackgroundModes(BackgroundModesOptions.RemoteNotifications);
            capManager.AddSignInWithApple();
            capManager.AddInAppPurchase();
            
            // for universal link 
            switch(GlobalDefinePopup.ServerSetting)
            {
                case "TEST":
                    capManager.AddAssociatedDomains(new string[] { "applinks:api01.ludo-draft.com" });
                    break;
                case "TESTVN":
                    capManager.AddAssociatedDomains(new string[] { "applinks:ludotestapi.sgcharo.com" });
                    break;
                case "PRODUCT":
                    capManager.AddAssociatedDomains(new string[] { "applinks:api01.appnandora.com" });   
                    break;
                default:
                    capManager.AddAssociatedDomains(new string[] { "applinks:ludodevapi.sgcharo.com" });
                    break;
            }
            Debug.Log("Sharelink : " + GlobalDefinePopup.ServerSetting);
            capManager.WriteToFile();

            // URL Scheme

            //App urlscheme
            addAppURLScheme(path);
            SettingLocalizeNative(path);

            // google sign in
            string ggConfigFilePath = Application.dataPath + "/GoogleSignIn/Resources/FileConfig/"
                + PlayerSettings.applicationIdentifier + "_googlesignin_configfile.plist";
            if (!File.Exists(ggConfigFilePath))
            {
                Debug.LogError("Cannot found file config google sign in " + ggConfigFilePath);
                return;
            }
            else
            {
                string config = File.ReadAllText(ggConfigFilePath);
                XmlSerializer serializer = new XmlSerializer(typeof(Plist));
                using (FileStream stream = new FileStream(ggConfigFilePath, FileMode.Open))
                {
                    Plist val = serializer.Deserialize(stream) as Plist;
                    if (val != null)
                    {
                        string configURL = "Google Sign In";
                        for (int i = 0; i < val.Dict.Key.Count; i++)
                        {
                            if ("REVERSED_CLIENT_ID" == val.Dict.Key[i])
                            {
                                configURL = val.Dict.String[i];
                                break;
                            }
                        }

                        string plistPath = path + "/Info.plist";

                        PlistDocument plist = new PlistDocument();
                        plist.ReadFromString(File.ReadAllText(plistPath));

                        // Change value of UIUserInterfaceStyle in Xcode plist
                        // Get root
                        PlistElementDict rootDict = plist.root;
                        var lightModeKey = "UIUserInterfaceStyle";
                        rootDict.SetString(lightModeKey, "Light");

                        PlistElementArray array = GetOrCreateArray(rootDict, "CFBundleURLTypes");
                        var googleSignInURLScheme = array.AddDict();
                        googleSignInURLScheme.SetString("CFBundleTypeRole", "Editor");
                        googleSignInURLScheme.SetString("CFBundleURLName", "Google Sign In");

                        var schemes = googleSignInURLScheme.CreateArray("CFBundleURLSchemes");
                        schemes.AddString(configURL);

                        File.WriteAllText(plistPath, plist.WriteToString());
                    }
                }
            }
        }

     
    }

    static PlistElementArray GetOrCreateArray(PlistElementDict dict, string key)
    {
        PlistElement array = dict[key];
        if (array != null)
        {
            return array.AsArray();
        }
        else
        {
            return dict.CreateArray(key);
        }
    }

    static void addAppURLScheme(string projectPath)
    {
        string plistPath = projectPath + "/Info.plist";

        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));

        PlistElementDict rootDict = plist.root;
        PlistElementArray array = GetOrCreateArray(rootDict, "CFBundleURLTypes");
        var googleSignInURLScheme = array.AddDict();
        googleSignInURLScheme.SetString("CFBundleTypeRole", "Editor");
        googleSignInURLScheme.SetString("CFBundleURLName", "ludo");

        var schemes = googleSignInURLScheme.CreateArray("CFBundleURLSchemes");
        string appURLScheme = PlayerSettings.applicationIdentifier.Replace(".","");

        schemes.AddString(appURLScheme);
        File.WriteAllText(plistPath, plist.WriteToString());
    }

    static void SettingLocalizeNative(string pathToBuiltProject)
    {
        // Get plist
        string plistPath = pathToBuiltProject + "/Info.plist";
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));

        // Get root
        PlistElementDict rootDict = plist.root;

        // remove exit on suspend if it exists.
        string exitsOnSuspendKey = "CFBundleDevelopmentRegion";
        if (rootDict.values.ContainsKey(exitsOnSuspendKey))
        {
            rootDict.values.Remove(exitsOnSuspendKey);
        }
        rootDict.SetString("CFBundleDevelopmentRegion ", "ja_JP");
        rootDict.SetString("UIUserInterfaceStyle ", "Light");
        

        // Write to file
        File.WriteAllText(plistPath, plist.WriteToString());
    }
}
