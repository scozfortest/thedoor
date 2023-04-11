using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Scoz.Func {
    public class ScreenCaptureCam : MonoBehaviour {
        public bool AlphaBackground = true;
        public Int2D ScrSize = new Int2D(1080, 2340);
        public Int2D PicSize = new Int2D(160, 160);
        Camera MyCam;
        Vector3 OriginalCamPos;
        float OriginalFOV;
        public float ScreenShotFOV = 20;
        public float ScreenShotPosY = 0.5f;

        public delegate void GetSprite(Sprite _sprite);
        public delegate void GetBytes(byte[] _bytes);

        bool IsInit = false;
        private void Start() {
            Init();
        }
        public void Init() {
            if (IsInit)
                return;
            IsInit = true;
            MyCam = GetComponent<Camera>();
            //MyCam.clearFlags = CameraClearFlags.SolidColor;
            MyCam.backgroundColor = Color.clear;
            OriginalCamPos = transform.localPosition;
            OriginalFOV = MyCam.fieldOfView;
        }
        void ScreenShotSet() {
            transform.localPosition = new Vector3(OriginalCamPos.x, ScreenShotPosY, OriginalCamPos.z);
            MyCam.fieldOfView = ScreenShotFOV;
        }
        void ResetCam() {
            transform.localPosition = OriginalCamPos;
            MyCam.fieldOfView = OriginalFOV;
        }
        public void Init(Color backgroundColor) {
            MyCam = GetComponent<Camera>();
            //MyCam.clearFlags = CameraClearFlags.SolidColor;
            MyCam.backgroundColor = backgroundColor;
        }
        public void TakePicAndGetSprite(GetSprite _func) {
            if (!IsInit)
                return;
            StartCoroutine(TakeSpriteShot(_func));
        }
        IEnumerator TakeSpriteShot(GetSprite _func) {
            yield return new WaitForEndOfFrame();
            _func(TakePicAndGetSprite());
        }
        public void TakePicAndGetBytes(GetBytes _func) {
            if (!IsInit)
                return;
            StartCoroutine(TakeSpriteShot_Bytes(_func));
        }
        IEnumerator TakeSpriteShot_Bytes(GetBytes _func) {
            yield return new WaitForEndOfFrame();
            ScreenShotSet();
            yield return new WaitForEndOfFrame();
            _func(TakePicAndGetBytes());
            ResetCam();
        }


        public byte[] TakePicAndGetBytes() {
            if (!IsInit)
                return null;
            Texture2D texture = GetTexture();
            var bytes = texture.EncodeToPNG();
            return bytes;
        }

        Sprite TakePicAndGetSprite() {
            Texture2D texture = GetTexture();
            Rect rect = new Rect(0, 0, PicSize.X, PicSize.Y);
            Sprite s = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
            return s;
        }
        Texture2D GetTexture() {
            Rect rect = new Rect(0, 0, ScrSize.X, ScrSize.Y);
            RenderTexture rendererTexture = new RenderTexture((int)rect.width, (int)rect.height, 0);
            RenderTexture savedRT = MyCam.targetTexture;
            MyCam.targetTexture = rendererTexture;
            MyCam.Render();
            RenderTexture.active = rendererTexture;

            Texture2D texture = null;
            if (AlphaBackground) {
                texture = new Texture2D(PicSize.X, PicSize.Y, TextureFormat.ARGB32, false);
            } else {
                texture = new Texture2D(PicSize.X, PicSize.Y, TextureFormat.RGB24, false);
            }
            texture.ReadPixels(rect, 0, 0);
            texture.Apply();
            RenderTexture.active = null;
            MyCam.targetTexture = savedRT;
            return texture;
        }
    }
}