//基本設定
const functions = require('firebase-functions');
const admin = require('firebase-admin');
//自訂方法
const GameSetting = require('./GameTools/GameSetting.js');
const PlayerItemManager = require('./GameTools/PlayerItemManager.js');
const FirestoreManager = require('./FirebaseTools/FirestoreManager.js');
const GameDataManager = require('./GameTools/GameDataManager.js');
const Logger = require('./GameTools/Logger.js');
const Probability = require('./Scoz/Probability.js');
const MyTime = require('./Scoz/MyTime.js');
const GCloudProject = process.env.GCLOUD_PROJECT//專案ID


//送推播給玩家
/*
exports.RoleCall_Notification = functions.region('asia-east1').https.onCall(async (data, context) => {

});
*/
//當集到3個腳色碎片的當下玩家會獲得此資料(尚未實作)
exports.Role_Combine = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    let roleDocs = await FirestoreManager.GetDocs_Where(GameSetting.PlayerDataCols.Role, "OwnerUID", context.auth.uid);
    let ownedRoleIDs = [];//玩家擁有的腳色ID清單
    if (roleDocs != null) {
        for (let roleDoc of roleDocs) {
            ownedRoleIDs.push(roleDoc.data()["RoleID"]);
        }
    }

    let itemDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Item, context.auth.uid);//取得玩家擁有道具資料
    if (itemDocData == null) {
        return {
            Result: GameSetting.ResultTypes.Success,
        };
    }

    if (!("Stuff" in itemDocData)) {
        return {
            Result: GameSetting.ResultTypes.Success,
        };
    }


    let needCombineRoleIDs = [];//根據玩家擁有那些腳色碎片來取出預計要合成的腳色
    let stuffJson = GameDataManager.GetJson(GameSetting.GameJsonNames.Stuff);//取得stuff json表
    let roleFragmentMap = {};//腳色ID對造需求道具ID表 [道具ID][腳色ID]

    for (let jsonData of stuffJson) {
        if (jsonData["StuffType"] != "RoleFragment")//不是腳色碎片就不處理
            continue;
        if (ownedRoleIDs.includes(Number(jsonData["StuffTypeValue"])))//排除玩家已經有的腳色
            continue;
        let roleID = Number(jsonData["StuffTypeValue"]);
        let stuffID = Number(jsonData["ID"]);
        if (!(stuffID in roleFragmentMap))
            roleFragmentMap[stuffID] = roleID;
    }

    let roleFragmentCount = {};//[腳色ID][StuffID[]]碎片量統計
    //找出滿足所有碎片腳色並設定預計要合成的腳色needCombineRoleIDs
    for (let stuffID of Object.keys(itemDocData["Stuff"])) {
        if (!(stuffID in roleFragmentMap))
            continue;
        roleID = roleFragmentMap[stuffID];
        if (roleID in roleFragmentCount)
            roleFragmentCount[roleID] += 1;
        else
            roleFragmentCount[roleID] = 1;
        if (roleFragmentCount[roleID] == 3) {
            needCombineRoleIDs.push(roleID);
        } else if (roleFragmentCount[roleID] > 3) {
            console.log("一個腳色的碎片種類不可能超過3種，可能是Stuff企劃表填錯了");
            return { Result: "資料錯誤" };
        }
    }
    if (needCombineRoleIDs == 0) {//沒有能合成的腳色就返回
        return {
            Result: GameSetting.ResultTypes.Success,
        };
    }
    let addRoleDatas = [];
    for (let i = 0; i < needCombineRoleIDs.length; i++) {
        let docUID = context.auth.uid + "=" + needCombineRoleIDs[i];//腳色文件ID為 玩家UID=腳色表格ID
        console.log("docUID=" + docUID);
        addRoleDatas.push({
            UID: docUID,
            ColName: GameSetting.PlayerDataCols.Role,
            RoleID: Number(needCombineRoleIDs[i]),
            OwnerUID: context.auth.uid,
        });
    }
    await FirestoreManager.AddDocs(addRoleDatas);//寫入資料庫
    Logger.Role_Combine(context.auth.uid, needCombineRoleIDs);//寫Log


    let anyCombinedRole = (addRoleDatas.length > 0);

    return {
        Result: GameSetting.ResultTypes.Success,
        Data: anyCombinedRole,
    };

});
//取得隨機腳色來電資料
exports.Role_GetRandomCalls = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    let roleCallDocs = await FirestoreManager.GetDocs_Where(GameSetting.PlayerDataCols.RoleCall, "OwnerUID", context.auth.uid);
    let roleDocs = await FirestoreManager.GetDocs_Where(GameSetting.PlayerDataCols.Role, "OwnerUID", context.auth.uid);
    let roleCallSettingData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.Setting, "RoleCall");
    let historyDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.History, context.auth.uid);
    let maxCallCount = roleCallSettingData["CallCount"];
    let callTimeList = roleCallSettingData["CallTimeList"];

    if (roleDocs == null || roleDocs.length <= 0) {//沒資料代表玩家沒腳色
        return {
            Result: GameSetting.ResultTypes.Success,
        };
    }

    let randIndexs = Probability.GetRandInts(roleDocs.length, maxCallCount);//取得隨機腳色索引


    //設定下X次的來電時間
    let nextCallDates = [];//下次來電Date陣列
    let nowDate = admin.firestore.Timestamp.now().toDate();//取得現在時間
    nowDate = MyTime.AddHours(nowDate, 8);//時區是+8
    let lastCallTime = nowDate.getTime();//上一次來電的時間戳，初始從現在時間開始
    let todayNoonDate = nowDate.setHours(0, 0, 0, 0);//取得今日UTC+8凌晨時間
    let day = 0;//距離今日往後的天數
    for (let i = 0; i < maxCallCount; i++) {
        //console.log("/////////////////////////////下" + (i + 1) + "次來電");
        for (let j = 0; j < callTimeList.length; j++) {
            let expectTime = MyTime.AddDays(todayNoonDate, day);//預期要設定來電的時間
            expectTime = expectTime.setHours(callTimeList[j], 0, 0, 0);//預期要設定的來電時間戳
            //console.log("expectTime=" + new Date(expectTime));
            //console.log("lastCallTime=" + new Date(lastCallTime));
            if (expectTime > lastCallTime) {//晚於上一筆可以來電的時段就新增來電時間
                //console.log("push expectTime=" + new Date(expectTime));
                nextCallDates.push(MyTime.AddHours(new Date(expectTime), -8));//因為之前有+8小時，現在要減回來
                lastCallTime = expectTime;
                break;
            }
            if (j == callTimeList.length - 1) {//如果上一筆設定的來電時間已經晚於當天所有可以來電的時段則把可以來電的時間設為明天
                day += 1;
                var nextDayCallDate = new Date(MyTime.AddDays(todayNoonDate, day).setHours(callTimeList[0], 0, 0, 0))//取得下一日的最早的來電時間
                nextCallDates.push(MyTime.AddHours(nextDayCallDate, -8));
                //console.log("push nextDayCallDate=" + nextDayCallDate);//因為之前有+8小時，現在要減回來
                lastCallTime = nextDayCallDate.getTime();//紀錄上一次來電的時間戳
            }
        }
    }

    //設定來電資料
    let addRoleCallDatas = [];
    for (let i = 0; i < randIndexs.length; i++) {
        let newRoleID = roleDocs[randIndexs[i]].data()["RoleID"];
        let existSameCallTime = false;//是否已經有同時段的腳色來電資料
        if (roleCallDocs != null) {
            for (let j = 0; j < roleCallDocs.length; j++) {
                //console.log("a=" + nextCallDates[i]);
                //console.log("b=" + roleCallDocs[j].data()["CallTime"].toDate());
                //console.log("diff=" + MyTime.GetDateDiff_Hour(nextCallDates[i], roleCallDocs[j].data()["CallTime"].toDate()));
                if (MyTime.GetDateDiff_Hour(nextCallDates[i], roleCallDocs[j].data()["CallTime"].toDate()) == 0) {//該時段有來電資料就不處理
                    existSameCallTime = true;
                    break;
                }
            }
        }
        if (existSameCallTime) {//該時段有來電資料就不處理
            continue;
        } else {//該時段沒有腳色來電資料就建立新的一筆腳色來電資料
            console.log("a=" + GameSetting.PlayerDataCols.RoleCall);
            console.log("b=" + context.auth.uid);
            console.log("c=" + nextCallDates[i]);
            console.log("d=" + newRoleID);
            addRoleCallDatas.push({
                ColName: GameSetting.PlayerDataCols.RoleCall,
                OwnerUID: context.auth.uid,
                RoleID: newRoleID,
                CallTime: nextCallDates[i],
            });
        }
    }
    console.log("addRoleCallDatas=" + addRoleCallDatas.length);
    if (addRoleCallDatas.length > 0)
        FirestoreManager.AddDocs(addRoleCallDatas);

    //設定今日已經取得腳色來電資料了
    let updateData = {
        LastGetRoleCallTime: admin.firestore.Timestamp.now(),
    }
    if (historyDocData != null)
        await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
    else
        await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
    return {
        Result: GameSetting.ResultTypes.Success,
    };

});


