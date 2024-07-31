import { createApp } from 'vue'

import App from './App.vue'
import { setupI18n } from './i18n'
import { setupRouter } from './router'
import { setupPiniaStore } from './store'

import './style/index.scss'

const app = createApp(App)
// 配置国际化
setupI18n(app)
// 配置状态存储
setupPiniaStore(app)
// 配置路由
setupRouter(app)

app.mount('#app')
