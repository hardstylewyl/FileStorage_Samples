import { mergeConfig } from 'vite'
import checker from 'vite-plugin-checker'
import mkcert from 'vite-plugin-mkcert'
import baseConfig from './config.base'

// 开发缓解
export default mergeConfig({
	mode: 'development',
	base: '/',
	plugins: [
		// 运行时检查类型
		checker({
			vueTsc: true,
			stylelint: undefined,
		}),
		// ssl证书证书
		mkcert(),
	],
	server: {
		https: true,
		port: 3000,
		host: '0.0.0.0',
		open: false,
		fs: {
			// 文件打开是相对于根目录的绝对路径
			strict: true,
		},
		proxy: {
			// 授权中心地址代理
			// '/auth': {
			//     target: loadEnv('development', process.cwd()).VITE_AUTH_URL,
			//     changeOrigin: true,
			// },
		},
	},
}, baseConfig)
