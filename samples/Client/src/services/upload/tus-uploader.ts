import { FileUploadStatus, urls } from '@/contracts'
import { api } from '@/services/api'
import type { ProgressReport, UploadContext } from '@/types'
import type { CancellationToken } from '@/utils'
import * as tus from 'tus-js-client'
import type { PreviousUpload, UploadOptions, UrlStorage } from 'tus-js-client'

// 是否支持tus
export const TusUploadSupported = tus.isSupported

// 基于tus协议的文件上传,支持取消,支持上传的进度报告
export function TusUploadAsync(
	context: UploadContext,
	cancelToken?: CancellationToken,
	uploadProgressReport?: ProgressReport,
) {
	const {
		status,
		fileId,
		file,
		extension,
		md5,
	} = context

	// 【失败,同步,成功,同步失败】状态无需上传
	if (
		status === 'error'
		|| status === FileUploadStatus.inSynchronizing
		|| status === FileUploadStatus.completed
		|| status === FileUploadStatus.syncFailed
	) {
		return
	}

	return new Promise<void>((resolve, reject) => {
		try {
			// 被取消上传时
			cancelToken?.throwIfRequested()

			// 构建上传参数
			const options: UploadOptions = {
				endpoint: urls.TusEndpoint,
				// 并发数目 设置为1 避免并发上传
				parallelUploads: 1,
				// 重试时间间隔
				retryDelays: [0, 1000, 3000, 5000],
				// 文件元数据
				metadata: {
					filename: file.name,
					contentType: file.type || 'application/octet-stream',
					extension: extension,
					size: file.size.toString(),
					// hash过后这两个属性一定有值
					md5: md5!,
					fileId: fileId!,
				},
				// 替换为自己的storage实现
				urlStorage: remoteRedisStorage,
				// 不添加请求id
				// addRequestId: false,
				// 上传成功后删除文件指纹，README:不要删除
				// removeFingerprintOnSuccess: true,
				// 生成文件指纹，这里使用md5
				fingerprint: () => Promise.resolve(md5!),
				// 请求前配置
				onBeforeRequest: function (req) {
					// 配置请求前带cookies
					const xhr = req.getUnderlyingObject()
					xhr.withCredentials = true
					return Promise.resolve()
				},
				// 当出现错误了
				onError: function (error) {
					console.log('tus上传出错', error)
					offCanceled?.()
					reject(error)
				},
				// 进度发生变化
				onProgress: function (bytesUploaded, bytesTotal) {
					console.log('tus上传进度变化:', bytesUploaded, bytesTotal)
					// 上传进度报告
					uploadProgressReport?.(bytesUploaded, bytesTotal)
				},
				// 上传成功
				onSuccess: function () {
					console.log('tus上传成功')
					offCanceled?.()
					resolve()
				},
			}

			const upload = new tus.Upload(file, options)

			const offCanceled = cancelToken?.onCanceled(() => {
				console.log('tus取消上传')
				upload.abort()
					.then(cancelToken?.throwIfRequested)
					.catch(reason => reject(reason))
			})

			// 从上一次上传的文件中寻找与当前文件相同的文件指纹
			findUploadByFingerprintAndSize(md5!, file.size)
				.then(previousUpload => {
					// 如果当前文件已经上传过，则从上一次上传的断点处继续上传
					if (previousUpload) {
						console.log('tus上传续传', previousUpload)
						upload.resumeFromPreviousUpload(previousUpload)
					}

					upload.start()
				})
				.catch(() => {
					upload.start()
				})
		} catch (e) {
			reject(e)
		}
	})
}

// fingerprint：使用md5
// fileId：使用{md5}_{size} (增加size填充提高唯一性概率)
// urlStorageKey：保持与fileId定义一致
const remoteRedisStorage: UrlStorage = {
	async addUpload(fingerprint: string, upload: PreviousUpload): Promise<string> {
		const fileId = `${fingerprint}_${upload.size}`
		const result = await api.AddUpload({
			fileId,
			value: upload,
		})

		if (result.IsFailure) {
			throw new Error('addUpload Error:' + result.Error)
		}

		return fileId
	},
	async findAllUploads(): Promise<PreviousUpload[]> {
		const result = await api.FindAllUploads()

		if (result.IsFailure) {
			throw new Error('findAllUploads Error:' + result.Error)
		}

		return result.Value ?? []
	},
	// WARNING:不要使用tus.Upload().findPreviousUploads方法
	// 替换为findUploadByFingerprintAndSize的调用
	async findUploadsByFingerprint(_: string): Promise<PreviousUpload[]> {
		throw new Error('findUploadsByFingerprint not implement please use findUploadByFingerprintAndSize')
	},
	async removeUpload(urlStorageKey: string): Promise<void> {
		const result = await api.RemoveUpload(urlStorageKey)

		if (result.IsFailure) {
			throw new Error('removeUpload Error:' + result.Error)
		}

		console.info('removeUpload success key:' + urlStorageKey)
	},
}

// 查找文件上传记录，如果有可以恢复上传
async function findUploadByFingerprintAndSize(fingerprint: string, size: number): Promise<PreviousUpload | undefined> {
	const redisKey = `${fingerprint}_${size}`
	const result = await api.FindUploadsByFileId(redisKey)

	if (result.IsFailure) {
		throw new Error('findUploadsByFingerprint Error:' + result.Error)
	}

	const v = result.Value
	return v ? lowercaseFirstCharKeys(v) as any : undefined
}

// 对象属性名转为小写开头
function lowercaseFirstCharKeys(obj: any) {
	const newObj: { [key: string]: any } = {}

	// 遍历原对象的所有可枚举属性
	for (const key in obj) {
		// 检查属性不是从原型链继承的
		if (obj.hasOwnProperty(key)) {
			// 获取属性名的小写首字母版本
			const lowercaseKey = key.charAt(0).toLowerCase() + key.slice(1)
			newObj[lowercaseKey] = obj[key]
		}
	}

	return newObj
}
