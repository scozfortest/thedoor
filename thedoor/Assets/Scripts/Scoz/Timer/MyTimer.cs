using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scoz.Func
{
    public class MyTimer
    {
        public float CurTimer { get; private set; }
        public float CircleTime { get; private set; }
        public bool StartRunTimer;
        public bool Loop;
        public int CurLoopTimes { get; private set; }
        Action TimeOutCB;//結束時執行
        Action RunFunc;//每偵執行


        public MyTimer(float _circleTime, Action _timeOutCB, bool _startRunTimer, bool _loop)
        {
            CurLoopTimes = 0;
            CircleTime = _circleTime;
            CurTimer = CircleTime;
            TimeOutCB = _timeOutCB;
            StartRunTimer = _startRunTimer;
            Loop = _loop;
            if (CircleTime == 0)
                WriteLog.LogWarning(string.Format("{0}'s CircleTime of MyTimer is 0", _timeOutCB.Method.Name));
        }
        public MyTimer(float _circleTime, Action _timeOutCB, Action _runTimeFunc, bool _startRunTimer, bool _loop)
        {
            CurLoopTimes = 0;
            CircleTime = _circleTime;
            CurTimer = CircleTime;
            TimeOutCB = _timeOutCB;
            RunFunc = _runTimeFunc;
            StartRunTimer = _startRunTimer;
            Loop = _loop;
            if (CircleTime == 0)
                WriteLog.LogWarning(string.Format("{0}'s CircleTime of MyTimer is 0", _timeOutCB.Method.Name));
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
            RunFunc?.Invoke();
            if (CurTimer > 0)
                CurTimer -= Time.deltaTime;
            else
            {
                CurLoopTimes++;
                CurTimer = CircleTime;
                StartRunTimer = Loop;
                TimeOutCB?.Invoke();
            }
        }
    }
}
