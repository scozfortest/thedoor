using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scoz.Func
{
    public class MyTimerT<T>
    {
        public float CurTimer { get; private set; }
        public float CircleTime { get; private set; }
        public bool StartRunTimer;
        public bool Loop;
        public int CurLoopTimes { get; private set; }
        public T Parameter;
        Action<T> TimeOutCBWithT;//結束時執行(帶參數T)
        Action<T> RunTimeFuncWithT;//每偵執行(帶參數T)


        public MyTimerT(float _circleTime, Action<T> _timeOutCBWithT, Action<T> _runTimeFuncWithT, bool _startRunTimer, bool _loop)
        {
            CurLoopTimes = 0;
            CircleTime = _circleTime;
            CurTimer = CircleTime;
            TimeOutCBWithT = _timeOutCBWithT;
            RunTimeFuncWithT = _runTimeFuncWithT;
            StartRunTimer = _startRunTimer;
            Loop = _loop;
            if (CircleTime == 0)
                DebugLogger.LogWarning(string.Format("{0}'s CircleTime of MyTimer is 0", TimeOutCBWithT.Method.Name));
        }
        /// <summary>
        /// 重置LoopTimes並重新倒數
        /// </summary>
        public void ResetTimer(bool _startTimer)
        {
            CurLoopTimes = 0;
            RestartCountDown();
            StartRunTimer = _startTimer;
        }
        /// <summary>
        /// 重新倒數
        /// </summary>
        public void RestartCountDown()
        {
            StartRunTimer = true;
            CurTimer = CircleTime;
        }
        /// <summary>
        /// 重新指定循環時間
        /// </summary>
        public void ResetCircleTime(float _CircleTime)
        {
            CircleTime = _CircleTime;
        }
        /// <summary>
        /// 指定目前的倒數時間
        /// </summary>
        public void SetCurTimer(float _time)
        {
            CurTimer = _time;
        }
        /// <summary>
        /// 指定目前的倒數百分比傳入0~1，1就是重新倒數0.5就是倒數到一半了
        /// </summary>
        public void SetCurTimerRatio(float _ratio)
        {
            if (_ratio > 1)
                _ratio = 1;
            else if (_ratio < 0)
                _ratio = 0;
            CurTimer = CircleTime * _ratio;
        }
        /// <summary>
        /// 取得目前倒數進度(百分比)
        /// </summary>
        public float GetCurTimerRatio()
        {
            return CurTimer / CircleTime;
        }
        public void RunTimer()
        {
            if (CircleTime == 0)
                return;
            if (!StartRunTimer)
                return;
            RunTimeFuncWithT?.Invoke(Parameter);
            if (CurTimer > 0)
                CurTimer -= Time.deltaTime;
            else
            {
                CurLoopTimes++;
                CurTimer = CircleTime;
                StartRunTimer = Loop;
                TimeOutCBWithT?.Invoke(Parameter);
            }
        }
    }
}
