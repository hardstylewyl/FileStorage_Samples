import { type CheckFileResponse, FileUploadStatus } from '@/contracts'
import { api } from '@/services/api'
import type { ProgressReport, UploadContext } from '@/types'
import { type CancellationToken, FileMd5HashAsync } from '@/utils'
import { NativeUploadAsync } from './native-uploader'
import { TusUploadAsync, TusUploadSupported as isSupportTus } from './tus-uploader'

// 进度报告
const DefaultUploadProgressReport: ProgressReport = (loaded: number, total: number) => {
	console.log('upload progress ', loaded / total * 100 + '%')
}

const DefaultHashProgressReport: ProgressReport = (loaded: number, total: number) => {
	console.log('hash progress ', loaded / total * 100 + '%')
}

/*上传文件，进度报告（可选）*/
async function UploadAsync(
	context: UploadContext,
	cancelToken?: CancellationToken,
	uploadProgressReport = DefaultUploadProgressReport,
) {
	const { status, type } = context
	// 这五种状态不用进行上传 【失败，取消，同步，成功，同步失败】
	if (
		status === 'error'
		|| status === 'canceled'
		|| status === FileUploadStatus.inSynchronizing
		|| status === FileUploadStatus.completed
		|| status === FileUploadStatus.syncFailed
	) {
		return
	}

	if (type === 'tus') {
		await TusUploadAsync(context, cancelToken, uploadProgressReport)
	} else {
		await NativeUploadAsync(context, cancelToken, uploadProgressReport)
	}

	return context
}

/*构建上传上下文，用于跟踪上传状态*/
async function BuildContextAsync(
	file: File,
	cancelToken?: CancellationToken,
	hashProgressReport = DefaultHashProgressReport,
	isNative = false,
) {
	// 生成文件md5,可能抛出取消异常
	const md5 = await FileMd5HashAsync(file, cancelToken, hashProgressReport)
	// 构建文件id {md5}_{size}
	const fileId = `${md5}_${file.size}`
	// 构建文件上下文
	const context: UploadContext = {
		file,
		filename: file.name,
		extension: file.name.split('.').pop() ?? '',
		md5,
		fileId,
		status: 'error',
		type: isSupportTus && !isNative ? 'tus' : 'native',
	}

	const checkResponse = await UploadCheckAsync(context, cancelToken)

	// completed和inProgress需要填充上下文信息
	const state = checkResponse.Status
	if (state === FileUploadStatus.completed) {
		context.fileUrl = checkResponse.FileUrl!
	} else if (state === FileUploadStatus.inProgress) {
		context.missChunkList = checkResponse.MissChunkList!
	}

	context.status = state
	return context
}

/*检查上传状态，用于轮询上传状态*/
async function UploadCheckAsync(context: UploadContext, cancelToken?: CancellationToken): Promise<CheckFileResponse> {
	// 抛出取消异常
	cancelToken?.throwIfRequested()

	const { type, fileId } = context

	// 根据类型选择一个请求方式
	const request = api.CheckFile({ isTus: type === 'tus', fileId: fileId! })

	const offCanceled = cancelToken?.onCanceled(() => {
		console.log('中断正在进行的检查请求')
		request.abort()
	})

	// 发送请求
	const result = await request
		.send()
		.catch((reason) => {
			console.log('检查请求出现错误', reason)
			return Promise.reject(reason)
		})
		.finally(offCanceled)

	return result.Value!
}

// 直接上传
async function DirectUploadAsync(file: File) {
	const context = await BuildContextAsync(file, undefined, undefined, true)
	await UploadAsync(context)
	return context.fileUrl!
}

export const uploadService = {
	UploadAsync,
	BuildContextAsync,
	UploadCheckAsync,
	DirectUploadAsync,
}
