/*获取字符串实际长度，中文2英文数字1*/
export function getCharacterCount(str: string) {
    let len = 0;
    for (let i = 0; i < str.length; i++) {
        let c = str.charCodeAt(i);
        //单字节加1
        if ((c >= 0x0001 && c <= 0x007e) || (0xff60 <= c && c <= 0xff9f)) {
            len++;
        } else {
            len += 2;
        }
    }
    return len;
}
/*截取字符串，中文2英文数字1*/
export function subStringCharacter(str: string, start: number, end: number) {
    let len = 0;
    let result = '';
    for (let i = 0; i < str.length; i++) {
        let c = str.charCodeAt(i);
        //单字节加1
        if ((c >= 0x0001 && c <= 0x007e) || (0xff60 <= c && c <= 0xff9f)) {
            len++;
        } else {
            len += 2;
        }
        if (len > start && len <= end) {
            result += str[i];
        }
    }
    return result;
}