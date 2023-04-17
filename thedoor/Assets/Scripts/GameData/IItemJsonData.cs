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
        public string Ref { get; set; }//參考名稱
        public string TypeName { get; }//該類道具的類型名稱(會抓String表)
        public ItemType MyItemType { get; }//該類道具的類型 不會有貨幣(Gold,Point...)與寶箱(ItemGroup)
        public int Rank { get; }//此道具的品階
    }

}