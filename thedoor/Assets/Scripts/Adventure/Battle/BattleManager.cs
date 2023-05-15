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
        public static List<ItemData> RewardItems { get; private set; }

        static PlayerAction CurPlayerAction;//玩家要執行的行動




        public void Init() {
            Instance = this;
        }
        public static void ResetBattle(PlayerRole _pRole, int _monsterID, List<ItemData> _rewardDatas) {
            RewardItems = _rewardDatas;
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
                .Build();
        }

        public static void PlayerDoAction(PlayerAction _action) {
            if (_action == null) return;
            CurBattleState = BattleState.ActionPerform;
            CurPlayerAction = _action;
            DoActions(0);
        }
        static int BattlePassTime = 0;


        static void DoActions(int _roundPassTime) {
            //一輪行動是由至少一個玩家行動+0~任意數量個敵方行動組成，直到玩家行動完才會結束一輪行動

            //時間軸UI移除已經執行的的ActionToken
            TimelineBattleUI.Instance.RemoveDoneActionToken();

            if (PRole.IsDead || ERole.IsDead) {
                OnActionFinish();
                return;
            }


            var eAction = ERole.GetCurAction();
            var eActionNeedTime = eAction.NeedTimeBeforeAction - BattlePassTime;
            var pActionNeedTime = CurPlayerAction.NeedTimeBeforeAction - _roundPassTime;
            //Debug.LogError("eAction.NeedTimeBeforeAction=" + eAction.NeedTimeBeforeAction);
            //Debug.LogError("pActionNeedTime=" + pActionNeedTime);
            //Debug.LogError("eActionNeedTime=" + eActionNeedTime);
            //Debug.LogError("BattlePassTime=" + BattlePassTime);
            //Debug.LogError("_roundPassTime=" + _roundPassTime);

            if (!CurPlayerAction.Done && pActionNeedTime <= 0) {//玩家行動

                CurPlayerAction.DoAction();
                CoroutineJob.Instance.StartNewAction(() => {
                    DoActions(_roundPassTime);
                }, 1f);


            } else if (eActionNeedTime <= 0) {//怪物行動

                //怪物預先新增新行動
                var newEAction = ERole.AddNewAction();
                TimelineBattleUI.Instance.SpawnNewAction(newEAction);
                //怪物行動
                eAction.DoAction();

                ERole.AddActionIndex();
                CoroutineJob.Instance.StartNewAction(() => {
                    DoActions(_roundPassTime);
                }, 1f);


            } else {

                if (CurPlayerAction.Done)//玩家已經行動過就結束此輪行動
                    OnActionFinish();
                else {
                    //時間軸往前推進1格
                    TimelineBattleUI.Instance.PassTime(1, () => {
                        _roundPassTime++;
                        BattlePassTime++;
                        ERole.DoTimePass(1);
                        PRole.DoTimePass(1);
                        DoActions(_roundPassTime);
                    });
                }

            }

        }



        static void OnActionFinish() {
            CurBattleState = BattleState.PlayerTurn;
            if (PRole.IsDead || ERole.IsDead) {
                OnBattleEnd();
            }
        }
        static void OnBattleEnd() {
            PRole.RemoveAffertBattle();//移除戰鬥類型狀態
            CurBattleState = BattleState.End;
            if (PRole.IsDead)
                BattleUI.Instance.Lose();
            else if (ERole.IsDead) {
                GetReward(() => {
                    BattleUI.Instance.Win();
                });
            }
            WriteLog.LogColor("戰鬥結束", WriteLog.LogType.Battle);
        }
        /// <summary>
        /// 勝利才有的獎勵
        /// </summary>
        static void GetReward(Action _ac) {
            if (RewardItems == null || RewardItems.Count == 0) {
                _ac?.Invoke();
                return;
            }
            GamePlayer.Instance.GainItems(RewardItems);
            PopupUI.ShowGainItemListUI(StringData.GetUIString("GainItem"), RewardItems, null, () => {
                RewardItems.Clear();
                _ac?.Invoke();
            });
        }
    }
}