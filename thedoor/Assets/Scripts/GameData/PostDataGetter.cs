using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using SimpleJSON;
using System;

namespace TheDoor.Main {

    public class PostDataGetter {

        //測試用
        const string AddDatasURL = "https://asia-east1-majampachinko-dev.cloudfunctions.net/AddDatas";
        const string GetMaJamRoomDataURL = "https://asia-east1-majampachinko-dev.cloudfunctions.net/GetMaJamRoomData";


        public static PostData GetPostData_Mail(string _title, params ItemData[] _itemDatas) {
            JSONObject postJson = new JSONObject();
            JSONArray datas = new JSONArray();

            if (_itemDatas == null || _itemDatas.Length == 0)
                return null;

            string colName = FirebaseManager.ColNames.GetValueOrDefault(ColEnum.Mail);
            if (colName == null) {
                DebugLogger.LogErrorFormat("ColNames Error ");
                return null;
            }

            JSONObject data = new JSONObject();
            data.Add("ColName", colName);
            data.Add("OwnerUID", GamePlayer.Instance.Data.UID);
            data.Add("SenderUID", "");
            data.Add("Title", _title);
            JSONArray itemDatas = new JSONArray();
            for (int i = 0; i < _itemDatas.Length; i++) {
                JSONObject itemData = new JSONObject();
                itemData.Add("ItemType", _itemDatas[i].Type.ToString());
                itemData.Add("ItemValue", _itemDatas[i].Value);
                itemDatas.Add(itemData);
            }
            data.Add("Items", itemDatas);
            datas.Add(data);
            postJson.Add("Datas", datas);
            DebugLogger.LogError(postJson);
            PostData postData = new PostData(AddDatasURL, postJson.ToString());
            return postData;
        }
        /*
        public static PostData GetAddData_Stuff(params int[] _ids) {
            JSONObject postJson = new JSONObject();
            JSONArray datas = new JSONArray();

            if (_ids == null || _ids.Length == 0)
                return null;

            string colName = FirebaseManager.ColNames.GetValueOrDefault(ColEnum.Stuff);
            if (colName == null) {
                DebugLogger.LogErrorFormat("ColNames Error ");
                return null;
            }

            for (int i = 0; i < _ids.Length; i++) {
                JSONObject data = new JSONObject();

                data.Add("ColName", colName);
                data.Add(OwnedEnum.OwnerUID.ToString(), GamePlayer.Instance.Data.UID);
                datas.Add(data);
            }
            postJson.Add("Datas", datas);
            DebugLogger.LogError(postJson);
            PostData postData = new PostData(AddDatasURL, postJson.ToString());
            return postData;
        }
        */
    }
}
