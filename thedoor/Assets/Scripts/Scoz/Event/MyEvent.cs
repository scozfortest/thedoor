
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace Scoz.Func {
    public class MyEvent : MonoBehaviour {
        public UnityEvent Event;
        public void DoEvent() {
            Event.Invoke();
        }
        public void AddEvent(UnityAction _action) {
            Event.AddListener(_action);
        }
    }
}