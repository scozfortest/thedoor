using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using UnityEngine.Networking;
using System.Text;
using System;

namespace TheDoor.Main {
    public class PostData {
        public string URL;
        public string BodyJson;
        public PostData(string _url, string _bodyJson) {
            URL = _url;
            BodyJson = _bodyJson;
        }
    }
    public class Poster : MonoBehaviour {


        public static IEnumerator Post(string _url, string _bodyJson, Action<object> _cb = null) {
            WriteLog.Log("SendPost");
            //string url = "https://asia-east1-lanlansbizarre-dev.cloudfunctions.net/AddData";
            //string jsonString = "{\"Col\":\"Dev\"}";
            var request = new UnityWebRequest(_url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(_bodyJson);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            _cb?.Invoke(request.result);
            WriteLog.Log("Status Code: " + request.responseCode);
            WriteLog.Log("Result:" + request.result);
        }
    }
}
