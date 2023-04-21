//基本設定
const functions = require('firebase-functions');
//自訂方法
const Logger = require('./GameTools/Logger.js');
const GameSetting = require('./GameTools/GameSetting.js');
const FirestoreManager = require('./FirebaseTools/FirestoreManager.js');
const GameDataManager = require('./GameTools/GameDataManager.js');
const Prob = require('./Scoz/Probability.js');
const TextManager = require('./Scoz/TextManager.js')
const PlayerItemManager = require('./GameTools/PlayerItemManager.js');


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

    //給予玩家物品
    let gainItems = [];//紀錄要獲得的道具清單 將清單送PlayerItemManager.GiveItem來獲得道具並取得returnGainItems與replaceGainItems
    let returnGainItems = [];//回傳給Client用 獲得道具清單
    let replaceGainItems = [];//回傳給Client用 被取代的獎勵清單(此獎勵清單是本來有抽到 但因為重複道具而被取代的道具清單 回傳給cleint顯示用)

    //腳色
    let roleID = data["RoleID"]
    if (roleID == 0) {
        let roleIDs = GameDataManager.GetJsonDataIDs(GameSetting.GameJsonNames.Role)
        roleID = Prob.GetRandFromArray(roleIDs)
    }
    roleID = Number(roleID)
    let roleJsonData = GameDataManager.GetData(GameSetting.GameJsonNames.Role, roleID)
    let writeRoleData = {
        OwnerUID: context.auth.uid,
        ID: roleID,
        CurHP: Number(roleJsonData["HP"]),
        CurSanP: Number(roleJsonData["SanP"]),
        Effect: {},
        Talent: [roleJsonData["Talent"]],
    }
    //寫DB
    let roleUID = await FirestoreManager.AddDoc(GameSetting.PlayerDataCols.Role, writeRoleData)
    await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Player, context.auth.uid, { CurRoleUID: roleUID })
    returnGainItems.push({
        ItemType: GameSetting.GameJsonNames.Role,
        ItemValue: roleID,
        ItemUID: roleUID,
    })

    //道具
    let dbRoleSetting = await FirestoreManager.GetDocData(GameSetting.GameDataCols.Setting, "Role")
    let rank1SupplyIDs = GameDataManager.GetSpcificIDs2(GameSetting.GameJsonNames.Supply, { Rank: 1, Lock: false })
    let supplyIDs = []
    let defaultSupplyIDs = []//紀錄腳色初始隨機獲得道具IDs(回傳client用)
    for (let i = 0; i < dbRoleSetting["DefaultSupplyCount"]; i++) {
        let rndID = Number(Prob.GetRandFromArray(rank1SupplyIDs))
        supplyIDs.push(rndID)
        defaultSupplyIDs.push(rndID)
    }

    let exclusiveSupplyIDs = []//紀錄腳色獨特道具IDs(回傳client用)
    if ("Supplies" in roleJsonData) {
        let ids = TextManager.SplitToInts(roleJsonData["Supplies"], ',')
        for (let id of ids) {
            supplyIDs.push(id)
            exclusiveSupplyIDs.push(id)
        }
    }
    for (let supplyID of supplyIDs) {
        gainItems.push({
            ItemType: GameSetting.ItemTypes.Supply,
            ItemValue: supplyID,
        })
    }

    //寫DB
    if (supplyIDs.length > 0) {
        tmpReturnGainItems = await PlayerItemManager.GiveItems(gainItems, context.auth.uid, replaceGainItems)
        returnGainItems = returnGainItems.concat(tmpReturnGainItems)
    }


    //寫Log
    let writeLogData = {
        GainItems: returnGainItems,
    }
    Logger.Write(context.auth.uid, GameSetting.GameLogCols.CreateRole, writeLogData)

    return {
        Result: GameSetting.ResultTypes.Success,
        Data: {
            RoleUID: roleUID,
            DefaultSupplyIDs: defaultSupplyIDs,
            ExclusiveSupplyIDs: exclusiveSupplyIDs,
            ReturnGainItems: returnGainItems,
            ReplaceGainItems: replaceGainItems,
        }
    };

});


//移除腳色
exports.RemoveRole = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    if (!("RoleUID" in data) || !("RemoveType" in data)) {
        console.log("格式錯誤:Key值錯誤");
        return { Result: "格式錯誤:Key值錯誤" };
    }

    //寫DB
    await FirestoreManager.DeleteDocByUID(GameSetting.PlayerDataCols.Role, data["RoleUID"])
    await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Player, context.auth.uid, { CurRoleUID: "" })
    await FirestoreManager.DeleteDocs_WhereOperation(GameSetting.PlayerDataCols.Supply, "OwnRoleUID", "==", data["RoleUID"])
    await FirestoreManager.DeleteDocByUID(GameSetting.PlayerDataCols.Adventure, data["RoleUID"])

    //寫Log
    let writeLogData = {
        RemoveType: data["RemoveType"],
        RoleUID: data["RoleUID"],
    }
    Logger.Write(context.auth.uid, GameSetting.GameLogCols.RemoveRole, writeLogData)

    return {
        Result: GameSetting.ResultTypes.Success,
    };

});


