//基本設定
const admin = require('firebase-admin');
const db = admin.firestore();
//自訂方法
const ArrayTool = require('../Scoz/ArrayTool.js');
const Sizeof = require('firestore-size');

var methods = {
    //取得Firestore Document.data()的大小(byte)
    GetSizeOfData(data) {
        return Sizeof.default(data);
    },
    //在CloudFunction中取得的Timestam格式如果要傳到UnityC#那，要轉為C#可讀的時間格式
    CovertTimeStampToScozTimeStr: function (firestoreTimestamp) {
        if (firestoreTimestamp instanceof admin.firestore.Timestamp) {
            let scozTimeStr = MyTime.ConvertToScozTimeStr(firestoreTimestamp.toDate(), 8);
            return scozTimeStr;
        }

        //傳入的不是FirebaseTimestamp時就回傳現在時間
        let nowTime = admin.firestore.Timestamp.now();
        let scozTimeStr = MyTime.ConvertToScozTimeStr(MyTime.AddHours(nowTime.toDate(), 8));
        return scozTimeStr;

    },
    //確認文件是否存在
    CheckDocExist: async function (colName, docName) {
        const doc = await db.collection(colName).doc(docName).get();
        return doc.exists;
    },
    //確認文件是否存在(傳入where條件FieldName與Value值)，找不到資料時回傳null
    CheckDocExist_Where: async function (colName, fieldName, value) {
        const snapshot = await db.collection(colName).where(fieldName, '==', value).get();
        if (snapshot.empty)
            return null;
        return !snapshot.empty;
    },
    //取得文件資料，找不到資料時回傳null
    GetDocData: async function (colName, docName) {
        const doc = await db.collection(colName).doc(docName).get();
        if (!doc.exists)
            return null;
        return doc.data();
    },
    //取的Collection中所有的Doc
    GetAllDocs: async function (colName) {
        const snapshot = await db.collection(colName).get();
        if (snapshot.empty)
            return null;
        return snapshot.docs;
    },
    //搜尋欄位取得符合條件的文件並回傳Docs(傳入where條件FieldName與Value值)，找不到資料時回傳null
    GetDocs_Where: async function (colName, fieldName, value) {
        const snapshot = await db.collection(colName).where(fieldName, '==', value).get();
        if (snapshot.empty)
            return null;
        return snapshot.docs;
    },
    //搜尋欄位取得符合條件的文件並回傳Docs(傳入where條件FieldName與Value值)，找不到資料時回傳null
    GetDocs_WhereOperation: async function (colName, fieldName, operation, value) {
        const snapshot = await db.collection(colName).where(fieldName, operation, value).get();
        if (snapshot.empty)
            return null;
        return snapshot.docs;
    },
    //搜尋欄位取得符合條件的文件並回傳Docs(傳入where條件FieldName與Value值)，找不到資料時回傳null
    GetDocs_WhereIn: async function (colName, fieldName, values) {

        if (values.length < 10) {
            const snapshot = await db.collection(colName).where(fieldName, 'in', values).get();
            if (snapshot.empty)
                return null;
            return snapshot.docs;
        } else {
            let newValues = ArrayTool.SplitArray(values, 10);
            let docs = [];
            for (let i = 0; i < newValues.length; i++) {
                let snapshot = await db.collection(colName).where(fieldName, 'in', newValues[i]).get();
                if (snapshot.empty)
                    continue;
                docs = docs.concat(snapshot.docs);
            }
            if (docs.length <= 0)
                return null;
            return docs;
        }

    },
    //搜尋欄位取得正逆排序並限制數量的文件並回傳Docs(傳入FieldName與limitvalue值,isDesc是否逆排序)，找不到資料時回傳null
    GetDocs_OrderLimit: async function (colName, fieldName, isDesc, limitValue) {

        if (isDesc) {//逆排序
            const snapshot = await db.collection(colName).orderBy(fieldName, 'desc').limit(limitValue).get();
            if (snapshot.empty)
                return null;
            return snapshot.docs;
        } else {//正排序
            const snapshot = await db.collection(colName).orderBy(fieldName).limit(limitValue).get();
            if (snapshot.empty)
                return null;
            return snapshot.docs;
        }

    },

    //新增單筆文件
    AddDoc: async function (colName, data) {
        let nowTime = admin.firestore.Timestamp.now();
        let doc = db.collection(colName).doc();
        data["UID"] = doc.id;
        data["CreateTime"] = nowTime;
        delete data["ColName"];//因為AddDocs方法傳入的data會包含ColName這個key值，但AddDoc不用，避免有人誤送，把這個key值刪掉
        const batch = db.batch();
        batch.create(doc, data);
        await batch.commit();
        //const doc = await db.collection(colName).add(data);
        return doc.id;
    },
    //新增單筆文件(自定文件名)
    AddDoc_DesignatedDocName: async function (colName, docName, data) {
        let nowTime = admin.firestore.Timestamp.now();
        let doc = db.collection(colName).doc(docName);
        data["UID"] = docName;
        data["CreateTime"] = nowTime;
        delete data["ColName"];//因為AddDocs方法傳入的data會包含ColName這個key值，但AddDoc_DesignatedDocName不用，避免有人誤送，把這個key值刪掉
        const batch = db.batch();
        batch.create(doc, data);
        await batch.commit();
        //const doc = await db.collection(colName).doc(docName).set(data);
        return doc.id;
    },
    //新增單筆文件(不等待)
    AddDoc_DontWait: function (colName, data) {
        let nowTime = admin.firestore.Timestamp.now();
        let doc = db.collection(colName).doc();
        data["UID"] = doc.id;
        data["CreateTime"] = nowTime;
        delete data["ColName"];//因為AddDocs方法傳入的data會包含ColName這個key值，但AddDoc_DontWait不用，避免有人誤送，把這個key值刪掉
        const batch = db.batch();
        batch.create(doc, data);
        batch.commit();
        //const doc = db.collection(colName).add(data);
    },
    //新增多筆文件(傳入Datas)，並返回UIDs[]
    AddDocs: async function (datas) {


        /*
        Datas格式格式範例
        [
          {
              ColName:"CollectionName",
              ID:1,
              OwnerUID:"PlayerUID",
          },
          {
              ColName:"CollectionName",
              ID:1,
              OwnerUID:"PlayerUID",
          },
        ]
        */

        let returnUIDs = [];
        await DocsBatchedAdd_Recursive(datas, 0, returnUIDs);
        return returnUIDs;
    },
    //設定單筆文件(自定文件名)
    SetDoc_DesignatedDocName: async function (colName, docName, data) {
        let nowTime = admin.firestore.Timestamp.now();
        let doc = db.collection(colName).doc(docName);
        data["UID"] = docName;
        data["CreateTime"] = nowTime;
        const batch = db.batch();
        batch.set(doc, data);
        await batch.commit();
        return doc.id;
    },
    //設定單筆文件並Merge欄位(自定文件名)
    SetDoc_DesignatedDocName_Merge: async function (colName, docName, data) {
        let nowTime = admin.firestore.Timestamp.now();
        let doc = db.collection(colName).doc(docName);
        data["UID"] = docName;
        data["CreateTime"] = nowTime;
        const batch = db.batch();
        batch.set(doc, data, { merge: true });
        await batch.commit();
        return doc.id;
    },

    //更新單筆文件
    UpdateDoc: async function (colName, docName, data) {
        await db.collection(colName).doc(docName).update(data);
    },
    //更新多筆文件
    UpdateDocs: async function (datas) {
        await DocsBatchedUpdate_Recursive(datas, 0);
    },
    //刪除單筆
    DeleteDocByUID: async function (colName, uid) {
        await db.collection(colName).doc(uid).delete();
    },
    //刪除多筆文件
    DeleteDocsByUIDs: async function (colName, uids) {
        await DocsBatchedDelete_Recursive(colName, uids, 0);
    },
    //條件式刪除多筆文件
    DeleteDocs_WhereOperation: async function (colName, fieldName, operation, value) {
        let docs = await this.GetDocs_WhereOperation(colName, fieldName, operation, value);
        if (docs == null) return;
        let docUIDs = [];
        for (let doc of docs) {
            docUIDs.push(doc.id);
        }
        if (docUIDs.length != 0)
            await this.DeleteDocsByUIDs(colName, docUIDs);
    },

}
module.exports = methods;


