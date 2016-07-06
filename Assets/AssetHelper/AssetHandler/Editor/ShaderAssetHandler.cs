using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace AssetHelper.AssetHandler
{
    public class ShaderAssetHandler
    {

        static string handler = null;

        [OnOpenAsset(1)]
        public static bool Handle(int instanceID, int line)
        {
            string path = AssetDatabase.GetAssetPath(EditorUtility.InstanceIDToObject(instanceID));
            string name = Application.dataPath + "/" + path.Replace("Assets/", "");

            if (name.EndsWith(".shader"))
            {
                if (CheckHandler())
                {
                    Debug.Log("[AssetHandler] Used " + handler + " \nto open the " + name);

                    try
                    {
                        System.Diagnostics.Process process = new System.Diagnostics.Process();
                        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        startInfo.FileName = handler;
                        startInfo.Arguments = name;
                        process.StartInfo = startInfo;
                        bool success = process.Start();

                        if (!success)
                        {
                            handler = null;
                            Debug.LogError("[AssetHandler] Failed to open asset");
                        }

                        return success;
                    }
                    catch
                    {
                        handler = null;
                        Debug.LogError("[AssetHandler] Failed to open asset");

                        return false;
                    }
                }
                else
                {
                    Debug.LogError("[AssetHandler] Not found the " + handler + " program");
                }
            }

            return false;
        }

        static bool CheckHandler()
        {
            if (string.IsNullOrEmpty(handler))
            {
                handler = EditorUtility.OpenFilePanel(
                    "Select a program to open asset",
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles).TrimEnd('\\'),
                    "exe");
            }

            return System.IO.File.Exists(handler);
        }
    }
}
