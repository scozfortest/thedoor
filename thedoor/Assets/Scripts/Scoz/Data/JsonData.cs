using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using LitJson;
using System.Linq;


namespace Scoz.Func {

    public abstract partial class MyJsonData {
        public static bool ShowLoadTime = true;
        public int ID { get; set; }


        /// <summary>
        /// 將字典傳入，依json表設定資料，字典索引類型為int
        /// </summary>
        public static void SetData<T>(Dictionary<int, T> _dic, string _dataName) where T : MyJsonData, new() {
            DateTime startTime = DateTime.Now;

            string jsonStr = Resources.Load<TextAsset>(string.Format("Json/{0}", _dataName)).ToString();
            JsonData jd = JsonMapper.ToObject(jsonStr);
            JsonData items = jd[_dataName];
            for (int i = 0; i < items.Count; i++) {
                T data = new T();
                data.GetDataFromJson(items[i], _dataName);
                int id = int.Parse(items[i]["ID"].ToString());
                if (!_dic.ContainsKey(id))
                    _dic.Add(id, data as T);
                else
                    WriteLog.LogError(string.Format("{0}表有重複的ID {1}", _dataName, id));
            }
            DateTime endTime = DateTime.Now;
            if (ShowLoadTime) {
                TimeSpan spendTime = new TimeSpan(endTime.Ticks - startTime.Ticks);
                WriteLog.LogFormat("<color=#008080>Load {0}.json:{1}s</color>", _dataName, spendTime.TotalSeconds);
            }
        }

        /// <summary>
        /// 將字典傳入，依json表設定資料，字典索引類型為string
        /// </summary>
        public static void SetData<T>(Dictionary<string, T> _dic, string _dataName) where T : MyJsonData, new() {
            DateTime startTime = DateTime.Now;
            string jsonStr = Resources.Load<TextAsset>(string.Format("Json/{0}", _dataName)).ToString();
            JsonData jd = JsonMapper.ToObject(jsonStr);
            JsonData items = jd[_dataName];
            for (int i = 0; i < items.Count; i++) {
                T data = new T();
                data.GetDataFromJson(items[i], _dataName);
                string id = items[i]["ID"].ToString();
                if (!_dic.ContainsKey(id))
                    _dic.Add(id, data as T);
                else
                    WriteLog.LogError(string.Format("{0}表有重複的ID {1}", _dataName, id));
            }
            DateTime endTime = DateTime.Now;
            if (ShowLoadTime) {
                TimeSpan spendTime = new TimeSpan(endTime.Ticks - startTime.Ticks);
                WriteLog.LogFormat("<color=#008080>Load {0}.json:{1}s</color>", _dataName, spendTime.TotalSeconds);
            }
        }

        /// <summary>
        /// 依json表設定資料(Key為String)
        /// </summary>
        public static Dictionary<string, MyJsonData> GetDataStringKey<T>(string _dataName) where T : MyJsonData, new() {
            string jsonStr = Resources.Load<TextAsset>(string.Format("Json/{0}", _dataName)).ToString();
            JsonData jd = null;
            try {
                jd = JsonMapper.ToObject(jsonStr);
            } catch (Exception _e) {
                WriteLog.LogErrorFormat("{0}表的json格式錯誤: {1}", _dataName, _e);
            }
            JsonData items = jd[_dataName];
            Dictionary<string, MyJsonData> dic = new Dictionary<string, MyJsonData>();
            for (int i = 0; i < items.Count; i++) {
                T data = new T();
                data.GetDataFromJson(items[i], _dataName);
                string id = items[i]["ID"].ToString();
                if (!dic.ContainsKey(id))
                    dic.Add(id, data);
                else
                    WriteLog.LogError(string.Format("{0}表有重複的ID {1}", _dataName, id));
            }
            return dic;
        }


        /// <summary>
        /// 依json表設定資料，不建立字典
        /// </summary>
        public static void SetData<T>(string _dataName) where T : MyJsonData, new() {
            DateTime startTime = DateTime.Now;
            string jsonStr = Resources.Load<TextAsset>(string.Format("Json/{0}", _dataName)).ToString();
            JsonData jd = JsonMapper.ToObject(jsonStr);
            JsonData items = jd[_dataName];
            for (int i = 0; i < items.Count; i++) {
                T data = new T();
                data.GetDataFromJson(items[i], _dataName);
            }
            DateTime endTime = DateTime.Now;
            if (ShowLoadTime) {
                TimeSpan spendTime = new TimeSpan(endTime.Ticks - startTime.Ticks);
                WriteLog.LogFormat("<color=#008080>Load {0}.json:{1}s</color>", _dataName, spendTime.TotalSeconds);
            }
        }
        /// <summary>
        /// 依json表設定資料
        /// </summary>
        public static void SetKeyValueData<T>(string _dataName) where T : MyJsonData, new() {
            DateTime startTime = DateTime.Now;
            string jsonStr = Resources.Load<TextAsset>(string.Format("Json/{0}", _dataName)).ToString();
            JsonData jd = JsonMapper.ToObject(jsonStr);
            JsonData items = jd[_dataName];
            for (int i = 0; i < items.Count; i++) {
                T data = new T();
                data.GetDataFromJson(items[i], _dataName);
            }
            DateTime endTime = DateTime.Now;
            if (ShowLoadTime) {
                TimeSpan spendTime = new TimeSpan(endTime.Ticks - startTime.Ticks);
                WriteLog.LogFormat("<color=#008080>Load {0}.json:{1}s</color>", _dataName, spendTime.TotalSeconds);
            }
        }

        protected abstract void GetDataFromJson(JsonData _item, string _dataName);
        static List<T> GetDataList<T>(string _dataName, string _jsonStr) where T : MyJsonData, new() {
            List<T> list = new List<T>();
            JsonData jd = JsonMapper.ToObject(_jsonStr);
            JsonData items = jd[_dataName];
            for (int i = 0; i < items.Count; i++) {
                T data = new T();
                data.GetDataFromJson(items[i], _dataName);
                list.Add(data);
            }
            return list;
        }

    }
}