//遞迴新增資料(跑一次最多更新500筆(Firebase的限制))
async function DocsBatchedAdd_Recursive(datas, startIndex, returnUIDs) {
    const batch = db.batch();
    let nextStartIndex = 0;
    let maxBatchSize = 500;//一次最多新增500筆(Firebase的限制)
    let curSize = 0;
    let nowTime = admin.firestore.Timestamp.now();
    for (let i = startIndex; i < datas.length; i++) {
        let colName = datas[i].ColName;
        //delete datas[i]["ColName"];
        let doc = null;
        if (!("UID" in datas[i])) {
            doc = db.collection(colName).doc();
            datas[i]["UID"] = doc.id;
        } else
            doc = db.collection(colName).doc(datas[i]["UID"]);

        datas[i]["CreateTime"] = nowTime;
        returnUIDs.push(datas[i]["UID"]);
        let newData = Object.assign({}, datas[i])//淺複製dic
        delete newData["ColName"]
        batch.create(doc, newData);
        nextStartIndex = i + 1;
        curSize++;
        if (curSize >= maxBatchSize)
            break;
    }
    await batch.commit();
    console.log("新增到第" + nextStartIndex + "筆資料");
    if (nextStartIndex < datas.length) {//資料還沒跑完就繼續
        await DocsBatchedAdd_Recursive(datas, nextStartIndex, returnUIDs);
    }
}
//遞迴更新資料(跑一次最多更新500筆(Firebase的限制))
async function DocsBatchedUpdate_Recursive(datas, startIndex) {
    const batch = db.batch();
    let nextStartIndex = 0;
    let maxBatchSize = 500;//一次最多更新500筆(Firebase的限制)
    let curSize = 0;
    for (let i = startIndex; i < datas.length; i++) {
        let colName = datas[i].ColName;
        let doc = db.collection(colName).doc(datas[i].UID);
        delete datas[i]["ColName"];
        batch.update(doc, datas[i]);
        nextStartIndex = i + 1;
        curSize++;
        if (curSize >= maxBatchSize)
            break;
    }
    await batch.commit();
    console.log("更新到第" + nextStartIndex + "筆資料");
    if (nextStartIndex < datas.length) {//資料還沒跑完就繼續
        await DocsBatchedUpdate_Recursive(datas, nextStartIndex);
    }
}

//遞迴移除資料(跑一次最多移除500筆(Firebase的限制))
async function DocsBatchedDelete_Recursive(colName, docIDs, startIndex) {
    const batch = db.batch();
    let nextStartIndex = 0;
    let maxBatchSize = 500;//一次最多更新500筆(Firebase的限制)
    let curSize = 0;
    for (let i = startIndex; i < docIDs.length; i++) {
        let doc = db.collection(colName).doc(docIDs[i]);
        batch.delete(doc);
        nextStartIndex = i + 1;
        curSize++;
        if (curSize >= maxBatchSize)
            break;
    }
    await batch.commit();
    console.log("刪除到第" + nextStartIndex + "筆資料");
    if (nextStartIndex < docIDs.length) {//資料還沒跑完就繼續
        await DocsBatchedDelete_Recursive(colName, docIDs, nextStartIndex);
    }
}
