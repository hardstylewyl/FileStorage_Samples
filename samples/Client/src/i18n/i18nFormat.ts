// 不同地区日期格式化
export const datetimeFormats: Record<string, any> = {
	'en-US': {
		short: {
			year: 'numeric',
			month: 'short',
			day: 'numeric',
		},
		long: {
			year: 'numeric',
			month: 'long',
			day: 'numeric',
			weekday: 'long',
			hour: 'numeric',
			minute: 'numeric',
		},
	},
	'zh-CN': {
		short: {
			year: 'numeric',
			month: 'short',
			day: 'numeric',
		},
		long: {
			year: 'numeric',
			month: 'long',
			day: 'numeric',
			weekday: 'long',
			hour: 'numeric',
			minute: 'numeric',
			// 默认是24小时制，如果想显示为12小时制，则进行下面这个属性的设置
			//   hour12: true,
		},
	},
}
