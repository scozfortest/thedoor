var methods = {
    //傳入Array與分割長度，將陣列依據指定長度切分成多個陣列
    ToBoolean: function (text) {
        if ((text === 'true') || (text === 'True') || (text === 'TRUE') || (text === true))
            return true;
        return false;
    },
    //將傳入的字串轉為數字，如果不是數字就轉為0
    ToNumber: function (text) {
        let number = Number(text);
        if (isNaN(number))
            number = 0;
        return number;
    },
    //將傳入的字串轉為Int(無條件捨去)
    ToInt: function (text) {
        let number = Number(text);
        if (isNaN(number))
            number = 0;
        number = Math.floor(number)
        return number;
    },
    SplitToNums: function (str, char) {
        return str
            .split(char)
            .map(Number)
            .filter(value => !isNaN(value));
    },
    SplitToInts: function (str, char) {
        return str
            .split(char)
            .map(Number)
            .filter(value => !isNaN(value) && Number.isInteger(value));
    },
}
module.exports = methods;
