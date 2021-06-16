using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.Xml;
using System.Xml.Serialization;

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
            //string targetName = PBXProject.GetUnityTestTargetName();
            string targetGUID = pbxProject.GetUnityMainTargetGuid();

            pbxProject.SetBuildProperty(targetGUID, "ENABLE_BITCODE", "NO");
            //pbxProject.SetBuildProperty(targetGUID, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
            pbxProject.AddBuildProperty(targetGUID, "OTHER_LDFLAGS", "-ObjC");

            // framework
            // pbxProject.AddFrameworkToProject(targetGUID, "StoreKit.framework", false);
            pbxProject.AddFrameworkToProject(targetGUID, "UserNotifications.framework", false);
            pbxProject.AddFrameworkToProject(targetGUID, "AuthenticationServices.framework", false);
            // pbxProject.AddFrameworkToProject(targetGUID, "StoreKit.framework", false);
            pbxProject.AddFrameworkToProject(targetGUID, "MessageUI.framework", false);

            // Write.
            File.WriteAllText(projectPath, pbxProject.WriteToString());
        }

    }
}
