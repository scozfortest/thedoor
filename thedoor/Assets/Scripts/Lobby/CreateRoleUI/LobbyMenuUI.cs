using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using System;
using System.Linq;
using TMPro;

namespace TheDoor.Main {

    public class LobbyMenuUI : BaseUI {
        [SerializeField] Image BG;
        [SerializeField] Image RoleImg;
        [SerializeField] Button ContinueBtn;
        [SerializeField] Button CreateRoleBtn;
        [SerializeField] Button RemoveRoleBtn;

        public override void RefreshUI() {
            base.RefreshUI();
            RoleImg.gameObject.SetActive(GamePlayer.Instance.Data.CurRole != null);
            ContinueBtn.gameObject.SetActive(GamePlayer.Instance.Data.CurRole != null);
            CreateRoleBtn.gameObject.SetActive(GamePlayer.Instance.Data.CurRole == null);
            RemoveRoleBtn.gameObject.SetActive(GamePlayer.Instance.Data.CurRole != null);

            if (GamePlayer.Instance.Data.CurRole == null) {



            } else {

                //腳色圖
                AssetGet.GetImg("Role", GamePlayer.Instance.Data.CurRole.RoleID.ToString(), sprite => {
                    RoleImg.sprite = sprite;
                });
                //背景圖
                AssetGet.GetImg("LobbyBG", "lobbybg" + GamePlayer.Instance.Data.CurRole.RoleID, sprite => {
                    BG.sprite = sprite;
                });

            }
        }


        /// <summary>
        /// 繼續遊戲
        /// </summary>
        public void Continue() {
            PopupUI.InitSceneTransitionProgress(1, "AdventureUILoaded");
            PopupUI.CallTransition(MyScene.AdventureScene);
        }

        public void RoleInfo() {
            PopupUI.ShowRoleInfoUI(GamePlayer.Instance.Data.CurRole);
        }
        /// <summary>
        /// 創腳
        /// </summary>
        public void CreateRole() {

        }
        /// <summary>
        /// 刪除腳色
        /// </summary>
        public void RemoveRole() {

        }
        /// <summary>
        /// 收藏
        /// </summary>
        public void Collection() {

        }
    }
}