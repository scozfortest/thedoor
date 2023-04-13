using Scoz.Func;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheDoor.Main {
    public enum LinkType {
        None,
        LinkUI,
        LinkURL,
    }
    public class UIManager {

        public static void Link(LinkType _type, string _value) {
            switch (_type) {
                case LinkType.None:
                    break;
                case LinkType.LinkUI:
                    if (MyEnum.TryParseEnum(_value, out LinkUIType _ui)) {
                        //LobbyUI_backup.GetInstance<LobbyUI_backup>()?.JumpToUI(_ui);
                    }
                    break;
                case LinkType.LinkURL:
                    Application.OpenURL(_value);
                    break;
                default:
                    WriteLog.LogError("尚未定義的LinkType: " + _type);
                    break;
            }
        }
    }

}