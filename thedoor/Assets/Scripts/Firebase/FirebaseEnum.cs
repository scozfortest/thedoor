using System;
using System.Collections;
using System.Collections.Generic;
namespace TheDoor.Main {



    /// <summary>
    /// Firestore上的Collection
    /// </summary>
    public enum ColEnum {
        GameSetting,//遊戲設定
        Player,//玩家資料
        Item,//玩家道具
        History,//玩家紀錄
        Role,//玩家腳色
        Supply,//腳色道具
        Adventure,//腳色冒險
        Shop,//商城
        Purchase,//儲值商城
    }
    /// <summary>
    /// 玩家擁有個人資料共用欄位
    /// </summary>
    public enum OwnedEnum {
        UID,
        OwnerUID,
        CreateTime,
    }
    /// <summary>
    /// GameData-Setting中的Doc名稱
    /// 有加到這個Enum中，遊戲開始時會自動去取Firestore上GameData-Setting中對應的文件並對文件偵聽
    /// </summary>
    public enum GameDataDocEnum {
        DefaultPlayer,//玩家初始設定
        Timer,//計時器相關設定
        Version,//版本
        Adventure,//冒險設定
        ADReward,//主動觀看廣告設定
        TriggerEvent,//觸發事件
        ScheduledInGameNotification,//遊戲內推播通知
        BackendURL,//網頁位置
    }




}
