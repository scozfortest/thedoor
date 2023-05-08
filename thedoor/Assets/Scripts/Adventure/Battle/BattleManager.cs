using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;

namespace TheDoor.Main {
    public enum BattleState {
        PlayerTurn,//玩家出牌階段
        ActionPerform,//出牌後行動表演階段
        End,//戰鬥結束
    }

    public class BattleManager : MonoBehaviour {
        public static PlayerRole PRole { get; private set; }
        public static EnemyRole ERole { get; private set; }
        public static BattleManager Instance { get; private set; }
        public static BattleState CurBattleState { get; private set; }

        static PlayerAction CurPlayerAction;//玩家要執行的行動




        public void Init() {
            Instance = this;
        }
        public static void ResetBattle(PlayerRole _pRole, int _monsterID) {

            PRole = _pRole;
            SetEnemyRole(_monsterID);
            TimelineBattleUI.Instance.ResetBattleUI(ERole.Actions);
            CurBattleState = BattleState.PlayerTurn;
            BattlePassTime = 0;
        }
        static void SetEnemyRole(int _monsterID) {
            MonsterData mData = MonsterData.GetData(_monsterID);
            ERole = new EnemyRole.Builder()
                .SetData(mData)
                .SetMaxHP(mData.HP)
                .SetCurHP(mData.HP)
                .Build();
        }

        public static void PlayerDoAction(PlayerAction _action) {
            if (_action == null) return;
            CurBattleState = BattleState.ActionPerform;
            CurPlayerAction = _action;
            DoActions(0, 0);
        }
        static int BattlePassTime = 0;


        static void DoActions(int _roundPassTime, int _eActionIndex) {
            //一輪行動是由至少一個玩家行動+0~任意數量個敵方行動組成，直到玩家行動完才會結束一輪行動


            //時間軸UI移除已經執行的的ActionToken
            TimelineBattleUI.Instance.RemoveDoneActionToken();

            if (PRole.IsDead || ERole.IsDead)
                OnActionFinish();

            var eAction = ERole.Actions[_eActionIndex];
            var eActionNeedTime = eAction.NeedTimeBeforeAction - BattlePassTime;
            var pActionNeedTime = CurPlayerAction.NeedTimeBeforeAction - _roundPassTime;

            //Debug.LogError("_roundPassTime=" + _roundPassTime);
            //Debug.LogError("BattlePassTime=" + BattlePassTime);
            //Debug.LogError("eActionNeedTime=" + eActionNeedTime);
            //Debug.LogError("pActionNeedTime=" + pActionNeedTime);

            if (!CurPlayerAction.Done && pActionNeedTime <= 0) {//玩家行動

                CurPlayerAction.DoAction();
                CoroutineJob.Instance.StartNewAction(() => {
                    DoActions(_roundPassTime, _eActionIndex);
                }, 0.5f);


            } else if (eActionNeedTime <= 0) {//怪物行動

                //怪物預先新增新行動
                var newEAction = ERole.AddNewAction();
                TimelineBattleUI.Instance.SpawnNewAction(newEAction);
                //怪物行動
                eAction.DoAction();
                _eActionIndex++;
                CoroutineJob.Instance.StartNewAction(() => {
                    DoActions(_roundPassTime, _eActionIndex);
                }, 0.5f);


            } else {

                if (CurPlayerAction.Done)//玩家已經行動過就結束此輪行動
                    OnActionFinish();
                else {
                    //時間軸往前推進1格
                    TimelineBattleUI.Instance.PassTime(1, () => {
                        _roundPassTime++;
                        BattlePassTime++;
                        CoroutineJob.Instance.StartNewAction(() => {
                            DoActions(_roundPassTime, _eActionIndex);
                        }, 0);
                    });
                }

            }




        }



        static void OnActionFinish() {
            CurBattleState = BattleState.PlayerTurn;
            if (PRole.IsDead || ERole.IsDead) {
                CurBattleState = BattleState.End;
                if (PRole.IsDead)
                    BattleUI.Instance.Lose();
                else if (ERole.IsDead)
                    BattleUI.Instance.Win();
                WriteLog.LogColor("戰鬥結束", WriteLog.LogType.Battle);
            }
        }
    }
}