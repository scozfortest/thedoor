using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;

namespace TheDoor.Main {
    public enum DoorType {
        Start,//起始
        Monster,//妖靈
        Rest,//休息
        Encounter,//遭遇事件
        Boss,//頭目
    }
    public class DoorData : IScozJsonConvertible {
        [ScozSerializable] public DoorType DoorType { get; private set; }
        [ScozSerializable] public Dictionary<string, object> Values { get; private set; } = new Dictionary<string, object>();


        /// <summary>
        /// 初始化門資料
        /// </summary>
        public DoorData(DoorType _type, Dictionary<string, object> _values) {
            DoorType = _type;
            Values = _values;
        }
        /// <summary>
        /// 初始化門資料
        /// </summary>
        public DoorData(DoorType _type) {
            DoorType = _type;
            Values = GetDoorValueDicByType(_type);
        }

        public string Name {
            get {
                return StringData.GetUIString("DoorType_" + DoorType.ToString());
            }
        }

        Dictionary<string, object> GetDoorValueDicByType(DoorType _type) {

            var dataDic = new Dictionary<string, object>();
            MonsterData monsterData;
            switch (_type) {
                case DoorType.Monster:
                    monsterData = MonsterData.GetRndMonsterData(MonsterType.Normal);
                    dataDic.Add("MonsterID", monsterData.ID);
                    break;
                case DoorType.Boss:
                    monsterData = MonsterData.GetRndMonsterData(MonsterType.Boss);
                    dataDic.Add("MonsterID", monsterData.ID);
                    break;
                case DoorType.Encounter:
                    var scriptWeight = GameSettingData.GetJsNode(GameSetting.Adventure_ScriptWeight);
                    var rndScriptType = MyEnum.ParseEnum<ScriptType>(Prob.GetRandomKeyFromJsNodeKeyWeight(scriptWeight));
                    var rndScriptTitleData = ScriptTitleData.GetRndDataByType(rndScriptType);
                    dataDic.Add("ScriptTitleID", rndScriptTitleData.ID);
                    break;
                case DoorType.Rest:
                    break;
                case DoorType.Start:
                    break;
                default:
                    WriteLog.LogError("GetDoorValueDicByType有尚未實作的DoorType: " + _type);
                    break;
            }
            return dataDic;
        }

        //public string Name {
        //    get {
        //        string name = "";
        //        switch (MyType) {
        //            case DoorType.:
        //                name = string.Format(StringData.GetUIString(MyType.ToString()), Values[0]);
        //                break;
        //            case DoorType.Stun:
        //                name = string.Format(StringData.GetUIString(MyType.ToString()), Values[0]);
        //                break;
        //            default:
        //                name = StringData.GetUIString(MyType.ToString());
        //                break;
        //        }
        //        return name;
        //    }
        //}
    }
}