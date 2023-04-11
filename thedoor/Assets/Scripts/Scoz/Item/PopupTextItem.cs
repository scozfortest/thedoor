using Scoz.Func;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupTextItem : MonoBehaviour, IItem {
    [SerializeField]
    Text NameText = null;
    [SerializeField]
    Image Icon = null;
    [SerializeField]
    AnimationPlayer AniPlayer = null;

    public bool IsActive { get; set; }

    public void Init(string _text, PopupText.AniName _name) {
        NameText.text = _text;
        Icon.gameObject.SetActive(false);
        AniPlayer.Play(_name.ToString(), 0);
        IsActive = true;
    }
    public void Init(string _text, Sprite _icon, PopupText.AniName _name) {
        NameText.text = _text;
        Icon.gameObject.SetActive(true);
        Icon.sprite = _icon;
        AniPlayer.Play(_name.ToString(), 0);
        IsActive = true;
    }
    public void IsEndPlay() {
        IsActive = false;
        AniPlayer.Play("default", 0);
    }
}
