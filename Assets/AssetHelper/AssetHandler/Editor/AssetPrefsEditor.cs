using UnityEngine;
using UnityEditor;

namespace AssetHelper.AssetHandler
{
    public class AssetPrefsEditor
    {
        private static bool prefsLoad = false;

        public static string Shader_Editor = "None";
        public static string Json_Editor = "None";

        [PreferenceItem("Assets Handler")]
        static void PreferencesGUI()
        {

            if (!prefsLoad)
            {
                LoadPreferences();
                prefsLoad = true;
            }

            GUILayout.Space(10);

            EditorGUILayout.BeginVertical();

            // Shader handler
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Shader Editor", GUILayout.Width(200));
            if (GUILayout.Button(Shader_Editor, GUILayout.Width(150)))
            {
                Shader_Editor = ProgramSelection("shader");
            }
            EditorGUILayout.EndHorizontal();

            // Json handler
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Json Editor", GUILayout.Width(200));
            if (GUILayout.Button(Json_Editor, GUILayout.Width(150)))
            {
                Json_Editor = ProgramSelection("json");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            if (GUI.changed)
                SavePreferences();
        }

        static void LoadPreferences()
        {
            if (EditorPrefs.HasKey("ShaderEditor"))
                Shader_Editor = EditorPrefs.GetString("ShaderEditor");

            if (EditorPrefs.HasKey("JsonEditor"))
                Json_Editor = EditorPrefs.GetString("JsonEditor");
        }

        static void SavePreferences()
        {
            EditorPrefs.SetString("ShaderEditor", Shader_Editor);
            EditorPrefs.SetString("JsonEditor", Json_Editor);
        }

        static string ProgramSelection(string type)
        {
            return EditorUtility.OpenFilePanel(
                "Select a program to open " + type + " asset",
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles).TrimEnd('\\'),
                "exe");
        }
    }
}
