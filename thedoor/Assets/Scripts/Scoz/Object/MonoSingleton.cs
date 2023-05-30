/*
 *	Singleton
 *
 *	Any script inherits from this class become singleton
 *
 *	結構：
 *		MonoSingletonA<T> : BaseMonoSingleton<T> : BaseMonoSingleton
 *		MonoSingletonB<T> : BaseMonoSingleton<T> : BaseMonoSingleton
 *
 */

using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using Scoz.Func;

/// <summary>
/// A 類 Singleton(沒有實例時會自動建立)
/// </summary>
public abstract class MonoSingletonA<T> : BaseMonoSingleton<T> where T : MonoSingletonA<T> {
    /// <summary> Returns the instance of this singleton. </summary>
    public static T Inst {
        get {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                isApplicationQuit = false;
                return null;
            }

            if (isApplicationQuit)
                return null;
#endif

            if (_inst == null) {
                //				setInst((T) FindObjectOfType(typeof(T)));

                if (_inst == null) {
                    GameObject go = new GameObject(typeof(T).Name);
                    setInst(go.AddComponent<T>());
                }
            }

            return _inst;
        }
    }
}

/// <summary>
/// B 類 Singleton(沒有實例時不會自動建立)
/// </summary>
public abstract class MonoSingletonB<T> : BaseMonoSingleton<T> where T : MonoSingletonB<T> {
    /// <summary> Returns the instance of this singleton. </summary>
    public static T Inst {
        get {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                isApplicationQuit = false;
                return null;
            }

            if (isApplicationQuit)
                return null;
#endif

            if (_inst == null)
                setInst((T)FindObjectOfType(typeof(T)));

            return _inst;
        }
    }
}


/// <summary>
/// C 類 Singleton(沒有實例時不會自動用指定方式建立)
/// 類別須建立一個靜態方法 private static T CreateInst() {}
/// </summary>
public abstract class MonoSingletonC<T> : BaseMonoSingleton<T> where T : MonoSingletonC<T> {
    /// <summary> Returns the instance of this singleton. </summary>
    public static T Inst {
        get {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                isApplicationQuit = false;
                return null;
            }

            if (isApplicationQuit)
                return null;
#endif

            if (_inst == null) {
                MethodInfo mInfo = typeof(T).GetMethod("CreateInst", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                if (mInfo == null) {
                    WriteLog.LogError("無法取得 " + typeof(T).Name + " 單例物件建立的建立方法：CreateInst()");
                    return null;
                }
                T newInst = mInfo.Invoke(null, null) as T;
                if (_inst == null) {
                    WriteLog.LogError("自動建立 " + typeof(T).Name + " 單例物件失敗！");
                    return null;
                }
                setInst(newInst);
            }

            return _inst;
        }
    }
}


/// <summary>
/// 基礎 Singleton
/// </summary>
public abstract class BaseMonoSingleton<T> : BaseMonoSingleton where T : BaseMonoSingleton<T> {
    protected static T _inst { get; private set; }
    /// <summary>
    /// 設定靜態實例，傳回指定的實例是否因重複而被移除
    /// </summary>
    protected static bool setInst(T inst) {
        if (_inst != null) {
            if (_inst != inst) {
                //				WriteLog.LogError("重複出現多個 AMonoSingleton<" + typeof(T).Name + "> 實例！以第一個實例為主！", inst.CacheGameObject);
                if (_inst.destroyWhenRepeated) {
                    Destroy(inst);
                    Destroy(inst.CacheGameObject);
                }
            }
            return true;
        }
        _inst = inst;

        BaseMonoSingleton<T> instT = _inst as BaseMonoSingleton<T>;
        if (instT != null) {
            if (instT.dontDestroyOnLoad)
                DontDestroyOnLoad(_inst.CacheGameObject);
            instT.OnReferenceInst();
        }
        return false;
    }

    public static bool HadInstance {
        get {
            if (isApplicationQuit)
                return false;
            if (_inst == null)
                return false;
            string str = _inst.ToString();
            bool val = (_inst != null) || str != "null";
            return val;
        }
    }

    protected virtual void OnReferenceInst() { }

    private void Awake() {
        if (setInst(this as T))
            return;
        onAwake();
    }

    protected virtual void OnDestroy() {
        if (_inst == this)
            _inst = null;
    }


    protected virtual void onAwake() {

    }
}



/// <summary>
/// 基礎 Singleton
/// </summary>
public abstract class BaseMonoSingleton : CacheGobjTranComponent {
    protected static bool isApplicationQuit = false;

    /// <summary>換場景時不要移除</summary>
    protected abstract bool dontDestroyOnLoad { get; }
    /// <summary>重複出現時，是否移除後來的物件</summary>
    protected abstract bool destroyWhenRepeated { get; }

    // ios does not dispatch
    protected virtual void OnApplicationQuit() {
        if (!isApplicationQuit) {
            WriteLog.Log("----------------結束運行----------------");
        }
        isApplicationQuit = true;
    }
}