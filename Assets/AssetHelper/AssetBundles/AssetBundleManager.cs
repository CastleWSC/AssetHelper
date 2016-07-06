using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AssetHelper.AssetBundles
{
    public class LoadedAssetBundle
    {
        public AssetBundle _assetBundle = null;
        public int _referencedCount = 0;

        public LoadedAssetBundle(AssetBundle assetBundle)
        {
            this._assetBundle = assetBundle;
            this._referencedCount = 1;
        }
    }

    public class AssetBundleManager : MonoBehaviour
    {
        public enum LogType { Info, Error };

        static string _baseDownloadingURL = "";
        static AssetBundleManifest _assetBundleManifest = null;
        static string[] _activeVariants = new string[] { };


        static Dictionary<string, LoadedAssetBundle> _loadedAssetBundles = new Dictionary<string, LoadedAssetBundle>();
        static Dictionary<string, WWW> _downloadingWWWs = new Dictionary<string, WWW>();
        static List<AssetBundleLoadOperation> _operations = new List<AssetBundleLoadOperation>();
        static Dictionary<string, string[]> _dependencies = new Dictionary<string, string[]>();
        static Dictionary<string, string> _downloadingErrors = new Dictionary<string, string>();


        static public AssetBundleManifest AssetBundleManifestObject
        {
            set { _assetBundleManifest = value; }
        }

        static public string[] ActiveVariants
        {
            get { return _activeVariants; }
            set { _activeVariants = value; }
        }

        static public string BaseDownloadingURL
        {
            get { return _baseDownloadingURL; }
            set { _baseDownloadingURL = value; }
        }

        static public string SetSourceAssetBundleDirectory(string relativePath)
        {
            BaseDownloadingURL = Utility.GetStreamingAssetPath() + relativePath;

            return BaseDownloadingURL;
        }

        static public string SetSourceAssetBundleURL(string absolutePath)
        {
            BaseDownloadingURL = absolutePath + Utility.GetPlatformName() + "/";

            return BaseDownloadingURL;
        }

        static public LoadedAssetBundle GetLoadedAssetBundle(string assetBundleName, out string error)
        {
            // Error when download assetBundle
            if (_downloadingErrors.TryGetValue(assetBundleName, out error))
                return null;

            LoadedAssetBundle bundle = null;

            _loadedAssetBundles.TryGetValue(assetBundleName, out bundle);
            if (bundle == null) return null;

            // No dependencies
            string[] dependencies = null;
            if (!_dependencies.TryGetValue(assetBundleName, out dependencies))
                return bundle;

            // Make sure all dependencies are loaded
            foreach(var dependency in dependencies)
            {
                // Error when download the dependency
                if (_downloadingErrors.TryGetValue(dependency, out error))
                    return bundle;

                LoadedAssetBundle dependent;
                _loadedAssetBundles.TryGetValue(dependency, out dependent);
                if (dependent == null) return null;
            }

            return bundle;
        }

        static public AssetBundleManifestOperation Initialize()
        {
            return Initialize(Utility.GetPlatformName());
        }

        static public AssetBundleManifestOperation Initialize(string manifestAssetBundleName)
        {

            var go = new GameObject("[AssetBundleManager]");
            DontDestroyOnLoad(go);

            // load asset bundle
            LoadAssetBundle(manifestAssetBundleName, true);
            

            var operation = new AssetBundleManifestOperation(manifestAssetBundleName, "AssetBundleManifest", typeof(AssetBundleManifest));
            _operations.Add(operation);

            return operation;
        }

        static protected void LoadAssetBundle(string assetBundleName, bool isLoadingAssetBundleManifest = false)
        {
            Log(LogType.Info, "Loading Asset Bundle: " + assetBundleName);

            if(!isLoadingAssetBundleManifest)
            {
                if(_assetBundleManifest == null)
                {
                    Log(LogType.Error, "Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");

                    return;
                }
            }

            bool isAlreadyProcessed = LoadAssetBundleInternal(assetBundleName, isLoadingAssetBundleManifest);

            if (!isAlreadyProcessed && !isLoadingAssetBundleManifest)
                LoadDependencies(assetBundleName);
        }

        static protected bool LoadAssetBundleInternal(string assetBundleName, bool isLoadingAssetBundleManifest)
        {
            LoadedAssetBundle bundle = null;
            _loadedAssetBundles.TryGetValue(assetBundleName, out bundle);
            if(bundle != null)
            {
                bundle._referencedCount++;
                return true;
            }

            if (_downloadingWWWs.ContainsKey(assetBundleName))
                return true;

            WWW download = null;
            string url = _baseDownloadingURL + assetBundleName;

            if (isLoadingAssetBundleManifest)
                download = new WWW(url);
            else
                download = WWW.LoadFromCacheOrDownload(url, _assetBundleManifest.GetAssetBundleHash(assetBundleName), 0);

            _downloadingWWWs.Add(assetBundleName, download);

            return false;
        }

        static protected string RemapVariantName(string assetBundleName)
        {
            string[] bundleWithVariant = _assetBundleManifest.GetAllAssetBundlesWithVariant();

            string[] split = assetBundleName.Split('.');

            int bestFit = int.MaxValue;
            int bestFitIndex = -1;

            for(int i=0; i<bundleWithVariant.Length; i++)
            {
                string[] curSplit = bundleWithVariant[i].Split('.');
                if (curSplit[0] != split[0]) continue;

                int found = System.Array.IndexOf(_activeVariants, curSplit[1]);

                if (found == -1)
                    found = int.MaxValue - 1;

                if(found < bestFit)
                {
                    bestFit = found;
                    bestFitIndex = i;
                }
            }

            if (bestFit == int.MaxValue - 1)
            {
                Debug.LogWarning("[AssetBundleManager] Ambigious asset bundle variant chosen because there was no matching active variant: " + bundleWithVariant[bestFitIndex]);
            }

            if (bestFitIndex != -1)
            {
                return bundleWithVariant[bestFitIndex];
            }
            else
            {
                return assetBundleName;
            }
        }

        static protected void LoadDependencies(string assetBundleName)
        {
            if (_assetBundleManifest == null)
            {
                Log(LogType.Error, "Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");

                return;
            }

            // Get dependencies
            string[] dependencies = _assetBundleManifest.GetAllDependencies(assetBundleName);
            if (dependencies.Length == 0) return;

            // Remap variant name
            for (int i = 0; i < dependencies.Length; i++)
                dependencies[i] = RemapVariantName(dependencies[i]);

            // Record and load all dependencies
            _dependencies.Add(assetBundleName, dependencies);
            for (int i = 0; i < dependencies.Length; i++)
                LoadAssetBundle(dependencies[i]);
        }

        static public void UnloadAssetBundle(string assetBundleName)
        {
            UnloadDependencies(assetBundleName);
            UnloadAssetBundleInternal(assetBundleName);
        }

        static protected void UnloadDependencies(string assetBundleName)
        {
            string[] dependencies = null;
            if (!_dependencies.TryGetValue(assetBundleName, out dependencies))
                return;

            foreach (var dependency in dependencies)
                UnloadAssetBundleInternal(dependency);

            _dependencies.Remove(assetBundleName);    
        }

        static protected void UnloadAssetBundleInternal(string assetBundleName)
        {
            string error;
            LoadedAssetBundle bundle = GetLoadedAssetBundle(assetBundleName, out error);
            if (bundle == null)
                return;

            if(--bundle._referencedCount == 0)
            {
                bundle._assetBundle.Unload(false);
                _loadedAssetBundles.Remove(assetBundleName);

                Log(LogType.Info, assetBundleName + " has been unloaded successfully");
            }
        }

        void Update()
        {
            var keysToRemove = new List<string>();

            foreach(var kv in _downloadingWWWs)
            {
                WWW download = kv.Value;

                // Downloading fails
                if(download.error != null)
                {
                    _downloadingErrors.Add(kv.Key, string.Format("Failed downloading bundle {0} from {1}: {2}", kv.Key, download.url, download.error));

                    keysToRemove.Add(kv.Key);
                    continue;
                }

                // Downloading succeeds
                if (download.isDone)
                {
                    AssetBundle bundle = download.assetBundle;
                    if(bundle == null)
                    {
                        _downloadingErrors.Add(kv.Key, string.Format("{0} is not a valid asset bundle", kv.Key));

                        keysToRemove.Add(kv.Key);
                        continue;
                    }

                    _loadedAssetBundles.Add(kv.Key, new LoadedAssetBundle(bundle));
                    keysToRemove.Add(kv.Key);
                }
            }

            // Remove the finished WWWs
            foreach(var key in keysToRemove)
            {
                WWW downloaded = _downloadingWWWs[key];
                _downloadingWWWs.Remove(key);
                downloaded.Dispose();
            }

            // Update all in process operations
            for(int i=0; i<_operations.Count;)
            {
                if (!_operations[i].Update())
                    _operations.RemoveAt(i);
                else
                    i++;
            }
        }

        static public AssetBundleLoadOperation LoadAssetAsync(string assetBundleName, string assetName, System.Type type)
        {
            Log(LogType.Info, "Loading " + assetName + " from " + assetBundleName + " bundle");

            AssetBundleLoadOperation operation = null;

            assetBundleName = RemapVariantName(assetBundleName);
            LoadAssetBundle(assetBundleName);
            operation = new AssetBundleLoadAssetOperation(assetBundleName, assetName, type);
            _operations.Add(operation);

            return operation;
        }

        static public AssetBundleLoadOperation LoadLevelAsync(string assetBundleName, string levelName, bool isAdditive)
        {
            Log(LogType.Info, "Loading " + levelName + " from " + assetBundleName + " bundle");

            AssetBundleLoadOperation operation = null;

            assetBundleName = RemapVariantName(assetBundleName);
            LoadAssetBundle(assetBundleName);
            operation = new AssetBundleLoadLevelOperation(assetBundleName, levelName, isAdditive);
            _operations.Add(operation);

            return operation;
        }

        static private void Log(LogType logType, string text)
        {
            if (logType == LogType.Error)
                Debug.LogError("[AssetBundleManager] " + text);
            else
                Debug.Log("[AssetBundleManager] " + text);
        }
    }
}
