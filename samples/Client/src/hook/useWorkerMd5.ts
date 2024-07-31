import type { CalculateChunkRequest, CalculateChunkResponse } from '@/types'
import { type CancellationToken, createMd5Worker } from '@/utils'
import { createEventHook } from '@vueuse/core'

// 最大worker(线程)数目,尝试跟逻辑核心数一致
const MAX_WORKER_COUNT = window.navigator.hardwareConcurrency || 4

/*计算文件分片集合的md5*/
export function useWorkerMD5(requests: CalculateChunkRequest[], cancelToken?: CancellationToken) {
	// 如果取消则抛出异常
	cancelToken?.throwIfRequested()
	// 用来回调每个计算成功的分片
	const calcSuccessEventHook = createEventHook<CalculateChunkResponse>()
	// 回调取消的方法
	const cancelEventHook = createEventHook<void>()
	// 全部计算完毕回调
	const finishedEventHook = createEventHook<void>()
	// 每个线程分到的分片数目
	const workerChunkCount = Math.ceil(requests.length / MAX_WORKER_COUNT)
	// 计算完成计数
	let totalFinishedCount = 0

	// 当取消时触发回调方法
	cancelToken?.onCanceled(() => {
		cancelEventHook.trigger()
	})

	for (let i = 0; i < MAX_WORKER_COUNT; i++) {
		const worker = createMd5Worker()
		// 当计算被取消时
		const no = i
		cancelToken?.onCanceled(() => {
			console.log(no + '当前worker被关闭')
			worker.terminate()
		})

		// 这个worker需要计算的分片
		const needRequest = requests
			.slice(i * workerChunkCount, (i + 1) * workerChunkCount)

		// 提交计算请求
		needRequest
			.forEach(request => worker.postMessage(request))

		let finishedCount = 0
		// 计算成功回调
		worker.onmessage = ({ data: response }: MessageEvent<CalculateChunkResponse>) => {
			calcSuccessEventHook.trigger(response)
			// 这个worker需要计算的部分全部计算完成,释放worker
			if (++finishedCount >= needRequest.length) {
				worker.terminate()
			}
			// 当全部计算完毕
			if (++totalFinishedCount >= requests.length) {
				finishedEventHook.trigger()
			}
		}

		worker.onerror = (e) => {
			console.error(e)
		}
	}

	return {
		onSuccessCalc: calcSuccessEventHook.on,
		onCancel: cancelEventHook.on,
		onFinished: finishedEventHook.on,
	}
}
