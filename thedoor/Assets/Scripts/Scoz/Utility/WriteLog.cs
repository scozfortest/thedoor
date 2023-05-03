using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Scoz.Func {
    public static class WriteLog {

        public enum LogType {
            Adventure,
            Battle,
        }
        public static Dictionary<LogType, string> LocColorCodes = new Dictionary<LogType, string>() {
            { LogType.Adventure,"db77c9"},
            { LogType.Battle,"dbdb77"},
        };



#if UNITY_EDITOR

        public delegate void LogDelegate(object logMsg, UnityEngine.Object context = null);
        public static void DoNothing(object logMsg, UnityEngine.Object context = null) { }
        public static void DoNothing(UnityEngine.Object context, string format, params object[] args) { }
        public static void DoNothing(string format, params object[] args) { }

        public static LogDelegate Log { get { return UnityEngine.Debug.Log; } }
        public static LogDelegate LogError { get { return UnityEngine.Debug.LogError; } }
        public static LogDelegate LogWarning { get { return UnityEngine.Debug.LogWarning; } }
        public static void LogColor(string format, LogType _type) {
            UnityEngine.Debug.Log(TextManager.GetColorText(format, LocColorCodes[_type]));
        }
        public static void LogColorFormat(string format, LogType _type, params object[] args) {
            UnityEngine.Debug.LogFormat(TextManager.GetColorText(format, LocColorCodes[_type]), args);
        }
        public static void LogFormat(string format, params object[] args) {
            UnityEngine.Debug.LogFormat(format, args);
        }
        public static void LogFormat(UnityEngine.Object context, string format, params object[] args) {
            UnityEngine.Debug.LogFormat(context, format, args);
        }
        public static void LogWarningFormat(string format, params object[] args) {
            UnityEngine.Debug.LogWarningFormat(format, args);
        }
        public static void LogWarningFormat(UnityEngine.Object context, string format, params object[] args) {
            UnityEngine.Debug.LogWarningFormat(context, format, args);
        }
        public static void LogErrorFormat(string format, params object[] args) {
            UnityEngine.Debug.LogErrorFormat(format, args);
        }
        public static void LogErrorFormat(UnityEngine.Object context, string format, params object[] args) {
            UnityEngine.Debug.LogErrorFormat(context, format, args);
        }
        public static void LogException(Exception exception) {
            UnityEngine.Debug.LogException(exception);
        }
        public static void LogException(Exception exception, UnityEngine.Object context) {
            UnityEngine.Debug.LogException(exception, context);
        }
        public static void Break() {
            UnityEngine.Debug.Break();
        }
#else

    [Conditional("DEBUG_LOG")]
	public static void Log(object logMsg)
	{
        //LogManager.Inst.AddLog(logMsg.ToString(),"", LogType.Log);
		UnityEngine.Debug.Log (logMsg);
	}

	[Conditional("DEBUG_LOG")]
    public static void Log(object logMsg, UnityEngine.Object context)
	{
		//LogManager.Inst.AddLog(logMsg.ToString(), "", LogType.Log);
		UnityEngine.Debug.Log (logMsg, context);
	}

	[Conditional("DEBUG_LOG")]
    public static void LogError(object logMsg)
	{
		//LogManager.Inst.AddLog(logMsg.ToString(), "", LogType.Error);
		UnityEngine.Debug.LogError (logMsg);
	}

	[Conditional("DEBUG_LOG")]
    public static void LogError(object logMsg, UnityEngine.Object context)
	{
		//LogManager.Inst.AddLog(logMsg.ToString(), "", LogType.Error);
		UnityEngine.Debug.LogError (logMsg, context);
	}

	[Conditional("DEBUG_LOG")]
    public static void LogWarning(object logMsg)
	{
		//LogManager.Inst.AddLog(logMsg.ToString(), "", LogType.Warning);
		UnityEngine.Debug.LogWarning (logMsg);
	}

	[Conditional("DEBUG_LOG")]
    public static void LogWarning(object logMsg, UnityEngine.Object context)
	{
		//LogManager.Inst.AddLog(logMsg.ToString(), "", LogType.Warning);
		UnityEngine.Debug.LogWarning (logMsg, context);
	}

    [Conditional("DEBUG_LOG")]
	public static void LogFormat(string format, params object[] args) 
	{
		UnityEngine.Debug.LogFormat(format, args);
	}

	[Conditional("DEBUG_LOG")]
	public static void LogFormat(UnityEngine.Object context, string format, params object[] args) 
	{
		UnityEngine.Debug.LogFormat(context, format, args);
    }

	[Conditional("DEBUG_LOG")]
	public static void LogWarningFormat(string format, params object[] args) 
	{
		UnityEngine.Debug.LogWarningFormat(format, args);
	}

	[Conditional("DEBUG_LOG")]
	public static void LogWarningFormat(UnityEngine.Object context, string format, params object[] args) 
	{
		UnityEngine.Debug.LogWarningFormat(context, format, args);
	}

	[Conditional("DEBUG_LOG")]
	public static void LogErrorFormat(string format, params object[] args) 
	{
		UnityEngine.Debug.LogErrorFormat(format, args);
	}
	
	[Conditional("DEBUG_LOG")]
	public static void LogErrorFormat(UnityEngine.Object context, string format, params object[] args) 
	{
		UnityEngine.Debug.LogErrorFormat(context, format, args);
	}

	[Conditional("DEBUG_LOG")]
	public static void LogException(Exception exception) 
	{
		UnityEngine.Debug.LogException(exception);
	}
	
	[Conditional("DEBUG_LOG")]
	public static void LogException(Exception exception, UnityEngine.Object context) 
	{
		UnityEngine.Debug.LogException(exception, context);
	}

	[Conditional("DEBUG_LOG")]
	public static void Break() 
	{
		UnityEngine.Debug.Break();
	}

#endif
    }
}