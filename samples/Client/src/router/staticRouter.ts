import type { RouteRecordRaw } from 'vue-router'
// 静态路由表
export const staticRouter: RouteRecordRaw[] = [
	{
		path: '/',
		redirect: '/base'
	},
	{
		path: '/base',
		component: () => import('@/views/File/Upload.vue'),
	},
	{
		path: '/queue',
		component: () => import('@/views/File/UploadQueue.vue'),
	},
]
