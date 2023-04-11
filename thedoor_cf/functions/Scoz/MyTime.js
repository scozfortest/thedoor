var methods = {
  //將Date轉為dd/MM/yyyy HH:mm:ss格式
  ConvertToScozTimeStr: function (date) {
    var YY = date.getFullYear();
    var MM = (date.getMonth() + 1 < 10 ? "0" + (date.getMonth() + 1) : date.getMonth() + 1);
    var DD = date.getDate() < 10 ? "0" + date.getDate() : date.getDate();
    var hh = (date.getHours() < 10 ? "0" + date.getHours() : date.getHours());
    var mm = (date.getMinutes() < 10 ? "0" + date.getMinutes() : date.getMinutes());
    var ss = date.getSeconds() < 10 ? "0" + date.getSeconds() : date.getSeconds();
    let time = DD + "/" + MM + "/" + YY + " " + hh + ':' + mm + ':' + ss;
    return time;
  },
  GetDateFromStr: function (str) {
    let index = GetPosition(str, '-', 3);
    str = ReplaceAt(str, index, " ");
    return new Date(str);
  },
  CheckIfDatesAreTheSameDay: function (date1, date2) {
    return DatesAreTheSameDay(date1, date2);
  },
  GetDateDiff_Sec: function (date1, date2) {
    let diff = date1 - date2;
    let seconds = diffSeconds(diff);//Math.floor(diff / (1000));
    return seconds;
  },
  GetDateDiff_Min: function (date1, date2) {
    let diff = date1 - date2;
    let mins = diffMinutes(diff);//Math.floor(diff / (1000 * 60));
    return mins;
  },
  GetDateDiff_Hour: function (date1, date2) {
    let diff = date1 - date2;
    let hours = diffHours(diff);//Math.floor(diff / (1000 * 60 * 60));
    return hours;
  },
  GetDateDiff_Day: function (date1, date2) {
    let diff = date1 - date2;
    let days = diffDays(diff);//Math.floor(diff / (1000 * 60 * 60 * 24));
    return days;
  },
  AddSecs: function (date, secs) {
    let newDate = new Date(date).addSeconds(secs);
    return newDate;
  },
  AddMins: function (date, mins) {
    let newDate = new Date(date).addMinutes(mins);
    return newDate;
  },
  AddHours: function (date, hours) {
    let newDate = new Date(date).addHours(hours);
    return newDate;
  },
  AddDays: function (date, days) {
    let newDate = new Date(date).addDays(days);
    return newDate;
  },
  AddMonths: function (date, months) {
    let newDate = new Date(date).addMonths(months);
    return newDate;
  },
  AddYears: function (date, years) {
    let newDate = new Date(date).addYears(years);
    return newDate;
  },

}
module.exports = methods;


function GetPosition(string, subString, index) {
  return string.split(subString, index).join(subString).length;
}

function ReplaceAt(str, index, replacement) {
  return str.substr(0, index) + replacement + str.substr(index + replacement.length);
}

Date.prototype.format = function (fmt) {
  var o = {
    "M+": this.getMonth() + 1, //月份
    "d+": this.getDate(), //日
    "h+": this.getHours(), //小時
    "m+": this.getMinutes(), //分
    "s+": this.getSeconds(), //秒
    "q+": Math.floor((this.getMonth() + 3) / 3), //季度
    "S": this.getMilliseconds() //毫秒
  };
  if (/(y+)/.test(fmt)) fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
  for (var k in o)
    if (new RegExp("(" + k + ")").test(fmt)) fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
  return fmt;
}

Date.prototype.addSeconds = function (seconds) {
  this.setSeconds(this.getSeconds() + seconds);
  return this;
}

Date.prototype.addMinutes = function (minutes) {
  this.setMinutes(this.getMinutes() + minutes);
  return this;
}

Date.prototype.addHours = function (hours) {
  this.setHours(this.getHours() + hours);
  return this;
}

Date.prototype.addDays = function (days) {
  this.setDate(this.getDate() + days);
  return this;
}

Date.prototype.addMonths = function (months) {
  this.setMonth(this.getMonth() + months);
  return this;
}

Date.prototype.addYears = function (years) {
  this.setFullYear(this.getFullYear() + years);
  return this;
}

function diffSeconds(milliseconds) {
  return Math.floor(milliseconds / 1000);
}

function diffMinutes(milliseconds) {
  return Math.floor(milliseconds / 1000 / 60);
}

function diffHours(milliseconds) {
  return Math.floor(milliseconds / 1000 / 60 / 60);
}

function diffDays(milliseconds) {
  return Math.floor(milliseconds / 1000 / 60 / 60 / 24);
}
function DatesAreTheSameDay(date1, date2) {
  if (date1.getFullYear() === date2.getFullYear() &&
    date1.getMonth() === date2.getMonth() &&
    date1.getDate() === date2.getDate()) {
    return true;
  }
}