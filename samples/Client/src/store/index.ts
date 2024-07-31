import { createPinia } from 'pinia'
import pinia_persisted_state from 'pinia-plugin-persistedstate'
import type { App } from 'vue'

export const setupPiniaStore = (app: App) => {
	const pinia = createPinia()
	pinia.use(pinia_persisted_state)
	app.use(pinia)
}
