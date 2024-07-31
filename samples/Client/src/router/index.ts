import type { App } from 'vue'
import { createRouter, createWebHistory } from 'vue-router'

import { routers } from './routerSheel'
import { staticRouter } from './staticRouter'

const router = createRouter({
	history: createWebHistory(import.meta.env.BASE_URL),
	routes: [...routers, ...staticRouter],
})

const setupRouter = (app: App) => {
	app.use(router)
}

export { router, setupRouter }
