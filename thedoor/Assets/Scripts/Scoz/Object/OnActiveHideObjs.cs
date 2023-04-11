using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scoz.Func
{
    public class OnActiveHideObjs : MonoBehaviour
    {
        public GameObject[] HideOnActiveObjs;
        public void Start()
        {
            for (int i = 0; i < HideOnActiveObjs.Length; i++)
            {
                HideOnActiveObjs[i].gameObject.SetActive(false);
            }
        }
    }
}