//情境對話獎勵
exports.Role_GetPlotReward = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }
    if (!("RoleCallUID" in data)) {
        console.log("格式錯誤");
        return { Result: "格式錯誤" };
    }

    let roleCallDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.RoleCall, data["RoleCallUID"]);


    if (roleCallDocData == null) {
        console.log("找不到資料");
        return { Result: "資料錯誤" };
    }

    let ownedRoleCallDocs = await FirestoreManager.GetDocs_WhereOperation(GameSetting.PlayerDataCols.RoleCall, "OwnerUID", "==", context.auth.uid);
    let theSameRoleIDCallDocs = [];
    for (let doc of ownedRoleCallDocs) {
        if (doc.data()["RoleID"] == roleCallDocData["RoleID"])
            theSameRoleIDCallDocs.push(doc);
    }
    let deleteUIDs = [];
    let alreadyPassCallTimeDatas = [];
    for (let i = 0; i < theSameRoleIDCallDocs.length; i++) {
        if (admin.firestore.Timestamp.now() > theSameRoleIDCallDocs[i].data()["CallTime"]) {
            alreadyPassCallTimeDatas.push(theSameRoleIDCallDocs[i].data());
            deleteUIDs.push(theSameRoleIDCallDocs[i].id);
        }
    }
    if (!deleteUIDs.includes(roleCallDocData["UID"]))//確保給予獎勵後要刪除的腳色來電包含傳入的腳色來電(通常不會用到)
        deleteUIDs.push(roleCallDocData["UID"]);

    let roleJsonData = GameDataManager.GetData(GameSetting.GameJsonNames.Role, roleCallDocData["RoleID"]);
    let returnGainItems = [];
    let replaceGainItems = [];//被取代的獎勵清單(此獎勵清單是本來有抽到 但因為重複道具而被取代的道具清單 回傳給cleint顯示用)
    if ("ItemType" in roleJsonData && "ItemValue" in roleJsonData) {
        let itemType = roleJsonData["ItemType"];
        let itemValue = Number(roleJsonData["ItemValue"]);

        returnGainItems = await PlayerItemManager.GiveItem(itemType, itemValue, alreadyPassCallTimeDatas.length, context.auth.uid, replaceGainItems);//給予玩家道具，有幾個該腳色的過去來電就給幾次獎勵

    }

    //console.log("deleteUIDs=" + JSON.stringify(deleteUIDs.length));

    if (GCloudProject != GameSetting.GCloudProjects.Release) {//不是正式版
        let removeRoleCallDoc = true;
        let roleCallSettingData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.Setting, "RoleCall");
        if (roleCallSettingData != null) {
            if (roleCallSettingData["DontRemoveCall"] == true)
                removeRoleCallDoc = false;
        }
        if (removeRoleCallDoc == true)
            await FirestoreManager.DeleteDocsByUIDs(GameSetting.PlayerDataCols.RoleCall, deleteUIDs);//移除所有同腳色的過去來電
    } else
        await FirestoreManager.DeleteDocsByUIDs(GameSetting.PlayerDataCols.RoleCall, deleteUIDs);//移除所有同腳色的過去來電


    //寫LOG
    let logData = {
        GainItems: returnGainItems,
    }
    Logger.RoleCallReward(context.auth.uid, logData);


    return {
        Result: GameSetting.ResultTypes.Success,
        Data: {
            ReturnGainItems: returnGainItems,
            ReplaceGainItems: replaceGainItems,
        }
    };

});

