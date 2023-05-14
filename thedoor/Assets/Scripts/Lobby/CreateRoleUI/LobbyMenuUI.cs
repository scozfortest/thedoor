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
                var roleData = RoleData.GetData(GamePlayer.Instance.Data.CurRole.ID);
                if (roleData != null) {
                    AssetGet.GetImg("Role", roleData.Ref, sprite => {
                        RoleImg.sprite = sprite;
                    });
                    //背景圖
                    AssetGet.GetImg("LobbyBG", "lobbybg_" + roleData.Ref, sprite => {
                        BG.sprite = sprite;
                    });
                }



            }
        }


        /// <summary>
        /// 繼續遊戲
        /// </summary>
        public void Continue() {
            PopupUI.InitSceneTransitionProgress(1, "AdventureUILoaded");
            PopupUI.CallSceneTransition(MyScene.AdventureScene);
        }

        public void RoleInfo() {
            PopupUI.ShowRoleInfoUI(GamePlayer.Instance.Data.CurRole, AdventureManager.PRole);
        }
        /// <summary>
        /// 創腳
        /// </summary>
        public void CreateRole() {

            LocoServerManager.CreateAdventure();
            LobbyUI.Instance?.SwitchUI(LobbyUIs.CreateRole);

            /*
            PopupUI.ShowLoading(string.Format("Loading"));
            FirebaseManager.CreateRole(0, cbData => {//創腳
                Dictionary<string, object> cbDic = DicExtension.ConvertToStringKeyDic(cbData);
                string roleUID = cbDic["RoleUID"].ToString();
                FirebaseManager.GetDataByDocID(ColEnum.Role, roleUID, (col, roleData) => {//取DB上最新的腳色資料
                    GamePlayer.Instance.SetOwnedData<OwnedRoleData>(col, roleData);
                    GamePlayer.Instance.Data.SetCurRole_Loco(roleUID);
                    PopupUI.HideLoading();
                    CreateRoleUI.GetInstance<CreateRoleUI>().SetCreateRoleCBDic(cbDic);
                    LobbyUI.GetInstance<LobbyUI>()?.SwitchUI(LobbyUIs.CreateRole);
                    FirebaseManager.CreateAdventure(roleUID, () => {//建立冒險
                        FirebaseManager.GetDataByDocID(ColEnum.Adventure, roleUID, (col, advData) => {//取DB上最新的冒險資料
                            GamePlayer.Instance.SetOwnedData<OwnedRoleData>(col, advData);
                        });
                    });
                });
            });
            */
        }
        /// <summary>
        /// 刪除腳色
        /// </summary>
        public void RemoveRole() {
            if (GamePlayer.Instance.Data.CurRole == null) return;
            LocoServerManager.RemoveCurUseRole();
            RefreshUI();
            //PopupUI.ShowLoading(StringData.GetUIString("Loading"));
            //FirebaseManager.RemoveRole(GamePlayer.Instance.Data.CurRole.UID, "SelfRemove", () => {
            //    PopupUI.HideLoading();
            //    RefreshUI();
            //});
        }
        /// <summary>
        /// 收藏
        /// </summary>
        public void Collection() {

        }
    }
}