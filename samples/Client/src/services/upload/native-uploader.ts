import { FileUploadStatus, type FragmentUploadRequest } from '@/contracts'
import { useWorkerMD5 } from '@/hook/useWorkerMd5'
import { api } from '@/services/api'
import type { CalculateChunkRequest, CalculateChunkResponse, ProgressReport, UploadContext } from '@/types'
import { type CancellationToken, FileSlicing } from '@/utils'

// 最大文件大小(字节) 5mb
const MAX_FILE_SIZE = 5 * 1024 * 1024
// 分片大小 5mb
const CHUNK_SIZE = 5 * 1024 * 1024

// 原生文件上传
export async function NativeUploadAsync(
	context: UploadContext,
	cancelToken?: CancellationToken,
	uploadProgressReport?: ProgressReport,
) {
	const { status, file } = context

	// 这四种状态不用进行上传 【失败，同步，成功，同步失败】
	if (
		status === 'error'
		|| status === FileUploadStatus.inSynchronizing
		|| status === FileUploadStatus.completed
		|| status === FileUploadStatus.syncFailed
	) {
		return
	}

	/*在上传前检查是否被取消*/
	cancelToken?.throwIfRequested()

	// 文件不存在
	if (status === FileUploadStatus.notExist) {
		console.log('文件不存在，开始上传', context)
		// 根据文件大小选择一种文件上传方式
		if (file.size > MAX_FILE_SIZE) {
			// 大文件分片上传
			await Native_FragmentUpload(context, cancelToken, uploadProgressReport)
		} else {
			// 小文件直接上传
			await Native_UploadFile(context, cancelToken, uploadProgressReport)
		}
	}

	// 文件处于上传中
	if (status === FileUploadStatus.inProgress) {
		console.log('native上传续传', context)
		// 进行续传逻辑...
		await Native_FragmentUpload(context, cancelToken, uploadProgressReport)
	}
}

// 直接上传（不分片）
async function Native_UploadFile(
	context: UploadContext,
	cancelToken?: CancellationToken,
	uploadProgressReport?: ProgressReport,
) {
	// 构建文件传输请求
	const uploadRequest = api.DirectUpload({
		fileId: context.fileId!,
		filename: context.filename,
		extension: context.extension,
		md5: context.md5!,
		file: context.file,
	})

	// 上传进度监听绑定
	const offUpload = uploadRequest.onUpload(x => uploadProgressReport?.(x.loaded, x.total))

	// 取消回调
	const offCanceled = cancelToken?.onCanceled(() => {
		uploadRequest.abort()
	})

	// 发送请求
	const uploadResult = await uploadRequest
		.send()
		.catch(reason => {
			console.log('[native upload 直传]文件上传失败', reason)
			return Promise.reject(reason)
		})
		.finally(() => {
			offUpload()
			offCanceled?.()
		})

	// 成功或者失败更新上下文信息
	if (uploadResult.IsSuccess) {
		context.status = FileUploadStatus.completed
		context.fileUrl = uploadResult.Value!.FileUrl
		console.log('[native upload 直传]文件上传成功', uploadResult.Value)
	} else {
		context.status = 'error'
		context.errorMsg = uploadResult.Error!.Message
		console.log('[native upload 直传]文件上传失败', uploadResult.Error)
	}
}

