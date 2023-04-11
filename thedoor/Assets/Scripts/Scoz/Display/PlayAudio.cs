using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.AddressableAssets;
namespace Scoz.Func {

    public class PlayAudio : MonoBehaviour {
        [System.Serializable]
        public struct PlayAudioData {
            public PlayMode Mode;
            public MyAudioType Type;
            public string Name;
            public AssetReference Clip;
            public bool Loop;
            [HideInInspector]
            public string ClipName;
        }
        public enum PlayMode {
            StartPlay,
            EnablePlay,
            DontAutoPlay,
        }

        public PlayAudioData[] AudioDatas;

        private void OnEnable() {
            for (int i = 0; i < AudioDatas.Length; i++) {
                if (AudioDatas[i].Mode == PlayMode.EnablePlay)
                    Play(i);
            }
        }
        private void Start() {
            for (int i = 0; i < AudioDatas.Length; i++) {
                if (AudioDatas[i].Mode == PlayMode.StartPlay)
                    Play(i);
            }
        }
        public void PlayByName(string _name) {
            for (int i = 0; i < AudioDatas.Length; i++) {
                if (AudioDatas[i].Name == _name && AudioDatas[i].Clip != null) {
                    Play(i);
                    break;
                }
            }
        }
        public void Play(int _index) {
            AudioPlayer.PlayAudioByAudioAsset(AudioDatas[_index].Type, AudioDatas[_index].Clip, AudioDatas[_index].Loop, 1, (clip, handle) => {
                AudioDatas[_index].ClipName = clip.name;
            });
        }
        public void PlaySoundByAudioClip(AssetReference _clip) {
            AudioPlayer.PlayAudioByAudioAsset(MyAudioType.Sound, _clip);
        }
        public void PlayMusicByAudioClip(AssetReference _clip) {
            AudioPlayer.PlayAudioByAudioAsset(MyAudioType.Music, _clip);
        }
        public void PlayVoiceByAudioClip(AssetReference _clip) {
            AudioPlayer.PlayAudioByAudioAsset(MyAudioType.Voice, _clip);
        }
        public void StopByName(string _name) {
            for (int i = 0; i < AudioDatas.Length; i++) {
                if (AudioDatas[i].Name == _name && AudioDatas[i].Clip != null) {
                    if (string.IsNullOrEmpty(AudioDatas[i].ClipName)) return;
                    switch (AudioDatas[i].Type) {
                        case MyAudioType.Sound:
                            AudioPlayer.StopSounds(AudioDatas[i].ClipName);
                            break;
                        case MyAudioType.Music:
                            AudioPlayer.StopMusics(AudioDatas[i].ClipName);
                            break;
                        case MyAudioType.Voice:
                            AudioPlayer.StopVoices(AudioDatas[i].ClipName);
                            break;
                    }
                    break;
                }
            }


        }
    }
}