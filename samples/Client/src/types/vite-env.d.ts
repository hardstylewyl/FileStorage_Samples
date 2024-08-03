/// <reference types="vite/client" />

// vue文件默认导出
// declare module '*.vue' {
// 	import { DefineComponent } from 'vue'
// 	const component: DefineComponent<{}, {}, any>
// 	export default component
// }
// .env文件的环境变量类型声明
interface ImportMetaEnv {

	VITE_API_BASE_URL: string
}

interface ImportMeta {
	readonly env: ImportMetaEnv
}