// 分片上传
async function Native_FragmentUpload(
	context: UploadContext,
	cancelToken?: CancellationToken,
	uploadProgressReport?: ProgressReport,
) {
	const {
		file,
		fileId,
		filename,
		extension,
		missChunkList,
	} = context

	// 文件先进行分片
	const fileChunks = FileSlicing(file, CHUNK_SIZE)
	const chunkCount = fileChunks.length
	console.log('分片上传', chunkCount)

	// 分片计算请求集合
	let calcRequests: CalculateChunkRequest[] = []
	// 分片计算结果集合,如果有缓存的话
	const calcResults: CalculateChunkResponse[] = context.calcResults ?? []
	// 暂存计算结果
	context.calcResults = calcResults
	// 如果文件是第一次上传，需要在服务器存储上传信息
	if (missChunkList === undefined) {
		const createRequester = api.FragmentUploadCreate({
			fileId: fileId!,
			filename: filename,
			extension: extension,
			chunkCount: chunkCount,
			chunkSize: CHUNK_SIZE,
		})

		/*更新context的missChunkList*/
		context.missChunkList = Array.from({ length: chunkCount }, (_, index) => index)

		/*销毁取消回调*/
		const offCanceled = cancelToken?.onCanceled(() => {
			createRequester.abort()
		})

		// 发起创建请求，请求取消/过程失败 都抛出异常
		await createRequester
			.send()
			.then(result => {
				if (result.IsFailure) {
					throw new Error(result.Error!.Message)
				}
			})
			.catch(reason => {
				if (cancelToken?.isCanceled) {
					console.warn('[native upload 分片]文件上传取消')
				} else {
					console.log('[native upload 分片]文件上传失败', reason)
				}
				return Promise.reject(reason)
			})
			.finally(() => {
				offCanceled?.()
			})

		// 第一次上传，需要计算所有分片的md5
		fileChunks.forEach((value: Blob, index: number) => {
			calcRequests.push({
				chunkData: value,
				fileId: fileId!,
				chunkSeq: index,
			})
		})
	} // 断点续传，只填充缺失的分片数据
	else {
		for (let i of missChunkList) {
			const chunk = fileChunks[i]
			if (!chunk) {
				throw new Error('[native upload 分片]分片不存在, 文件终止上传')
			}
			calcRequests.push({
				chunkData: chunk,
				fileId: fileId!,
				chunkSeq: i,
			})
		}
	}

	// 排除计算结果缓存，无需计算
	const completedCalcChunkSeqs = calcResults.map(x => x.chunkSeq)
	calcRequests = calcRequests
		.filter(x => !completedCalcChunkSeqs.includes(x.chunkSeq))

	// 成功请求数目，用于统计上传进度
	let successRequest = missChunkList !== undefined
		? chunkCount - missChunkList.length
		: 0

	return new Promise<void>((resolve, reject) => {
		let isErrored = false
		// 先利用缓存结果来进行请求
		calcResults.forEach(calcResult => {
			// 放入请求队列
			pushUploadRequestTask(buildRequest(calcResult))
		})

		if (calcRequests.length === 0) return

		const {
			onSuccessCalc,
			onFinished,
		} = useWorkerMD5(calcRequests, cancelToken)

		// 分片计算成功回调，每个分片计算成功后，放入请求队列
		onSuccessCalc(calcResult => {
			calcResults.push(calcResult)
			// 放入请求队列
			pushUploadRequestTask(buildRequest(calcResult))
		})

		onFinished(() => {
			console.log('[native upload 分片] 分片结果全部计算完成')
		})

		// 构建上传请求
		function buildRequest(result: CalculateChunkResponse) {
			return <UploadRequestTask> {
				cancelToken,
				request: {
					fileId: result.fileId,
					chunkSeq: result.chunkSeq,
					md5: result.md5,
					file: fileChunks[result.chunkSeq]!,
				},
				onError(reason: any) {
					if (isErrored) return
					isErrored = true
					reject(reason)
				},
				onSuccess() {
					uploadProgressReport?.(++successRequest, chunkCount)
					// 上传成功删除缺少的分片 避免重复 计算+上传
					context.missChunkList = context.missChunkList?.filter(x => x !== result.chunkSeq)
					context.calcResults = context.calcResults?.filter(x => x.chunkSeq !== result.chunkSeq)
					if (context.missChunkList && context.missChunkList.length === 0) {
						resolve()
					}
				},
			}
		}
	})
}

type UploadRequestTask = {
	request: FragmentUploadRequest
	cancelToken?: CancellationToken
	onSuccess: () => void
	onError: (reason: any) => void
}

// 最大请求数量限制最大请求数量为1
const MAX_CONCURRENT_TASKS = 4
// 活跃的任务数量
let activeTasks = 0
// 任务队列
const taskQueue: UploadRequestTask[] = []

// 添加一个请求上传任务
function pushUploadRequestTask(task: UploadRequestTask) {
	// 将新的请求参数加入队列
	taskQueue.push(task)

	// 如果当前没有活跃任务，则开始处理队列中的下一个任务
	if (activeTasks < MAX_CONCURRENT_TASKS) {
		processUploadTask()
	}
}

// 处理队列中的请求，这个方法以限制的情况执行
function processUploadTask() {
	if (taskQueue.length === 0 || activeTasks >= MAX_CONCURRENT_TASKS) {
		return
	}

	// 请求数据，请求器，绑定取消回调
	const {
		request,
		cancelToken,
		onError,
		onSuccess,
	} = taskQueue.shift()!

	try {
		// 检查是否已经取消
		cancelToken?.throwIfRequested()

		// 活跃任务数目+1
		activeTasks++

		const requester = api.FragmentUpload(request)
		const offCanceled = cancelToken?.onCanceled(() => {
			requester.abort()
		})

		requester
			.send()
			.then(result => {
				if (result.IsFailure) {
					throw new Error(result.Error!.Message)
				}
				onSuccess()
				console.log('[native upload 分片上传]--Seq=[', request.chunkSeq, '] 上传状态:', 'success', result)
			})
			.catch((reason) => {
				console.log('[native upload 分片上传]--Seq=[', request.chunkSeq, '] 上传状态:', 'error', reason)
				return Promise.reject(reason)
			})
			.finally(() => {
				offCanceled?.()
				activeTasks--
				processUploadTask()
			})
	} catch (reason) {
		onError(reason)
	}
}
