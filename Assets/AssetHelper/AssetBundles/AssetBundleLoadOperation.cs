using UnityEngine;
using System.Collections;
using System;

namespace AssetHelper.AssetBundles
{
    public abstract class AssetBundleLoadOperation : IEnumerator
    {
        public object Current { get { return null; } }

        public bool MoveNext() { return !IsDone(); }

        public void Reset() { }

        public abstract bool Update();

        public abstract bool IsDone();
    }

    public abstract class BaseAssetBundleLoadAssetOperation : AssetBundleLoadOperation
    {
        public abstract T GetAsset<T>() where T : UnityEngine.Object;
    }

    public class AssetBundleLoadAssetOperation : BaseAssetBundleLoadAssetOperation
    {

        protected string _assetBundleName;
        protected string _assetName;
        protected System.Type _type;
        protected AssetBundleRequest _request = null;

        protected string _downloadingError;

        public AssetBundleLoadAssetOperation(string bundleName, string assetName, System.Type type)
        {
            this._assetBundleName = bundleName;
            this._assetName = assetName;
            this._type = type;
        }

        public override T GetAsset<T>()
        {
            if (_request != null && _request.isDone)
                return _request.asset as T;
            else
                return null;
        }

        public override bool Update()
        {
            if (_request != null)
                return false;

            // Get loaded asset bundle from AssetBundleManager
            LoadedAssetBundle bundle = AssetBundleManager.GetLoadedAssetBundle(_assetBundleName, out _downloadingError);
            if(bundle != null)
            {
                _request = bundle._assetBundle.LoadAssetAsync(_assetName, _type);
                return false;
            }
            else
            {
                return true;
            }
        }

        public override bool IsDone()
        {
            if (_request == null && _downloadingError != null)
            {
                Debug.LogError(_downloadingError);
                return true;
            }

            return _request != null && _request.isDone;
        }
    }

    public class AssetBundleManifestOperation : AssetBundleLoadAssetOperation
    {
        public AssetBundleManifestOperation(string bundleName, string assetName, System.Type type)
            : base(bundleName, assetName, type)
        {
        }

        public override bool Update()
        {
            base.Update();

            if(_request != null && _request.isDone)
            {
                AssetBundleManager.AssetBundleManifestObject = GetAsset<AssetBundleManifest>();
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    public class AssetBundleLoadLevelOperation : AssetBundleLoadOperation
    {

        protected string _assetBundleName;
        protected string _levelName;
        protected bool _isAdditive;
        protected AsyncOperation _request;

        protected string _downloadingError;

        public AssetBundleLoadLevelOperation(string assetBundleName, string levelName, bool isAdditive)
        {
            this._assetBundleName = assetBundleName;
            this._levelName = levelName;
            this._isAdditive = isAdditive;
        }

        public override bool Update()
        {
            if (_request != null)
                return false;

            // Get loaded asset bundle from AssetBundleManager
            LoadedAssetBundle bundle = AssetBundleManager.GetLoadedAssetBundle(_assetBundleName, out _downloadingError);
            if (bundle != null)
            {
                if (_isAdditive)
                    _request = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(_levelName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
                else
                    _request = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(_levelName);

                return false;
            }
            else
                return true;
        }

        public override bool IsDone()
        {
            if (_request == null && _downloadingError != null)
            {
                Debug.LogError(_downloadingError);
                return true;
            }

            return _request != null && _request.isDone;
        }
    }
}
