//基本設定
const functions = require('firebase-functions');
const admin = require('firebase-admin');
//自訂方法
const Logger = require('./GameTools/Logger.js');
const GameSetting = require('./GameTools/GameSetting.js');
const FirestoreManager = require('./FirebaseTools/FirestoreManager.js');
const GameDataManager = require('./GameTools/GameDataManager.js');
const Prob = require('./Scoz/Probability.js');
const TextManager = require('./Scoz/TextManager.js')


//創腳色
exports.CreateRole = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    if (!("RoleID" in data)) {
        console.log("格式錯誤:Key值錯誤");
        return { Result: "格式錯誤:Key值錯誤" };
    }

    //腳色
    let roleID = data["RoleID"]
    if (roleID == 0) {
        let roleIDs = GameDataManager.GetJsonDataIDs(GameSetting.GameJsonNames.Role)
        roleID = Prob.GetRandFromArray(roleIDs)
    }
    roleID = Number(roleID)
    let roleJsonData = GameDataManager.GetData(GameSetting.GameJsonNames.Role, roleID)
    let writeRoleData = {
        RoleID: roleID,
        CurHP: Number(roleJsonData["HP"]),
        CurSanP: Number(roleJsonData["SanP"]),
        Effect: {},
        Talent: [roleJsonData["Talent"]],
    }
    //寫DB
    let roleUID = await FirestoreManager.AddDoc(GameSetting.PlayerDataCols.Role, writeRoleData)

    //道具
    let dbRoleSetting = await FirestoreManager.GetDocData(GameSetting.GameDataCols.Setting, "Role")
    let rank1SupplyIDs = GameDataManager.GetSpcificIDs2(GameSetting.GameJsonNames.Supply, { Rank: 1, Lock: false })
    let supplyIDs = []
    for (let i = 0; i < dbRoleSetting["DefaultSupplyCount"]; i++) {
        supplyIDs.push(Prob.GetRandFromArray(rank1SupplyIDs))
    }
    supplyIDs.push(TextManager.SplitToInts(roleJsonData["Supplies"]))
    let supplyJsonDatas = GameDataManager.GetDatas(GameSetting.GameJsonNames.Supply, supplyIDs)
    let writeSupplyDatas = []
    for (let supplyData of supplyJsonDatas) {
        let tmpData = {
            ColName: GameSetting.PlayerDataCols.Supply,
            OwnRoleUID: roleUID,
            OwnerUID: context.auth.uid,
            SupplyID: Number(supplyData["ID"]),
            Usage: Number(supplyData["Usage"]),
        }
        writeSupplyDatas.push(tmpData)
    }
    //寫DB
    await FirestoreManager.AddDocs(writeSupplyDatas)
    //寫DB
    await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Player, context.auth.uid, { CurRoleUID: roleUID })
    //寫Log
    let logData = {
        RoleID: roleID,
        SupplyIDs: supplyIDs,
    }
    //寫DB
    Logger.Write(context.auth.uid, GameSetting.GameLogCols.CreateRole, logData);

    return {
        Result: GameSetting.ResultTypes.Success,
        Data: {
            RoleUID: roleUID,
        }
    };

});