using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.AddressableAssets;
using System;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Scoz.Func {
    public partial class AudioPlayer : MonoBehaviour {
        public static AudioPlayer Instance;
        public static bool IsInit;
        static List<AudioSource> SoundList;
        static List<AudioSource> MusicList;
        static List<AudioSource> VoiceList;
        static GameObject MySoundObject;
        static GameObject MyVoiceObject;
        static GameObject MyMusicObject;

        static AudioSource CurPlayAudio;


        static AudioMixer MyAudioMixer = null;
        static string MusicGroup = "Music";
        static string SoundGroup = "Sound";
        static string VoiceGroup = "Voice";
        static int MaxAttenuation = -30;//最高降低音量為把Volume調整為-30，低於-30就會設定不開音量
        public static float MusicVolumeRatio { get; private set; }
        public static float SoundVolumeRatio { get; private set; }
        public static float VoiceVolumeRatio { get; private set; }


        public static AudioPlayer CreateNewAudioPlayer() {
            if (Instance != null) {
                //WriteLog.Log("AudioPlayer之前已經被建立了");
            } else {
                GameObject prefab = Resources.Load<GameObject>("Prefabs/Common/AudioPlayer");
                GameObject go = Instantiate(prefab);
                go.name = "AudioPlayer";
                Instance = go.GetComponent<AudioPlayer>();
                Instance.Init();

            }
            return Instance;
        }
        public void Init() {
            if (IsInit)
                return;
            IsInit = true;
            MyAudioMixer = Resources.Load<AudioMixer>("Prefabs/Common/MyAudioMixer");
            DontDestroyOnLoad(gameObject);
            SoundList = new List<AudioSource>();
            MusicList = new List<AudioSource>();
            VoiceList = new List<AudioSource>();
            MySoundObject = new GameObject("SoundPlayer");
            MyMusicObject = new GameObject("MusicPlayer");
            MyVoiceObject = new GameObject("VoicePlayer");
            MySoundObject.transform.SetParent(Instance.transform);
            MyMusicObject.transform.SetParent(Instance.transform);
            MyVoiceObject.transform.SetParent(Instance.transform);
            AudioSource mySound = MySoundObject.AddComponent<AudioSource>();
            mySound.outputAudioMixerGroup = MyAudioMixer.FindMatchingGroups(SoundGroup)[0];
            AudioSource myMusic = MyMusicObject.AddComponent<AudioSource>();
            myMusic.outputAudioMixerGroup = MyAudioMixer.FindMatchingGroups(MusicGroup)[0];
            AudioSource myVoice = MyVoiceObject.AddComponent<AudioSource>();
            myVoice.outputAudioMixerGroup = MyAudioMixer.FindMatchingGroups(VoiceGroup)[0];
            SoundList.Add(mySound);
            MusicList.Add(myMusic);
            VoiceList.Add(myVoice);
        }
        /// <summary>
        /// 傳入0~1
        /// </summary>
        public static void SetMusicVolume(float _volume) {
            if (MyAudioMixer == null)
                return;
            if (_volume > 1)
                _volume = 1;
            else if (_volume < 0)
                _volume = 0;
            float attenuation = (MaxAttenuation * (1 - _volume));
            if (attenuation <= MaxAttenuation) {
                attenuation = -80;
            } else {
                if (attenuation > 0)
                    attenuation = 0;
            }
            _volume = MyMath.Round(_volume, 2);
            MusicVolumeRatio = _volume;
            MyAudioMixer.SetFloat("MusicVol", attenuation);
        }
        /// <summary>
        /// 傳入0~1
        /// </summary>
        public static void SetSoundVolume(float _volume) {
            if (MyAudioMixer == null)
                return;
            if (_volume > 1)
                _volume = 1;
            else if (_volume < 0)
                _volume = 0;
            float attenuation = (MaxAttenuation * (1 - _volume));
            if (attenuation <= MaxAttenuation) {
                attenuation = -80;
            } else {
                if (attenuation > 0)
                    attenuation = 0;
            }

            _volume = MyMath.Round(_volume, 2);
            SoundVolumeRatio = _volume;
            MyAudioMixer.SetFloat("SoundVol", attenuation);
            MyAudioMixer.SetFloat("BallVol", attenuation);
        }
        public static void SetVoiceVolume(float _volume) {
            if (MyAudioMixer == null)
                return;
            if (_volume > 1)
                _volume = 1;
            else if (_volume < 0)
                _volume = 0;
            float attenuation = (MaxAttenuation * (1 - _volume));
            if (attenuation <= MaxAttenuation) {
                attenuation = -80;
            } else {
                if (attenuation > 0)
                    attenuation = 0;
            }
            _volume = MyMath.Round(_volume, 2);
            VoiceVolumeRatio = _volume;
            MyAudioMixer.SetFloat("VoiceVol", attenuation);
            MyAudioMixer.SetFloat("PachinkoVol", attenuation);
        }
        public static void StopAllSounds_static() {
            for (int i = 0; i < SoundList.Count; i++) {
                SoundList[i].Stop();
            }
        }

        public static void StopSounds(params string[] _soundNames) {
            for (int i = 0; i < SoundList.Count; i++) {
                if (SoundList[i].isPlaying) {
                    for (int j = 0; j < _soundNames.Length; j++) {
                        if (SoundList[i].clip.name == _soundNames[j]) {
                            SoundList[i].Stop();
                        }
                    }
                }
            }
        }
        public static bool IncludingNameSound(params string[] _soundNames) {
            for (int i = 0; i < SoundList.Count; i++) {
                if (SoundList[i].isPlaying) {
                    for (int j = 0; j < _soundNames.Length; j++) {
                        if (SoundList[i].clip.name.Contains(_soundNames[j])) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public static void StopIncludingNameSounds(params string[] _soundNames) {
            for (int i = 0; i < SoundList.Count; i++) {
                if (SoundList[i].isPlaying) {
                    for (int j = 0; j < _soundNames.Length; j++) {
                        if (SoundList[i].clip != null) {
                            if (SoundList[i].clip.name.Contains(_soundNames[j]))
                                SoundList[i].Stop();
                        }

                    }
                }
            }
        }
        public static bool IncludingNameVoice(params string[] _VoiceNames) {
            for (int i = 0; i < VoiceList.Count; i++) {
                if (VoiceList[i].isPlaying) {
                    for (int j = 0; j < _VoiceNames.Length; j++) {
                        if (VoiceList[i].clip.name.Contains(_VoiceNames[j])) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public static void StopIncludingNameVoices(params string[] _voiceNames) {
            for (int i = 0; i < VoiceList.Count; i++) {
                if (VoiceList[i].isPlaying) {
                    for (int j = 0; j < _voiceNames.Length; j++) {
                        if (_voiceNames[j] == null)
                            continue;
                        if (VoiceList[i].clip.name.Contains(_voiceNames[j])) {
                            VoiceList[i].Stop();
                        }
                    }
                }
            }
        }
        public static void StopAllVoices_static() {
            for (int i = 0; i < VoiceList.Count; i++) {
                VoiceList[i].Stop();
            }
        }
        public static void StopVoices(params string[] _voiceNames) {
            for (int i = 0; i < VoiceList.Count; i++) {
                if (VoiceList[i].isPlaying) {
                    for (int j = 0; j < _voiceNames.Length; j++) {
                        if (VoiceList[i].clip.name == _voiceNames[j]) {
                            VoiceList[i].Stop();
                        }
                    }
                }
            }
        }
        public static void StopAllMusic_static() {
            for (int i = 0; i < MusicList.Count; i++) {
                MusicList[i].Stop();
            }
        }
        public static void StopMusics(params string[] _musicNames) {
            for (int i = 0; i < MusicList.Count; i++) {
                if (MusicList[i].isPlaying) {
                    for (int j = 0; j < _musicNames.Length; j++) {
                        if (MusicList[i].clip.name == _musicNames[j]) {
                            MusicList[i].Stop();
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 某音樂是否還在播放中
        /// </summary>
        public static bool IncludingNameMusics(params string[] _musicNames) {
            for (int i = 0; i < MusicList.Count; i++) {
                if (MusicList[i].isPlaying) {
                    for (int j = 0; j < _musicNames.Length; j++) {
                        if (MusicList[i].clip.name.Contains(_musicNames[j])) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        static AudioSource GetApplicableSoundSource() {
            CurPlayAudio = null;
            for (int i = 0; i < SoundList.Count; i++) {
                if (!SoundList[i].isPlaying) {
                    CurPlayAudio = SoundList[i];
                    return CurPlayAudio;
                }
            }
            return CurPlayAudio;
        }
        static AudioSource GetNewSoundSource() {
            CurPlayAudio = MySoundObject.AddComponent<AudioSource>();
            CurPlayAudio.outputAudioMixerGroup = MyAudioMixer.FindMatchingGroups(SoundGroup)[0];
            SoundList.Add(CurPlayAudio);
            return CurPlayAudio;
        }
        static AudioSource GetApplicableVoiceSource() {
            CurPlayAudio = null;
            for (int i = 0; i < VoiceList.Count; i++) {
                if (!VoiceList[i].isPlaying) {
                    CurPlayAudio = VoiceList[i];
                    return CurPlayAudio;
                }
            }
            return CurPlayAudio;
        }
        static AudioSource GetNewVoiceSource() {
            CurPlayAudio = MyVoiceObject.AddComponent<AudioSource>();
            CurPlayAudio.outputAudioMixerGroup = MyAudioMixer.FindMatchingGroups(VoiceGroup)[0];
            VoiceList.Add(CurPlayAudio);
            return CurPlayAudio;
        }
        static AudioSource GetApplicableMusicSource() {
            CurPlayAudio = null;
            for (int i = 0; i < MusicList.Count; i++) {
                if (!MusicList[i].isPlaying) {
                    CurPlayAudio = MusicList[i];
                    return CurPlayAudio;
                }
            }
            return CurPlayAudio;
        }
        static AudioSource GetNewMusicSource() {
            CurPlayAudio = MyMusicObject.AddComponent<AudioSource>();
            CurPlayAudio.outputAudioMixerGroup = MyAudioMixer.FindMatchingGroups(MusicGroup)[0];
            MusicList.Add(CurPlayAudio);
            return CurPlayAudio;
        }

        static void PlayAudio(MyAudioType _type, AudioSource _source, AudioClip _clip) {
            _source.clip = _clip;
            CurPlayAudio.Play(0);
            switch (_type) {
                case MyAudioType.Music:
                    if (!MusicList.Contains(CurPlayAudio)) {
                        MusicList.Add(CurPlayAudio);
                    }
                    break;
                case MyAudioType.Sound:
                    if (!SoundList.Contains(CurPlayAudio)) {
                        SoundList.Add(CurPlayAudio);
                    }
                    break;
                case MyAudioType.Voice:
                    if (!VoiceList.Contains(CurPlayAudio)) {
                        VoiceList.Add(CurPlayAudio);
                    }

                    break;
            }
        }
        /// <summary>
        /// Call back回傳audio clip name有需要的話可以用來Stop音檔
        /// </summary>
        public static void PlayAudioByPath(MyAudioType _type, string _path, Action<string> _cb = null, bool _loop = false, float _pitch = 1) {
            if (!IsInit)
                return;
            if (string.IsNullOrEmpty(_path))
                return;
            AddressablesLoader.GetAudio(_type, _path, audio => {
                if (audio != null) {
                    switch (_type) {
                        case MyAudioType.Music:
                            if (GetApplicableMusicSource() == null)
                                GetNewMusicSource();
                            break;
                        case MyAudioType.Sound:
                            if (GetApplicableSoundSource() == null)
                                GetNewSoundSource();
                            break;
                        case MyAudioType.Voice:
                            if (GetApplicableVoiceSource() == null)
                                GetNewVoiceSource();
                            break;
                    }
                    PlayAudio(_type, CurPlayAudio, audio);
                    CurPlayAudio.pitch = _pitch;
                    CurPlayAudio.loop = _loop;
                    _cb?.Invoke(audio.name);
                } else {
                    WriteLog.LogWarning("不存在的音檔:" + _path);
                    return;
                }
            });
        }

        public static void PlayAudioByAudioAsset(MyAudioType _type, AssetReference _clip, bool _loop = false, float _pitch = 1, Action<AudioClip, AsyncOperationHandle> _cb = null) {
            if (!IsInit)
                return;
            if (_clip == null) {
                WriteLog.LogWarning("不存在的音檔");
                return;
            }
            MyAudioType type = _type;
            bool loop = _loop;
            float pitch = _pitch;
            AddressablesLoader.GetAudioClipByRef(_clip, (clip, handle) => {
                PlayAudioByAudioClip(type, clip, loop, pitch);
                _cb?.Invoke(clip, handle);
            });
        }
        public static void PlayAudioByAudioClipAsset(MyAudioType _type, AssetReference _clip, Action<AudioSource> _cb, bool _loop = false) {
            if (!IsInit)
                return;
            if (_clip == null) {
                WriteLog.LogWarning("不存在的音檔");
                return;
            }
            MyAudioType type = _type;
            bool loop = _loop;
            AddressablesLoader.GetAudioClipByRef(_clip, (clip, handle) => {
                AudioSource audioSource = PlayAudioByAudioClip(type, clip, loop);
                _cb?.Invoke(audioSource);
            });
        }
        public static AudioSource PlayAudioByAudioClip(MyAudioType _type, AudioClip _clip, bool _loop = false, float _pitch = 1) {
            if (!IsInit)
                return null;
            if (_clip == null) {
                WriteLog.LogWarning("不存在的音檔");
                return null;
            }
            switch (_type) {
                case MyAudioType.Music:
                    if (GetApplicableMusicSource() == null)
                        GetNewMusicSource();
                    break;
                case MyAudioType.Sound:
                    if (GetApplicableSoundSource() == null)
                        GetNewSoundSource();
                    break;
                case MyAudioType.Voice:
                    if (GetApplicableVoiceSource() == null)
                        GetNewVoiceSource();
                    break;
            }
            PlayAudio(_type, CurPlayAudio, _clip);
            CurPlayAudio.loop = _loop;
            CurPlayAudio.pitch = _pitch;
            return CurPlayAudio;
        }

        public static void SetMusicPitch(float _pitchValue) {
            if (GetApplicableMusicSource() == null)
                GetNewMusicSource();
            foreach (AudioSource audioSource in MusicList) {
                audioSource.pitch = _pitchValue;
            }
        }
        /*Resources版本
        public static void PlayAudioByString_static(AudioType _type, string _name)
        {
            switch (_type)
            {
                case AudioType.Music:
                    if (GetApplicableMusicSource() == null)
                        GetNewMusicSource();
                    break;
                case AudioType.Sound:
                    if (GetApplicableSoundSource() == null)
                        GetNewSoundSource();
                    break;
                case AudioType.Voice:
                    if (GetApplicableVoiceSource() == null)
                        GetNewVoiceSource();
                    break;
            }

            AudioClip ac = Resources.Load<AudioClip>(string.Format("{0}/{1}", _type, _name));
            if (ac != null)
            {
                PlayAudio(_type, CurPlayAudio, ac);
            }
            else
            {
                WriteLog.LogWarning("不存在的音檔:" + string.Format("{0}/{1}", _type, _name));
            }
        }
        public void PlaySoundByString(string _name)
        {
            PlayAudioByString_static(AudioType.Sound, _name);
        }
        public void PlayMusicByString(string _name)
        {
            StopAllMusic_static();
            PlayAudioByString_static(AudioType.Music, _name);
        }
        public void PlayVoiceByString(string _name)
        {
            PlayAudioByString_static(AudioType.Voice, _name);
        }
        */
        public void StopAllVoice() {
            StopAllVoices_static();
        }
        public void StopMusic() {
            StopAllMusic_static();
        }
    }
}