//移除來電
exports.Role_RemoveCall = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }
    if (!("RoleCallUID" in data)) {
        console.log("格式錯誤");
        return { Result: "格式錯誤" };
    }
    let roleCallDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.RoleCall, data["RoleCallUID"]);
    if (roleCallDocData == null) {
        console.log("找不到資料");
        return { Result: "資料錯誤" };
    }

    let theSameRoleIDCallDocs = await FirestoreManager.GetDocs_WhereOperation(GameSetting.PlayerDataCols.RoleCall, "RoleID", "==", roleCallDocData["RoleID"]);
    let deleteUIDs = [];
    let alreadyPassCallTimeDatas = [];
    for (let i = 0; i < theSameRoleIDCallDocs.length; i++) {
        if (admin.firestore.Timestamp.now() > theSameRoleIDCallDocs[i].data()["CallTime"]) {
            alreadyPassCallTimeDatas.push(theSameRoleIDCallDocs[i].data());
            deleteUIDs.push(theSameRoleIDCallDocs[i].id);
        }
    }
    if (!deleteUIDs.includes(roleCallDocData["UID"]))//確保要刪除的腳色來電包含傳入的腳色來電(通常不會用到)
        deleteUIDs.push(roleCallDocData["UID"]);

    await FirestoreManager.DeleteDocsByUIDs(GameSetting.PlayerDataCols.RoleCall, deleteUIDs);//移除所有同腳色的過去來電

    return {
        Result: GameSetting.ResultTypes.Success,
    };

});

//將該來電設定為已經顯示給玩家過了
exports.Role_SetCalled = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }
    if (!("RoleCallUID" in data)) {
        console.log("格式錯誤");
        return { Result: "格式錯誤" };
    }

    FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.RoleCall, data["RoleCallUID"], { Called: true });

    return {
        Result: GameSetting.ResultTypes.Success,
    };

});