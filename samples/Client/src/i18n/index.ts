import type { App } from 'vue'
import { createI18n } from 'vue-i18n'

import { datetimeFormats } from './i18nFormat'
import { messages } from './i18nMessage'

import { LOCAL_KEYS } from '@/types'

const supportLanguages = Object.keys(messages)

const i18n = createI18n({
	// 当前语言
	locale: 'zh-CN',
	// 当语言不存在使用默认替换语言
	fallbackLocale: 'zh-CN',
	// 使用兼容模式 支持compositionApi
	legacy: false,
	// 语言包
	messages,
	// 日期格式化
	datetimeFormats,
})

const setLanguage = (lang: string | null) => {
	if (!lang) return
	i18n.global.locale.value = lang
	i18n.global.setLocaleMessage(lang, messages[lang])
	// 设置html的lang属性
	document.documentElement.setAttribute('lang', lang)
	// 存储当前语言
	localStorage.setItem(LOCAL_KEYS.LANGUAGE, lang)
}

const initI18n = () => {
	// 加载存储中的语言选项
	const lang = localStorage.getItem(LOCAL_KEYS.LANGUAGE)
	setLanguage(lang || 'zh-CN')
}

// 动态加载语言资源文件
// async function loadLocaleMessages(locale: string) {
//     // 这里可以换成axios从服务器加载
//     const messages = await import(/* webpackChunkName: "locale-[request]" */ `./i18n/lang/${locale}.json`)
//     // 用新的语言数据覆盖已有的语言数据（要使用哪种自己选择）
//     //   i18n.global.setLocaleMessage(locale, messages.default)
//     return nextTick()
// }

const setupI18n = (app: App) => {
	app.use(i18n)
}

export { initI18n, setLanguage, setupI18n, supportLanguages }
