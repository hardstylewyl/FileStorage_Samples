import viteImagemin from 'vite-plugin-imagemin'

/**
 * 配置并返回ImageMin插件实例。
 *
 * 该函数用于初始化和配置vite-imagemin插件，该插件用于在构建过程中优化图像文件。
 * 通过设置不同的优化级别和参数，可以针对不同的图像格式进行压缩和优化，以减少最终生成的资源大小。
 *
 * @returns {Plugin} 返回配置好的ImageMin插件实例。
 */
export default function configImageminPlugin() {
	// 使用viteImagemin函数配置并初始化ImageMin插件
	const imageminPlugin = viteImagemin({
		// 配置GIF处理选项，包括优化级别和是否使用交错方式
		gifsicle: {
			optimizationLevel: 7,
			interlaced: false,
		},
		// 配置PNG处理选项，包括优化级别
		optipng: {
			optimizationLevel: 7,
		},
		// 配置JPEG处理选项，包括质量设置
		mozjpeg: {
			quality: 20,
		},
		// 配置PNG处理选项，包括质量范围和速度设置
		pngquant: {
			quality: [0.8, 0.9],
			speed: 4,
		},
		// 配置SVG处理选项，包括使用哪些插件进行优化
		svgo: {
			plugins: [
				// 使用removeViewBox插件移除viewBox属性
				{
					name: 'removeViewBox',
				},
				// 禁用removeEmptyAttrs插件，避免移除空属性
				{
					name: 'removeEmptyAttrs',
					active: false,
				},
			],
		},
	})
	// 返回配置好的ImageMin插件实例
	return imageminPlugin
}
