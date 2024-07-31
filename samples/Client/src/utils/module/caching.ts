/*缓存键*/
type CacheKey = string | number | symbol

/*单个缓存项目*/
type CacheEntry = {
	/*缓存键*/
	key: CacheKey
	/*缓存数据*/
	data: unknown
	/*最后一次更新时间 时间 毫秒数*/
	lastUpdateTime: number
	/*绝对过期时间 时间间隔 毫秒 undefined表示永不过期*/
	absoluteExpiration?: number
}

/*缓存源 用于存放缓存数据*/
type CacheSource = Record<CacheKey, CacheEntry>

/*批量缓存配置*/
type BatchCacheConfig = {
	/*缓存过期时间 时间间隔 毫秒 用于批量设置缓存过期时间*/
	absoluteExpiration?: number
	/*未命中的键*/
	missKeys: CacheKey[]
}

/*单个缓存工厂*/
type CacheEntryFactory<TItem> = (entry: CacheEntry) => Promise<TItem>

/*批量缓存工厂*/
type BatchCacheEntryFactory<TItem> = (config: BatchCacheConfig) => Promise<Map<CacheKey, TItem>>

// 缓存是否有效，存在并且未过期
function isCacheValid(entry?: CacheEntry) {
	return entry && (Date.now() - entry.lastUpdateTime) < (entry.absoluteExpiration ?? Infinity)
}

// 缓存获取或者创建（单个）
async function getOrCreateAsync<T>(
	source: CacheSource,
	key: CacheKey,
	factory: CacheEntryFactory<T>,
	forceRefresh: boolean = false,
) {
	return new Promise<T>((resolve, reject) => {
		const entry = source[key]
		// 缓存命中条件：不强制刷新，缓存有效
		if (!forceRefresh && isCacheValid(entry)) {
			resolve(entry.data as T)
			return
		}

		// 缓存需要更新
		const newEntry: CacheEntry = { key, lastUpdateTime: 0, data: undefined, absoluteExpiration: undefined }
		// 执行缓存创建异步
		factory(newEntry)
			.then(data => {
				newEntry.data = data
				newEntry.lastUpdateTime = Date.now()
				source[key] = newEntry
				resolve(data)
			})
			.catch(reason => reject(reason))
	})
}

// 缓存获取或者创建（批量）
async function batchGetOrCreateAsync<T>(
	source: CacheSource,
	keys: CacheKey[],
	factory: BatchCacheEntryFactory<T>,
	forceRefresh: boolean = false,
) {
	return new Promise<T[]>((resolve, reject) => {
		// 所有命中的缓存
		const entries = keys
			.map(key => source[key])
			.filter(isCacheValid)

		// 缓存结果值
		const results = forceRefresh
			? []
			: [...entries.map(entry => entry.data as T)]

		// 当不为强制刷新并且缓存全部命中
		if (!forceRefresh && entries.length === keys.length) {
			console.log('缓存命中')
			resolve(results)
			return
		}

		// 未命中的键 如果为强制刷新则任务所有键都未命中
		const missKeys = forceRefresh
			? keys
			: keys.filter(key => !entries.some(entry => entry.key === key))

		// 缓存需要更新
		const cacheConfig: BatchCacheConfig = { missKeys, absoluteExpiration: undefined }
		factory(cacheConfig)
			.then(resultMap => {
				resultMap.forEach((value, key) => {
					source[key] = {
						key,
						data: value,
						lastUpdateTime: Date.now(),
						absoluteExpiration: cacheConfig.absoluteExpiration,
					}
					// 添加至结果中
					results.push(value)
				})
				console.log('缓存更新')
				resolve(results)
			})
			.catch(reason => reject(reason))
	})
}

export {
	type BatchCacheConfig,
	type BatchCacheEntryFactory,
	batchGetOrCreateAsync,
	type CacheEntry,
	type CacheEntryFactory,
	type CacheKey,
	type CacheSource,
	getOrCreateAsync,
	isCacheValid,
}
