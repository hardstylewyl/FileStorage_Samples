import type { ProgressReport, UploadTask } from '@/types'
import { reactive, ref, type Reactive } from 'vue'
import { type CancellationTokenSource, cancelTokenUtil } from '@/utils'
import { FileUploadStatus } from '@/contracts'
import { uploadService } from '@/services/upload'
import { defineStore } from 'pinia'

/*最大并行上传任务数目*/
const MAX_UPLOAD_TASK_NUM = 3
/*任务调度定时器间隔*/
const DISPATCH_INTERVAL = 1500
export const useUploadQueueStore = defineStore('upload-queue', () => {
	/*上传任务队列*/
	const runningQueue = reactive<UploadTask[]>([])
	/*等待队列*/
	const waitingQueue = reactive<UploadTask[]>([])
	/*任务和取消令牌源映射*/
	const cancelTokenMap = new WeakMap<UploadTask, CancellationTokenSource>()
	/*调度定时器*/
	const dispatchTimer = ref<NodeJS.Timeout | null>(null)


	/*开始调度器定时器*/
	function _startDispatchTimer() {
		if (dispatchTimer.value) {
			return
		}

		dispatchTimer.value = setInterval(_TryDispatch, DISPATCH_INTERVAL)
	}

	/*立即尝试调度一个任务*/
	function _TryDispatch() {
		if (runningQueue.length >= MAX_UPLOAD_TASK_NUM) return
		if (waitingQueue.length === 0) return
		const task = waitingQueue.shift()!
		runningQueue.push(task)

		// 任务开始运行，当runner结束带别这个任务也结束
		const runner = async () => {
			// 装配取消令牌
			const cts = cancelTokenUtil.newSource()
			task.cancelToken = cts.token
			// 配置映射关系，过程中可能取消或者暂停任务
			cancelTokenMap.set(task, cts)

			// 该task是第一次启动，需要构建上传上下文
			// 暂停恢复则不用在次进行初始化上下文
			if (!task.context) {
				// 检查阶段
				task.uploadStatus = 'hashing'
				task.context = await uploadService
					.BuildContextAsync(task.file, cts.token, buildProgressReport(true, task))
			}

			// 处理状态
			const { status } = task.context
			if (status === 'error' || status === FileUploadStatus.syncFailed) {
				task.uploadStatus = 'error'
				return
			}
			if (status === FileUploadStatus.inSynchronizing) {
				task.uploadStatus = 'syncing'
				return
			}
			if (status === FileUploadStatus.completed) {
				task.uploadStatus = 'success'
				task.uploadProgress = 100
				return
			}

			// 启动一个定时器轮询状态
			listenerTaskStatus(task)

			// 上传阶段
			task.uploadStatus = 'uploading'
			await uploadService
				.UploadAsync(task.context, cts.token, buildProgressReport(false, task))
		}

		runner()
			.catch(reason => handleCatch(task, reason))
			.finally(() => {
				task.cancelToken?.dispose()
				runningQueue.splice(runningQueue.indexOf(task), 1)
				handleFinished(task)
			})
	}

	/*启动一个上传任务，返回一个响应式对象，可能不会立即进行上传行为*/
	function startUploadTask(
		file: File,
		onSuccess?: (fileUrl: string) => void,
		onError?: (error: string) => void,
		onCancel?: () => void,
	): UploadTask {
		const task = reactive<UploadTask>({
			context: undefined,
			cancelToken: undefined,
			file: file,
			filename: file.name,
			extension: file.name.split('.').pop() ?? '',
			hashProgress: 0,
			uploadProgress: 0,
			uploadStatus: 'waiting',
			onSuccess,
			onError,
			onCancel,
		})

		/*放入等待队列，并立即尝试调度一个任务*/
		waitingQueue.push(task)
		_TryDispatch()

		/*开启调度定时器进行调度循环*/
		_startDispatchTimer()
		return task
	}

	/*取消一个任务，要求：该任务处于hashing|uploading|waiting|paused状态*/
	function cancelTask(task: Reactive<UploadTask>) {
		if (!canTaskOperate(task, 'cancel')) return

		// 等待过程中取消
		if (task.uploadStatus === 'waiting') {
			// 从等待队列中移除任务
			waitingQueue.splice(waitingQueue.indexOf(task), 1)
			task.uploadStatus = 'canceled'
			return
		}

		// 暂停过程中取消
		if (task.uploadStatus === 'paused') {
			task.uploadStatus = 'canceled'
			return
		}

		// 处于hash过程和上传过程直接取消即可
		const cts = cancelTokenMap.get(task)!
		cts.cancel()
	}

	/*暂停一个任务；要求：该任务处于上传状态，并且上传进度小于98*/
	function pauseTask(task: Reactive<UploadTask>) {
		if (!canTaskOperate(task, 'pause')) return

		// 获取取消令牌源，执行取消动作
		const cts = cancelTokenMap.get(task)!
		cts.cancel()
		const token = cts.token

		// 等待token被dispose
		token.promise.then(() => {
			task.uploadStatus = 'paused'
		})
	}

	/*恢复任务；要求：该任务处于暂停的状态*/
	function resumeTask(task: Reactive<UploadTask>) {
		if (!canTaskOperate(task, 'resume')) return

		// 放入等待队列，等待调度器进行调度
		waitingQueue.push(task)
		task.uploadStatus = 'waiting'
	}

	return {
		runningQueue,
		waitingQueue,
		startUploadTask,
		cancelTask,
		pauseTask,
		resumeTask,
	}
})

