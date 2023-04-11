using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineTimer 
{
    public float CircleTime;
    public delegate void MyDelegate();
    MyDelegate TimeOutFunc;
    public CoroutineTimer(float _circleTime, MyDelegate _timeOutFunc)
    {
        CircleTime = _circleTime;
        TimeOutFunc = _timeOutFunc;
    }

    public IEnumerator Run()
    {
        yield return new WaitForSecondsRealtime(CircleTime);
        TimeOutFunc();
    }

}
