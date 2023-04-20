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
        public string Name { get; }//物品名稱
        public string Description { get; }//物品說明
        public string Ref { get; set; }//參考名稱
        public ItemType MyItemType { get; }//該類道具的類型 不會有貨幣(Gold,Point...)與寶箱(ItemGroup)
        public int Rank { get; }//此道具的品階
    }

}