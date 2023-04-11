using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scoz.Func
{
    public class SpriteConvert
    {
        public static byte[] GetByte(Sprite sp)
        {
            Texture2D temp = DuplicateTexture(sp.texture);
            byte[] photoByte = temp.EncodeToPNG();
            return photoByte;
        }
        public static byte[] GetByte(Texture2D _texture)
        {
            Texture2D texture = DuplicateTexture(_texture);
            byte[] photoByte = texture.EncodeToPNG();
            return photoByte;
        }
        public static Sprite GetSprite(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return null;
            Sprite sp = null;
            try
            {
                Texture2D texture = new Texture2D(10, 10);
                texture.LoadImage(bytes);
                sp = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
            catch (Exception e)
            {
                DebugLogger.LogWarning(e);
            }
            return sp;
        }
        /// <summary>
        /// 取得readable Texture
        /// </summary>
        public static Texture2D DuplicateTexture(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }
    }
}