using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;
using System;
using System.Linq;

namespace TheDoor.Main {
    public class RolePlotData : MyJsonData {
        public static string DataName { get; set; }
        public string Description {
            get {
                return StringData.GetString_static(DataName + "_" + ID, "Description");
            }
        }
        public string Ref { get; private set; }
        public int RoleID { get; private set; }

        static Dictionary<int, List<RolePlotData>> RolePlotDataDic = new Dictionary<int, List<RolePlotData>>();
        public static void ClearStaticDic() {
            RolePlotDataDic.Clear();
        }
        protected override void GetDataFromJson(JsonData _item, string _dataName) {
            DataName = _dataName;
            JsonData item = _item;
            foreach (string key in item.Keys) {
                switch (key) {
                    case "ID":
                        ID = int.Parse(item[key].ToString());
                        break;
                    case "Ref":
                        Ref = item[key].ToString();
                        break;
                    case "RoleID":
                        RoleID = int.Parse(item[key].ToString());
                        break;
                    default:
                        WriteLog.LogWarning(string.Format("{0}表有不明屬性:{1}", DataName, key));
                        break;
                }
            }
            AddToRolePlotDic(this);
        }

        void AddToRolePlotDic(RolePlotData _data) {
            if (RolePlotDataDic.ContainsKey(_data.RoleID)) {
                RolePlotDataDic[_data.RoleID].Add(_data);
            } else {
                RolePlotDataDic.Add(_data.RoleID, new List<RolePlotData> { _data });
            }
        }

        /// <summary>
        /// 取得下一段劇本
        /// </summary>
        public static RolePlotData GetData(int _roleID, int _index) {
            if (RolePlotDataDic[_roleID] == null || RolePlotDataDic[_roleID].Count == 0) return null;

            if (_index >= RolePlotDataDic[_roleID].Count) {
                //WriteLog.LogErrorFormat("GetScript的傳入索引超出範圍");
                return null;
            }
            return RolePlotDataDic[_roleID][_index];
        }
    }

}
