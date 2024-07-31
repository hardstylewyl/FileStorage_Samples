import type { RouteRecordRaw } from 'vue-router'
// 静态路由表
export const staticRouter: RouteRecordRaw[] = [
	{
		path: '/',
		component: () => import('@/views/Home.vue'),
	},
]
