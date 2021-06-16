#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Editor.CocoaPods
{
    public class IOSCocoaPodInstalling
    {
        [PostProcessBuildAttribute(
            45)] //must be between 40 and 50 to ensure that it's not overriden by Podfile generation (40) and that it's added before "pod install" (50)
        public static void PostProcessBuild(BuildTarget target, string path)
        {
            if (target == BuildTarget.iOS)
            {
                var currentDirectory = Directory.GetCurrentDirectory();
                
                var podFileLocation = Path.Combine(path, "Podfile");
                if (File.Exists(podFileLocation))
                {
                    var text =
                        @"A Podfile is already existing under Xcode project root";
                    UnityEngine.Debug.Log(text);
                }
                else
                {
                    var podfilePath = Path.Combine(currentDirectory, "Assets/Editor/CocoaPods/Podfile");
                    UnityEngine.Debug.Log(podfilePath);
                    File.Copy(podfilePath, podFileLocation);
                }

                string pod_extend = Application.dataPath + "/Editor/CocoaPods/podfile_extension.txt";
                if (File.Exists(podFileLocation) && File.Exists(pod_extend))
                {
                    UnityEngine.Debug.Log("podFileLocation " + podFileLocation);
                    string podStr = File.ReadAllText(podFileLocation);
                    string extendStr = File.ReadAllText(pod_extend);
                    File.WriteAllText(podFileLocation, podStr + "\n" + extendStr);
                }
                
                Directory.SetCurrentDirectory(path);
                var log = ShellCommand.Run("pod", "install");
                UnityEngine.Debug.Log(log);
                Directory.SetCurrentDirectory(currentDirectory);
            }
        }
    }
}
#endif
