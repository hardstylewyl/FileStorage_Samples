import { FileUploadStatus } from '@/contracts'
import { uploadService } from '@/services/upload'
import type { ProgressReport, UploadTask } from '@/types'
import { type CancellationTokenSource, cancelTokenUtil } from '@/utils'
import { defineStore } from 'pinia'
import { reactive, ref } from 'vue'

/*最大并行上传任务数目*/
const MAX_UPLOAD_TASK_NUM = 3
/*任务调度定时器间隔*/
const DISPATCH_INTERVAL = 1500
export const useUploadStore_v2 = defineStore('upload-v2', () => {
	/*上传任务队列*/
	const runningQueue = reactive<UploadTask[]>([])
	/*等待队列*/
	const waitingQueue = reactive<UploadTask[]>([])
	/*任务和取消令牌源映射*/
	const cancelTokenMap = new WeakMap<UploadTask, CancellationTokenSource>()
	/*调度定时器*/
	const dispatchTimer = ref<NodeJS.Timeout | null>(null)

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
					.BuildContextAsync(task.file, cts.token, changeProgress(true, task))
			}

			// 处理状态
			const { status } = task.context
			if (
				status === 'error'
				|| status === FileUploadStatus.syncFailed
			) {
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
				.UploadAsync(task.context, cts.token, changeProgress(false, task))
		}

		runner()
			.catch(reason => handleCatch(task, reason))
			.finally(() => {
				task.cancelToken?.dispose()
				runningQueue.splice(runningQueue.indexOf(task), 1)
				handleFinished(task)
			})
	}

	/*取消一个任务，要求：该任务处于hashing|uploading|waiting|paused状态*/
	function cancelTask(task: UploadTask) {
		console.log('取消任务', task)

		const status = task.uploadStatus
		if (
			status !== 'hashing'
			&& status !== 'uploading'
			&& status !== 'waiting'
			&& status !== 'paused'
		) return

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
	function pauseTask(task: UploadTask) {
		console.log('暂停当前正在上传的任务', task)

		if (task.uploadStatus !== 'uploading') return
		if (task.uploadProgress >= 98) return

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
	function resumeTask(task: UploadTask) {
		console.log('恢复当前暂停的任务', task)

		if (!task.context) return
		if (task.uploadStatus !== 'paused') return

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

/*轮询请求更新task状态*/
function listenerTaskStatus(task: UploadTask) {
	const { context, cancelToken } = task
	if (!context) return

	// 轮询开始前取消，抛出异常
	cancelToken?.throwIfRequested()

	let timer: NodeJS.Timeout
	/*轮询间隔,根据文件大小来确定*/
	const fileSize = context.file.size
	const interval = 500 + (fileSize / (1024 * 1024 * 37.5)) * 300

	// 外部进行了取消动作,取消轮询
	cancelToken?.onCanceled(() => {
		console.log('轮询已经被取消')
		clearInterval(timer)
		task.uploadStatus = 'canceled'
		handleFinished(task)
	})

	// 通过请求，更新任务状态
	const changeTaskStatus = async () => {
		// 发送请求检查上传状态
		const result = await uploadService
			.UploadCheckAsync(context, cancelToken)
			.then((r) => {
				task.uploadStatus = 'uploading'
				return Promise.resolve(r)
			})
			.catch(reason => {
				console.log('检查请求出错了', reason)
				task.uploadStatus = 'error'
				clearInterval(timer)
				handleFinished(task)
			})

		if (result) {
			// 每次收到响应都更新状态
			const { Status } = result
			context.status = Status
			/*上传中*/
			if (Status === FileUploadStatus.inProgress) {
				task.uploadStatus = 'uploading'
				return
			}

			/*同步中*/
			if (Status === FileUploadStatus.inSynchronizing) {
				task.uploadStatus = 'syncing'
				return
			}

			/*文件从来没有被上传过*/
			if (Status === FileUploadStatus.notExist) {
				return
			}

			/*同步失败了*/
			if (Status === FileUploadStatus.syncFailed) {
				clearInterval(timer)
				task.uploadStatus = 'error'
				handleFinished(task)
				return
			}

			/*上传完成了*/
			if (Status === FileUploadStatus.completed) {
				clearInterval(timer)
				task.uploadProgress = 100
				task.uploadStatus = 'success'
				context.fileUrl = result.FileUrl!
				handleFinished(task)
			}
		}
	}

	// 首次执行一次
	changeTaskStatus()

	/*开启一个轮询，更新状态*/
	timer = setInterval(changeTaskStatus, interval)
}

/*更新task进度，hash和上传进度*/
function changeProgress(isHashProgress: boolean, task: UploadTask): ProgressReport {
	if (isHashProgress) {
		return (loaded, total) => {
			task.hashProgress = loaded / total * 100
		}
	}

	return (loaded, total) => {
		task.uploadProgress = loaded / total * 100
	}
}
