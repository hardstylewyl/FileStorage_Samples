// 取消令牌取消动作控制者
export type CancellationTokenSource = {
	token: CancellationToken
	cancel: () => void
}

// 取消令牌
export type CancellationToken = {
	isCanceled: boolean
	/*当令牌进行了dispose这个promise会被resolve*/
	promise: Promise<void>
	/*用于在执行某个操作之前用来立即响应取消请求*/
	throwIfRequested(): void
	/*用于执行过程中进行响应取消请求, 返回一个函数用于立即销毁监听,任何回调只要被执行了一次就会被销毁*/
	onCanceled(callback: () => void): () => void
	dispose(): void
}

/*当令牌被取消时执行回调*/
type CancelCallbackRecord = {
	wrappedCallback: () => void
	executed: boolean
}

// 用于判断该错误是否是由取消动作引起的
const CancelError = {
	__CANCEL__: true,
}

// 创建取消令牌源
function newSource(): CancellationTokenSource {
	/*终结promise，代表取消令牌已经完成取消工作*/
	let pResolve: any
	/*取消令牌被取消时执行的回调*/
	let callbacks: Array<CancelCallbackRecord> = []
	const token: CancellationToken = {
		isCanceled: false,
		promise: new Promise(resolve => pResolve = resolve),
		throwIfRequested() {
			if (token.isCanceled) {
				throw CancelError
			}
		},
		onCanceled(callback: () => void): () => void {
			/*标记这个回调是否已经执行了一次*/
			let callbackExecuted = false
			/*包装回调*/
			const wrappedCallback = () => {
				if (!token.isCanceled || callbackExecuted) return
				callback()
				callbackExecuted = true
				callbacks = callbacks.filter(cb => cb !== wrappedCallbackRecord)
			}

			const wrappedCallbackRecord = { wrappedCallback, executed: callbackExecuted }
			callbacks.push(wrappedCallbackRecord)

			/*返回回调销毁函数*/
			return () => {
				const index = callbacks.findIndex(cb => cb.wrappedCallback === wrappedCallback)
				if (index !== -1) {
					callbacks.splice(index, 1)
				}
			}
		},

		dispose() {
			pResolve()
			// 回调函
			callbacks = []
		},
	}

	return {
		cancel() {
			token.isCanceled = true
			callbacks.forEach(callbackWrap => callbackWrap.wrappedCallback())
		},
		token: token,
	}
}

export const cancelTokenUtil = {
	newSource,
	isCancel: (reason?: any) => reason && reason.__CANCEL__,
}
