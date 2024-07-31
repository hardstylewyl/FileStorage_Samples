import { resolve } from 'path'

import vue from '@vitejs/plugin-vue'
import { defineConfig } from 'vite'
// jsx支持插件
import vueJsx from '@vitejs/plugin-vue-jsx'
// 浏览器css兼容前缀插件
import autoprefixer from 'autoprefixer'
// 兼容低版本浏览器插件
// import legacy from '@vitejs/plugin-legacy'

export default defineConfig({
	// 插件配置
	plugins: [
		vue(),
		vueJsx(),
	],
	// 别名配置
	resolve: {
		alias: [
			{
				// 别名 @ => ../src
				find: '@',
				replacement: resolve(__dirname, '../src'),
			},
		],
	},
	// 在windows定义对象
	define: {
		'process.env': {},
	},
	// 样式附加/样式兼容
	css: {
		preprocessorOptions: {
			// 可以添加scss全局样式
			scss: {
				// additionalData: '@import "@/style.css";'
			},
		},
		postcss: {
			plugins: [
				// 自动添加css样式兼容前缀
				autoprefixer({
					overrideBrowserslist: ['iOS 7.1', 'last 2 versions'],
				}),
			],
		},
	},
})
