using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class IOSCocoaPodInstalling
{
    [PostProcessBuildAttribute(45)]//must be between 40 and 50 to ensure that it's not overriden by Podfile generation (40) and that it's added before "pod install" (50)
    public static void PostProcessBuild(BuildTarget target, string path)
    {
        if (target == BuildTarget.iOS)
        {
            var podFileLocation = Path.Combine(path, "Podfile");
            string pod_extend = Application.dataPath + "/Editor/CocoaPods/podfile_extension.txt";
            if (File.Exists(podFileLocation) && File.Exists(pod_extend))
            {
                string podStr = File.ReadAllText(podFileLocation);
                string extendStr = File.ReadAllText(pod_extend);
                File.WriteAllText(podFileLocation, podStr + "\n" + extendStr);
            }


        }
    }
}
