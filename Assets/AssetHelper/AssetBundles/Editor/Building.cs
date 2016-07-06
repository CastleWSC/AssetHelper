using UnityEngine;
using UnityEditor;
using System.IO;

namespace AssetHelper.AssetBundles
{
    public class Building
    {
        [MenuItem("AssetHelper/AssetBundle/Build AssetBundles")]
        static void BuildAssetBundles()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            string outputPath = Path.Combine(Utility.AssetBundleOutputPath, Utility.GetPlatformName());
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);

            sw.Stop();

            Debug.Log("[AssetBundleManager] Successfully to build asset bundles \nTotal cost " + sw.Elapsed.TotalSeconds.ToString("F2") + " seconds");

            AssetDatabase.Refresh();
        }
    }
}
