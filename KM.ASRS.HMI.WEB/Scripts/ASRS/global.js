/**
 * 格式化文字
 * @param {any} formatStr 結果字串
 * @param {any} partStrArray 插入值
 */
String.prototype.Format = function (partStrArray) {
    var tmp = this;
    $.each(partStrArray, function (index, partStr) {
        tmp = tmp.replace('{' + index + '}', partStrArray[index]);
    });
    return tmp;
}

/**
 * 格式化時間
 * @param {any} fmt
 */
Date.prototype.Format = function (fmt) { //author: meizz 
    var o = {
        "M+": this.getMonth() + 1, //月份 
        "d+": this.getDate(), //日 
        "H+": this.getHours(), //小时 
        "m+": this.getMinutes(), //分 
        "s+": this.getSeconds(), //秒 
        "q+": Math.floor((this.getMonth() + 3) / 3), //季度 
        "S": this.getMilliseconds() //毫秒 
    };
    if (/(y+)/.test(fmt))
        fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    for (var k in o)
        if (new RegExp("(" + k + ")").test(fmt))
            fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
    return fmt;
}