//基本設定
const ArrayTool = require('../Scoz/ArrayTool.js');

var methods = {
    //傳入0.x機率來獲得是否抽重的結果 ex. 傳入0.3就是有30%機率返回true
    GetResult: function (probability) {
        return Math.random() < probability;
    },
    //取得隨機整數0~(max-1)
    GetRandInt: function (max) {
        return GetRandomInt(max);
    },
    //取得多個隨機整數0~(max-1)
    GetRandInts: function (max, count) {
        let randoms = [];
        for (let i = 0; i < count; i++)
            randoms.push(GetRandomInt(max));
        return randoms;
    },
    //取得隨機整數介於min~(max-1)
    GetRandIntBetween: function (min, max) {
        return GetRandomIntBetween(min, max);
    },
    //取得隨機整數介於min~(max-1)
    GetRandFloatBetween: function (min, max, decimals) {
        return GetRandomFloatBetween(min, max, decimals);
    },
    //從陣列中取得隨機內容
    GetRandFromArray: function (myArray) {
        if (myArray == null || myArray.length == 0)
            return null;
        let randIndex = GetRandomInt(myArray.length);
        return myArray[randIndex];
    },
    //從字典{key:weight)中依據權重取得隨機key
    GetRandKeyByWeight: function (dic) {
        if (dic == null || Object.keys(dic).length == 0)
            return null;
        let totalWeight = Object.values(dic).reduce((acc, weight) => acc + weight, 0);
        let randomValue = Math.random() * totalWeight;

        let accumulatedWeight = 0;
        for (const key in dic) {
            accumulatedWeight += dic[key];
            if (randomValue <= accumulatedWeight) {
                return key;
            }
        }
        return null
    },
    //取得數個不重複隨機整數0~(max-1)，傳入數值不合理 或 要取的數量大於max就回傳空陣列
    GetRandNoDuplicatedIndexFromMax: function (max, count) {
        if (max <= 0 || count <= 0)
            return [];
        let result = [];

        if (count < max) {
            tmpList = [];
            for (let i = 0; i < max; i++) {
                tmpList.push(i);
            }
            for (let i = 0; i < count; i++) {
                let pickedItem = GetRandomTFromTList(tmpList);
                let removeIndex = tmpList.indexOf(pickedItem);
                tmpList.splice(removeIndex, 1);
                result.push(pickedItem);
            }
        }
        else if (count == max) {
            for (let i = 0; i < max; i++) {
                result.push(i);
            }
        }
        return result;
    },
    //取得數個不重複隨機整數0~(max-1)，傳入數值不合理 或 要取的數量大於max就回傳空陣列
    GetRandNoDuplicatedItems: function (items, count) {
        if (items == null || items == undefined || items.length == 0 || count <= 0) {
            console.log("傳入資料錯誤");
            return [];
        }

        if (count > items.length) {
            console.log("要取的數量大於陣列長度");
            return [];
        }
        if (count == items.length)
            return items;

        let pickedItems = [];

        for (let i = 0; i < count; i++) {
            let item = this.GetRandFromArray(items);
            pickedItems.push(item);
            items = ArrayTool.RemoveItem(items, item);
        }

        return pickedItems;
    },
    //傳入權重陣列，根據權重隨機挑出索引 EX. 傳入[9,1] 就是有90%回傳0，10%回傳1 
    GetIndexByWeight: function (weights) {
        if (weights.length == 0) {
            console.log("傳入的權重陣列至少長度要1以上");
            return -1;
        }
        if (weights.length == 1) {
            return 0;
        }
        let index = 0;
        let sumWeight = 0;
        for (let i = 0; i < weights.length; i++) {//計算總權重
            sumWeight += weights[i];
        }
        let randWeight = GetRandomInt(sumWeight);
        for (let i = 0; i < weights.length; i++) {
            randWeight -= weights[i];
            if (randWeight < 0) {
                index = i;
                break;
            }
        }
        return index;
    },


    //傳入權重陣列，根據權重依序隨機挑出索引 EX. 傳入[9,1] 就是有90%回傳[0,1]，10%回傳[1,0] 
    GetIndexsByWeights: function (weights) {
        if (weights.length == 0) {
            console.log("傳入的權重陣列至少長度要1以上");
            return -1;
        }
        if (weights.length == 1) {
            return 0;
        }
        let resultIndexs = [];//結果Index陣列
        for (let pickTimes = 0; pickTimes < weights.length; pickTimes++) {
            let index = 0;
            let sumWeight = 0;
            for (let i = 0; i < weights.length; i++) {//計算總權重
                sumWeight += weights[i];
            }
            //根據總權重隨機取出此次挑出的Index
            let randWeight = GetRandomInt(sumWeight);
            for (let i = 0; i < weights.length; i++) {
                randWeight -= weights[i];
                if (randWeight < 0) {
                    index = i;
                    break;
                }
            }
            //設定結果Index陣列
            resultIndexs[index] = pickTimes;
            //把被取出的該項權重設為0，代表不會再被取到
            weights[index] = 0;
        }
        return resultIndexs;
    },

    //傳入items(帶有權重的字典陣列)與weightKey(權種的key名稱)，根據字典中的權重隨機挑出item
    GetItemByWeight: function (items, weightKey) {
        if (items.length == 0) {
            console.log("傳入的item陣列至少長度要1以上");
            return -1;
        }
        if (items.length == 1) {
            return items[0];
        }
        let index = 0;
        let sumWeight = 0;
        let weights = [];
        for (let item of items) {
            if (!(weightKey in item))
                continue;
            let weight = Number(item[weightKey]);
            sumWeight += weight; //計算總權重
            weights.push(weight);
        }
        let randWeight = GetRandomInt(sumWeight);
        for (let i = 0; i < weights.length; i++) {
            randWeight -= weights[i];
            if (randWeight < 0) {
                index = i;
                break;
            }
        }
        return items[index];
    },
    //傳入items(帶有權重的字典陣列)與數量x，根據字典中的權重隨機取出x個item(會重複取)
    GetItemsByWeights: function (items, weightKey, count) {
        if (items.length == 0) {
            console.log("傳入的item陣列至少長度要1以上");
            return null;
        }
        let pickedItems = [];
        for (let i = 0; i < count; i++) {
            pickedItems.push(this.GetItemByWeight(items, weightKey));
        }
        return pickedItems;
    },
    //傳入權重陣列與取出數量，根據權重隨機取出不重複數量的索引陣列
    GetRandomDistinctIndices: function (items, count) {
        if (Object.keys(items).length < count) {
            throw new Error("GetRandomDistinctIndices中 傳入的item陣列少於要取出的數量");
        }
        let keyWeights = {};
        let i = 0;
        for (let i = 0; i < items.length; i++) {
            keyWeights[i] = items[i];
        }

        const indices = [];
        for (let i = 0; i < count; i++) {
            const k = GetIndexFromKeyWeights(keyWeights);
            indices.push(k);
            delete keyWeights[k];
        }
        return indices;
    }

}
module.exports = methods;

function GetRandomInt(max) {
    return Math.floor(Math.random() * max);
}
function GetRandomTFromTList(list) {
    let index = GetRandomInt(list.length);
    return list[index];
}
function GetRandomIntBetween(min, max) { // min and max included 
    if (isNaN(min) || isNaN(max))
        return null;
    if (min == max)
        return min;
    return Math.floor(Math.random() * (max - min) + min)
}

function GetRandomFloatBetween(min, max, decimals) { // min and max included 
    if (isNaN(min) || isNaN(max))
        return null;
    if (min == max)
        return min;
    return parseFloat((Math.random() * (max - min) + min).toFixed(decimals))
}





function GetIndexFromKeyWeights(keyWeights) {
    const keys = Object.keys(keyWeights);
    if (keys.length === 0) {
        throw new Error("GetIndexFromWeights傳入的keyWeight長度為0");
    }
    let allWeight = 0;
    keys.forEach(key => {
        allWeight += keyWeights[key];
    })

    let randNum = GetRandomInt(allWeight);
    for (let k of keys) {
        randNum -= keyWeights[k];
        if (randNum < 0) {
            return parseInt(k, 10);
        }
    }
    throw new Error("GetIndexFromWeights錯誤");
}
