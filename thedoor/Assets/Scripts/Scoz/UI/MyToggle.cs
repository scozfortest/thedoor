using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Scoz.Func {
    public class MyToggle : Toggle {

        public Graphic OffGraphic;

        protected override void Start() {
            base.Start();
            ValueChange(isOn);
            onValueChanged.AddListener(isOn => { ValueChange(isOn); });
        }

        public void ValueChange(bool _isOn) {
            OffGraphic.gameObject.SetActive(!_isOn);
        }
    }
}
