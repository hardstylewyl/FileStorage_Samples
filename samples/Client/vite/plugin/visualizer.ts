import visualizer from 'rollup-plugin-visualizer'

/**
 * 配置视觉化插件，根据是否开启来决定是否生成打包分析报告。
 *
 * 该函数用于条件性地启用rollup打包分析插件。当插件被启用时，它会生成一个包含包大小和依赖关系图的HTML报告。
 * 这对于理解项目的构建输出和优化是非常有帮助的。
 *
 * @param isOpen - 指示是否启用插件的布尔值。默认为false，即默认不启用。
 * @returns 如果插件启用，则返回配置对象；如果插件未启用，则返回空数组。
 */
export default function configVisualizerPlugin(isOpen = false) {
	// 当isOpen为true时，配置并启用视觉化插件
	// 自己手动开启或者定义打包分析环境变量开启打包分析
	if (isOpen) {
		return visualizer({
			filename: './node_modules/.cache/visualizer/stats.html',
			open: true,
			gzipSize: true,
			brotliSize: true,
		})
	}

	// 如果isOpen为false，返回空数组，表示不启用任何插件
	return []
}
