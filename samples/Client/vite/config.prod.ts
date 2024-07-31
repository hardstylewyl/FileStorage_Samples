import { mergeConfig, type UserConfig } from 'vite'

import baseConfig from './config.base'
import configCompressPlugin from './plugin/compress'
import configImageminPlugin from './plugin/imagemin'
import configVisualizerPlugin from './plugin/visualizer'

// vite生产模式配置
export default mergeConfig(
	<UserConfig> {
		mode: 'production',
		base: '/',
		// 插件的具体配置请查看对应的文件
		plugins: [
			// 编译压缩
			configCompressPlugin('gzip'),
			// 打包分析
			configVisualizerPlugin(),
			// 图片压缩
			configImageminPlugin(),
		],
		// 清理
		esbuild: {
			pure: ['alert', 'console.log', 'console.warn', 'debugger'],
		},
		build: {
			chunkSizeWarningLimit: 2000,
			reportCompressedSize: false, // 禁用 gzip 压缩大小报告，可略微减少打包时间
			rollupOptions: {
				output: {
					// 分包策略优化
					manualChunks: {
						vue: ['vue'],
						vuePlugin: ['vue-router', 'pinia'],
						vueUse: ['@vueuse/core'],
					},
				},
				chunkFileNames: 'assets/js/[name]-[hash].js',
				entryFileNames: 'assets/js/[name]-[hash].js',
				assetFileNames: 'assets/[ext]/[name]-[hash].[ext]',
			},
		},
	},
	baseConfig,
)
