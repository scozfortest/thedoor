//基本設定
const functions = require('firebase-functions');

//自訂方法
const GameSetting = require('./GameTools/GameSetting.js');
const FirestoreManager = require('./FirebaseTools/FirestoreManager.js');
const ArrayTool = require('./Scoz/ArrayTool.js');

//設定最愛貼圖清單
exports.Collection_SetFavoriteEmojiID = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }
    if (!("EmojiID" in data) || !("Use" in data)) {
        console.log("格式錯誤");
        return { Result: "格式錯誤" };
    }
    let newFavoriteEmojiIDs = [];
    let itemDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Item, context.auth.uid);
    if ("FavoriteEmojiIDs" in itemDocData) {
        newFavoriteEmojiIDs = itemDocData["FavoriteEmojiIDs"];
        if (data["Use"] == true) {
            if (!newFavoriteEmojiIDs.includes(data["EmojiID"]))
                newFavoriteEmojiIDs.push(data["EmojiID"]);
        } else {
            if (newFavoriteEmojiIDs.includes(data["EmojiID"]))
                newFavoriteEmojiIDs = ArrayTool.RemoveItem(newFavoriteEmojiIDs, data["EmojiID"]);
        }
    } else {
        newFavoriteEmojiIDs.push(data["EmojiID"]);
    }

    let updateData = {
        FavoriteEmojiIDs: newFavoriteEmojiIDs,
    }
    await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Item, context.auth.uid, updateData);

    return {
        Result: GameSetting.ResultTypes.Success,
    };

});

//設定出牌語音清單
exports.Collection_SetUseVoiceIDs = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }
    if (!("UseVoiceIDs" in data)) {
        console.log("格式錯誤");
        return { Result: "格式錯誤" };
    }
    let itemDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Item, context.auth.uid);

    if (itemDocData == null) {
        console.log("找不到PlayerData-Item");
        return { Result: "找不到PlayerData-Item" };
    }
    let updateData = {
        UseVoiceIDs: data["UseVoiceIDs"],
    }
    await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Item, context.auth.uid, updateData);

    return {
        Result: GameSetting.ResultTypes.Success,
    };

});

//設定目前使用中的稱號、頭像、咪牌手
exports.Collection_SetUseItem = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }
    if (!("ItemType" in data) || !("ItemValue" in data)) {
        console.log("格式錯誤");
        return { Result: "格式錯誤" };
    }
    let playerDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Player, context.auth.uid);

    if (playerDocData == null) {
        console.log("找不到PlayerData-Player");
        return { Result: "找不到PlayerData-Player" };
    }
    let keyStr = "Cur" + data["ItemType"] + "ID";
    let updateData = {
        [keyStr]: data["ItemValue"],
    }
    await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Player, context.auth.uid, updateData);

    return {
        Result: GameSetting.ResultTypes.Success,
    };

});