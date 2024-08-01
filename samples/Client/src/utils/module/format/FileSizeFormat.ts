/*文件大小的单位*/
export enum FileSizeUnit {
    B = 0,
    KB = 1,
    MB = 2,
    GB = 3,
    TB = 4,
    PB = 5,
    EB = 6,
    ZB = 7,
    YB = 8
}

/*文件大小单位的缩写*/
const unitAbbreviations = ['B', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB']

/*给定一个文件大小Bytes和单位，返回一个指定单位的大小*/
export function convertSizeToUnit(bytes: number, unit: FileSizeUnit) {
    return bytes / 1024 ** unit
}

/*给定一个文件大小Bytes，返回一个对象，包含单位缩写、单位索引和单位大小*/
export function findFileSizeUnit(bytes: number) {
    const index = Math.floor(Math.log(bytes) / Math.log(1024))
    return {
        unitAbbreviation: unitAbbreviations[index],
        unitIndex: index as FileSizeUnit,
        sizeInUnit: convertSizeToUnit(bytes, index)
    }
}

/*给定一个文件大小Bytes，返回一个格式化后的字符串*/
export function formatFileSize(bytes: number) {
    if (bytes === 0) {
        return '0 Bytes'
    }

    const {unitAbbreviation, unitIndex, sizeInUnit} = findFileSizeUnit(bytes)
    //保留位数，由单位而定 B 0位 KB 1位 >=MB 2位
    const decimals = unitIndex < FileSizeUnit.MB ? unitIndex : 2
    return `${sizeInUnit.toFixed(decimals)} ${unitAbbreviation}`
}




