// 如果值为 null | undefined | value.trim() ==='' 则返回false | string
export function checkNotEmpty(message?: string) {
	return async function(value: any) {
		if (value === null || value === undefined) {
			return message ? message : false
		}
		if (typeof value === 'string' && value.trim().length <= 0) {
			return message ? message : false
		}

		return true
	}
}
