using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;

namespace TheDoor.Main {
    public class MonsterActionData : MyJsonData {
        public static string DataName { get; set; }
        public int MonsterID { get; private set; }
        public float Probability { get; private set; }
        public int Time { get; private set; }

        public List<TargetEffectData> MyEffects = new List<TargetEffectData>();

        static Dictionary<int, List<MonsterActionData>> MonsterActionDataDic = new Dictionary<int, List<MonsterActionData>>();
        public static void ClearStaticDic() {
            MonsterActionDataDic.Clear();
        }

        protected override void GetDataFromJson(JsonData _item, string _dataName) {
            DataName = _dataName;
            JsonData item = _item;
            Target tmpTarget = Target.Enemy;
            TargetEffectType tmpTEffectType = TargetEffectType.HP;
            List<float> tmpTEffectValues = new List<float>();
            MyEffects.Clear();
            foreach (string key in item.Keys) {
                switch (key) {
                    case "ID":
                        ID = int.Parse(item[key].ToString());
                        break;
                    case "MonsterID":
                        MonsterID = int.Parse(item[key].ToString());
                        break;
                    case "Probability":
                        Probability = float.Parse(item[key].ToString());
                        break;
                    case "Time":
                        Time = int.Parse(item[key].ToString());
                        break;
                    default:
                        try {
                            if (key.Contains("Target")) {
                                tmpTarget = MyEnum.ParseEnum<Target>(item[key].ToString());
                            } else if (key.Contains("EffectType")) {
                                tmpTEffectType = MyEnum.ParseEnum<TargetEffectType>(item[key].ToString());
                            } else if (key.Contains("EffectValue")) {
                                tmpTEffectValues.Add(int.Parse(item[key].ToString()));
                            } else if (key.Contains("EffectProb")) {
                                TargetEffectData tmpTEffectData = new TargetEffectData(tmpTarget, tmpTEffectType, float.Parse(item[key].ToString()), tmpTEffectValues.ToArray());
                                MyEffects.Add(tmpTEffectData);
                            }
                        } catch (Exception _e) {
                            WriteLog.LogErrorFormat(DataName + "表格格式錯誤 ID:" + ID + "    Log: " + _e);
                        }
                        //DebugLogger.LogWarning(string.Format("{0}表有不明屬性:{1}", DataName, key));
                        break;
                }
            }
            AddToMonsterActionDic(this);
        }

        void AddToMonsterActionDic(MonsterActionData _data) {
            if (MonsterActionDataDic.ContainsKey(_data.MonsterID)) {
                MonsterActionDataDic[_data.MonsterID].Add(_data);
            } else {
                MonsterActionDataDic.Add(_data.MonsterID, new List<MonsterActionData> { _data });
            }
        }
        public static List<MonsterActionData> GetMonsterActionDatas(int _id) {
            if (MonsterActionDataDic.ContainsKey(_id)) return MonsterActionDataDic[_id];
            return null;
        }
    }
}