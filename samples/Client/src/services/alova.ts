import type { Result, ResultT } from '@/contracts'
import { axiosRequestAdapter } from '@alova/adapter-axios'
import { createAlova } from 'alova'
import VueHook from 'alova/vue'

const baseURL = import.meta.env.VITE_BASE_URL
console.log(baseURL)

export const alovaIns = createAlova({
	baseURL: baseURL,
	// 在vue项目下引入VueHook，它可以帮我们用vue的ref函数创建请求相关的，可以被alova管理的状态
	statesHook: VueHook,
	// axiosRequestAdapter GlobalFetch
	requestAdapter: axiosRequestAdapter(),
	// 请求拦截器
	beforeRequest({ config }) {
		// config.credentials = 'include' //fetch配置携带cookies

		// axios配置xsrf token和携带cookies
		// 表示跨域请求时是否需要使用凭证
		config.withCredentials = true
		// 支持xsrf token
		config.withXSRFToken = true
		config.xsrfHeaderName = 'X-XSRF-TOKEN'
		config.xsrfCookieName = '__Host-X-XSRF-TOKEN'
	},
	// 响应拦截器 TODO:需要进行测试响应拦截器
	responded: {
		onSuccess(response, methodInstance) {
			let result: Result = {
				IsFailure: false,
				IsSuccess: true,
				HttpStatusCode: response.status,
				...response.data,
			}
			console.info(methodInstance.url, '成功响应', result)
			return result
		},
		onError(error, methodInstance) {
			const response = error.response
			const result: Result = {
				IsFailure: true,
				IsSuccess: false,
				HttpStatusCode: response.status,
				Error: response.data,
			}

			console.error(methodInstance.url, '错误响应', result)
			switch (result.HttpStatusCode) {
				case 400:
					// error参数已经在 【...response.data】添加
					break
				case 401:
					result.Error = { Code: 'NotAuthenticated', Message: '用户没有登录' }
					return result as any
				// break
				case 403:
					result.Error = { Code: 'NoPermissions', Message: '权限不足' }
					break
				case 404:
					result.Error = { Code: 'NotFound', Message: '资源不存在' }
					break
			}
			if (Math.floor(result.HttpStatusCode / 100) === 5) {
				result.Error = { Code: 'InternalServerError', Message: '服务器异常请尽快联系管理员' }
			}

			return result as any
		},
		onComplete(methodInstance) {
			return methodInstance
		},
	},
})

export const getRequest = <T = Result>(url: string, params?: any) =>
	alovaIns.Get<T>(url, {
		params,
		cacheFor: 0,
		shareRequest: false,
	})
export const getRequestR = <T>(url: string, params?: any) => getRequest<ResultT<T>>(url, params)

export const postRequest = <T = Result>(url: string, body?: any, config?: any) =>
	alovaIns.Post<T, unknown>(url, body, {
		...config,
		shareRequest: false,
	})
export const postRequestR = <T>(url: string, body?: any, config?: any) => postRequest<ResultT<T>>(url, body, config)