//建立冒險
exports.CreateAdventure = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    if (!("RoleUID" in data)) {
        console.log("格式錯誤:Key值錯誤");
        return { Result: "格式錯誤:Key值錯誤" };
    }

    //設定門資料
    let dbAdventureSetting = await FirestoreManager.GetDocData(GameSetting.GameDataCols.Setting, "Adventure")
    let nodeWeight = dbAdventureSetting["NodeTypeWeight"]
    let scriptWeight = dbAdventureSetting["ScriptWeight"]
    let monsterJsonData = GameDataManager.GetJson(GameSetting.GameJsonNames.Monster)
    let allMonsterIDs = []
    let allBossIDs = []
    for (let mData of monsterJsonData) {
        if ("MonsterType" in mData) {
            console.log("monsterType=" + mData["MonsterType"])
            switch (mData["MonsterType"]) {
                case "Normal":
                    allMonsterIDs.push(Number(mData["ID"]))
                    break;
                case "Boss":
                    allBossIDs.push(Number(mData["ID"]))
                    console.log(Number(mData["ID"]))
                    break;
            }
        } else {
            allMonsterIDs.push(Number(mData["ID"]))
        }
    }
    let scriptTitleJsonData = GameDataManager.GetJson(GameSetting.GameJsonNames.ScriptTitle)
    let mainStorylineIDs = []
    let sideStorylineIDs = []
    for (let stData of scriptTitleJsonData) {
        if ("ScriptType" in stData) {
            if (stData["ScriptType"] == "Main")
                mainStorylineIDs.push(stData["ID"])
            else if (stData["ScriptType"] == "Side")
                sideStorylineIDs.push(stData["ID"])
        }
    }


    let doors = []
    //起始門
    doors.push({
        DoorType: "Start",
        Values: [],
    })
    //中間門
    for (let i = 0; i < dbAdventureSetting["DoorCount"] - 1; i++) {
        let doorType = Prob.GetRandKeyByWeight(nodeWeight)
        let values = {}
        switch (doorType) {
            case "Monster":
                let randMonsterID = Prob.GetRandFromArray(allMonsterIDs)
                values["MonsterID"] = randMonsterID
                break;
            case "Rest":
                break;
            case "Encounter":
                let scriptTitleType = Prob.GetRandKeyByWeight(scriptWeight)
                let randScriptTitleID;
                switch (scriptTitleType) {
                    case "Main":
                        randScriptTitleID = Prob.GetRandFromArray(mainStorylineIDs)
                        values["ScriptTitleID"] = randScriptTitleID
                        break;
                    case "Side":
                        randScriptTitleID = Prob.GetRandFromArray(sideStorylineIDs)
                        values["ScriptTitleID"] = randScriptTitleID
                        break;
                }
                break;
            default:
                break;
        }

        let doorData = {
            DoorType: doorType,
            Values: values,
        }
        doors.push(doorData)
    }
    //頭目門
    console.log(allBossIDs.length)
    let randBossID = Prob.GetRandFromArray(allBossIDs)
    console.log("randBossID=" + randBossID)
    let doorData = {
        DoorType: "Boss",
        Values: { MonsterID: randBossID },
    }
    doors.push(doorData)

    //設定冒險資料
    let writeAdventureData = {
        OwnerUID: context.auth.uid,
        OwnRoleUID: data["RoleUID"],
        Doors: doors,
        CurDoor: 0,
    }

    //寫DB
    await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.Adventure, data["RoleUID"], writeAdventureData)

    //寫Log
    Logger.Write(context.auth.uid, GameSetting.GameLogCols.CreateAdventure, writeAdventureData)

    return {
        Result: GameSetting.ResultTypes.Success,
    };

});

//冒險更新
exports.UpdateAdventure = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    if (!("OwnedAdventureData" in data) || !("OwnedRoleData" in data) || !("OwnedSupplyDatas" in data)) {
        console.log("格式錯誤:Key值錯誤");
        return { Result: "格式錯誤:Key值錯誤" };
    }


    let playerDBData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Player, context.auth.uid)
    let ownedAdventureData = data["OwnedAdventureData"]
    let ownedRoleData = data["OwnedRoleData"]
    let ownedSupplyDatas = data["OwnedSupplyDatas"]

    //寫DB
    await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Adventure, playerDBData["CurRoleUID"], ownedAdventureData)
    //await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Role, playerDBData["CurRoleUID"], ownedRoleData)



    // //寫Log
    // let writeLogData = {
    //     RemoveType: data["RemoveType"],
    //     RoleUID: data["RoleUID"],
    // }
    // Logger.Write(context.auth.uid, GameSetting.GameLogCols.RemoveRole, writeLogData)

    return {
        Result: GameSetting.ResultTypes.Success,
    };

});