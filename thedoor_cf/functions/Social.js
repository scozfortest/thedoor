//基本設定
const functions = require('firebase-functions');
const admin = require('firebase-admin');
//自訂方法
const GameSetting = require('./GameTools/GameSetting.js');
const FirestoreManager = require('./FirebaseTools/FirestoreManager.js');
const ArrayTool = require('./Scoz/ArrayTool.js');

//寄送好友邀請
exports.Social_SendFriendRequest = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }
    if (!("PlayerUID" in data)) {
        console.log("格式錯誤");
        return { Result: "格式錯誤" };
    }

    let docID = data["PlayerUID"] + "=" + context.auth.uid;
    let setData = {
        OwnerUID: data["PlayerUID"],
        SenderUID: context.auth.uid,
    }
    let exist = await FirestoreManager.CheckDocExist(GameSetting.PlayerDataCols.Player, data["PlayerUID"]);
    if (exist)
        FirestoreManager.SetDoc_DesignatedDocName(GameSetting.PlayerDataCols.FriendRequest, docID, setData);

    return {
        Result: GameSetting.ResultTypes.Success,
        Data: exist,
    };

});

//接受好友邀請
exports.Social_AcceptFriendRequest = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }
    if (!("PlayerUID" in data) || !("RequestUID" in data)) {
        console.log("格式錯誤");
        return { Result: "格式錯誤" };
    }

    //移除好友邀請
    FirestoreManager.DeleteDocByUID(GameSetting.PlayerDataCols.FriendRequest, data["RequestUID"]);

    //自己新增好友資料
    let selfDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Friendship, context.auth.uid);
    if (selfDocData != null) {
        FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Friendship, context.auth.uid, {
            FriendUIDs: admin.firestore.FieldValue.arrayUnion(data["PlayerUID"]),
        });

    } else {
        let addData = {
            FriendUIDs: [data["PlayerUID"]],
        }
        FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.Friendship, context.auth.uid, addData);
    }

    //對方新增好友資料
    let senderDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Friendship, data["PlayerUID"]);
    if (senderDocData != null) {
        FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Friendship, data["PlayerUID"], {
            FriendUIDs: admin.firestore.FieldValue.arrayUnion(context.auth.uid),
        });
    } else {
        let addData = {
            FriendUIDs: [context.auth.uid],
        }
        FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.Friendship, data["PlayerUID"], addData);
    }

    return {
        Result: GameSetting.ResultTypes.Success,
    };

});

//接受好友邀請
exports.Social_RemoveFriendRequest = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }
    if (!("RequestUID" in data)) {
        console.log("格式錯誤");
        return { Result: "格式錯誤" };
    }

    //移除好友邀請
    FirestoreManager.DeleteDocByUID(GameSetting.PlayerDataCols.FriendRequest, data["RequestUID"]);

    return {
        Result: GameSetting.ResultTypes.Success,
    };

});

//移除好友
exports.Social_RemoveFriend = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }
    if (!("PlayerUID" in data)) {
        console.log("格式錯誤");
        return { Result: "格式錯誤" };
    }

    //更新好友清單
    friendshipDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Friendship, context.auth.uid);
    if (friendshipDocData != null && "FriendUIDs" in friendshipDocData) {
        let friends = friendshipDocData["FriendUIDs"];
        friends = ArrayTool.RemoveItem(friends, data["PlayerUID"]);
        FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Friendship, context.auth.uid, {
            FriendUIDs: friends,
        });
    }


    return {
        Result: GameSetting.ResultTypes.Success,
    };

});

