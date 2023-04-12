
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.AddressableAssets.ResourceLocators;
using System.Linq;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Scoz.Func {

    [Serializable]
    public sealed class AddressableManage : MonoBehaviour {
        public static AddressableManage Instance;

        public List<string> Keys = null;
        [SerializeField] Image ProgressImg = null;
        [SerializeField] Text ProgressText = null;
        [SerializeField] GameObject DownloadGO = null;
        [SerializeField] GameObject BG;
        Coroutine CheckInternetCoroutine = null;
        Action FinishedAction = null;

        const float TryReDownloadCD = 8;//嘗試重新call Addressables.DownloadDependenciesAsync的冷卻秒數(載入卡住時重新呼叫下載)
        void Awake() {
            BG.SetActive(false);
            ShowDownloadUI(false);
        }
        public static AddressableManage CreateNewAddressableManage() {
            if (Instance != null) {
            } else {
                GameObject prefab = Resources.Load<GameObject>("Prefabs/Common/AddressableManage");
                GameObject go = Instantiate(prefab);
                go.name = "AddressableManage";
                Instance = go.GetComponent<AddressableManage>();
                DontDestroyOnLoad(Instance.gameObject);
            }
            return Instance;
        }
        IEnumerator ClearAllCache(Action _cb) {
            /*沒辦法清 常常會報錯 但偶爾又不會
            AsyncOperationHandle handler = Addressables.ClearDependencyCacheAsync(Keys, false);
            yield return handler;
            Addressables.Release(handler);
            */
            yield return null;
            //Addressables.ClearResourceLocators();
            //AssetBundle.UnloadAllAssetBundles(true);
            if (Caching.ClearCache()) {
                DebugLogger.Log("Successfully cleaned the cache");
                _cb?.Invoke();
            } else {
                DebugLogger.Log("Cache is being used");
                _cb?.Invoke();
            }

            //ProgressImg.fillAmount = 0;
            //顯示載入進度文字
            ProgressText.text = StringData.GetUIString("ReDownload");
            DebugLogger.Log("重新載入中....................");
        }
        Coroutine Downloader;
        public void StartLoadAsset(Action _action) {
            BG.SetActive(false);
            DebugLogger.Log("<color=#008080>[Addressables] LoadAsset-Start</color>");
            Keys.RemoveAll(a => a == "");
            PopupUI_Local.ShowLoading(StringData.GetUIString("AddressableLoading"));
            FinishedAction = _action;
            //StartCoroutine(ClearAllAssetCoro(OnClearCatchCB));//要測試輕快取用這個(測試用)
            Downloader = StartCoroutine(LoadAssets());//不輕快取用這個(正式版)
        }
        void OnClearCatchCB() {
            Downloader = StartCoroutine(LoadAssets());
        }
        public void ReDownload() {
            if (Downloader != null)
                StopCoroutine(Downloader);
            StartCoroutine(ClearAllCache(OnClearCatchCB));
        }
        IEnumerator WaitForCheckingBundle() {
            yield return new WaitForSeconds(20);
            if (CheckInternetCoroutine != null)
                StopCoroutine(CheckInternetCoroutine);
            PopupUI_Local.ShowClickCancel(StringData.GetUIString("NoInternetShutDown"), () => {
                Application.Quit();
            });
        }

        IEnumerator LoadAssets() {
            PopupUI_Local.HideLoading();//開始抓到bundle包就取消loading
            yield return new WaitForSeconds(0.1f);

            //等待20秒若時間到還沒載到資源包需求內容就當網路錯誤
            if (CheckInternetCoroutine != null)
                StopCoroutine(CheckInternetCoroutine);
            CheckInternetCoroutine = StartCoroutine(WaitForCheckingBundle());
            ////初始化(好像不需要 會占用讀取時間)
            //AsyncOperationHandle<IResourceLocator> initializeAsyncHandle = Addressables.InitializeAsync();
            //yield return initializeAsyncHandle;
            //取Bundle包大小
            AsyncOperationHandle<long> getDownloadSize = Addressables.GetDownloadSizeAsync(Keys);
            yield return getDownloadSize;
            long totalSize = getDownloadSize.Result;
            DebugLogger.LogFormat("<color=#008080>[Addressables] LoadAsset-TotalSize={0}</color>", MyMath.BytesToMB(totalSize).ToString("0.00"));

            //已經抓到資料就取消Coroutine
            if (CheckInternetCoroutine != null)
                StopCoroutine(CheckInternetCoroutine);

            if (totalSize > 0) {//有要下載跳訊息
                string downloadStr = string.Format(StringData.GetUIString("StartDownloadAsset"), MyMath.BytesToMB(totalSize).ToString("0.00"));
                PopupUI_Local.ShowClickCancel(downloadStr, () => {
                    //顯示下載條
                    ShowDownloadUI(true);
                    StartCoroutine(Loading(totalSize));
                });
            } else {//沒需要下載就直接跳到完成
                OnFinishedDownload();
                yield break;
            }

        }
        IEnumerator Loading(long _totalSize) {
            //開始下載
            AsyncOperationHandle curDownloading = new AsyncOperationHandle();
            curDownloading = Addressables.DownloadDependenciesAsync(Keys, Addressables.MergeMode.Union);
            bool downloading = true;
            float tryReDownloadCD = TryReDownloadCD;//嘗試重新call Addressables.DownloadDependenciesAsync的冷卻秒數(載入卡住時重新呼叫下載)
            while (downloading) {
                float curDownloadPercent = curDownloading.GetDownloadStatus().Percent;
                long curDownloadSize = (long)(curDownloadPercent * _totalSize);

                //顯示載入進度與文字
                ProgressImg.fillAmount = curDownloadPercent;
                ProgressText.text = string.Format(StringData.GetUIString("AssetUpdating"), MyMath.BytesToMB(curDownloadSize).ToString("0.00"), MyMath.BytesToMB(_totalSize).ToString("0.00"));
                //完成後跳出迴圈
                if (curDownloading.GetDownloadStatus().IsDone)
                    downloading = false;
                yield return new WaitForSeconds(0.1f);
                tryReDownloadCD -= 0.1f;
                if (tryReDownloadCD <= 0) {
                    tryReDownloadCD = TryReDownloadCD;
                    curDownloading = Addressables.DownloadDependenciesAsync(Keys, Addressables.MergeMode.Union);
                }

            }
            OnFinishedDownload();
        }

        void OnFinishedDownload() {
            FinishedAction?.Invoke();
            MyScene scene = MyEnum.ParseEnum<MyScene>(SceneManager.GetActiveScene().name);
            ShowDownloadUI(false);
            DebugLogger.Log("<color=#008080>[Addressables] LoadAsset-Finished</color>");


        }
        public static void PreLoadToMemory(Action _ac = null) {
            DateTime now = DateTime.Now;
            DebugLogger.LogErrorFormat("開始下載MaJam資源圖");
            //初始化UI
            Addressables.LoadAssetsAsync<Texture>("MaJam", null).Completed += handle => {
                DebugLogger.LogErrorFormat("載入MaJam花費: {0}秒", (DateTime.Now - now).TotalSeconds);
                _ac?.Invoke();
            };
        }
        public void ShowDownloadUI(bool _show) {
            DownloadGO.gameObject.SetActive(_show);
            if (_show) {
                ProgressImg.fillAmount = 0;
                ProgressText.text = StringData.GetUIString("Downloading");
            }
        }



        /// <summary>
        /// 傳入Addressable的key確認此Addressable是否存在
        /// </summary>
        public void CheckIfAddressableExist(List<string> _keys, Action<bool> _cb) {
            StartCoroutine(CheckIfAddressableExistCoroutine(_keys, _cb));
        }
        IEnumerator CheckIfAddressableExistCoroutine(List<string> _keys, Action<bool> _cb) {
            if (_keys == null || _keys.Count == 0) {
                _cb?.Invoke(false);
                yield break;
            }

            var locationHandle = Addressables.LoadResourceLocationsAsync(_keys, Addressables.MergeMode.Union);
            yield return locationHandle;
            if (locationHandle.Result.Count == 0) {//為0代表沒有此資源包
                _cb?.Invoke(false);
                yield break;
            }
            _cb?.Invoke(true);
        }

        /// <summary>
        /// 傳入Addressable的key取得下載大小(mb)(用於App玩到一半才有載入需求的資源)
        /// </summary>
        public void GetDownloadAddressableSize(List<string> _keys, Action<float> _cb) {
            CheckIfAddressableExist(_keys, result => {
                if (result == true) {
                    StartCoroutine(GetDownloadAddressableSizeCoroutine(_keys, _cb));
                } else {
                    _cb?.Invoke(0);
                }
            });

        }
        IEnumerator GetDownloadAddressableSizeCoroutine(List<string> _keys, Action<float> _cb) {
            if (_keys == null && _keys.Count == 0) {
                _cb?.Invoke(0);
                yield break;
            }

            //取Bundle包大小
            AsyncOperationHandle<long> getDownloadSize = Addressables.GetDownloadSizeAsync(_keys);
            yield return getDownloadSize;
            long totalSize = getDownloadSize.Result;
            float mb = MyMath.BytesToMB(totalSize);
            _cb?.Invoke(mb);
        }

        /// <summary>
        /// 傳入Addressable的key目標下載大小(mb)(用於App玩到一半才有載入需求的資源)
        /// </summary>
        public void DownloadAddressable(List<string> _keys, bool _showBG, Action<bool> _cb) {
            CheckIfAddressableExist(_keys, result => {
                if (result == true) {
                    StartCoroutine(DownloadAddressableCheck(_keys, _showBG, _cb));
                } else {
                    _cb?.Invoke(false);
                }
            });
        }
        IEnumerator DownloadAddressableCheck(List<string> _keys, bool _showBG, Action<bool> _cb) {
            if (_keys == null || _keys.Count == 0) {
                _cb?.Invoke(false);
                yield break;
            }

            //取Bundle包大小
            AsyncOperationHandle<long> getDownloadSize = Addressables.GetDownloadSizeAsync(_keys);
            yield return getDownloadSize;


            long totalSize = getDownloadSize.Result;
            DebugLogger.Log("Download TotalSize=" + totalSize);
            if (totalSize > 0) {//有要下載跳訊息
                string downloadStr = string.Format(StringData.GetUIString("StartDownloadAsset"), MyMath.BytesToMB(totalSize).ToString("0.00"));
                //顯示下載條
                ShowDownloadUI(true);
                StartCoroutine(DownloadingAddressable(_keys, totalSize, _showBG, _cb));
                /*
                PopupUI.ShowConfirmCancel(downloadStr, () => {

                }, () => {
                    _cb?.Invoke(false);//取消下載
                });
                */
            } else {//沒需要下載就直接跳到完成
                _cb?.Invoke(true);
                yield break;
            }

        }
        IEnumerator DownloadingAddressable(List<string> _keys, long _totalSize, bool _showBG, Action<bool> _cb) {
            float tryReDownloadCD = TryReDownloadCD;//嘗試重新call Addressables.DownloadDependenciesAsync的冷卻秒數(載入卡住時重新呼叫下載)
            AsyncOperationHandle curDownloading = new AsyncOperationHandle();
            curDownloading = Addressables.DownloadDependenciesAsync(_keys, Addressables.MergeMode.Union);
            bool downloading = true;
            BG.SetActive(_showBG);
            while (downloading) {

                float curDownloadPercent = curDownloading.GetDownloadStatus().Percent;
                long curDownloadSize = (long)(curDownloadPercent * _totalSize);

                //顯示載入進度與文字
                ProgressImg.fillAmount = curDownloadPercent;
                ProgressText.text = string.Format(StringData.GetUIString("AssetUpdating"), MyMath.BytesToMB(curDownloadSize).ToString("0.00"), MyMath.BytesToMB(_totalSize).ToString("0.00"));
                //完成後跳出迴圈
                if (curDownloading.GetDownloadStatus().IsDone)
                    downloading = false;
                yield return new WaitForSeconds(0.1f);
                tryReDownloadCD -= 0.1f;
                if (tryReDownloadCD <= 0) {
                    tryReDownloadCD = TryReDownloadCD;
                    curDownloading = Addressables.DownloadDependenciesAsync(Keys, Addressables.MergeMode.Union);
                }
            }
            ShowDownloadUI(false);
            BG.SetActive(false);
            _cb?.Invoke(true);
        }
    }
}