using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using System;
using System.Linq;

namespace TheDoor.Main {
    public class ScreenShotBtn : MonoBehaviour {

        static ScreenCaptureCam MyCam;
        private void Start() {
            if (MyCam == null)
                MyCam = Camera.main.GetComponent<ScreenCaptureCam>();
        }

        public static void SetScreenShotCamera(ScreenCaptureCam _cam) {
            if (_cam == null) {
                DebugLogger.LogError("傳入的的ScreenCaptureCam為null");
                return;
            }
            MyCam = _cam;
        }

        public void OnScreenShotClick() {
            if (MyCam == null) {
                DebugLogger.LogError("尚未設定要螢幕截圖的ScreenCaptureCam");
                return;
            }
            PopupUI.CallScreenEffect("Flash");
            string fileName = string.Format("{0}.jpg", GameManager.Instance.NowTime.ToScozTimeStr());
            MyCam.TakePicAndGetBytes(bytes => {
                //SpriteConvert.GetSprite(bytes);<------要將bytes轉sprite可以用這個Function
                DebugLogger.Log("螢幕截圖成功");
                NativeGallery.SaveImageToGallery(bytes, Application.productName, fileName, OnScreenShotCB);
            });
        }
        void OnScreenShotCB(bool _result, string _path) {
            if (_result) {
                DebugLogger.Log("截圖存入裝置成功");
            } else {
                DebugLogger.LogError("截圖存入裝置時失敗");
                PopupUI.ShowClickCancel("Error", null);
            }
        }
    }
}
