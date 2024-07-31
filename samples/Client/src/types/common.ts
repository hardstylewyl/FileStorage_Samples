import type { FileUploadStatus } from '@/contracts'
import type { CancellationToken } from '@/utils'

/**
 * LocalStorage的缓存key
 */
export const LOCAL_KEYS = {
	// 语言
	LANGUAGE: 'language',
	// 主题
	THEME: 'theme',
}

/*worker分片计算请求*/
export type CalculateChunkRequest = {
	/*文件唯一标识*/
	fileId: string
	/*分片序号*/
	chunkSeq: number
	/*分片数据*/
	chunkData: Blob
}

/*worker分片计算响应*/
export type CalculateChunkResponse = {
	/*文件唯一标识*/
	fileId: string
	/*分片序号*/
	chunkSeq: number
	/*分片md5*/
	md5: string
}

/*进度报告*/
export type ProgressReport = (loaded: number, total: number) => void

/*上传过程的上下文*/
export type UploadContext = {
	file: File
	filename: string
	extension: string
	// 如果为undefined说明在hash(md5)过程任务被取消 fileId构成: {md5}_{size}
	md5?: string
	fileId?: string

	// 选用了哪种方式进行上传
	type?: 'tus' | 'native'
	// 文件状态
	status: FileUploadStatus | 'canceled' | 'error'
	// 缺失的分片列表 在NativeUpload中使用 根据文件状态为【FileUploadStatus.inProgress】可以拿到
	// 在上传过程中进行不断更新以便于支持NativeUpload暂停恢复操作
	missChunkList?: number[]
	// md5计算结果缓存 在NativeUpload中使用
	// 目的为了支持NativeUpload暂停恢复操作
	calcResults?: CalculateChunkResponse[]
	// 错误消息
	errorMsg?: string
	// 文件访问url 根据文件状态为【FileUploadStatus.completed】可以拿到
	fileUrl?: string
}

/*上传任务,用于观测上传的实时状态*/
export type UploadTask = {
	// 上传上下文
	context?: UploadContext
	// 取消令牌
	cancelToken?: CancellationToken

	file: File
	filename: string
	extension: string

	uploadStatus: 'waiting' | 'hashing' | 'uploading' | 'syncing' | 'success' | 'paused' | 'canceled' | 'error'
	hashProgress: number
	uploadProgress: number
	// 成功回调
	onSuccess?: (fileUrl: string) => void
	// 失败回调
	onError?: (errorMsg: string) => void
	// 取消回调
	onCancel?: () => void
}
