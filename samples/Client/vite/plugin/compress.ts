import type { Plugin } from 'vite'
import compressPlugin from 'vite-plugin-compression'

/**
 * 配置压缩插件。
 *
 * 根据传入的压缩类型（gzip 或 brotli），生成对应的压缩插件配置。
 * 如果需要，可以删除原始文件。
 *
 * @param compress 压缩类型，可选值为 'gzip' 或 'brotli'。
 * @param deleteOriginFile 是否删除原始文件，默认为 false。
 * @returns 返回一个 Plugin 对象或 Plugin 对象的数组，用于配置压缩插件。
 */
export default function configCompressPlugin(
	compress: 'gzip' | 'brotli',
	deleteOriginFile = false,
): Plugin | Plugin[] {
	// 初始化插件数组
	const plugins: Plugin[] = []

	// 根据传入的压缩类型，决定是否添加 gzip 压缩插件
	if (compress === 'gzip') {
		plugins.push(
			compressPlugin({
				ext: '.gz',
				deleteOriginFile,
			}),
		)
	}

	// 根据传入的压缩类型，决定是否添加 brotli 压缩插件
	if (compress === 'brotli') {
		plugins.push(
			compressPlugin({
				ext: '.br',
				algorithm: 'brotliCompress',
				deleteOriginFile,
			}),
		)
	}

	// 返回配置好的插件数组
	return plugins
}
