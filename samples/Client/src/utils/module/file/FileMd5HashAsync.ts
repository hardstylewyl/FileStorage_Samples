import type { ProgressReport } from '@/types'
import type { CancellationToken } from '@/utils'
import { FileSlicing } from '@/utils'
import sparkMd5 from 'spark-md5'

/*文件分片大小*/
const FILE_CHUNK_SIZE = 10 * 1024 * 1024

/**
 * 异步生成文件MD5
 * 支持取消，支持监听进度
 */
export async function FileMd5HashAsync(
	file: File,
	cancelToken?: CancellationToken,
	hashProgressReport?: ProgressReport,
): Promise<string> {
	// 如果在build开始前取消了则抛出异常
	cancelToken?.throwIfRequested()

	/*文件切片*/
	const chunks = FileSlicing(file, FILE_CHUNK_SIZE)
	/*开启异步任务*/
	return new Promise((resolve, reject) => {
		const spark = new sparkMd5.ArrayBuffer()

		function _read(chunk: number) {
			try {
				/*被取消*/
				cancelToken?.throwIfRequested()

				/*分片全部计算完成*/
				if (chunk >= chunks.length) {
					resolve(spark.end())
					return
				}

				const blob = chunks[chunk]
				const reader = new FileReader()
				reader.onload = function(e) {
					const bytes = e.target!.result
					spark.append(bytes as ArrayBuffer)
					/*进行进度报告*/
					hashProgressReport?.(chunk + 1, chunks.length)
					_read(chunk + 1)
				}
				reader.onerror = function() {
					reject(new Error('FileMd5HashAsync read Error'))
				}

				reader.readAsArrayBuffer(blob)
			} catch (e) {
				reject(e)
			}
		}

		_read(0)
	})
}
