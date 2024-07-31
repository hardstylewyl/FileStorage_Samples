/**
 * 错误模型
 */
export interface Error {
	Code: string
	Message: string
}

/**
 * 通用的Result
 */
export type Result = {
	HttpStatusCode: number
	IsSuccess: boolean
	IsFailure: boolean
	Error?: Error
	// | Error[]
}

/**
 * 具有数据的Result
 */
export interface ResultT<T> extends Result {
	Value?: T
}

/**
 * 分页模型
 */
export type PaginatedList<T> = {
	readonly Items: T[]
	PageNumber: number
	TotalPages: number
	TotalCount: number
	HasPreviousPage: boolean
	HasNextPage: boolean
}
