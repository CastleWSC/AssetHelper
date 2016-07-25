using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace AssetHelper.AssetHandler
{
    class JsonHandler
    {
        [OnOpenAsset(1)]
        public static bool Handle(int instanceID, int line)
        {
            string path = AssetDatabase.GetAssetPath(EditorUtility.InstanceIDToObject(instanceID));
            string name = Application.dataPath + "/" + path.Replace("Assets/", "");

            if (name.EndsWith(".json"))
            {
                string program = "";
                if (EditorPrefs.HasKey("JsonEditor"))
                    program = EditorPrefs.GetString("JsonEditor");

                if (System.IO.File.Exists(program))
                {
                    try
                    {
                        System.Diagnostics.Process process = new System.Diagnostics.Process();
                        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        startInfo.FileName = program;
                        startInfo.Arguments = name;
                        process.StartInfo = startInfo;

                        return process.Start();
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            return false;
        }
    }
}
