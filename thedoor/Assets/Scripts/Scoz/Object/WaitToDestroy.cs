using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scoz.Func
{

    public class WaitToDestroy : MonoBehaviour
    {
        public float DestroyAfterSeconds = 0;
        int CoroutineID = 0;
        private void Start()
        {
            if (CoroutineJob.Instance == null)
                return;
            if (DestroyAfterSeconds > 0)
            {
                CoroutineID = CoroutineJob.Instance.StartNewAction(() =>
                  {
                      if (gameObject != null)
                          Destroy(gameObject);
                  }, DestroyAfterSeconds);
            }
        }
        private void OnDisable()
        {
            if (gameObject != null)
            {
                CoroutineJob.Instance.StopCoroutine(CoroutineID);
                Destroy(gameObject);
            }
        }
    }
}