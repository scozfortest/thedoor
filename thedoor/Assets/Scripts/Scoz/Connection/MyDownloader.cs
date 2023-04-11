using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Scoz.Func;
using Firebase.Auth;

namespace Scoz.Func
{
    public class MyDownloader : MonoBehaviour
    {
        public static MyDownloader Instance;
        static DateTime LastCallDownloadTime;
        static double MinInterval = 0.2f;//最短呼叫下載的間隔時間(秒)
        private void Start()
        {
            Instance = this;
        }
        public static void GetSpriteFromUrl_Coroutine(string _url, Action<Sprite> _cb)
        {
            if (Instance == null)
                DebugLogger.LogError("MyDownload尚未初始化");
            Instance.StartCoroutine(Instance.DownloadCoroutine(_url, _cb));
        }
        public IEnumerator DownloadCoroutine(string _url, Action<Sprite> _cb)
        {
            float waitTime = 0;
            if (LastCallDownloadTime!=null)
            {
                double diffSeconds = (DateTime.Now - LastCallDownloadTime).TotalSeconds;
                if(diffSeconds < MinInterval)
                {
                    waitTime = (float)(MinInterval - diffSeconds);
                }
            }
            LastCallDownloadTime = DateTime.Now.AddSeconds(waitTime);
            yield return new WaitForSeconds(waitTime);
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(_url))
            {
                request.downloadHandler = new DownloadHandlerTexture();
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError)
                    DebugLogger.LogError(request.error);
                else
                {                    
                    byte[] result = request.downloadHandler.data;
                    _cb?.Invoke(SpriteConvert.GetSprite(result));                    
                }
                //UnityEngine.Object.Destroy(DownloadHandlerTexture.GetContent(request));
                //request.Dispose();
            }
        }


        /*
        public static void GetSpriteFromUrl(string _url, Action<Sprite> _cb)
        {
            UnityWebRequest request = UnityWebRequest.Get(_url);
            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
            asyncOperation.completed += (asyncOperation) =>
            {
                byte[] result = request.downloadHandler.data;
                Sprite s = SpriteConvert.GetSprite(result);
                _cb?.Invoke(s);
                UnityEngine.Object.Destroy(DownloadHandlerTexture.GetContent(request));
                request.Dispose();
            };
        }
        */
    }
}