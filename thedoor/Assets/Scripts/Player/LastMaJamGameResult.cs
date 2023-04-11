using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheDoor.Main {
    public class LastMaJamGameResult {
        public enum Result {
            Win,
            Lose,
            Tie,   // 平手
        }
        public Result MyResult { get; private set; }
        public Currency BetType { get; private set; }
        public long GoldChange { get; private set; }
        public long BallChange { get; private set; }
        public long PointChange { get; private set; }
        /// <summary>
        /// 玩家最終排名
        /// </summary>
        public int RankChange { get; private set; }
        /// <summary>
        /// 玩家贏得總台數
        /// </summary>
        public int TaiNumber { get; private set; }
        public LastMaJamGameResult(Currency _betType, Result _result, long _goldChange, long _ballChange, long _pointChange, int _rankChange, int _taiNumber) {
            BetType = _betType;
            MyResult = _result;
            GoldChange = _goldChange;
            BallChange = _ballChange;
            PointChange = _pointChange;
            RankChange = _rankChange;
            TaiNumber = _taiNumber;
        }
    }
}