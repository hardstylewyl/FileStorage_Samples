import type { CalculateChunkRequest, CalculateChunkResponse } from '@/types'
import SparkMD5 from 'spark-md5'

/*最大并发数5*/
const MAX_CONCURRENT_TASKS = 5
/*当前活跃任务数*/
let activeTasks = 0
/*任务队列*/
const taskQueue: CalculateChunkRequest[] = []
/*SparkMD5实例池*/
const sparkPool: SparkMD5.ArrayBuffer[] = []

// 初始化SparkMD5实例池
for (let i = 0; i < MAX_CONCURRENT_TASKS; i++) {
	sparkPool.push(new SparkMD5.ArrayBuffer())
}

self.onmessage = ({ data: request }: MessageEvent<CalculateChunkRequest>) => {
	console.log('worker收到消息', request)

	// 将新的计算请求添加到队列中
	taskQueue.push(request)

	// 如果当前没有活跃任务，则开始处理队列中的下一个任务
	if (activeTasks < MAX_CONCURRENT_TASKS) {
		processNextTask()
	}
}

function processNextTask() {
	if (taskQueue.length === 0 || activeTasks >= MAX_CONCURRENT_TASKS) {
		return
	}

	const { fileId, chunkSeq, chunkData } = taskQueue.shift()!
	const spark = sparkPool[activeTasks]

	activeTasks++
	chunkData.arrayBuffer().then(buffer => {
		const md5 = spark.append(buffer).end()
		spark.reset()

		self.postMessage(
			<CalculateChunkResponse> {
				fileId: fileId,
				chunkSeq: chunkSeq,
				md5,
			},
		)

		// 完成任务后，减少活跃任务计数，并检查是否可以处理更多任务
		activeTasks--
		processNextTask()
	})
}