//查看该任务是否可以进行指定动作
function canTaskOperate(task: Reactive<UploadTask>, operate: 'pause' | 'resume' | 'cancel' | 'download'): true | undefined {
	if (!isReactive(task)) return

	const { uploadStatus: status } = task
	switch (operate) {
		//暂停
		case 'pause':
			if (status !== 'uploading') return
			if (task.uploadProgress >= 98) return
			break
		//恢复
		case 'resume':
			if (!task.context) return
			if (status !== 'paused') return
			break
		//取消
		case 'cancel':
			if (status !== 'hashing' &&
				status !== 'uploading' &&
				status !== 'waiting' &&
				status !== 'paused'
			) return
			break
		//下载
		case 'download':
			if (status !== 'success') return
			break
	}

	return true
}

/*处理task运行过程中的异常*/
function handleCatch(task: UploadTask, reason?: any) {
	if (cancelTokenUtil.isCancel(reason)) {
		console.log('上传已经取消')
		task.uploadStatus = 'canceled'
	} else {
		console.log('上传过程出现错误')
		task.uploadStatus = 'error'
		throw reason
	}
}

/*处理任务结束态回调*/
function handleFinished(task: UploadTask) {
	const status = task.uploadStatus
	const context = task.context
	switch (status) {
		case 'error':
			task.onError?.(context?.errorMsg!)
			break
		case 'canceled':
			task.onCancel?.()
			break
		case 'success':
			task.onSuccess?.(context?.fileUrl!)
			break
	}
}

//轮询请求更新任务状态
function listenerTaskStatus(task: Reactive<UploadTask>) {
	if (!isReactive(task)) return false
	const { context, cancelToken } = task
	if (!context) return

	//轮询开始若取消，抛出异常
	cancelToken?.throwIfRequested()

	let timer: NodeJS.Timeout
	/*轮询间隔,根据文件大小来确定*/
	const fileSize = context.file.size
	const interval = 500 + (fileSize / (1024 * 1024 * 37.5)) * 300

	//取消轮询
	cancelToken?.onCanceled(() => {
		clearInterval(timer)
		task.uploadStatus = 'canceled'
		task.onCancel?.()
	})

	//请求任务状态更新
	const requestStatusAsync = async () => {
		const result = await uploadService
			.UploadCheckAsync(context, cancelToken)
			.then(r => {
				task.uploadStatus = 'uploading'
				return r
			})
			.catch(reason => {
				clearInterval(timer)
				task.uploadStatus = 'error'
				task.onError?.(reason)
			})


		if (!result) return

		switch (result.Status) {
			case FileUploadStatus.inProgress: {
				task.uploadStatus = 'uploading'
				break
			}
			case FileUploadStatus.inSynchronizing: {
				task.uploadStatus = 'syncing'
				break
			}
			case FileUploadStatus.notExist: {
				//什么也不做
				break
			}
			case FileUploadStatus.syncFailed: {
				clearInterval(timer)
				task.uploadStatus = 'error'
				break
			}
			case FileUploadStatus.completed: {
				clearInterval(timer)
				task.uploadStatus = 'success'
				task.uploadProgress = 100
				context.fileUrl = result.FileUrl!
				task.onSuccess?.(result.FileUrl!)
				break
			}
		}
	}

	//首次执行一次
	requestStatusAsync()

	//开始轮询更新状态
	timer = setInterval(requestStatusAsync, interval)
}

/*更新task进度，hash和上传进度*/
function buildProgressReport(isHashProgress: boolean, task: UploadTask): ProgressReport {
	const key = isHashProgress ? 'hashProgress' : 'uploadProgress'
	return (loaded, total) => {
		task[key] = (loaded * 100 / total)
	}
}
