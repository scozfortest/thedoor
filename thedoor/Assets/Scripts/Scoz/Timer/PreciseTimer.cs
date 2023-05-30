using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;

namespace Scoz.Func {
    public class PreciseTimer {
        public float CurTimer;
        public float CircleTime { get; private set; }
        public bool StartRunTimer;
        public bool Loop;
        public object Obj;
        public delegate void MyDelegate();
        public delegate void MyParameterDelegate(object _obj);
        MyDelegate TimeOutFunc;
        MyDelegate RunTimeFunc;
        MyParameterDelegate TimeOutFuncWithObj;
        float DurationTime = 0;
        int MaxTriggerTimes = 0;
        int TriggerTimes = 0;


        public PreciseTimer(float _circleTime, float _durationTime, MyDelegate _timeOutFunc, bool _startRunTimer, bool _loop) {
            CircleTime = _circleTime;
            CurTimer = CircleTime;
            TimeOutFunc = _timeOutFunc;
            StartRunTimer = _startRunTimer;
            DurationTime = _durationTime;
            MaxTriggerTimes = Mathf.RoundToInt(DurationTime / CircleTime);
            Loop = _loop;
            TriggerTimes = 0;
            if (CircleTime == 0) {
                WriteLog.LogWarning(string.Format("{0}'s CircleTime of MyTimer is 0", _timeOutFunc.Method.Name));
            }
        }
        public PreciseTimer(float _circleTime, float _durationTime, MyDelegate _timeOutFunc, MyDelegate _runTimeFunc, bool _startRunTimer, bool _loop) {
            CircleTime = _circleTime;
            CurTimer = CircleTime;
            TimeOutFunc = _timeOutFunc;
            RunTimeFunc = _runTimeFunc;
            StartRunTimer = _startRunTimer;
            DurationTime = _durationTime;
            MaxTriggerTimes = (int)(DurationTime / CircleTime);
            Loop = _loop;
            TriggerTimes = 0;
            if (CircleTime == 0)
                WriteLog.LogWarning("CircleTime of MyTimer is 0");
        }
        public PreciseTimer(float _circleTime, float _durationTime, MyParameterDelegate _timeOutFunc, MyDelegate _runTimeFunc, bool _startRunTimer, bool _loop, object _obj) {
            CircleTime = _circleTime;
            CurTimer = CircleTime;
            TimeOutFuncWithObj = _timeOutFunc;
            RunTimeFunc = _runTimeFunc;
            StartRunTimer = _startRunTimer;
            DurationTime = _durationTime;
            MaxTriggerTimes = (int)(DurationTime / CircleTime);
            Loop = _loop;
            Obj = _obj;
            TriggerTimes = 0;
            if (CircleTime == 0) {
                WriteLog.LogWarning(string.Format("{0}'s CircleTime of MyTimer is 0", _timeOutFunc.Method.Name));
            }
        }
        public PreciseTimer(float _circleTime, float _durationTime, MyParameterDelegate _timeOutFunc, bool _startRunTimer, bool _loop, object _obj) {
            CircleTime = _circleTime;
            CurTimer = CircleTime;
            TimeOutFuncWithObj = _timeOutFunc;
            StartRunTimer = _startRunTimer;
            DurationTime = _durationTime;
            MaxTriggerTimes = (int)(DurationTime / CircleTime);
            Loop = _loop;
            Obj = _obj;
            TriggerTimes = 0;
            if (CircleTime == 0) {
                WriteLog.LogWarning(string.Format("{0}'s CircleTime of MyTimer is 0", _timeOutFunc.Method.Name));
            }
        }
        public void RestartCountDown() {
            CurTimer = CircleTime;
        }
        public void ResetCircleTime(float _CircleTime) {
            CircleTime = _CircleTime;
        }
        public void RunTimer(ref float _remainTime) {
            if (CircleTime == 0)
                return;
            if (!StartRunTimer)
                return;
            RunTimeFunc?.Invoke();
            if (CurTimer > 0)
                CurTimer -= Time.deltaTime;
            else {
                TriggerTimes++;
                CurTimer = CircleTime;
                if (TriggerTimes < MaxTriggerTimes)
                    _remainTime = DurationTime - (TriggerTimes * CircleTime);
                else
                    _remainTime = 0;
                //WriteLog.Log("_remainTime=" + _remainTime);
                StartRunTimer = Loop;
                if (TimeOutFunc != null)
                    TimeOutFunc();
                else
                    TimeOutFuncWithObj(Obj);
            }
        }
    }
}
