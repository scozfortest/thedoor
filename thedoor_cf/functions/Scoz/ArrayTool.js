var methods = {
    //傳入Array與分割長度，將陣列依據指定長度切分成多個陣列
    //EX. 傳入陣列[1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22]與10，會回傳Array [Array [1, 2, 3, 4, 5, 6, 7, 8, 9, 10], Array [11, 12, 13, 14, 15, 16, 17, 18, 19, 20], Array [21, 22]]
    SplitArray: function (array, splitLength) {
        let result = [];
        while (array.length > splitLength) {
            result.push(array.splice(0, splitLength));
        }
        result.push(array);
        return result;
    },
    SortByBoolean: function (array, trueFirst) {
        array.sort(function (x, y) {
            if (trueFirst) {// true values first
                return (x === y) ? 0 : x ? -1 : 1;
            }
            else {// false values first
                return (x === y) ? 0 : x ? 1 : -1;
            }
        });
    },
    Count: function (array, item) {
        return array.filter(x => x == item).length;
    },
    //移除字典陣列中不包含某key值的項目EX. RemoveDontContainKeyItems( [{a:5,b:3},{a:3,b:1},{a:2},{a:3,b:1}], "b")陣列中 會把第三項移除，因為第三項不包含b
    GetArrayDictWhichDontContainKey: function (dictArray, key) {
        let newDictArray = dictArray.filter(obj => Object.keys(obj).includes(key));
        return newDictArray;
    },
    RemoveItem: function (array, item) {
        if (array == null)
            return;
        const index = array.indexOf(item);
        if (index > -1) {
            array.splice(index, 1);
        }
        return array;
    },
    RemoveItems: function (array, items) {
        if (array == null)
            return;
        for (let item of array) {
            RemoveItem(array, item);
        }
        return array;
    },
}
module.exports = methods;
