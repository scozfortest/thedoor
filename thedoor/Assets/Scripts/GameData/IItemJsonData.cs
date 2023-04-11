using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using System;

namespace TheDoor.Main {
    /// <summary>
    /// 玩家會獲得的道具類除了貨幣(Gold,Point,Ball...)與寶箱(ItemGroup)都的JsonData都會繼承這個介面
    /// </summary>
    public interface IItemJsonData {
        public int ID { get; set; }
        public string Name { get; }//此道具的中文名稱(會抓String表)
        public string TypeName { get; }//該類道具抽卡後獲得時顯示的類型中文名稱(會抓String表)
        public ItemType MyItemType { get; }//不會有貨幣(Gold,Point,Ball...)與寶箱(ItemGroup)
        public int Rank { get; }//此道具的品階
        public void GetIconSprite(Action<Sprite> _ac);//取得此道具ICON
    }

}